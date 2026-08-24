using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 3C6 — performance/talent authorization. Contextual access: self,
/// manager (direct reports), HR/Admin. Sensitive fields (manager-private notes,
/// talent/potential) are HR-restricted.
/// </summary>
public interface IPerformanceAccessService
{
    bool IsPrivileged();
    int? GetCurrentEmployeeId();

    Task<bool> CanViewGoalAsync(int goalId, CancellationToken ct = default);
    Task<bool> CanManageGoalAsync(int goalId, CancellationToken ct = default);
    Task<bool> CanViewReviewAsync(int reviewId, CancellationToken ct = default);
    Task<bool> CanSubmitSelfReviewAsync(int reviewId, CancellationToken ct = default);
    Task<bool> CanSubmitManagerReviewAsync(int reviewId, CancellationToken ct = default);
    Task<bool> CanViewFeedbackAsync(int feedbackId, CancellationToken ct = default);
    Task<bool> CanViewOneOnOneAsync(int oneOnOneId, CancellationToken ct = default);
    Task<bool> CanViewDevelopmentPlanAsync(int planId, CancellationToken ct = default);
    Task<bool> CanManageTrainingAsync(CancellationToken ct = default);
    Task<bool> CanViewTalentReviewAsync(CancellationToken ct = default);
    Task<bool> IsManagerOfAsync(int employeeId, CancellationToken ct = default);
}

public sealed class PerformanceAccessService : IPerformanceAccessService
{
    private readonly LaoHRDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PerformanceAccessService(LaoHRDbContext context, IHttpContextAccessor httpContextAccessor)
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

    public async Task<bool> IsManagerOfAsync(int employeeId, CancellationToken ct = default)
    {
        var empId = GetCurrentEmployeeId();
        if (empId == null) return false;
        return await _context.Employees
            .AnyAsync(e => e.EmployeeId == employeeId && e.ManagerId == empId.Value, ct);
    }

    public async Task<bool> CanViewGoalAsync(int goalId, CancellationToken ct = default)
    {
        if (IsPrivileged()) return true;
        var empId = GetCurrentEmployeeId();
        if (empId == null) return false;
        var goal = await _context.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.GoalId == goalId, ct);
        if (goal == null) return false;
        return goal.EmployeeId == empId.Value || goal.ManagerEmployeeId == empId.Value;
    }

    public async Task<bool> CanManageGoalAsync(int goalId, CancellationToken ct = default)
    {
        if (IsPrivileged()) return true;
        var empId = GetCurrentEmployeeId();
        if (empId == null) return false;
        var goal = await _context.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.GoalId == goalId, ct);
        if (goal == null) return false;
        return goal.EmployeeId == empId.Value || goal.ManagerEmployeeId == empId.Value;
    }

    public async Task<bool> CanViewReviewAsync(int reviewId, CancellationToken ct = default)
    {
        if (IsPrivileged()) return true;
        var empId = GetCurrentEmployeeId();
        if (empId == null) return false;
        var review = await _context.PerformanceReviews.AsNoTracking().FirstOrDefaultAsync(r => r.ReviewId == reviewId, ct);
        if (review == null) return false;
        return review.EmployeeId == empId.Value || review.ManagerEmployeeId == empId.Value;
    }

    public async Task<bool> CanSubmitSelfReviewAsync(int reviewId, CancellationToken ct = default)
    {
        var empId = GetCurrentEmployeeId();
        if (empId == null) return false;
        var review = await _context.PerformanceReviews.AsNoTracking().FirstOrDefaultAsync(r => r.ReviewId == reviewId, ct);
        return review != null && review.EmployeeId == empId.Value;
    }

    public async Task<bool> CanSubmitManagerReviewAsync(int reviewId, CancellationToken ct = default)
    {
        if (IsPrivileged()) return true;
        var empId = GetCurrentEmployeeId();
        if (empId == null) return false;
        var review = await _context.PerformanceReviews.AsNoTracking().FirstOrDefaultAsync(r => r.ReviewId == reviewId, ct);
        return review != null && review.ManagerEmployeeId == empId.Value;
    }

    public async Task<bool> CanViewFeedbackAsync(int feedbackId, CancellationToken ct = default)
    {
        if (IsPrivileged()) return true;
        var empId = GetCurrentEmployeeId();
        if (empId == null) return false;
        var feedback = await _context.Feedbacks.AsNoTracking().FirstOrDefaultAsync(f => f.FeedbackId == feedbackId, ct);
        if (feedback == null) return false;
        return feedback.ToEmployeeId == empId.Value || feedback.FromEmployeeId == empId.Value;
    }

    public async Task<bool> CanViewOneOnOneAsync(int oneOnOneId, CancellationToken ct = default)
    {
        if (IsPrivileged()) return true;
        var empId = GetCurrentEmployeeId();
        if (empId == null) return false;
        var o = await _context.OneOnOnes.AsNoTracking().FirstOrDefaultAsync(x => x.OneOnOneId == oneOnOneId, ct);
        if (o == null) return false;
        return o.ManagerEmployeeId == empId.Value || o.EmployeeId == empId.Value;
    }

    public async Task<bool> CanViewDevelopmentPlanAsync(int planId, CancellationToken ct = default)
    {
        if (IsPrivileged()) return true;
        var empId = GetCurrentEmployeeId();
        if (empId == null) return false;
        var plan = await _context.DevelopmentPlans.AsNoTracking().FirstOrDefaultAsync(p => p.DevelopmentPlanId == planId, ct);
        if (plan == null) return false;
        return plan.EmployeeId == empId.Value || plan.ManagerEmployeeId == empId.Value;
    }

    public async Task<bool> CanManageTrainingAsync(CancellationToken ct = default)
    {
        return IsPrivileged();
    }

    public async Task<bool> CanViewTalentReviewAsync(CancellationToken ct = default)
    {
        // Talent/potential data is HR-restricted.
        return IsPrivileged();
    }
}
