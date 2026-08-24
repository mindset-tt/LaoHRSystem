using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 3C1 — lightweight approval engine. Sequential steps with server-side
/// approver resolution. Approver identity is never trusted from the client.
/// </summary>
public interface IApprovalService
{
    /// <summary>Creates an approval request with resolved sequential steps.</summary>
    Task<ApprovalRequest> CreateRequestAsync(
        string requestType, int entityId, int requesterEmployeeId,
        List<ApprovalStepDefinition> steps, CancellationToken ct = default);

    /// <summary>Approves the current step as the given actor (must be the resolved approver).</summary>
    Task<ApprovalRequest> ApproveAsync(int approvalRequestId, int actorEmployeeId, string? comment, CancellationToken ct = default);

    /// <summary>Rejects the current step as the given actor.</summary>
    Task<ApprovalRequest> RejectAsync(int approvalRequestId, int actorEmployeeId, string? comment, CancellationToken ct = default);

    /// <summary>Cancels a pending request (requester only).</summary>
    Task<ApprovalRequest> CancelAsync(int approvalRequestId, int requesterEmployeeId, CancellationToken ct = default);
}

public sealed class ApprovalStepDefinition
{
    public string ResolverType { get; set; } = "DIRECT_MANAGER";
    public string? RoleName { get; set; }
    public int? ApproverEmployeeId { get; set; }
}

public sealed class ApprovalService : IApprovalService
{
    private readonly LaoHRDbContext _context;
    private readonly IOrganizationHierarchyService _hierarchy;

    public ApprovalService(LaoHRDbContext context, IOrganizationHierarchyService hierarchy)
    {
        _context = context;
        _hierarchy = hierarchy;
    }

    public async Task<ApprovalRequest> CreateRequestAsync(
        string requestType, int entityId, int requesterEmployeeId,
        List<ApprovalStepDefinition> steps, CancellationToken ct = default)
    {
        var request = new ApprovalRequest
        {
            RequestType = requestType,
            EntityId = entityId,
            RequesterEmployeeId = requesterEmployeeId,
            Status = "PENDING",
            CurrentStepIndex = 0
        };

        var order = 0;
        foreach (var stepDef in steps)
        {
            var approverId = await ResolveApproverAsync(stepDef, requesterEmployeeId, ct);
            request.Steps.Add(new ApprovalStep
            {
                StepOrder = order++,
                ResolverType = stepDef.ResolverType,
                RoleName = stepDef.RoleName,
                ApproverEmployeeId = approverId,
                Status = "PENDING"
            });
        }

        _context.ApprovalRequests.Add(request);
        await _context.SaveChangesAsync(ct);
        return request;
    }

    public async Task<ApprovalRequest> ApproveAsync(int approvalRequestId, int actorEmployeeId, string? comment, CancellationToken ct = default)
    {
        var request = await _context.ApprovalRequests
            .Include(r => r.Steps)
            .FirstOrDefaultAsync(r => r.ApprovalRequestId == approvalRequestId, ct)
            ?? throw new InvalidOperationException("Approval request not found.");

        if (request.Status != "PENDING")
            throw new InvalidOperationException("Request is not pending.");

        var currentStep = request.Steps.OrderBy(s => s.StepOrder).ElementAt(request.CurrentStepIndex);
        if (currentStep.ApproverEmployeeId != actorEmployeeId)
            throw new UnauthorizedAccessException("You are not the approver for the current step.");

        currentStep.Status = "APPROVED";
        currentStep.ActedAt = DateTime.UtcNow;
        currentStep.Comment = comment;

        _context.ApprovalActions.Add(new ApprovalAction
        {
            ApprovalRequestId = request.ApprovalRequestId,
            ActorEmployeeId = actorEmployeeId,
            Action = "APPROVED",
            Comment = comment
        });

        // Advance to next step, or complete.
        if (request.CurrentStepIndex + 1 < request.Steps.Count)
        {
            request.CurrentStepIndex++;
        }
        else
        {
            request.Status = "APPROVED";
            request.CompletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);
        return request;
    }

    public async Task<ApprovalRequest> RejectAsync(int approvalRequestId, int actorEmployeeId, string? comment, CancellationToken ct = default)
    {
        var request = await _context.ApprovalRequests
            .Include(r => r.Steps)
            .FirstOrDefaultAsync(r => r.ApprovalRequestId == approvalRequestId, ct)
            ?? throw new InvalidOperationException("Approval request not found.");

        if (request.Status != "PENDING")
            throw new InvalidOperationException("Request is not pending.");

        var currentStep = request.Steps.OrderBy(s => s.StepOrder).ElementAt(request.CurrentStepIndex);
        if (currentStep.ApproverEmployeeId != actorEmployeeId)
            throw new UnauthorizedAccessException("You are not the approver for the current step.");

        currentStep.Status = "REJECTED";
        currentStep.ActedAt = DateTime.UtcNow;
        currentStep.Comment = comment;

        request.Status = "REJECTED";
        request.CompletedAt = DateTime.UtcNow;

        _context.ApprovalActions.Add(new ApprovalAction
        {
            ApprovalRequestId = request.ApprovalRequestId,
            ActorEmployeeId = actorEmployeeId,
            Action = "REJECTED",
            Comment = comment
        });

        await _context.SaveChangesAsync(ct);
        return request;
    }

    public async Task<ApprovalRequest> CancelAsync(int approvalRequestId, int requesterEmployeeId, CancellationToken ct = default)
    {
        var request = await _context.ApprovalRequests
            .FirstOrDefaultAsync(r => r.ApprovalRequestId == approvalRequestId, ct)
            ?? throw new InvalidOperationException("Approval request not found.");

        if (request.RequesterEmployeeId != requesterEmployeeId)
            throw new UnauthorizedAccessException("Only the requester can cancel this request.");

        if (request.Status != "PENDING")
            throw new InvalidOperationException("Only pending requests can be cancelled.");

        request.Status = "CANCELLED";
        request.CompletedAt = DateTime.UtcNow;

        _context.ApprovalActions.Add(new ApprovalAction
        {
            ApprovalRequestId = request.ApprovalRequestId,
            ActorEmployeeId = requesterEmployeeId,
            Action = "CANCELLED"
        });

        await _context.SaveChangesAsync(ct);
        return request;
    }

    private async Task<int?> ResolveApproverAsync(ApprovalStepDefinition step, int requesterEmployeeId, CancellationToken ct)
    {
        switch (step.ResolverType)
        {
            case "EMPLOYEE":
                return step.ApproverEmployeeId;

            case "DIRECT_MANAGER":
                var requester = await _context.Employees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.EmployeeId == requesterEmployeeId, ct);
                return requester?.ManagerId;

            case "DEPARTMENT_MANAGER":
                var emp = await _context.Employees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.EmployeeId == requesterEmployeeId, ct);
                if (emp?.DepartmentId == null) return null;
                var dept = await _context.Departments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.DepartmentId == emp.DepartmentId, ct);
                return dept?.ManagerEmployeeId;

            case "ROLE":
                // Resolve to the first active employee whose AppUser has the given role.
                var user = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.Role == step.RoleName && u.IsActive && u.EmployeeId != null)
                    .OrderBy(u => u.UserId)
                    .FirstOrDefaultAsync(ct);
                return user?.EmployeeId;

            default:
                return null;
        }
    }
}