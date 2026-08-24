using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaoHR.API.Controllers;

/// <summary>
/// Phase 3C6 — performance management + talent + learning endpoints.
/// </summary>
[Authorize]
[ApiController]
[Route("api/performance")]
public class PerformanceController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IPerformanceAccessService _access;

    public PerformanceController(LaoHRDbContext context, IPerformanceAccessService access)
    {
        _context = context;
        _access = access;
    }

    // ---- Goals ----

    [HttpGet("goals")]
    public async Task<ActionResult<List<Goal>>> GetGoals([FromQuery] int? employeeId = null)
    {
        var query = _context.Goals.AsNoTracking().AsQueryable();
        if (employeeId.HasValue)
        {
            if (!_access.IsPrivileged() && !await _access.IsManagerOfAsync(employeeId.Value) && _access.GetCurrentEmployeeId() != employeeId.Value)
                return Forbid();
            query = query.Where(g => g.EmployeeId == employeeId.Value);
        }
        else if (!_access.IsPrivileged())
        {
            var selfId = _access.GetCurrentEmployeeId();
            if (selfId == null) return Ok(new List<Goal>());
            query = query.Where(g => g.EmployeeId == selfId.Value);
        }
        return await query.OrderByDescending(g => g.CreatedAt).ToListAsync();
    }

    [HttpGet("goals/{id:int}")]
    public async Task<ActionResult<Goal>> GetGoal(int id)
    {
        if (!await _access.CanViewGoalAsync(id)) return Forbid();
        var g = await _context.Goals.AsNoTracking().FirstOrDefaultAsync(x => x.GoalId == id);
        if (g == null) return NotFound();
        return g;
    }

    [HttpPost("goals")]
    public async Task<ActionResult<Goal>> CreateGoal([FromBody] CreateGoalRequest request)
    {
        var creatorId = _access.GetCurrentEmployeeId();
        if (creatorId == null) return Unauthorized("No linked employee profile.");

        var goal = new Goal
        {
            EmployeeId = request.EmployeeId,
            ManagerEmployeeId = request.ManagerEmployeeId,
            ParentGoalId = request.ParentGoalId,
            Title = request.Title,
            Description = request.Description,
            GoalType = request.GoalType ?? "Individual",
            StartDate = request.StartDate,
            DueDate = request.DueDate,
            Status = "DRAFT",
            CreatedByEmployeeId = creatorId.Value,
        };
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetGoal), new { id = goal.GoalId }, goal);
    }

    [HttpPost("goals/{id:int}/checkins")]
    public async Task<ActionResult<GoalCheckIn>> AddCheckIn(int id, [FromBody] AddCheckInRequest request)
    {
        if (!await _access.CanManageGoalAsync(id)) return Forbid();
        var checkIn = new GoalCheckIn
        {
            GoalId = id,
            ProgressPercent = request.ProgressPercent,
            Status = request.Status,
            Comment = request.Comment,
            CreatedByEmployeeId = _access.GetCurrentEmployeeId(),
        };
        _context.GoalCheckIns.Add(checkIn);

        var goal = await _context.Goals.FirstOrDefaultAsync(g => g.GoalId == id);
        if (goal != null)
        {
            goal.ProgressPercent = request.ProgressPercent;
            if (request.Status != null) goal.Status = request.Status;
            goal.UpdatedAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync();
        return Ok(checkIn);
    }

    [HttpGet("goals/{id:int}/checkins")]
    public async Task<ActionResult<List<GoalCheckIn>>> GetCheckIns(int id)
    {
        if (!await _access.CanViewGoalAsync(id)) return Forbid();
        return await _context.GoalCheckIns
            .AsNoTracking()
            .Where(c => c.GoalId == id)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    // ---- Performance cycles ----

    [HttpGet("cycles")]
    public async Task<ActionResult<List<PerformanceCycle>>> GetCycles()
    {
        return await _context.PerformanceCycles.AsNoTracking().OrderByDescending(c => c.CreatedAt).ToListAsync();
    }

    [HttpPost("cycles")]
    public async Task<ActionResult<PerformanceCycle>> CreateCycle([FromBody] CreateCycleRequest request)
    {
        if (!_access.IsPrivileged()) return Forbid();
        var cycle = new PerformanceCycle
        {
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            ReviewDueDate = request.ReviewDueDate,
            Status = "DRAFT",
        };
        _context.PerformanceCycles.Add(cycle);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCycles), new { id = cycle.CycleId }, cycle);
    }

    // ---- Reviews ----

    [HttpGet("reviews")]
    public async Task<ActionResult<List<PerformanceReview>>> GetReviews([FromQuery] int? cycleId = null)
    {
        var query = _context.PerformanceReviews.AsNoTracking().AsQueryable();
        if (cycleId.HasValue) query = query.Where(r => r.CycleId == cycleId.Value);

        if (!_access.IsPrivileged())
        {
            var selfId = _access.GetCurrentEmployeeId();
            if (selfId == null) return Ok(new List<PerformanceReview>());
            query = query.Where(r => r.EmployeeId == selfId.Value || r.ManagerEmployeeId == selfId.Value);
        }
        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    [HttpGet("reviews/{id:int}")]
    public async Task<ActionResult<PerformanceReview>> GetReview(int id)
    {
        if (!await _access.CanViewReviewAsync(id)) return Forbid();
        var r = await _context.PerformanceReviews.AsNoTracking().FirstOrDefaultAsync(x => x.ReviewId == id);
        if (r == null) return NotFound();
        return r;
    }

    [HttpPost("reviews")]
    public async Task<ActionResult<PerformanceReview>> CreateReview([FromBody] CreateReviewRequest request)
    {
        if (!_access.IsPrivileged()) return Forbid();
        var employee = await _context.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.EmployeeId == request.EmployeeId);
        if (employee == null) return NotFound("Employee not found.");

        var review = new PerformanceReview
        {
            CycleId = request.CycleId,
            EmployeeId = request.EmployeeId,
            ManagerEmployeeId = employee.ManagerId, // snapshot manager
            PositionId = employee.PositionId,       // snapshot position
            DepartmentId = employee.DepartmentId,    // snapshot department
            Status = "NOT_STARTED",
        };
        _context.PerformanceReviews.Add(review);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetReview), new { id = review.ReviewId }, review);
    }

    [HttpPost("reviews/{id:int}/self")]
    public async Task<IActionResult> SubmitSelfReview(int id, [FromBody] SubmitSelfReviewRequest request)
    {
        if (!await _access.CanSubmitSelfReviewAsync(id)) return Forbid();
        var review = await _context.PerformanceReviews.FirstOrDefaultAsync(r => r.ReviewId == id);
        if (review == null) return NotFound();

        review.SelfAchievements = request.Achievements;
        review.SelfChallenges = request.Challenges;
        review.EmployeeSubmittedAt = DateTime.UtcNow;
        if (review.Status == "NOT_STARTED") review.Status = "SELF_REVIEW";
        review.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("reviews/{id:int}/manager")]
    public async Task<IActionResult> SubmitManagerReview(int id, [FromBody] SubmitManagerReviewRequest request)
    {
        if (!await _access.CanSubmitManagerReviewAsync(id)) return Forbid();
        var review = await _context.PerformanceReviews.FirstOrDefaultAsync(r => r.ReviewId == id);
        if (review == null) return NotFound();

        review.OverallRating = request.OverallRating;
        review.ManagerComments = request.Comments;
        review.DevelopmentNeeds = request.DevelopmentNeeds;
        review.ManagerSubmittedAt = DateTime.UtcNow;
        review.Status = "MANAGER_REVIEW";
        review.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("reviews/{id:int}/finalize")]
    public async Task<IActionResult> FinalizeReview(int id)
    {
        if (!await _access.CanSubmitManagerReviewAsync(id)) return Forbid();
        var review = await _context.PerformanceReviews.FirstOrDefaultAsync(r => r.ReviewId == id);
        if (review == null) return NotFound();
        if (review.Status != "MANAGER_REVIEW") return BadRequest("Only MANAGER_REVIEW reviews can be finalized.");

        review.Status = "FINALIZED";
        review.FinalizedAt = DateTime.UtcNow;
        review.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("reviews/{id:int}/acknowledge")]
    public async Task<IActionResult> AcknowledgeReview(int id, [FromBody] AcknowledgeReviewRequest request)
    {
        if (!await _access.CanSubmitSelfReviewAsync(id)) return Forbid();
        var review = await _context.PerformanceReviews.FirstOrDefaultAsync(r => r.ReviewId == id);
        if (review == null) return NotFound();
        if (review.Status != "FINALIZED") return BadRequest("Only FINALIZED reviews can be acknowledged.");

        review.Status = "ACKNOWLEDGED";
        review.AcknowledgedAt = DateTime.UtcNow;
        review.AcknowledgementComment = request.Comment;
        review.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ---- Feedback ----

    [HttpGet("feedback")]
    public async Task<ActionResult<List<Feedback>>> GetFeedback([FromQuery] int? toEmployeeId = null)
    {
        var query = _context.Feedbacks.AsNoTracking().AsQueryable();
        if (toEmployeeId.HasValue)
        {
            if (!_access.IsPrivileged() && _access.GetCurrentEmployeeId() != toEmployeeId.Value)
                return Forbid();
            query = query.Where(f => f.ToEmployeeId == toEmployeeId.Value);
        }
        else if (!_access.IsPrivileged())
        {
            var selfId = _access.GetCurrentEmployeeId();
            if (selfId == null) return Ok(new List<Feedback>());
            query = query.Where(f => f.ToEmployeeId == selfId.Value || f.FromEmployeeId == selfId.Value);
        }
        return await query.OrderByDescending(f => f.CreatedAt).ToListAsync();
    }

    [HttpPost("feedback")]
    public async Task<ActionResult<Feedback>> CreateFeedback([FromBody] CreateFeedbackRequest request)
    {
        var fromId = _access.GetCurrentEmployeeId();
        if (fromId == null) return Unauthorized("No linked employee profile.");

        var feedback = new Feedback
        {
            FromEmployeeId = fromId.Value, // server-resolved, never client-supplied
            ToEmployeeId = request.ToEmployeeId,
            Type = request.Type ?? "Feedback",
            Message = request.Message,
            Visibility = request.Visibility ?? "Private",
        };
        _context.Feedbacks.Add(feedback);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetFeedback), new { id = feedback.FeedbackId }, feedback);
    }

    // ---- 1:1s ----

    [HttpGet("one-on-ones")]
    public async Task<ActionResult<List<OneOnOne>>> GetOneOnOnes()
    {
        var query = _context.OneOnOnes.AsNoTracking().AsQueryable();
        if (!_access.IsPrivileged())
        {
            var selfId = _access.GetCurrentEmployeeId();
            if (selfId == null) return Ok(new List<OneOnOne>());
            query = query.Where(o => o.ManagerEmployeeId == selfId.Value || o.EmployeeId == selfId.Value);
        }
        return await query.OrderBy(o => o.ScheduledAt).ToListAsync();
    }

    [HttpPost("one-on-ones")]
    public async Task<ActionResult<OneOnOne>> CreateOneOnOne([FromBody] CreateOneOnOneRequest request)
    {
        var managerId = _access.GetCurrentEmployeeId();
        if (managerId == null) return Unauthorized("No linked employee profile.");
        if (!_access.IsPrivileged() && !await _access.IsManagerOfAsync(request.EmployeeId))
            return Forbid();

        var o = new OneOnOne
        {
            ManagerEmployeeId = managerId.Value,
            EmployeeId = request.EmployeeId,
            ScheduledAt = request.ScheduledAt,
            Status = "SCHEDULED",
        };
        _context.OneOnOnes.Add(o);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetOneOnOnes), new { id = o.OneOnOneId }, o);
    }

    // ---- Competencies ----

    [HttpGet("competencies")]
    public async Task<ActionResult<List<Competency>>> GetCompetencies()
    {
        return await _context.Competencies.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.Code).ToListAsync();
    }

    [HttpPost("competencies")]
    public async Task<ActionResult<Competency>> CreateCompetency([FromBody] CreateCompetencyRequest request)
    {
        if (!_access.IsPrivileged()) return Forbid();
        var c = new Competency
        {
            Code = request.Code,
            Name = request.Name,
            NameLao = request.NameLao,
            Description = request.Description,
            Category = request.Category ?? "Core",
        };
        _context.Competencies.Add(c);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCompetencies), new { id = c.CompetencyId }, c);
    }

    [HttpGet("positions/{positionId:int}/competencies")]
    public async Task<ActionResult<List<PositionCompetency>>> GetPositionCompetencies(int positionId)
    {
        return await _context.PositionCompetencies
            .AsNoTracking()
            .Where(pc => pc.PositionId == positionId)
            .ToListAsync();
    }

    [HttpPost("positions/{positionId:int}/competencies")]
    public async Task<ActionResult<PositionCompetency>> AddPositionCompetency(int positionId, [FromBody] AddPositionCompetencyRequest request)
    {
        if (!_access.IsPrivileged()) return Forbid();
        var pc = new PositionCompetency
        {
            PositionId = positionId,
            CompetencyId = request.CompetencyId,
            RequiredLevel = request.RequiredLevel,
            IsRequired = request.IsRequired,
        };
        _context.PositionCompetencies.Add(pc);
        await _context.SaveChangesAsync();
        return Ok(pc);
    }

    [HttpGet("assessments")]
    public async Task<ActionResult<List<CompetencyAssessment>>> GetAssessments([FromQuery] int? employeeId = null)
    {
        var query = _context.CompetencyAssessments.AsNoTracking().AsQueryable();
        if (employeeId.HasValue)
        {
            if (!_access.IsPrivileged() && !await _access.IsManagerOfAsync(employeeId.Value) && _access.GetCurrentEmployeeId() != employeeId.Value)
                return Forbid();
            query = query.Where(a => a.EmployeeId == employeeId.Value);
        }
        else if (!_access.IsPrivileged())
        {
            var selfId = _access.GetCurrentEmployeeId();
            if (selfId == null) return Ok(new List<CompetencyAssessment>());
            query = query.Where(a => a.EmployeeId == selfId.Value);
        }
        return await query.OrderByDescending(a => a.AssessedAt).ToListAsync();
    }

    [HttpPost("assessments")]
    public async Task<ActionResult<CompetencyAssessment>> CreateAssessment([FromBody] CreateAssessmentRequest request)
    {
        var assessorId = _access.GetCurrentEmployeeId();
        if (assessorId == null) return Unauthorized("No linked employee profile.");

        var isSelf = request.EmployeeId == assessorId.Value;
        if (!isSelf && !_access.IsPrivileged() && !await _access.IsManagerOfAsync(request.EmployeeId))
            return Forbid();

        var assessment = new CompetencyAssessment
        {
            EmployeeId = request.EmployeeId,
            CompetencyId = request.CompetencyId,
            AssessorEmployeeId = assessorId.Value,
            AssessmentType = isSelf ? "Self" : "Manager",
            Level = request.Level,
            CycleId = request.CycleId,
        };
        _context.CompetencyAssessments.Add(assessment);
        await _context.SaveChangesAsync();
        return Ok(assessment);
    }

    // ---- Development plans ----

    [HttpGet("development-plans")]
    public async Task<ActionResult<List<DevelopmentPlan>>> GetDevelopmentPlans([FromQuery] int? employeeId = null)
    {
        var query = _context.DevelopmentPlans.AsNoTracking().AsQueryable();
        if (employeeId.HasValue)
        {
            if (!_access.IsPrivileged() && !await _access.IsManagerOfAsync(employeeId.Value) && _access.GetCurrentEmployeeId() != employeeId.Value)
                return Forbid();
            query = query.Where(p => p.EmployeeId == employeeId.Value);
        }
        else if (!_access.IsPrivileged())
        {
            var selfId = _access.GetCurrentEmployeeId();
            if (selfId == null) return Ok(new List<DevelopmentPlan>());
            query = query.Where(p => p.EmployeeId == selfId.Value);
        }
        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    [HttpPost("development-plans")]
    public async Task<ActionResult<DevelopmentPlan>> CreateDevelopmentPlan([FromBody] CreateDevelopmentPlanRequest request)
    {
        var creatorId = _access.GetCurrentEmployeeId();
        if (creatorId == null) return Unauthorized("No linked employee profile.");
        if (!_access.IsPrivileged() && request.EmployeeId != creatorId.Value && !await _access.IsManagerOfAsync(request.EmployeeId))
            return Forbid();

        var plan = new DevelopmentPlan
        {
            EmployeeId = request.EmployeeId,
            ManagerEmployeeId = request.ManagerEmployeeId,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            Status = "DRAFT",
        };
        _context.DevelopmentPlans.Add(plan);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetDevelopmentPlans), new { id = plan.DevelopmentPlanId }, plan);
    }

    // ---- Learning ----

    [HttpGet("courses")]
    public async Task<ActionResult<List<LearningCourse>>> GetCourses()
    {
        return await _context.LearningCourses.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.Code).ToListAsync();
    }

    [HttpPost("courses")]
    public async Task<ActionResult<LearningCourse>> CreateCourse([FromBody] CreateCourseRequest request)
    {
        if (!_access.IsPrivileged()) return Forbid();
        var course = new LearningCourse
        {
            Code = request.Code,
            Title = request.Title,
            TitleLao = request.TitleLao,
            Description = request.Description,
            Category = request.Category,
            DeliveryType = request.DeliveryType ?? "Classroom",
        };
        _context.LearningCourses.Add(course);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCourses), new { id = course.CourseId }, course);
    }

    [HttpGet("enrollments")]
    public async Task<ActionResult<List<TrainingEnrollment>>> GetEnrollments([FromQuery] int? employeeId = null)
    {
        var query = _context.TrainingEnrollments.AsNoTracking().AsQueryable();
        if (employeeId.HasValue)
        {
            if (!_access.IsPrivileged() && !await _access.IsManagerOfAsync(employeeId.Value) && _access.GetCurrentEmployeeId() != employeeId.Value)
                return Forbid();
            query = query.Where(e => e.EmployeeId == employeeId.Value);
        }
        else if (!_access.IsPrivileged())
        {
            var selfId = _access.GetCurrentEmployeeId();
            if (selfId == null) return Ok(new List<TrainingEnrollment>());
            query = query.Where(e => e.EmployeeId == selfId.Value);
        }
        return await query.OrderByDescending(e => e.EnrollmentId).ToListAsync();
    }

    [HttpPost("enrollments")]
    public async Task<ActionResult<TrainingEnrollment>> CreateEnrollment([FromBody] CreateEnrollmentRequest request)
    {
        var assignerId = _access.GetCurrentEmployeeId();
        if (assignerId == null) return Unauthorized("No linked employee profile.");
        if (!_access.IsPrivileged() && !await _access.IsManagerOfAsync(request.EmployeeId))
            return Forbid();

        var enrollment = new TrainingEnrollment
        {
            SessionId = request.SessionId,
            EmployeeId = request.EmployeeId,
            Status = "ASSIGNED",
            AssignedByEmployeeId = assignerId.Value,
        };
        _context.TrainingEnrollments.Add(enrollment);
        await _context.SaveChangesAsync();
        return Ok(enrollment);
    }

    [HttpPost("enrollments/{id:int}/complete")]
    public async Task<IActionResult> CompleteEnrollment(int id)
    {
        var enrollment = await _context.TrainingEnrollments.FirstOrDefaultAsync(e => e.EnrollmentId == id);
        if (enrollment == null) return NotFound();
        if (!_access.IsPrivileged() && !await _access.IsManagerOfAsync(enrollment.EmployeeId) && _access.GetCurrentEmployeeId() != enrollment.EmployeeId)
            return Forbid();

        enrollment.Status = "COMPLETED";
        enrollment.CompletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ---- Certifications ----

    [HttpGet("certifications")]
    public async Task<ActionResult<List<EmployeeCertification>>> GetCertifications([FromQuery] int? employeeId = null)
    {
        var query = _context.EmployeeCertifications.AsNoTracking().AsQueryable();
        if (employeeId.HasValue)
        {
            if (!_access.IsPrivileged() && !await _access.IsManagerOfAsync(employeeId.Value) && _access.GetCurrentEmployeeId() != employeeId.Value)
                return Forbid();
            query = query.Where(c => c.EmployeeId == employeeId.Value);
        }
        else if (!_access.IsPrivileged())
        {
            var selfId = _access.GetCurrentEmployeeId();
            if (selfId == null) return Ok(new List<EmployeeCertification>());
            query = query.Where(c => c.EmployeeId == selfId.Value);
        }
        return await query.OrderBy(c => c.ExpiryDate).ToListAsync();
    }

    [HttpPost("certifications")]
    public async Task<ActionResult<EmployeeCertification>> CreateCertification([FromBody] CreateCertificationRequest request)
    {
        var creatorId = _access.GetCurrentEmployeeId();
        if (creatorId == null) return Unauthorized("No linked employee profile.");
        if (!_access.IsPrivileged() && request.EmployeeId != creatorId.Value && !await _access.IsManagerOfAsync(request.EmployeeId))
            return Forbid();

        var cert = new EmployeeCertification
        {
            EmployeeId = request.EmployeeId,
            Name = request.Name,
            Issuer = request.Issuer,
            IssuedDate = request.IssuedDate,
            ExpiryDate = request.ExpiryDate,
            Status = "ACTIVE",
        };
        _context.EmployeeCertifications.Add(cert);
        await _context.SaveChangesAsync();
        return Ok(cert);
    }

    // ---- Career ----

    [HttpGet("career")]
    public async Task<ActionResult<CareerInterest>> GetCareerInterest()
    {
        var selfId = _access.GetCurrentEmployeeId();
        if (selfId == null) return Unauthorized("No linked employee profile.");
        var c = await _context.CareerInterests.AsNoTracking().FirstOrDefaultAsync(x => x.EmployeeId == selfId.Value);
        return c ?? new CareerInterest { EmployeeId = selfId.Value };
    }

    [HttpPost("career")]
    public async Task<ActionResult<CareerInterest>> SaveCareerInterest([FromBody] SaveCareerInterestRequest request)
    {
        var selfId = _access.GetCurrentEmployeeId();
        if (selfId == null) return Unauthorized("No linked employee profile.");

        var c = await _context.CareerInterests.FirstOrDefaultAsync(x => x.EmployeeId == selfId.Value);
        if (c == null)
        {
            c = new CareerInterest { EmployeeId = selfId.Value };
            _context.CareerInterests.Add(c);
        }
        c.Interests = request.Interests;
        c.FutureRoles = request.FutureRoles;
        c.DevelopmentInterests = request.DevelopmentInterests;
        c.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(c);
    }

    // ---- Talent review ----

    [HttpGet("talent")]
    public async Task<ActionResult<List<TalentReview>>> GetTalentReviews()
    {
        if (!await _access.CanViewTalentReviewAsync()) return Forbid();
        return await _context.TalentReviews.AsNoTracking().OrderByDescending(t => t.CreatedAt).ToListAsync();
    }

    [HttpPost("talent")]
    public async Task<ActionResult<TalentReview>> CreateTalentReview([FromBody] CreateTalentReviewRequest request)
    {
        if (!await _access.CanViewTalentReviewAsync()) return Forbid();
        var t = new TalentReview
        {
            EmployeeId = request.EmployeeId,
            CycleId = request.CycleId,
            Potential = request.Potential,
            Performance = request.Performance,
            Readiness = request.Readiness,
            Notes = request.Notes,
            ReviewedByEmployeeId = _access.GetCurrentEmployeeId(),
        };
        _context.TalentReviews.Add(t);
        await _context.SaveChangesAsync();
        return Ok(t);
    }
}

