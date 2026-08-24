using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

/// <summary>
/// Phase 3C5 — flagship end-to-end recruitment workflow regression:
/// requisition → approval → opening → candidate → application → screening →
/// interview → evaluation → offer → accept → hire → employee + onboarding.
/// </summary>
public class RecruitmentWorkflowTests
{
    private readonly LaoHRDbContext _context;
    private readonly ApprovalService _approval;
    private readonly HireConversionService _hire;

    public RecruitmentWorkflowTests()
    {
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new LaoHRDbContext(options);
        var hierarchy = new OrganizationHierarchyService(_context);
        _approval = new ApprovalService(_context, hierarchy);
        _hire = new HireConversionService(_context);
    }

    [Fact]
    public async Task FullRecruitmentWorkflow_ProducesEmployeeAndOnboarding()
    {
        // Seed org + HR approver.
        _context.Departments.Add(new Department { DepartmentId = 1, DepartmentName = "Finance", DepartmentCode = "FIN" });
        _context.Positions.Add(new Position { PositionId = 1, Title = "Accountant", DepartmentId = 1 });
        _context.Employees.AddRange(
            new Employee { EmployeeId = 1, EmployeeCode = "EMP001", LaoName = "Hiring Manager", IsActive = true },
            new Employee { EmployeeId = 2, EmployeeCode = "EMP002", LaoName = "HR Approver", IsActive = true });
        _context.Users.Add(new AppUser { Username = "hrapprover", PasswordHash = "x", Role = "HR", EmployeeId = 2, IsActive = true });
        await _context.SaveChangesAsync();

        // 1. Requisition
        var req = new JobRequisition
        {
            RequisitionNumber = "REQ-0001",
            PositionId = 1,
            DepartmentId = 1,
            RequestedByEmployeeId = 1,
            HiringManagerEmployeeId = 1,
            Headcount = 1,
            Status = "DRAFT",
        };
        _context.JobRequisitions.Add(req);
        await _context.SaveChangesAsync();

        // 2. Approval (HR role step)
        var approval = await _approval.CreateRequestAsync("REQUISITION", req.RequisitionId, 1,
            new List<ApprovalStepDefinition> { new() { ResolverType = "ROLE", RoleName = "HR" } });
        var approved = await _approval.ApproveAsync(approval.ApprovalRequestId, 2, null);
        approved.Status.Should().Be("APPROVED");
        req.Status = "APPROVED";
        await _context.SaveChangesAsync();

        // 3. Opening
        var opening = new JobOpening { RequisitionId = req.RequisitionId, Title = "Accountant", Status = "OPEN" };
        _context.JobOpenings.Add(opening);
        await _context.SaveChangesAsync();

        // 4. Candidate
        var candidate = new Candidate { FirstName = "Som", LastName = "Phon", Email = "som@example.com", Status = "ACTIVE" };
        _context.Candidates.Add(candidate);
        await _context.SaveChangesAsync();

        // 5. Application
        var application = new Application { CandidateId = candidate.CandidateId, OpeningId = opening.OpeningId, CurrentStage = "APPLIED", Status = "ACTIVE" };
        _context.Applications.Add(application);
        await _context.SaveChangesAsync();

        // 6. Screening → Interview
        application.CurrentStage = "SCREENING";
        await _context.SaveChangesAsync();
        application.CurrentStage = "INTERVIEW";
        await _context.SaveChangesAsync();

        // 7. Interview + evaluation
        var interview = new Interview { ApplicationId = application.ApplicationId, InterviewType = "HR", Status = "COMPLETED" };
        _context.Interviews.Add(interview);
        await _context.SaveChangesAsync();
        _context.InterviewEvaluations.Add(new InterviewEvaluation
        {
            InterviewId = interview.InterviewId,
            EvaluatorEmployeeId = 2,
            CommunicationScore = 4,
            ExperienceScore = 4,
            RoleFitScore = 5,
            Recommendation = "Recommend",
        });
        await _context.SaveChangesAsync();

        // 8. Offer → accept
        var offer = new Offer { ApplicationId = application.ApplicationId, PositionId = 1, Salary = 5000000, Currency = "LAK", Status = "SENT" };
        _context.Offers.Add(offer);
        await _context.SaveChangesAsync();
        offer.Status = "ACCEPTED";
        offer.AcceptedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // 9. Hire conversion
        var result = await _hire.ConvertToEmployeeAsync(application.ApplicationId);

        result.AlreadyHired.Should().BeFalse();
        var employee = await _context.Employees.FindAsync(result.EmployeeId);
        employee.Should().NotBeNull();
        employee!.Email.Should().Be("som@example.com");
        employee.PositionId.Should().Be(1);
        employee.DepartmentId.Should().Be(1);

        var onboarding = await _context.OnboardingProcesses.FindAsync(result.OnboardingProcessId);
        onboarding.Should().NotBeNull();
        onboarding!.EmployeeId.Should().Be(result.EmployeeId);

        // Application + candidate marked hired.
        var appAfter = await _context.Applications.FindAsync(application.ApplicationId);
        appAfter!.Status.Should().Be("HIRED");
        var candAfter = await _context.Candidates.FindAsync(candidate.CandidateId);
        candAfter!.Status.Should().Be("HIRED");
    }
}
