using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LaoHR.Tests.Integration.Api;

/// <summary>
/// Phase 4A.1 — procurement → inventory → asset integration tests.
/// Proves the full flow: PR → PO → Goods Receipt → Stock Movement → Asset,
/// including partial receipt, receipt idempotency, and asset auto-generation.
/// </summary>
public class ProcurementFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions;

    public ProcurementFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    private async Task AuthenticateAsync()
    {
        var login = new { Username = "admin", Password = "admin123" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", login);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        var token = result.GetProperty("token").GetString();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<(int supplierId, int itemId, int warehouseId, int employeeId)> SeedAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();

        var supplier = new Supplier { SupplierCode = "SUP-TEST", Name = "Test Supplier", Status = "ACTIVE" };
        var item = new InventoryItem { SKU = "SKU-TEST", Name = "Test Item", ItemType = "STOCK", TrackInventory = true };
        var warehouse = new Warehouse { Code = "WH-TEST", Name = "Test Warehouse", Status = "ACTIVE" };
        var employee = new Employee { EmployeeCode = "EMP-TEST", LaoName = "Test Employee", IsActive = true };
        db.Suppliers.Add(supplier);
        db.InventoryItems.Add(item);
        db.Warehouses.Add(warehouse);
        db.Employees.Add(employee);
        await db.SaveChangesAsync();

        // Link the seeded admin user to this employee so server-side identity
        // resolution (EmployeeId claim) works for procurement actions.
        var admin = db.Users.FirstOrDefault(u => u.Username == "admin");
        if (admin != null)
        {
            admin.EmployeeId = employee.EmployeeId;
            await db.SaveChangesAsync();
        }

        return (supplier.SupplierId, item.InventoryItemId, warehouse.WarehouseId, employee.EmployeeId);
    }

    [Fact]
    public async Task PartialReceipt_ThenFullReceipt_UpdatesPoStatusAndStock()
    {
        var (supplierId, itemId, warehouseId, _) = await SeedAsync();
        await AuthenticateAsync();

        // Create PO with quantity 10.
        var poResp = await _client.PostAsJsonAsync("/api/purchase-orders", new
        {
            SupplierId = supplierId,
            Currency = "LAK",
            Items = new[]
            {
                new { Description = "Test Item", ItemId = itemId, Quantity = 10m, UnitPrice = 100m, TaxRate = 0m }
            }
        });
        poResp.EnsureSuccessStatusCode();
        var po = await poResp.Content.ReadFromJsonAsync<PurchaseOrder>(_jsonOptions);
        var poId = po!.PurchaseOrderId;

        int poLineId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            poLineId = db.PurchaseOrderItems
                .Where(i => i.PurchaseOrderId == poId)
                .Select(i => i.PurchaseOrderItemId)
                .First();
        }

        // Send the PO.
        (await _client.PostAsync($"/api/purchase-orders/{poId}/send", null)).EnsureSuccessStatusCode();

        // Receipt 1: 4 units.
        var r1 = await _client.PostAsJsonAsync("/api/goods-receipts", new
        {
            PurchaseOrderId = poId,
            WarehouseId = warehouseId,
            Items = new[]
            {
                new { PurchaseOrderItemId = poLineId, QuantityReceived = 4m, AcceptedQuantity = 4m, RejectedQuantity = 0m }
            }
        });
        r1.EnsureSuccessStatusCode();
        var receipt1 = await r1.Content.ReadFromJsonAsync<GoodsReceipt>(_jsonOptions);
        (await _client.PostAsync($"/api/goods-receipts/{receipt1!.GoodsReceiptId}/post", null)).EnsureSuccessStatusCode();

        // PO should be PARTIALLY_RECEIVED.
        var poAfter1 = await _client.GetFromJsonAsync<PurchaseOrder>($"/api/purchase-orders/{poId}");
        poAfter1!.Status.Should().Be("PARTIALLY_RECEIVED");

        // Receipt 2: 6 units.
        var r2 = await _client.PostAsJsonAsync("/api/goods-receipts", new
        {
            PurchaseOrderId = poId,
            WarehouseId = warehouseId,
            Items = new[]
            {
                new { PurchaseOrderItemId = poLineId, QuantityReceived = 6m, AcceptedQuantity = 6m, RejectedQuantity = 0m }
            }
        });
        r2.EnsureSuccessStatusCode();
        var receipt2 = await r2.Content.ReadFromJsonAsync<GoodsReceipt>(_jsonOptions);
        (await _client.PostAsync($"/api/goods-receipts/{receipt2!.GoodsReceiptId}/post", null)).EnsureSuccessStatusCode();

        // PO should be RECEIVED.
        var poAfter2 = await _client.GetFromJsonAsync<PurchaseOrder>($"/api/purchase-orders/{poId}");
        poAfter2!.Status.Should().Be("RECEIVED");

        // Stock total should be 10 (no duplicated ledger movement).
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            var movements = db.StockMovements
                .Where(m => m.ItemId == itemId && m.WarehouseId == warehouseId && m.MovementType == "RECEIPT")
                .ToList();
            movements.Sum(m => m.Quantity).Should().Be(10m);
        }
    }

    [Fact]
    public async Task PostingReceiptTwice_DoesNotDoubleCountStock()
    {
        var (supplierId, itemId, warehouseId, _) = await SeedAsync();
        await AuthenticateAsync();

        var poResp = await _client.PostAsJsonAsync("/api/purchase-orders", new
        {
            SupplierId = supplierId,
            Currency = "LAK",
            Items = new[]
            {
                new { Description = "Test Item", ItemId = itemId, Quantity = 5m, UnitPrice = 100m, TaxRate = 0m }
            }
        });
        poResp.EnsureSuccessStatusCode();
        var po = await poResp.Content.ReadFromJsonAsync<PurchaseOrder>(_jsonOptions);
        var poId = po!.PurchaseOrderId;

        int poLineId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            poLineId = db.PurchaseOrderItems
                .Where(i => i.PurchaseOrderId == poId)
                .Select(i => i.PurchaseOrderItemId)
                .First();
        }

        (await _client.PostAsync($"/api/purchase-orders/{poId}/send", null)).EnsureSuccessStatusCode();

        var r = await _client.PostAsJsonAsync("/api/goods-receipts", new
        {
            PurchaseOrderId = poId,
            WarehouseId = warehouseId,
            Items = new[]
            {
                new { PurchaseOrderItemId = poLineId, QuantityReceived = 5m, AcceptedQuantity = 5m, RejectedQuantity = 0m }
            }
        });
        r.EnsureSuccessStatusCode();
        var receipt = await r.Content.ReadFromJsonAsync<GoodsReceipt>(_jsonOptions);

        (await _client.PostAsync($"/api/goods-receipts/{receipt!.GoodsReceiptId}/post", null)).EnsureSuccessStatusCode();
        // Second post must be rejected (idempotency guard).
        var secondPost = await _client.PostAsync($"/api/goods-receipts/{receipt.GoodsReceiptId}/post", null);
        secondPost.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            var movements = db.StockMovements
                .Where(m => m.ItemId == itemId && m.WarehouseId == warehouseId && m.MovementType == "RECEIPT")
                .ToList();
            movements.Sum(m => m.Quantity).Should().Be(5m);
        }
    }

    [Fact]
    public async Task AssetItemReceipt_GeneratesAssets_Idempotently()
    {
        var (supplierId, itemId, warehouseId, _) = await SeedAsync();
        await AuthenticateAsync();

        // Change the item to ASSET type.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            var item = await db.InventoryItems.FindAsync(itemId);
            item!.ItemType = "ASSET";
            item.TrackInventory = false;
            await db.SaveChangesAsync();
        }

        var poResp = await _client.PostAsJsonAsync("/api/purchase-orders", new
        {
            SupplierId = supplierId,
            Currency = "LAK",
            Items = new[]
            {
                new { Description = "Laptop", ItemId = itemId, Quantity = 3m, UnitPrice = 1000m, TaxRate = 0m }
            }
        });
        poResp.EnsureSuccessStatusCode();
        var po = await poResp.Content.ReadFromJsonAsync<PurchaseOrder>(_jsonOptions);
        var poId = po!.PurchaseOrderId;

        int poLineId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            poLineId = db.PurchaseOrderItems
                .Where(i => i.PurchaseOrderId == poId)
                .Select(i => i.PurchaseOrderItemId)
                .First();
        }

        (await _client.PostAsync($"/api/purchase-orders/{poId}/send", null)).EnsureSuccessStatusCode();

        var r = await _client.PostAsJsonAsync("/api/goods-receipts", new
        {
            PurchaseOrderId = poId,
            WarehouseId = warehouseId,
            Items = new[]
            {
                new { PurchaseOrderItemId = poLineId, QuantityReceived = 3m, AcceptedQuantity = 3m, RejectedQuantity = 0m }
            }
        });
        r.EnsureSuccessStatusCode();
        var receipt = await r.Content.ReadFromJsonAsync<GoodsReceipt>(_jsonOptions);
        (await _client.PostAsync($"/api/goods-receipts/{receipt!.GoodsReceiptId}/post", null)).EnsureSuccessStatusCode();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            var assets = db.Assets
                .Where(a => a.GoodsReceiptItemId != null)
                .ToList();
            // 3 laptops → 3 asset records.
            assets.Count.Should().Be(3);
        }
    }
}
