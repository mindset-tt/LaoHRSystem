using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

public class BookingServiceTests
{
    private readonly LaoHRDbContext _context;
    private readonly BookingService _service;

    public BookingServiceTests()
    {
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new LaoHRDbContext(options);
        _service = new BookingService(_context, new NumberSequenceService(_context));
    }

    private async Task<int> SeedRoomAsync(int? capacity = null)
    {
        var facility = new Facility { FacilityCode = "F1", Name = "HQ", FacilityType = "OFFICE", Status = "ACTIVE" };
        _context.Facilities.Add(facility);
        await _context.SaveChangesAsync();

        var room = new Room { FacilityId = facility.FacilityId, Code = "R1", Name = "Room 1", RoomType = "MEETING_ROOM", Capacity = capacity, Bookable = true, IsActive = true };
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        return room.RoomId;
    }

    private async Task<int> SeedVehicleAsync(string status = "AVAILABLE")
    {
        var vehicle = new Vehicle { VehicleCode = "V1", RegistrationNumber = "REG-1", Status = status };
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();
        return vehicle.VehicleId;
    }

    [Fact]
    public async Task BookRoom_NoOverlap_Succeeds()
    {
        var roomId = await SeedRoomAsync();
        var booking = await _service.BookRoomAsync(roomId, 1, new DateTime(2026, 1, 1, 9, 0, 0), new DateTime(2026, 1, 1, 10, 0, 0), "Meeting", null, null);
        booking.Status.Should().Be("CONFIRMED");
    }

    [Fact]
    public async Task BookRoom_Overlap_Throws()
    {
        var roomId = await SeedRoomAsync();
        await _service.BookRoomAsync(roomId, 1, new DateTime(2026, 1, 1, 9, 0, 0), new DateTime(2026, 1, 1, 10, 0, 0), "A", null, null);

        var act = () => _service.BookRoomAsync(roomId, 2, new DateTime(2026, 1, 1, 9, 30, 0), new DateTime(2026, 1, 1, 10, 30, 0), "B", null, null);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task BookRoom_AdjacentBoundary_NoOverlap()
    {
        var roomId = await SeedRoomAsync();
        await _service.BookRoomAsync(roomId, 1, new DateTime(2026, 1, 1, 9, 0, 0), new DateTime(2026, 1, 1, 10, 0, 0), "A", null, null);

        // 10:00–11:00 is adjacent (not overlapping).
        var booking = await _service.BookRoomAsync(roomId, 2, new DateTime(2026, 1, 1, 10, 0, 0), new DateTime(2026, 1, 1, 11, 0, 0), "B", null, null);
        booking.Status.Should().Be("CONFIRMED");
    }

    [Fact]
    public async Task BookRoom_ExceedsCapacity_Throws()
    {
        var roomId = await SeedRoomAsync(capacity: 5);
        var act = () => _service.BookRoomAsync(roomId, 1, new DateTime(2026, 1, 1, 9, 0, 0), new DateTime(2026, 1, 1, 10, 0, 0), "Meeting", null, 10);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task BookRoom_EndBeforeStart_Throws()
    {
        var roomId = await SeedRoomAsync();
        var act = () => _service.BookRoomAsync(roomId, 1, new DateTime(2026, 1, 1, 10, 0, 0), new DateTime(2026, 1, 1, 9, 0, 0), "Meeting", null, null);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CancelRoomBooking_FreesSlot()
    {
        var roomId = await SeedRoomAsync();
        var booking = await _service.BookRoomAsync(roomId, 1, new DateTime(2026, 1, 1, 9, 0, 0), new DateTime(2026, 1, 1, 10, 0, 0), "A", null, null);

        await _service.CancelRoomBookingAsync(booking.RoomBookingId, 1);

        // Same slot can now be rebooked.
        var booking2 = await _service.BookRoomAsync(roomId, 2, new DateTime(2026, 1, 1, 9, 0, 0), new DateTime(2026, 1, 1, 10, 0, 0), "B", null, null);
        booking2.Status.Should().Be("CONFIRMED");
    }

    [Fact]
    public async Task BookVehicle_Unavailable_Throws()
    {
        var vehicleId = await SeedVehicleAsync("IN_MAINTENANCE");
        var act = () => _service.BookVehicleAsync(vehicleId, 1, null, new DateTime(2026, 1, 1, 9, 0, 0), new DateTime(2026, 1, 1, 10, 0, 0), "Trip", null, null);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task BookVehicle_Overlap_Throws()
    {
        var vehicleId = await SeedVehicleAsync();
        await _service.BookVehicleAsync(vehicleId, 1, null, new DateTime(2026, 1, 1, 9, 0, 0), new DateTime(2026, 1, 1, 10, 0, 0), "A", null, null);

        var act = () => _service.BookVehicleAsync(vehicleId, 2, null, new DateTime(2026, 1, 1, 9, 30, 0), new DateTime(2026, 1, 1, 10, 30, 0), "B", null, null);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
