using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

public class FacilityDto
{
    public int FacilityId { get; set; }
    public string FacilityCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameLao { get; set; }
    public int? WorkLocationId { get; set; }
    public string FacilityType { get; set; } = string.Empty;
    public int? ManagerEmployeeId { get; set; }
    public string? Address { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class RoomDto
{
    public int RoomId { get; set; }
    public int FacilityId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public int? Capacity { get; set; }
    public string? Floor { get; set; }
    public bool Bookable { get; set; }
    public bool IsActive { get; set; }
}

public class RoomBookingDto
{
    public int RoomBookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public int RoomId { get; set; }
    public string? RoomName { get; set; }
    public int BookedByEmployeeId { get; set; }
    public string? BookedByName { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Purpose { get; set; }
    public int? ParticipantCount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateFacilityRequest
{
    public string FacilityCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameLao { get; set; }
    public int? WorkLocationId { get; set; }
    public string FacilityType { get; set; } = "OFFICE";
    public int? ManagerEmployeeId { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
}

public class CreateRoomRequest
{
    public int FacilityId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string RoomType { get; set; } = "MEETING_ROOM";
    public int? Capacity { get; set; }
    public string? Floor { get; set; }
    public bool Bookable { get; set; } = true;
}

public class CreateRoomBookingRequest
{
    public int RoomId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Purpose { get; set; }
    public int? ParticipantCount { get; set; }
}

[Authorize]
[ApiController]
[Route("api/facilities")]
public class FacilitiesController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly ICorporateOperationsAccessService _access;
    private readonly ICurrentEmployeeService _currentEmployee;
    private readonly IBookingService _booking;

    public FacilitiesController(
        LaoHRDbContext context,
        ICorporateOperationsAccessService access,
        ICurrentEmployeeService currentEmployee,
        IBookingService booking)
    {
        _context = context;
        _access = access;
        _currentEmployee = currentEmployee;
        _booking = booking;
    }

    // ---- Facilities ----

    [HttpGet]
    public async Task<ActionResult<List<FacilityDto>>> GetFacilities()
    {
        if (!_access.CanViewFacilities())
            return Forbid();

        return await _context.Facilities.AsNoTracking()
            .OrderBy(f => f.Name)
            .Select(f => new FacilityDto
            {
                FacilityId = f.FacilityId,
                FacilityCode = f.FacilityCode,
                Name = f.Name,
                NameLao = f.NameLao,
                WorkLocationId = f.WorkLocationId,
                FacilityType = f.FacilityType,
                ManagerEmployeeId = f.ManagerEmployeeId,
                Address = f.Address,
                Status = f.Status,
                Notes = f.Notes,
            })
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Facility>> CreateFacility([FromBody] CreateFacilityRequest request)
    {
        if (!_access.CanManageFacilities())
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.FacilityCode) || string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("FacilityCode and Name are required.");

        var facility = new Facility
        {
            FacilityCode = request.FacilityCode,
            Name = request.Name,
            NameLao = request.NameLao,
            WorkLocationId = request.WorkLocationId,
            FacilityType = request.FacilityType,
            ManagerEmployeeId = request.ManagerEmployeeId,
            Address = request.Address,
            Notes = request.Notes,
            Status = "ACTIVE",
        };
        _context.Facilities.Add(facility);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetFacilities), new { }, facility);
    }

    // ---- Rooms ----

    [HttpGet("rooms")]
    public async Task<ActionResult<List<RoomDto>>> GetRooms([FromQuery] int? facilityId = null)
    {
        if (!_access.CanViewFacilities())
            return Forbid();

        var query = _context.Rooms.AsNoTracking().AsQueryable();
        if (facilityId.HasValue)
            query = query.Where(r => r.FacilityId == facilityId.Value);

        return await query
            .OrderBy(r => r.Code)
            .Select(r => new RoomDto
            {
                RoomId = r.RoomId,
                FacilityId = r.FacilityId,
                Code = r.Code,
                Name = r.Name,
                RoomType = r.RoomType,
                Capacity = r.Capacity,
                Floor = r.Floor,
                Bookable = r.Bookable,
                IsActive = r.IsActive,
            })
            .ToListAsync();
    }

    [HttpPost("rooms")]
    public async Task<ActionResult<Room>> CreateRoom([FromBody] CreateRoomRequest request)
    {
        if (!_access.CanManageFacilities())
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Code and Name are required.");

        var room = new Room
        {
            FacilityId = request.FacilityId,
            Code = request.Code,
            Name = request.Name,
            RoomType = request.RoomType,
            Capacity = request.Capacity,
            Floor = request.Floor,
            Bookable = request.Bookable,
            IsActive = true,
        };
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetRooms), new { }, room);
    }

    // ---- Room bookings ----

    [HttpGet("bookings")]
    public async Task<ActionResult<List<RoomBookingDto>>> GetBookings(
        [FromQuery] int? roomId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        if (!_access.CanViewFacilities())
            return Forbid();

        var query = _context.RoomBookings.AsNoTracking().AsQueryable();
        if (roomId.HasValue)
            query = query.Where(b => b.RoomId == roomId.Value);
        if (from.HasValue)
            query = query.Where(b => b.EndAt >= from.Value);
        if (to.HasValue)
            query = query.Where(b => b.StartAt <= to.Value);

        return await query
            .OrderBy(b => b.StartAt)
            .Select(b => new RoomBookingDto
            {
                RoomBookingId = b.RoomBookingId,
                BookingNumber = b.BookingNumber,
                RoomId = b.RoomId,
                RoomName = b.Room != null ? b.Room.Name : null,
                BookedByEmployeeId = b.BookedByEmployeeId,
                BookedByName = b.BookedBy != null ? (b.BookedBy.EnglishName ?? b.BookedBy.LaoName) : null,
                StartAt = b.StartAt,
                EndAt = b.EndAt,
                Title = b.Title,
                Purpose = b.Purpose,
                ParticipantCount = b.ParticipantCount,
                Status = b.Status,
            })
            .ToListAsync();
    }

    [HttpPost("bookings")]
    public async Task<ActionResult<RoomBooking>> BookRoom([FromBody] CreateRoomBookingRequest request)
    {
        if (!_access.CanBookRooms())
            return Forbid();

        var empId = _currentEmployee.GetCurrentEmployeeId();
        if (empId == null) return Unauthorized("No linked employee profile.");

        try
        {
            var booking = await _booking.BookRoomAsync(
                request.RoomId, empId.Value, request.StartAt, request.EndAt,
                request.Title, request.Purpose, request.ParticipantCount);
            return CreatedAtAction(nameof(GetBookings), new { }, booking);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("bookings/{id:int}/cancel")]
    public async Task<IActionResult> CancelBooking(int id)
    {
        var empId = _currentEmployee.GetCurrentEmployeeId();
        if (empId == null) return Unauthorized("No linked employee profile.");

        var booking = await _context.RoomBookings.FirstOrDefaultAsync(b => b.RoomBookingId == id);
        if (booking == null) return NotFound();

        // Only the booker or an admin/HR can cancel.
        if (booking.BookedByEmployeeId != empId.Value && !_access.CanManageFacilities())
            return Forbid();

        try
        {
            await _booking.CancelRoomBookingAsync(id, empId.Value);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
}
