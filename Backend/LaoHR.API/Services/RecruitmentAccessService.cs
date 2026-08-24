using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 3C5 — recruitment authorization. Contextual access: global HR/Admin
/// permission is distinct from hiring-manager scope and interview-panel scope.
/// </summary>
public interface IRecruitmentAccessService
{
    bool IsPrivileged();
    int? GetCurrentEmployeeId();

    Task<bool> CanViewRequisitionAsync(int requisitionId, CancellationToken ct = default);
    Task<bool> CanEditRequisitionAsync(int requisitionId, CancellationToken ct = default);
    Task<bool> CanViewCandidateAsync(int candidateId, CancellationToken ct = default);
    Task<bool> CanViewApplicationAsync(int applicationId, CancellationToken ct = default);
    Task<bool> CanMoveApplicationAsync(int applicationId, CancellationToken ct = default);
    Task<bool> CanViewInterviewAsync(int interviewId, CancellationToken ct = default);
    Task<bool> CanSubmitEvaluationAsync(int interviewId, CancellationToken ct = default);
    Task<bool> CanViewOfferAsync(int offerId, CancellationToken ct = default);
    Task<bool> CanCreateOfferAsync(int applicationId, CancellationToken ct = default);
    Task<bool> CanHireAsync(int applicationId, CancellationToken ct = default);
    Task<bool> CanManageOnboardingAsync(int onboardingProcessId, CancellationToken ct = default);
}

public sealed class RecruitmentAccessService : IRecruitmentAccessService
{
    private readonly LaoHRDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RecruitmentAccessService(LaoHRDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsPrivileged()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user != null && (user.IsInRole("Admin") || user.IsInRole("HR"));
    }

    public int? GetCurrentEmployeeId()
    {
        var empIdStr = _httpContextAccessor.HttpContext?.User.FindFirst("EmployeeId")?.Value;
        if (int.TryParse(empIdStr, out var empId)) return empId;
        return null;
    }

    private async Task<bool> IsHiringManagerAsync(int requisitionId, int employeeId, CancellationToken ct)
    {
        return await _context.JobRequisitions
            .AnyAsync(r => r.RequisitionId == requisitionId && r.HiringManagerEmployeeId == employeeId, ct);
    }

    private async Task<int?> GetRequisitionIdForApplicationAsync(int applicationId, CancellationToken ct)
    {
        return await _context.Applications
            .Where(a => a.ApplicationId == applicationId)
            .Select(a => a.Opening!.RequisitionId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> CanViewRequisitionAsync(int requisitionId, CancellationToken ct = default)
    {
        if (IsPrivileged()) return true;
        var empId = GetCurrentEmployeeId();
        if (empId == null) return false;
        return await _context.JobRequisitions
            .AnyAsync(r => r.RequisitionId == requisitionId
                && (r.RequestedByEmployeeId == empId.Value || r.HiringManagerEmployeeId == empId.Value), ct);
    }

    public async Task<bool> CanEditRequisitionAsync(int requisitionId, CancellationToken ct = default)
    {
        if (IsPrivileged()) return true;
        var empId = GetCurrentEmployeeId();
        if (empId == null) return false;
        return await _context.JobRequisitions
            .AnyAsync(r => r.RequisitionId == requisitionId && r.RequestedByEmployeeId == empId.Value, ct);
    }

    public async Task<bool> CanViewCandidateAsync(int candidateId, CancellationToken ct = default)
    {
        // Candidate PII is restricted to HR/Admin and hiring managers/recruiters
        // with an application for that candidate.
        if (IsPrivileged()) return true;
        var empId = GetCurrentEmployeeId();
        if (empId == null) return false;

        return await _context.Applications
            .AnyAsync(a => a.CandidateId == candidateId
                && (a.Opening!.Requisition!.HiringManagerEmployeeId == empId.Value
                    || a.Opening!.Requisition!.RequestedByEmployeeId == empId.Value), ct);
    }

    public async Task<bool> CanViewApplicationAsync(int applicationId, CancellationToken ct = default)
    {
        if (IsPrivileged()) return true;
        var empId = GetCurrentEmployeeId();
        if (empId == null) return false;

        var reqId = await GetRequisitionIdForApplicationAsync(applicationId, ct);
        if (reqId == null) return false;
        return await IsHiringManagerAsync(reqId.Value, empId.Value, ct)
            || await _context.JobRequisitions.AnyAsync(r => r.RequisitionId == reqId.Value && r.RequestedByEmployeeId == empId.Value, ct);
    }

    public async Task<bool> CanMoveApplicationAsync(int applicationId, CancellationToken ct = default)
    {
        return IsPrivileged() || await CanViewApplicationAsync(applicationId, ct);
    }

    public async Task<bool> CanViewInterviewAsync(int interviewId, CancellationToken ct = default)
    {
        if (IsPrivileged()) return true;
        var empId = GetCurrentEmployeeId();
        if (empId == null) return false;

        var interview = await _context.Interviews
            .Where(i => i.InterviewId == interviewId)
            .Select(i => new { i.ApplicationId, i.OrganizerEmployeeId })
            .FirstOrDefaultAsync(ct);
        if (interview == null) return false;

        if (interview.OrganizerEmployeeId == empId.Value) return true;
        if (await _context.InterviewParticipants.AnyAsync(p => p.InterviewId == interviewId && p.EmployeeId == empId.Value, ct)) return true;
        return await CanViewApplicationAsync(interview.ApplicationId, ct);
    }

    public async Task<bool> CanSubmitEvaluationAsync(int interviewId, CancellationToken ct = default)
    {
        if (IsPrivileged()) return true;
        var empId = GetCurrentEmployeeId();
        if (empId == null) return false;
        return await _context.InterviewParticipants
            .AnyAsync(p => p.InterviewId == interviewId && p.EmployeeId == empId.Value, ct);
    }

    public async Task<bool> CanViewOfferAsync(int offerId, CancellationToken ct = default)
    {
        // Offer (compensation) is more sensitive than candidate profile.
        if (IsPrivileged()) return true;
        var empId = GetCurrentEmployeeId();
        if (empId == null) return false;

        var appId = await _context.Offers
            .Where(o => o.OfferId == offerId)
            .Select(o => o.ApplicationId)
            .FirstOrDefaultAsync(ct);
        if (appId == 0) return false;

        var reqId = await GetRequisitionIdForApplicationAsync(appId, ct);
        if (reqId == null) return false;
        return await IsHiringManagerAsync(reqId.Value, empId.Value, ct);
    }

    public async Task<bool> CanCreateOfferAsync(int applicationId, CancellationToken ct = default)
    {
        return IsPrivileged() || await CanViewApplicationAsync(applicationId, ct);
    }

    public async Task<bool> CanHireAsync(int applicationId, CancellationToken ct = default)
    {
        // Only HR/Admin may perform the hire conversion (creates Employee + AppUser).
        return IsPrivileged();
    }

    public async Task<bool> CanManageOnboardingAsync(int onboardingProcessId, CancellationToken ct = default)
    {
        if (IsPrivileged()) return true;
        var empId = GetCurrentEmployeeId();
        if (empId == null) return false;
        return await _context.OnboardingProcesses
            .AnyAsync(p => p.OnboardingProcessId == onboardingProcessId && p.OwnerEmployeeId == empId.Value, ct);
    }
}
