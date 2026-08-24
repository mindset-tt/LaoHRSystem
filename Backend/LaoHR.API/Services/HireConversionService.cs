using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 3C5 — hire conversion. Converts an accepted offer's candidate/application
/// into an Employee (canonical worker identity) + onboarding process. Idempotent:
/// calling twice does NOT create two Employees.
/// </summary>
public interface IHireConversionService
{
    Task<HireResult> ConvertToEmployeeAsync(int applicationId, CancellationToken ct = default);
}

public sealed class HireResult
{
    public int EmployeeId { get; set; }
    public int OnboardingProcessId { get; set; }
    public bool AlreadyHired { get; set; }
}

public sealed class HireConversionService : IHireConversionService
{
    private readonly LaoHRDbContext _context;

    public HireConversionService(LaoHRDbContext context)
    {
        _context = context;
    }

    public async Task<HireResult> ConvertToEmployeeAsync(int applicationId, CancellationToken ct = default)
    {
        var application = await _context.Applications
            .Include(a => a.Candidate)
            .Include(a => a.Opening)
                .ThenInclude(o => o!.Requisition)
                    .ThenInclude(r => r!.Position)
            .FirstOrDefaultAsync(a => a.ApplicationId == applicationId, ct)
            ?? throw new InvalidOperationException("Application not found.");

        // Idempotency: if already hired, return the existing employee.
        if (application.Status == "HIRED")
        {
            var existing = await _context.OnboardingProcesses
                .Where(p => p.ApplicationId == applicationId)
                .Select(p => new { p.EmployeeId, p.OnboardingProcessId })
                .FirstOrDefaultAsync(ct);
            if (existing != null)
                return new HireResult { EmployeeId = existing.EmployeeId, OnboardingProcessId = existing.OnboardingProcessId, AlreadyHired = true };
        }

        // Require an accepted offer.
        var offer = await _context.Offers
            .Where(o => o.ApplicationId == applicationId && o.Status == "ACCEPTED")
            .OrderByDescending(o => o.AcceptedAt)
            .FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException("No accepted offer for this application.");

        var candidate = application.Candidate!;
        var requisition = application.Opening!.Requisition!;

        // Duplicate prevention: reject if an employee already exists with the same email.
        if (!string.IsNullOrWhiteSpace(candidate.Email))
        {
            var dup = await _context.Employees
                .AnyAsync(e => e.Email == candidate.Email, ct);
            if (dup)
                throw new InvalidOperationException("An employee with this email already exists. HR confirmation required.");
        }

        // Generate employee code (reuse existing convention).
        var count = await _context.Employees.CountAsync(ct) + 1;
        var code = $"EMP{count:D4}";

        var employee = new Employee
        {
            EmployeeCode = code,
            LaoName = candidate.FirstNameLao != null && candidate.LastNameLao != null
                ? $"{candidate.FirstNameLao} {candidate.LastNameLao}"
                : $"{candidate.FirstName} {candidate.LastName}",
            EnglishName = $"{candidate.FirstName} {candidate.LastName}",
            Email = candidate.Email,
            Phone = candidate.Phone,
            DepartmentId = requisition.DepartmentId,
            PositionId = requisition.PositionId,
            WorkLocationId = requisition.WorkLocationId,
            ManagerId = requisition.HiringManagerEmployeeId,
            JobTitle = requisition.Position?.Title,
            HireDate = offer.ProposedStartDate ?? DateTime.UtcNow,
            BaseSalary = offer.Salary ?? 0,
            SalaryCurrency = offer.Currency ?? "LAK",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync(ct);

        // Mark application hired.
        application.Status = "HIRED";
        application.CurrentStage = "HIRED";
        application.UpdatedAt = DateTime.UtcNow;

        // Mark candidate hired.
        candidate.Status = "HIRED";
        candidate.UpdatedAt = DateTime.UtcNow;

        // Create onboarding process.
        var onboarding = new OnboardingProcess
        {
            EmployeeId = employee.EmployeeId,
            ApplicationId = applicationId,
            CandidateId = candidate.CandidateId,
            StartDate = offer.ProposedStartDate ?? DateTime.UtcNow,
            OwnerEmployeeId = requisition.HiringManagerEmployeeId,
            Status = "NOT_STARTED",
        };
        _context.OnboardingProcesses.Add(onboarding);
        await _context.SaveChangesAsync(ct);

        return new HireResult
        {
            EmployeeId = employee.EmployeeId,
            OnboardingProcessId = onboarding.OnboardingProcessId,
            AlreadyHired = false,
        };
    }
}
