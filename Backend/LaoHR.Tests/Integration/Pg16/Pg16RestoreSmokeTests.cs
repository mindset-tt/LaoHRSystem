using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using Xunit;

namespace LaoHR.Tests.Integration.Pg16;

/// <summary>
/// Phase 4C.1 — application-level restore smoke.
///
/// Connects the application's <see cref="LaoHRDbContext"/> to a RESTORED
/// PostgreSQL 16 database (connection string from LAOHR_RESTORE_CONNECTION) and
/// queries the corporate entities, proving the restored data deserializes and
/// relationships resolve through the real application data layer.
///
/// Skipped when LAOHR_RESTORE_CONNECTION is unset.
/// </summary>
public class Pg16RestoreSmokeTests
{
    private static string? ConnectionString => Environment.GetEnvironmentVariable("LAOHR_RESTORE_CONNECTION");

    private static LaoHRDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseNpgsql(ConnectionString!)
            .Options;
        return new LaoHRDbContext(options);
    }

    private static bool IsAvailable() => !string.IsNullOrWhiteSpace(ConnectionString);

    [Fact]
    public async Task RestoredDb_CorporateEntities_DeserializeAndResolve()
    {
        if (!IsAvailable()) return;

        using var db = CreateContext();

        // Corporate document + versions.
        var doc = await db.CorporateDocuments
            .Include(d => d.Versions)
            .FirstOrDefaultAsync(d => d.DocumentNumber == "DR-DOC-001");
        doc.Should().NotBeNull();
        doc!.CurrentVersion.Should().Be(2);
        doc.Versions.Should().HaveCount(2);

        // Contract + history.
        var contract = await db.Contracts
            .FirstOrDefaultAsync(c => c.ContractNumber == "DR-CTR-001");
        contract.Should().NotBeNull();
        var history = await db.ContractHistories
            .FirstOrDefaultAsync(h => h.ContractId == contract!.ContractId);
        history.Should().NotBeNull();
        history!.ChangeType.Should().Be("RENEWAL");

        // Service request + history.
        var sr = await db.ServiceRequests
            .FirstOrDefaultAsync(s => s.RequestNumber == "DR-SR-001");
        sr.Should().NotBeNull();

        // Facility + room + booking.
        var facility = await db.Facilities
            .FirstOrDefaultAsync(f => f.FacilityCode == "DR-FAC-001");
        facility.Should().NotBeNull();
        var room = await db.Rooms.FirstOrDefaultAsync(r => r.FacilityId == facility!.FacilityId);
        room.Should().NotBeNull();
        var booking = await db.RoomBookings.FirstOrDefaultAsync(b => b.RoomId == room!.RoomId);
        booking.Should().NotBeNull();

        // Work order.
        var wo = await db.WorkOrders.FirstOrDefaultAsync(w => w.WorkOrderNumber == "DR-WO-001");
        wo.Should().NotBeNull();

        // Vehicle + booking + trip.
        var vehicle = await db.Vehicles
            .FirstOrDefaultAsync(v => v.RegistrationNumber == "DR-REG-001");
        vehicle.Should().NotBeNull();
        var trip = await db.VehicleTrips.FirstOrDefaultAsync(t => t.VehicleId == vehicle!.VehicleId);
        trip.Should().NotBeNull();
        trip!.EndOdometer.Should().Be(1050);

        // Travel + linked expense.
        var travel = await db.TravelRequests
            .FirstOrDefaultAsync(t => t.TravelNumber == "DR-TRV-001");
        travel.Should().NotBeNull();
        var expense = await db.Expenses
            .FirstOrDefaultAsync(e => e.TravelRequestId == travel!.TravelRequestId);
        expense.Should().NotBeNull();

        // Visitor + visit.
        var visitor = await db.Visitors.FirstOrDefaultAsync(v => v.FullName == "DR Visitor");
        visitor.Should().NotBeNull();
        var visit = await db.Visits.FirstOrDefaultAsync(v => v.VisitorId == visitor!.VisitorId);
        visit.Should().NotBeNull();
    }
}