// ---- DTOs ----

public sealed class CreateGoalRequest
{
    public int EmployeeId { get; set; }
    public int? ManagerEmployeeId { get; set; }
    public int? ParentGoalId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? GoalType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
}

public sealed class AddCheckInRequest
{
    public int ProgressPercent { get; set; }
    public string? Status { get; set; }
    public string? Comment { get; set; }
}

public sealed class CreateCycleRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? ReviewDueDate { get; set; }
}

public sealed class CreateReviewRequest
{
    public int CycleId { get; set; }
    public int EmployeeId { get; set; }
}

public sealed class SubmitSelfReviewRequest
{
    public string? Achievements { get; set; }
    public string? Challenges { get; set; }
}

public sealed class SubmitManagerReviewRequest
{
    public int? OverallRating { get; set; }
    public string? Comments { get; set; }
    public string? DevelopmentNeeds { get; set; }
}

public sealed class AcknowledgeReviewRequest
{
    public string? Comment { get; set; }
}

public sealed class CreateFeedbackRequest
{
    public int ToEmployeeId { get; set; }
    public string? Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Visibility { get; set; }
}

public sealed class CreateOneOnOneRequest
{
    public int EmployeeId { get; set; }
    public DateTime? ScheduledAt { get; set; }
}

public sealed class CreateCompetencyRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameLao { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
}

