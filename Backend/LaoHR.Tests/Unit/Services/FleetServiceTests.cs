using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

public class FleetServiceTests
{
    private readonly LaoHRDbContext _context;
    private readonly FleetService _service;

    public FleetServiceTests()
    {
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new LaoHRDbContext(options);
        _service = new FleetService(_context);
    }

    private async Task<int> SeedVehicleAsync(int currentOdometer = 1000, string status = "AVAILABLE")
    {
        var vehicle = new Vehicle { VehicleCode = "V1", RegistrationNumber = "REG-1", Status = status, CurrentOdometer = currentOdometer };
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();
        return vehicle.VehicleId;
    }

    [Fact]
    public async Task StartTrip_SetsVehicleInUse()
    {
        var vehicleId = await SeedVehicleAsync();
        var trip = await _service.StartTripAsync(vehicleId, 1, 1000, "Vientiane", "Delivery", null);

        trip.StartOdometer.Should().Be(1000);
        var vehicle = await _context.Vehicles.FindAsync(vehicleId);
        vehicle!.Status.Should().Be("IN_USE");
    }

    [Fact]
    public async Task CompleteTrip_UpdatesOdometer()
    {
        var vehicleId = await SeedVehicleAsync(1000);
        var trip = await _service.StartTripAsync(vehicleId, 1, 1000, "Vientiane", "Delivery", null);

        await _service.CompleteTripAsync(trip.VehicleTripId, 1100);

        var vehicle = await _context.Vehicles.FindAsync(vehicleId);
        vehicle!.CurrentOdometer.Should().Be(1100);
        vehicle.Status.Should().Be("AVAILABLE");
    }

    [Fact]
    public async Task CompleteTrip_EndBelowStart_Throws()
    {
        var vehicleId = await SeedVehicleAsync(1000);
        var trip = await _service.StartTripAsync(vehicleId, 1, 1000, "Vientiane", "Delivery", null);

        var act = () => _service.CompleteTripAsync(trip.VehicleTripId, 900);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CompleteTrip_RegressesVehicleOdometer_Throws()
    {
        var vehicleId = await SeedVehicleAsync(1500);
        var trip = await _service.StartTripAsync(vehicleId, 1, 1500, "Vientiane", "Delivery", null);

        // End odometer 1400 < vehicle current 1500 → rejected.
        var act = () => _service.CompleteTripAsync(trip.VehicleTripId, 1400);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CorrectOdometer_WritesAudit()
    {
        var vehicleId = await SeedVehicleAsync(1000);

        await _service.CorrectOdometerAsync(vehicleId, 2000, 1, "Odometer replaced");

        var vehicle = await _context.Vehicles.FindAsync(vehicleId);
        vehicle!.CurrentOdometer.Should().Be(2000);

        _context.AuditLogs.Should().Contain(a => a.Action == "ODOMETER_CORRECTION");
    }
}
