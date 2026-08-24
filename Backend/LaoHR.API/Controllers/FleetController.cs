using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

public class VehicleDto
{
    public int VehicleId { get; set; }
    public string VehicleCode { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public int? AssetId { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public string? VIN { get; set; }
    public string? VehicleType { get; set; }
    public string? FuelType { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? WorkLocationId { get; set; }
    public int CurrentOdometer { get; set; }
}

public class VehicleBookingDto
{
    public int VehicleBookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public int VehicleId { get; set; }
    public string? RegistrationNumber { get; set; }
    public int RequesterEmployeeId { get; set; }
    public string? RequesterName { get; set; }
    public int? DriverEmployeeId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string? Destination { get; set; }
    public int? ProjectId { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class VehicleTripDto
{
    public int VehicleTripId { get; set; }
    public int? VehicleBookingId { get; set; }
    public int VehicleId { get; set; }
    public int DriverEmployeeId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int StartOdometer { get; set; }
    public int? EndOdometer { get; set; }
    public string? Destination { get; set; }
    public string? Purpose { get; set; }
}

public class CreateVehicleRequest
{
    public string VehicleCode { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public int? AssetId { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public string? VIN { get; set; }
    public string? VehicleType { get; set; }
    public string? FuelType { get; set; }
    public int? WorkLocationId { get; set; }
    public int CurrentOdometer { get; set; }
}

public class CreateVehicleBookingRequest
{
    public int VehicleId { get; set; }
    public int? DriverEmployeeId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string? Destination { get; set; }
    public int? ProjectId { get; set; }
}

public class StartTripRequest
{
    public int VehicleId { get; set; }
    public int StartOdometer { get; set; }
    public string? Destination { get; set; }
    public string? Purpose { get; set; }
    public int? VehicleBookingId { get; set; }
}

public class CompleteTripRequest
{
    public int EndOdometer { get; set; }
}

public class CorrectOdometerRequest
{
    public int NewOdometer { get; set; }
    public string Reason { get; set; } = string.Empty;
}

[Authorize]
[ApiController]
[Route("api/fleet")]
public class FleetController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly ICorporateOperationsAccessService _access;
    private readonly ICurrentEmployeeService _currentEmployee;
    private readonly IBookingService _booking;
    private readonly IFleetService _fleet;

    public FleetController(
        LaoHRDbContext context,
        ICorporateOperationsAccessService access,
        ICurrentEmployeeService currentEmployee,
        IBookingService booking,
        IFleetService fleet)
    {
        _context = context;
        _access = access;
        _currentEmployee = currentEmployee;
        _booking = booking;
        _fleet = fleet;
    }

    [HttpGet("vehicles")]
    public async Task<ActionResult<List<VehicleDto>>> GetVehicles([FromQuery] string? status = null)
    {
        if (!_access.CanViewFleet())
            return Forbid();

        var query = _context.Vehicles.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(status))
            query = query.Where(v => v.Status == status);

        return await query
            .OrderBy(v => v.VehicleCode)
            .Select(v => new VehicleDto
            {
                VehicleId = v.VehicleId,
                VehicleCode = v.VehicleCode,
                RegistrationNumber = v.RegistrationNumber,
                AssetId = v.AssetId,
                Make = v.Make,
                Model = v.Model,
                Year = v.Year,
                VIN = v.VIN,
                VehicleType = v.VehicleType,
                FuelType = v.FuelType,
                Status = v.Status,
                WorkLocationId = v.WorkLocationId,
                CurrentOdometer = v.CurrentOdometer,
            })
            .ToListAsync();
    }

    [HttpPost("vehicles")]
    public async Task<ActionResult<Vehicle>> CreateVehicle([FromBody] CreateVehicleRequest request)
    {
        if (!_access.CanManageFleet())
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.VehicleCode) || string.IsNullOrWhiteSpace(request.RegistrationNumber))
            return BadRequest("VehicleCode and RegistrationNumber are required.");

        var vehicle = new Vehicle
        {
            VehicleCode = request.VehicleCode,
            RegistrationNumber = request.RegistrationNumber,
            AssetId = request.AssetId,
            Make = request.Make,
            Model = request.Model,
            Year = request.Year,
            VIN = request.VIN,
            VehicleType = request.VehicleType,
            FuelType = request.FuelType,
            WorkLocationId = request.WorkLocationId,
            CurrentOdometer = request.CurrentOdometer,
            Status = "AVAILABLE",
        };
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetVehicles), new { }, vehicle);
    }

    [HttpGet("bookings")]
    public async Task<ActionResult<List<VehicleBookingDto>>> GetBookings(
        [FromQuery] int? vehicleId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        if (!_access.CanViewFleet())
            return Forbid();

        var query = _context.VehicleBookings.AsNoTracking().AsQueryable();
        if (vehicleId.HasValue)
            query = query.Where(b => b.VehicleId == vehicleId.Value);
        if (from.HasValue)
            query = query.Where(b => b.EndAt >= from.Value);
        if (to.HasValue)
            query = query.Where(b => b.StartAt <= to.Value);

        return await query
            .OrderBy(b => b.StartAt)
            .Select(b => new VehicleBookingDto
            {
                VehicleBookingId = b.VehicleBookingId,
                BookingNumber = b.BookingNumber,
                VehicleId = b.VehicleId,
                RegistrationNumber = b.Vehicle != null ? b.Vehicle.RegistrationNumber : null,
                RequesterEmployeeId = b.RequesterEmployeeId,
                RequesterName = b.Requester != null ? (b.Requester.EnglishName ?? b.Requester.LaoName) : null,
                DriverEmployeeId = b.DriverEmployeeId,
                StartAt = b.StartAt,
                EndAt = b.EndAt,
                Purpose = b.Purpose,
                Destination = b.Destination,
                ProjectId = b.ProjectId,
                Status = b.Status,
            })
            .ToListAsync();
    }

    [HttpPost("bookings")]
    public async Task<ActionResult<VehicleBooking>> BookVehicle([FromBody] CreateVehicleBookingRequest request)
    {
        if (!_access.CanBookVehicles())
            return Forbid();

        var empId = _currentEmployee.GetCurrentEmployeeId();
        if (empId == null) return Unauthorized("No linked employee profile.");

        try
        {
            var booking = await _booking.BookVehicleAsync(
                request.VehicleId, empId.Value, request.DriverEmployeeId,
                request.StartAt, request.EndAt, request.Purpose, request.Destination, request.ProjectId);
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

        var booking = await _context.VehicleBookings.FirstOrDefaultAsync(b => b.VehicleBookingId == id);
        if (booking == null) return NotFound();

        if (booking.RequesterEmployeeId != empId.Value && !_access.CanManageFleet())
            return Forbid();

        try
        {
            await _booking.CancelVehicleBookingAsync(id, empId.Value);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpGet("trips")]
    public async Task<ActionResult<List<VehicleTripDto>>> GetTrips([FromQuery] int? vehicleId = null)
    {
        if (!_access.CanViewFleet())
            return Forbid();

        var query = _context.VehicleTrips.AsNoTracking().AsQueryable();
        if (vehicleId.HasValue)
            query = query.Where(t => t.VehicleId == vehicleId.Value);

        return await query
            .OrderByDescending(t => t.StartedAt)
            .Select(t => new VehicleTripDto
            {
                VehicleTripId = t.VehicleTripId,
                VehicleBookingId = t.VehicleBookingId,
                VehicleId = t.VehicleId,
                DriverEmployeeId = t.DriverEmployeeId,
                StartedAt = t.StartedAt,
                CompletedAt = t.CompletedAt,
                StartOdometer = t.StartOdometer,
                EndOdometer = t.EndOdometer,
                Destination = t.Destination,
                Purpose = t.Purpose,
            })
            .ToListAsync();
    }

    [HttpPost("trips/start")]
    public async Task<ActionResult<VehicleTrip>> StartTrip([FromBody] StartTripRequest request)
    {
        if (!_access.CanManageFleet())
            return Forbid();

        var empId = _currentEmployee.GetCurrentEmployeeId();
        if (empId == null) return Unauthorized("No linked employee profile.");

        try
        {
            var trip = await _fleet.StartTripAsync(
                request.VehicleId, empId.Value, request.StartOdometer,
                request.Destination, request.Purpose, request.VehicleBookingId);
            return Ok(trip);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("trips/{id:int}/complete")]
    public async Task<ActionResult<VehicleTrip>> CompleteTrip(int id, [FromBody] CompleteTripRequest request)
    {
        if (!_access.CanManageFleet())
            return Forbid();

        try
        {
            var trip = await _fleet.CompleteTripAsync(id, request.EndOdometer);
            return Ok(trip);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("vehicles/{id:int}/correct-odometer")]
    public async Task<ActionResult<Vehicle>> CorrectOdometer(int id, [FromBody] CorrectOdometerRequest request)
    {
        if (!_access.CanManageFleet())
            return Forbid();

        var empId = _currentEmployee.GetCurrentEmployeeId();
        if (empId == null) return Unauthorized("No linked employee profile.");

        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest("Reason is required for odometer correction.");

        try
        {
            var vehicle = await _fleet.CorrectOdometerAsync(id, request.NewOdometer, empId.Value, request.Reason);
            return Ok(vehicle);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
}
