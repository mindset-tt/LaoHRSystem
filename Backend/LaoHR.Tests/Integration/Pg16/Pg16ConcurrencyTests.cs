using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Xunit;

namespace LaoHR.Tests.Integration.Pg16;

/// <summary>
/// Phase 4B.2 — REAL PostgreSQL 16 concurrency evidence.
///
/// These tests run against a real Npgsql database (connection string from the
/// LAOHR_TEST_CONNECTION environment variable). They are SKIPPED when that
/// variable is unset (so the normal InMemory suite is unaffected). The
/// scripts/validate-finance-postgres.ps1 harness sets it and runs these.
///
/// This is the difference between "I wrote concurrency-safe code" and
/// "I proved concurrency on PostgreSQL".
/// </summary>
public class Pg16ConcurrencyTests
{
    private static string? ConnectionString => Environment.GetEnvironmentVariable("LAOHR_TEST_CONNECTION");

    private static LaoHRDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseNpgsql(ConnectionString!)
            .Options;
        return new LaoHRDbContext(options);
    }

    private static bool IsAvailable() => !string.IsNullOrWhiteSpace(ConnectionString);

    // ---- Payment race ----

    [Fact]
    public async Task PaymentRace_TwoConcurrentPayments_DoNotOverpay()
    {
        if (!IsAvailable()) return; // skipped in InMemory runs

        // Invoice total 100, remaining 100.
        int invoiceId;
        using (var db = CreateContext())
        {
            var supplier = new Supplier { SupplierCode = $"SUP-{Guid.NewGuid():N}"[..8], Name = "Race Supplier", Status = "ACTIVE" };
            db.Suppliers.Add(supplier);
            await db.SaveChangesAsync();
            var invoice = new SupplierInvoice
            {
                InvoiceNumber = $"INV-{Guid.NewGuid():N}"[..12],
                SupplierId = supplier.SupplierId,
                InvoiceDate = DateTime.UtcNow,
                Currency = "LAK",
                Subtotal = 100,
                TotalAmount = 100,
                RemainingAmount = 100,
                Status = "APPROVED",
            };
            db.SupplierInvoices.Add(invoice);
            await db.SaveChangesAsync();
            invoiceId = invoice.SupplierInvoiceId;
        }

        // Two concurrent payments of 80 each.
        var tasks = new[]
        {
            Task.Run(() => PostPaymentAsync(invoiceId, 80)),
            Task.Run(() => PostPaymentAsync(invoiceId, 80)),
        };

        var results = await Task.WhenAll(tasks.Select(t => t.ContinueWith(x => x.IsCompletedSuccessfully)));

        // Exactly one succeeds, one fails (overpayment rejected).
        results.Count(r => r).Should().Be(1);

        using (var db = CreateContext())
        {
            var invoice = await db.SupplierInvoices.FindAsync(invoiceId);
            invoice!.PaidAmount.Should().Be(80);
            invoice.RemainingAmount.Should().Be(20);
            invoice.Status.Should().Be("PARTIALLY_PAID");
        }
    }

    private static async Task PostPaymentAsync(int invoiceId, decimal amount)
    {
        using var db = CreateContext();
        var ap = new AccountsPayableService(db);
        var payment = new Payment
        {
            PaymentNumber = $"PAY-{Guid.NewGuid():N}"[..12],
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = "BANK_TRANSFER",
            Currency = "LAK",
            Amount = amount,
            Status = "DRAFT",
        };
        payment.Allocations.Add(new PaymentAllocation { SupplierInvoiceId = invoiceId, Amount = amount });
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        await ap.PostPaymentAsync(payment.PaymentId, 1);
    }

    // ---- Journal post race ----

    [Fact]
    public async Task JournalPostRace_TwoConcurrentPosts_OnePosting()
    {
        if (!IsAvailable()) return;

        int journalId;
        using (var db = CreateContext())
        {
            var cash = new Account { AccountCode = $"C-{Guid.NewGuid():N}"[..6], Name = "Cash", AccountType = "ASSET", IsPostingAccount = true };
            var expense = new Account { AccountCode = $"E-{Guid.NewGuid():N}"[..6], Name = "Expense", AccountType = "EXPENSE", IsPostingAccount = true };
            db.Accounts.AddRange(cash, expense);
            await db.SaveChangesAsync();
            var year = new FiscalYear { Name = "FY", StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), Status = "OPEN" };
            db.FiscalYears.Add(year);
            await db.SaveChangesAsync();
            var period = new FiscalPeriod { FiscalYearId = year.FiscalYearId, PeriodNumber = 1, StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), Status = "OPEN" };
            db.FiscalPeriods.Add(period);
            await db.SaveChangesAsync();

            var journal = new JournalEntry
            {
                JournalNumber = $"JE-{Guid.NewGuid():N}"[..12],
                PostingDate = DateTime.UtcNow,
                FiscalPeriodId = period.FiscalPeriodId,
                SourceType = "MANUAL_JOURNAL",
                Status = "DRAFT",
                Currency = "LAK",
            };
            journal.Lines.Add(new JournalLine { AccountId = expense.AccountId, Debit = 100, Credit = 0 });
            journal.Lines.Add(new JournalLine { AccountId = cash.AccountId, Debit = 0, Credit = 100 });
            db.JournalEntries.Add(journal);
            await db.SaveChangesAsync();
            journalId = journal.JournalEntryId;
        }

        var tasks = new[]
        {
            Task.Run(() => PostJournalAsync(journalId)),
            Task.Run(() => PostJournalAsync(journalId)),
        };
        var results = await Task.WhenAll(tasks.Select(t => t.ContinueWith(x => x.IsCompletedSuccessfully)));

        // One succeeds, one fails (already POSTED).
        results.Count(r => r).Should().Be(1);

        using (var db = CreateContext())
        {
            var journal = await db.JournalEntries.FindAsync(journalId);
            journal!.Status.Should().Be("POSTED");
            var lineCount = await db.JournalLines.CountAsync(l => l.JournalEntryId == journalId);
            lineCount.Should().Be(2); // no duplicate lines
        }
    }

    private static async Task PostJournalAsync(int journalId)
    {
        using var db = CreateContext();
        var numbers = new NumberSequenceService(db);
        var accounting = new AccountingService(db, numbers);
        await accounting.PostAsync(journalId, 1);
    }

    // ---- Auto-post race (source uniqueness at DB level) ----

    [Fact]
    public async Task AutoPostRace_TwoConcurrentPosts_OneSourceJournal()
    {
        if (!IsAvailable()) return;

        int invoiceId;
        using (var db = CreateContext())
        {
            // Seed config.
            var ap = new Account { AccountCode = $"AP-{Guid.NewGuid():N}"[..6], Name = "AP", AccountType = "LIABILITY", IsPostingAccount = true };
            var expense = new Account { AccountCode = $"EX-{Guid.NewGuid():N}"[..6], Name = "Expense", AccountType = "EXPENSE", IsPostingAccount = true };
            var cash = new Account { AccountCode = $"CA-{Guid.NewGuid():N}"[..6], Name = "Cash", AccountType = "ASSET", IsPostingAccount = true };
            var payable = new Account { AccountCode = $"PY-{Guid.NewGuid():N}"[..6], Name = "Payable", AccountType = "LIABILITY", IsPostingAccount = true };
            db.Accounts.AddRange(ap, expense, cash, payable);
            await db.SaveChangesAsync();
            // Idempotent config seeding (upsert by key) so re-runs don't collide.
            await UpsertSettingAsync(db, "AP_CONTROL_ACCOUNT", ap.AccountId.ToString());
            await UpsertSettingAsync(db, "DEFAULT_EXPENSE_ACCOUNT", expense.AccountId.ToString());
            await UpsertSettingAsync(db, "CASH_ACCOUNT", cash.AccountId.ToString());
            await UpsertSettingAsync(db, "EMPLOYEE_PAYABLE_ACCOUNT", payable.AccountId.ToString());
            var year = new FiscalYear { Name = "FY", StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), Status = "OPEN" };
            db.FiscalYears.Add(year);
            await db.SaveChangesAsync();
            var period = new FiscalPeriod { FiscalYearId = year.FiscalYearId, PeriodNumber = 1, StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), Status = "OPEN" };
            db.FiscalPeriods.Add(period);
            await db.SaveChangesAsync();

            var supplier = new Supplier { SupplierCode = $"SP-{Guid.NewGuid():N}"[..6], Name = "S", Status = "ACTIVE" };
            db.Suppliers.Add(supplier);
            await db.SaveChangesAsync();
            var invoice = new SupplierInvoice
            {
                InvoiceNumber = $"INV-{Guid.NewGuid():N}"[..12],
                SupplierId = supplier.SupplierId,
                InvoiceDate = DateTime.UtcNow,
                Currency = "LAK",
                Subtotal = 100,
                TotalAmount = 100,
                RemainingAmount = 100,
                Status = "APPROVED",
            };
            invoice.Lines.Add(new SupplierInvoiceLine { Description = "Item", Quantity = 1, UnitPrice = 100, Subtotal = 100 });
            db.SupplierInvoices.Add(invoice);
            await db.SaveChangesAsync();
            invoiceId = invoice.SupplierInvoiceId;
        }

        var tasks = new[]
        {
            Task.Run(() => PostInvoiceAsync(invoiceId)),
            Task.Run(() => PostInvoiceAsync(invoiceId)),
        };
        var results = await Task.WhenAll(tasks.Select(t => t.ContinueWith(x => x.IsCompletedSuccessfully)));

        // One succeeds, one fails (source uniqueness).
        results.Count(r => r).Should().Be(1);

        using (var db = CreateContext())
        {
            var count = await db.JournalEntries
                .CountAsync(j => j.SourceType == "SUPPLIER_INVOICE" && j.SourceId == invoiceId && j.PostingPurpose == "INVOICE");
            count.Should().Be(1); // exactly one source journal
        }
    }

    private static async Task PostInvoiceAsync(int invoiceId)
    {
        using var db = CreateContext();
        var numbers = new NumberSequenceService(db);
        var accounting = new AccountingService(db, numbers);
        var config = new AccountingConfigurationService(db);
        var budget = new BudgetService(db);
        var posting = new PostingService(db, config, accounting, budget, numbers);
        await posting.PostSupplierInvoiceAsync(invoiceId, 1);
    }

    private static async Task UpsertSettingAsync(LaoHRDbContext db, string key, string value)
    {
        var existing = await db.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == key);
        if (existing == null)
        {
            db.SystemSettings.Add(new SystemSetting { SettingKey = key, SettingValue = value });
        }
        else
        {
            existing.SettingValue = value;
        }
        await db.SaveChangesAsync();
    }

    // ---- Period-close race ----

    [Fact]
    public async Task PeriodCloseRace_PostingToClosedPeriod_Rejected()
    {
        if (!IsAvailable()) return;

        int periodId;
        using (var db = CreateContext())
        {
            var year = new FiscalYear { Name = "FY", StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), Status = "OPEN" };
            db.FiscalYears.Add(year);
            await db.SaveChangesAsync();
            var period = new FiscalPeriod { FiscalYearId = year.FiscalYearId, PeriodNumber = 1, StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), Status = "OPEN" };
            db.FiscalPeriods.Add(period);
            await db.SaveChangesAsync();
            periodId = period.FiscalPeriodId;
        }

        // Close the period.
        using (var db = CreateContext())
        {
            var period = await db.FiscalPeriods.FindAsync(periodId);
            period!.Status = "CLOSED";
            await db.SaveChangesAsync();
        }

        // Attempt to post a journal to the closed period → must be rejected.
        using (var db = CreateContext())
        {
            var cash = new Account { AccountCode = $"C-{Guid.NewGuid():N}"[..6], Name = "Cash", AccountType = "ASSET", IsPostingAccount = true };
            var expense = new Account { AccountCode = $"E-{Guid.NewGuid():N}"[..6], Name = "Expense", AccountType = "EXPENSE", IsPostingAccount = true };
            db.Accounts.AddRange(cash, expense);
            await db.SaveChangesAsync();
            var journal = new JournalEntry
            {
                JournalNumber = $"JE-{Guid.NewGuid():N}"[..12],
                PostingDate = DateTime.UtcNow,
                FiscalPeriodId = periodId,
                SourceType = "MANUAL_JOURNAL",
                Status = "DRAFT",
                Currency = "LAK",
            };
            journal.Lines.Add(new JournalLine { AccountId = expense.AccountId, Debit = 100, Credit = 0 });
            journal.Lines.Add(new JournalLine { AccountId = cash.AccountId, Debit = 0, Credit = 100 });
            db.JournalEntries.Add(journal);
            await db.SaveChangesAsync();

            var numbers = new NumberSequenceService(db);
            var accounting = new AccountingService(db, numbers);
            var act = () => accounting.PostAsync(journal.JournalEntryId, 1);
            await act.Should().ThrowAsync<InvalidOperationException>();
        }
    }

    // ---- Phase 4C — Room booking race ----

    [Fact]
    public async Task RoomBookingRace_TwoConcurrentOverlappingBookings_OneSucceeds()
    {
        if (!IsAvailable()) return;

        int roomId;
        using (var db = CreateContext())
        {
            var facility = new Facility { FacilityCode = $"F-{Guid.NewGuid():N}"[..6], Name = "HQ", FacilityType = "OFFICE", Status = "ACTIVE" };
            db.Facilities.Add(facility);
            await db.SaveChangesAsync();
            var room = new Room { FacilityId = facility.FacilityId, Code = $"R-{Guid.NewGuid():N}"[..6], Name = "Room", RoomType = "MEETING_ROOM", Bookable = true, IsActive = true };
            db.Rooms.Add(room);
            await db.SaveChangesAsync();
            roomId = room.RoomId;
        }

        var start = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        var tasks = new[]
        {
            Task.Run(() => BookRoomAsync(roomId, start, end)),
            Task.Run(() => BookRoomAsync(roomId, start, end)),
        };
        var results = await Task.WhenAll(tasks.Select(t => t.ContinueWith(x => x.IsCompletedSuccessfully)));

        // Exactly one succeeds.
        results.Count(r => r).Should().Be(1);

        using (var db = CreateContext())
        {
            var count = await db.RoomBookings.CountAsync(b => b.RoomId == roomId && b.Status != "CANCELLED");
            count.Should().Be(1);
        }
    }

    private static async Task BookRoomAsync(int roomId, DateTime start, DateTime end)
    {
        using var db = CreateContext();
        var numbers = new NumberSequenceService(db);
        var booking = new BookingService(db, numbers);
        await booking.BookRoomAsync(roomId, 1, start, end, "Meeting", null, null);
    }

    // ---- Phase 4C — Vehicle booking race ----

    [Fact]
    public async Task VehicleBookingRace_TwoConcurrentOverlappingBookings_OneSucceeds()
    {
        if (!IsAvailable()) return;

        int vehicleId;
        using (var db = CreateContext())
        {
            var vehicle = new Vehicle { VehicleCode = $"V-{Guid.NewGuid():N}"[..6], RegistrationNumber = $"REG-{Guid.NewGuid():N}"[..8], Status = "AVAILABLE" };
            db.Vehicles.Add(vehicle);
            await db.SaveChangesAsync();
            vehicleId = vehicle.VehicleId;
        }

        var start = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        var tasks = new[]
        {
            Task.Run(() => BookVehicleAsync(vehicleId, start, end)),
            Task.Run(() => BookVehicleAsync(vehicleId, start, end)),
        };
        var results = await Task.WhenAll(tasks.Select(t => t.ContinueWith(x => x.IsCompletedSuccessfully)));

        // Exactly one succeeds.
        results.Count(r => r).Should().Be(1);

        using (var db = CreateContext())
        {
            var count = await db.VehicleBookings.CountAsync(b => b.VehicleId == vehicleId && b.Status != "CANCELLED");
            count.Should().Be(1);
        }
    }

    private static async Task BookVehicleAsync(int vehicleId, DateTime start, DateTime end)
    {
        using var db = CreateContext();
        var numbers = new NumberSequenceService(db);
        var booking = new BookingService(db, numbers);
        await booking.BookVehicleAsync(vehicleId, 1, null, start, end, "Trip", null, null);
    }
}