public sealed class AddPositionCompetencyRequest
{
    public int CompetencyId { get; set; }
    public int RequiredLevel { get; set; } = 3;
    public bool IsRequired { get; set; } = true;
}

public sealed class CreateAssessmentRequest
{
    public int EmployeeId { get; set; }
    public int CompetencyId { get; set; }
    public int Level { get; set; } = 3;
    public int? CycleId { get; set; }
}

public sealed class CreateDevelopmentPlanRequest
{
    public int EmployeeId { get; set; }
    public int? ManagerEmployeeId { get; set; }
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
}

public sealed class CreateCourseRequest
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? TitleLao { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? DeliveryType { get; set; }
}

public sealed class CreateEnrollmentRequest
{
    public int SessionId { get; set; }
    public int EmployeeId { get; set; }
}

public sealed class CreateCertificationRequest
{
    public int EmployeeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Issuer { get; set; }
    public DateTime? IssuedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public sealed class SaveCareerInterestRequest
{
    public string? Interests { get; set; }
    public string? FutureRoles { get; set; }
    public string? DevelopmentInterests { get; set; }
}

public sealed class CreateTalentReviewRequest
{
    public int EmployeeId { get; set; }
    public int? CycleId { get; set; }
    public string? Potential { get; set; }
    public string? Performance { get; set; }
    public string? Readiness { get; set; }
    public string? Notes { get; set; }
}
