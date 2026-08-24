using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

/// <summary>
/// Phase 3C5 — hire conversion service tests: idempotency, employee mapping,
/// duplicate prevention.
/// </summary>
public class HireConversionServiceTests
{
    private readonly LaoHRDbContext _context;
    private readonly HireConversionService _service;

    public HireConversionServiceTests()
    {
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new LaoHRDbContext(options);
        _service = new HireConversionService(_context);
    }

    private async Task<int> SeedAcceptedApplicationAsync()
    {
        _context.Positions.Add(new Position { PositionId = 1, Title = "Accountant" });
        _context.Departments.Add(new Department { DepartmentId = 1, DepartmentName = "Finance", DepartmentCode = "FIN" });
        _context.Employees.Add(new Employee { EmployeeId = 1, EmployeeCode = "EMP001", LaoName = "Hiring Manager", IsActive = true });

        _context.JobRequisitions.Add(new JobRequisition
        {
            RequisitionId = 1,
            RequisitionNumber = "REQ-0001",
            PositionId = 1,
            DepartmentId = 1,
            RequestedByEmployeeId = 1,
            HiringManagerEmployeeId = 1,
            Status = "APPROVED",
        });
        _context.JobOpenings.Add(new JobOpening { OpeningId = 1, RequisitionId = 1, Title = "Accountant", Status = "OPEN" });
        _context.Candidates.Add(new Candidate { CandidateId = 1, FirstName = "Som", LastName = "Phon", Email = "som@example.com", Status = "ACTIVE" });
        _context.Applications.Add(new Application { ApplicationId = 1, CandidateId = 1, OpeningId = 1, CurrentStage = "OFFER", Status = "ACTIVE" });
        _context.Offers.Add(new Offer { OfferId = 1, ApplicationId = 1, PositionId = 1, Salary = 5000000, Currency = "LAK", Status = "ACCEPTED", AcceptedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();
        return 1;
    }

    [Fact]
    public async Task ConvertToEmployee_CreatesEmployeeAndOnboarding()
    {
        await SeedAcceptedApplicationAsync();

        var result = await _service.ConvertToEmployeeAsync(1);

        result.AlreadyHired.Should().BeFalse();
        result.EmployeeId.Should().BeGreaterThan(0);

        var employee = await _context.Employees.FindAsync(result.EmployeeId);
        employee.Should().NotBeNull();
        employee!.Email.Should().Be("som@example.com");
        employee.PositionId.Should().Be(1);
        employee.DepartmentId.Should().Be(1);

        var onboarding = await _context.OnboardingProcesses.FindAsync(result.OnboardingProcessId);
        onboarding.Should().NotBeNull();
        onboarding!.EmployeeId.Should().Be(result.EmployeeId);
    }

    [Fact]
    public async Task ConvertToEmployee_IsIdempotent()
    {
        await SeedAcceptedApplicationAsync();

        var first = await _service.ConvertToEmployeeAsync(1);
        var second = await _service.ConvertToEmployeeAsync(1);

        second.AlreadyHired.Should().BeTrue();
        second.EmployeeId.Should().Be(first.EmployeeId);

        // Only ONE employee should exist.
        var employeeCount = await _context.Employees.CountAsync(e => e.Email == "som@example.com");
        employeeCount.Should().Be(1);
    }

    [Fact]
    public async Task ConvertToEmployee_NoAcceptedOffer_Throws()
    {
        _context.Positions.Add(new Position { PositionId = 1, Title = "Accountant" });
        _context.Employees.Add(new Employee { EmployeeId = 1, EmployeeCode = "EMP001", LaoName = "HM", IsActive = true });
        _context.JobRequisitions.Add(new JobRequisition { RequisitionId = 1, RequisitionNumber = "REQ-0001", PositionId = 1, RequestedByEmployeeId = 1, Status = "APPROVED" });
        _context.JobOpenings.Add(new JobOpening { OpeningId = 1, RequisitionId = 1, Title = "Accountant", Status = "OPEN" });
        _context.Candidates.Add(new Candidate { CandidateId = 1, FirstName = "Som", LastName = "Phon", Status = "ACTIVE" });
        _context.Applications.Add(new Application { ApplicationId = 1, CandidateId = 1, OpeningId = 1, CurrentStage = "OFFER", Status = "ACTIVE" });
        await _context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ConvertToEmployeeAsync(1));
    }

    [Fact]
    public async Task ConvertToEmployee_DuplicateEmail_Throws()
    {
        await SeedAcceptedApplicationAsync();
        // Pre-existing employee with the same email.
        _context.Employees.Add(new Employee { EmployeeCode = "EMP999", LaoName = "Existing", Email = "som@example.com", IsActive = true });
        await _context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ConvertToEmployeeAsync(1));
    }
}
