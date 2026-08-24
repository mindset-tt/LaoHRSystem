using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 4C — room and vehicle booking with concurrency safety.
///
/// Overlap invariant (absolute, excludes CANCELLED):
///   existing.StartAt &lt; requested.EndAt AND existing.EndAt &gt; requested.StartAt
///
/// Concurrency: the room/vehicle row is locked (SELECT ... FOR UPDATE on
/// PostgreSQL) inside a transaction so two simultaneous overlapping bookings
/// serialize and only one succeeds. Proven on PostgreSQL 16.
/// </summary>
public interface IBookingService
{
    /// <summary>Creates a room booking, rejecting overlap and capacity violations.</summary>
    Task<RoomBooking> BookRoomAsync(
        int roomId, int bookedByEmployeeId, DateTime startAt, DateTime endAt,
        string title, string? purpose, int? participantCount, CancellationToken ct = default);

    /// <summary>Creates a vehicle booking, rejecting overlap and unavailable vehicles.</summary>
    Task<VehicleBooking> BookVehicleAsync(
        int vehicleId, int requesterEmployeeId, int? driverEmployeeId,
        DateTime startAt, DateTime endAt, string purpose, string? destination,
        int? projectId, CancellationToken ct = default);

    /// <summary>Cancels a room booking (frees the slot).</summary>
    Task CancelRoomBookingAsync(int roomBookingId, int actorEmployeeId, CancellationToken ct = default);

    /// <summary>Cancels a vehicle booking (frees the slot).</summary>
    Task CancelVehicleBookingAsync(int vehicleBookingId, int actorEmployeeId, CancellationToken ct = default);
}

public sealed class BookingService : IBookingService
{
    private readonly LaoHRDbContext _context;
    private readonly INumberSequenceService _numbers;

    public BookingService(LaoHRDbContext context, INumberSequenceService numbers)
    {
        _context = context;
        _numbers = numbers;
    }

    public async Task<RoomBooking> BookRoomAsync(
        int roomId, int bookedByEmployeeId, DateTime startAt, DateTime endAt,
        string title, string? purpose, int? participantCount, CancellationToken ct = default)
    {
        if (endAt <= startAt)
            throw new InvalidOperationException("Booking end must be after start.");

        var relational = _context.Database.IsRelational();
        await using var tx = relational ? await _context.Database.BeginTransactionAsync(ct) : null;
        try
        {
            var room = await LockRoomAsync(roomId, ct)
                ?? throw new InvalidOperationException("Room not found.");

            if (!room.Bookable || !room.IsActive)
                throw new InvalidOperationException("Room is not bookable.");

            if (participantCount.HasValue && room.Capacity.HasValue && participantCount.Value > room.Capacity.Value)
                throw new InvalidOperationException($"Participant count exceeds room capacity ({room.Capacity}).");

            var overlap = await _context.RoomBookings
                .AnyAsync(b => b.RoomId == roomId
                    && b.Status != "CANCELLED"
                    && b.StartAt < endAt
                    && b.EndAt > startAt, ct);
            if (overlap)
                throw new InvalidOperationException("Room is already booked for the requested time.");

            var booking = new RoomBooking
            {
                BookingNumber = await _numbers.NextAsync("RB"),
                RoomId = roomId,
                BookedByEmployeeId = bookedByEmployeeId,
                StartAt = startAt,
                EndAt = endAt,
                Title = title,
                Purpose = purpose,
                ParticipantCount = participantCount,
                Status = "CONFIRMED",
            };
            _context.RoomBookings.Add(booking);
            await _context.SaveChangesAsync(ct);
            if (tx != null) await tx.CommitAsync(ct);
            return booking;
        }
        catch
        {
            if (tx != null) await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<VehicleBooking> BookVehicleAsync(
        int vehicleId, int requesterEmployeeId, int? driverEmployeeId,
        DateTime startAt, DateTime endAt, string purpose, string? destination,
        int? projectId, CancellationToken ct = default)
    {
        if (endAt <= startAt)
            throw new InvalidOperationException("Booking end must be after start.");

        var relational = _context.Database.IsRelational();
        await using var tx = relational ? await _context.Database.BeginTransactionAsync(ct) : null;
        try
        {
            var vehicle = await LockVehicleAsync(vehicleId, ct)
                ?? throw new InvalidOperationException("Vehicle not found.");

            if (vehicle.Status is "IN_MAINTENANCE" or "OUT_OF_SERVICE" or "DISPOSED")
                throw new InvalidOperationException($"Vehicle is not available (status {vehicle.Status}).");

            var overlap = await _context.VehicleBookings
                .AnyAsync(b => b.VehicleId == vehicleId
                    && b.Status != "CANCELLED"
                    && b.StartAt < endAt
                    && b.EndAt > startAt, ct);
            if (overlap)
                throw new InvalidOperationException("Vehicle is already booked for the requested time.");

            var booking = new VehicleBooking
            {
                BookingNumber = await _numbers.NextAsync("VB"),
                VehicleId = vehicleId,
                RequesterEmployeeId = requesterEmployeeId,
                DriverEmployeeId = driverEmployeeId,
                StartAt = startAt,
                EndAt = endAt,
                Purpose = purpose,
                Destination = destination,
                ProjectId = projectId,
                Status = "CONFIRMED",
            };
            _context.VehicleBookings.Add(booking);
            await _context.SaveChangesAsync(ct);
            if (tx != null) await tx.CommitAsync(ct);
            return booking;
        }
        catch
        {
            if (tx != null) await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task CancelRoomBookingAsync(int roomBookingId, int actorEmployeeId, CancellationToken ct = default)
    {
        var booking = await _context.RoomBookings
            .FirstOrDefaultAsync(b => b.RoomBookingId == roomBookingId, ct)
            ?? throw new InvalidOperationException("Room booking not found.");

        if (booking.Status == "CANCELLED")
            throw new InvalidOperationException("Booking is already cancelled.");

        booking.Status = "CANCELLED";
        await _context.SaveChangesAsync(ct);
    }

    public async Task CancelVehicleBookingAsync(int vehicleBookingId, int actorEmployeeId, CancellationToken ct = default)
    {
        var booking = await _context.VehicleBookings
            .FirstOrDefaultAsync(b => b.VehicleBookingId == vehicleBookingId, ct)
            ?? throw new InvalidOperationException("Vehicle booking not found.");

        if (booking.Status == "CANCELLED")
            throw new InvalidOperationException("Booking is already cancelled.");

        booking.Status = "CANCELLED";
        await _context.SaveChangesAsync(ct);
    }

    private async Task<Room?> LockRoomAsync(int roomId, CancellationToken ct)
    {
        if (_context.Database.IsRelational())
        {
            return await _context.Rooms
                .FromSqlRaw("SELECT * FROM \"Rooms\" WHERE \"RoomId\" = {0} FOR UPDATE", roomId)
                .FirstOrDefaultAsync(ct);
        }
        return await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == roomId, ct);
    }

    private async Task<Vehicle?> LockVehicleAsync(int vehicleId, CancellationToken ct)
    {
        if (_context.Database.IsRelational())
        {
            return await _context.Vehicles
                .FromSqlRaw("SELECT * FROM \"Vehicles\" WHERE \"VehicleId\" = {0} FOR UPDATE", vehicleId)
                .FirstOrDefaultAsync(ct);
        }
        return await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == vehicleId, ct);
    }
}
