using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 4C — fleet / vehicle operations (trips + odometer invariants).
///
/// Odometer invariant: EndOdometer &gt;= StartOdometer, and a completed trip must
/// not decrease the vehicle's current odometer. Corrections require an explicit
/// admin action (audited).
/// </summary>
public interface IFleetService
{
    /// <summary>Starts a trip for a vehicle.</summary>
    Task<VehicleTrip> StartTripAsync(int vehicleId, int driverEmployeeId, int startOdometer,
        string? destination, string? purpose, int? vehicleBookingId, CancellationToken ct = default);

    /// <summary>Completes a trip, updating the vehicle odometer.</summary>
    Task<VehicleTrip> CompleteTripAsync(int vehicleTripId, int endOdometer, CancellationToken ct = default);

    /// <summary>Explicit odometer correction (admin, audited).</summary>
    Task<Vehicle> CorrectOdometerAsync(int vehicleId, int newOdometer, int changedByEmployeeId,
        string reason, CancellationToken ct = default);
}

public sealed class FleetService : IFleetService
{
    private readonly LaoHRDbContext _context;

    public FleetService(LaoHRDbContext context)
    {
        _context = context;
    }

    public async Task<VehicleTrip> StartTripAsync(int vehicleId, int driverEmployeeId, int startOdometer,
        string? destination, string? purpose, int? vehicleBookingId, CancellationToken ct = default)
    {
        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.VehicleId == vehicleId, ct)
            ?? throw new InvalidOperationException("Vehicle not found.");

        if (vehicle.Status is "IN_MAINTENANCE" or "OUT_OF_SERVICE" or "DISPOSED")
            throw new InvalidOperationException($"Vehicle is not available (status {vehicle.Status}).");

        var trip = new VehicleTrip
        {
            VehicleId = vehicleId,
            DriverEmployeeId = driverEmployeeId,
            StartedAt = DateTime.UtcNow,
            StartOdometer = startOdometer,
            Destination = destination,
            Purpose = purpose,
            VehicleBookingId = vehicleBookingId,
        };
        _context.VehicleTrips.Add(trip);

        vehicle.Status = "IN_USE";
        vehicle.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return trip;
    }

    public async Task<VehicleTrip> CompleteTripAsync(int vehicleTripId, int endOdometer, CancellationToken ct = default)
    {
        var trip = await _context.VehicleTrips
            .FirstOrDefaultAsync(t => t.VehicleTripId == vehicleTripId, ct)
            ?? throw new InvalidOperationException("Trip not found.");

        if (trip.CompletedAt.HasValue)
            throw new InvalidOperationException("Trip is already completed.");

        if (endOdometer < trip.StartOdometer)
            throw new InvalidOperationException("End odometer cannot be less than start odometer.");

        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.VehicleId == trip.VehicleId, ct)
            ?? throw new InvalidOperationException("Vehicle not found.");

        if (endOdometer < vehicle.CurrentOdometer)
            throw new InvalidOperationException("End odometer cannot regress the vehicle's current odometer.");

        trip.EndOdometer = endOdometer;
        trip.CompletedAt = DateTime.UtcNow;

        vehicle.CurrentOdometer = endOdometer;
        vehicle.Status = "AVAILABLE";
        vehicle.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return trip;
    }

    public async Task<Vehicle> CorrectOdometerAsync(int vehicleId, int newOdometer, int changedByEmployeeId,
        string reason, CancellationToken ct = default)
    {
        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.VehicleId == vehicleId, ct)
            ?? throw new InvalidOperationException("Vehicle not found.");

        if (newOdometer < 0)
            throw new InvalidOperationException("Odometer cannot be negative.");

        vehicle.CurrentOdometer = newOdometer;
        vehicle.UpdatedAt = DateTime.UtcNow;

        // Audit the correction explicitly (the interceptor also records the change).
        _context.AuditLogs.Add(new AuditLog
        {
            UserId = changedByEmployeeId.ToString(),
            EntityName = "Vehicle",
            Action = "ODOMETER_CORRECTION",
            KeyValues = System.Text.Json.JsonSerializer.Serialize(new { VehicleId = vehicleId }),
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { NewOdometer = newOdometer, Reason = reason }),
            Timestamp = DateTime.UtcNow,
        });

        await _context.SaveChangesAsync(ct);
        return vehicle;
    }
}
