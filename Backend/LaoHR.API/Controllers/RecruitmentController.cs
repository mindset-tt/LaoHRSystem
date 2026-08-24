using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaoHR.API.Controllers;

/// <summary>
/// Phase 3C5 — recruitment + ATS + onboarding endpoints.
/// </summary>
[Authorize]
[ApiController]
[Route("api/recruitment")]
public class RecruitmentController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IRecruitmentAccessService _access;
    private readonly IApprovalService _approval;
    private readonly IHireConversionService _hire;
    private readonly INotificationService _notifications;

    public RecruitmentController(
        LaoHRDbContext context,
        IRecruitmentAccessService access,
        IApprovalService approval,
        IHireConversionService hire,
        INotificationService notifications)
    {
        _context = context;
        _access = access;
        _approval = approval;
        _hire = hire;
        _notifications = notifications;
    }

    // ---- Requisitions ----

    [HttpGet("requisitions")]
    public async Task<ActionResult<List<JobRequisition>>> GetRequisitions([FromQuery] string? status = null)
    {
        var query = _context.JobRequisitions.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(r => r.Status == status);
        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    [HttpGet("requisitions/{id:int}")]
    public async Task<ActionResult<JobRequisition>> GetRequisition(int id)
    {
        if (!await _access.CanViewRequisitionAsync(id)) return Forbid();
        var r = await _context.JobRequisitions.AsNoTracking().FirstOrDefaultAsync(x => x.RequisitionId == id);
        if (r == null) return NotFound();
        return r;
    }

    [HttpPost("requisitions")]
    public async Task<ActionResult<JobRequisition>> CreateRequisition([FromBody] CreateRequisitionRequest request)
    {
        var requesterId = _access.GetCurrentEmployeeId();
        if (requesterId == null) return Unauthorized("No linked employee profile.");

        var count = await _context.JobRequisitions.CountAsync() + 1;
        var req = new JobRequisition
        {
            RequisitionNumber = $"REQ-{count:D4}",
            PositionId = request.PositionId,
            DepartmentId = request.DepartmentId,
            WorkLocationId = request.WorkLocationId,
            RequestedByEmployeeId = requesterId.Value,
            HiringManagerEmployeeId = request.HiringManagerEmployeeId,
            Headcount = request.Headcount,
            Reason = request.Reason ?? "Growth",
            Justification = request.Justification,
            TargetStartDate = request.TargetStartDate,
            Priority = request.Priority ?? "MEDIUM",
            Status = "DRAFT",
        };
        _context.JobRequisitions.Add(req);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetRequisition), new { id = req.RequisitionId }, req);
    }

    [HttpPost("requisitions/{id:int}/submit")]
    public async Task<IActionResult> SubmitRequisition(int id)
    {
        if (!await _access.CanEditRequisitionAsync(id)) return Forbid();
        var req = await _context.JobRequisitions.FirstOrDefaultAsync(r => r.RequisitionId == id);
        if (req == null) return NotFound();
        if (req.Status != "DRAFT") return BadRequest("Only DRAFT requisitions can be submitted.");

        req.Status = "PENDING_APPROVAL";
        req.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Create approval request (HR role step).
        await _approval.CreateRequestAsync("REQUISITION", req.RequisitionId, req.RequestedByEmployeeId,
            new List<ApprovalStepDefinition> { new() { ResolverType = "ROLE", RoleName = "HR" } });

        return NoContent();
    }

    [HttpPost("requisitions/{id:int}/approve")]
    public async Task<IActionResult> ApproveRequisition(int id)
    {
        var actorId = _access.GetCurrentEmployeeId();
        if (actorId == null) return Unauthorized("No linked employee profile.");

        var req = await _context.JobRequisitions.FirstOrDefaultAsync(r => r.RequisitionId == id);
        if (req == null) return NotFound();

        var approval = await _context.ApprovalRequests
            .FirstOrDefaultAsync(a => a.RequestType == "REQUISITION" && a.EntityId == id && a.Status == "PENDING");
        if (approval == null) return BadRequest("No pending approval.");

        try
        {
            var result = await _approval.ApproveAsync(approval.ApprovalRequestId, actorId.Value, null);
            if (result.Status == "APPROVED")
            {
                req.Status = "APPROVED";
                req.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }

        return NoContent();
    }

    // ---- Openings ----

    [HttpGet("openings")]
    public async Task<ActionResult<List<JobOpening>>> GetOpenings([FromQuery] string? status = null)
    {
        var query = _context.JobOpenings.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(o => o.Status == status);
        return await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
    }

    [HttpPost("openings")]
    public async Task<ActionResult<JobOpening>> CreateOpening([FromBody] CreateOpeningRequest request)
    {
        if (!await _access.CanViewRequisitionAsync(request.RequisitionId)) return Forbid();
        var req = await _context.JobRequisitions.FirstOrDefaultAsync(r => r.RequisitionId == request.RequisitionId);
        if (req == null) return NotFound();
        if (req.Status != "APPROVED") return BadRequest("Opening requires an APPROVED requisition.");

        var opening = new JobOpening
        {
            RequisitionId = request.RequisitionId,
            Title = request.Title,
            TitleLao = request.TitleLao,
            Summary = request.Summary,
            Responsibilities = request.Responsibilities,
            Requirements = request.Requirements,
            Status = "OPEN",
        };
        _context.JobOpenings.Add(opening);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetOpenings), new { id = opening.OpeningId }, opening);
    }

    // ---- Candidates ----

    [HttpGet("candidates")]
    public async Task<ActionResult<List<Candidate>>> GetCandidates([FromQuery] string? search = null)
    {
        var query = _context.Candidates.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(c => c.FirstName.ToLower().Contains(s) || c.LastName.ToLower().Contains(s)
                || (c.Email != null && c.Email.ToLower().Contains(s)));
        }
        return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
    }

    [HttpGet("candidates/{id:int}")]
    public async Task<ActionResult<Candidate>> GetCandidate(int id)
    {
        if (!await _access.CanViewCandidateAsync(id)) return Forbid();
        var c = await _context.Candidates.AsNoTracking().FirstOrDefaultAsync(x => x.CandidateId == id);
        if (c == null) return NotFound();
        return c;
    }

    [HttpPost("candidates")]
    public async Task<ActionResult<Candidate>> CreateCandidate([FromBody] CreateCandidateRequest request)
    {
        var candidate = new Candidate
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            FirstNameLao = request.FirstNameLao,
            LastNameLao = request.LastNameLao,
            Email = request.Email,
            Phone = request.Phone,
            CurrentLocation = request.CurrentLocation,
            CurrentCompany = request.CurrentCompany,
            CurrentTitle = request.CurrentTitle,
            Summary = request.Summary,
            Source = request.Source,
            Status = "ACTIVE",
        };
        _context.Candidates.Add(candidate);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCandidate), new { id = candidate.CandidateId }, candidate);
    }

    // ---- Applications ----

    [HttpGet("applications")]
    public async Task<ActionResult<List<Application>>> GetApplications([FromQuery] int? openingId = null, [FromQuery] string? stage = null)
    {
        var query = _context.Applications.AsNoTracking().AsQueryable();
        if (openingId.HasValue) query = query.Where(a => a.OpeningId == openingId.Value);
        if (!string.IsNullOrWhiteSpace(stage)) query = query.Where(a => a.CurrentStage == stage);
        return await query.OrderByDescending(a => a.AppliedAt).ToListAsync();
    }

    [HttpGet("applications/{id:int}")]
    public async Task<ActionResult<Application>> GetApplication(int id)
    {
        if (!await _access.CanViewApplicationAsync(id)) return Forbid();
        var a = await _context.Applications.AsNoTracking().FirstOrDefaultAsync(x => x.ApplicationId == id);
        if (a == null) return NotFound();
        return a;
    }

    [HttpPost("applications")]
    public async Task<ActionResult<Application>> CreateApplication([FromBody] CreateApplicationRequest request)
    {
        var opening = await _context.JobOpenings.FirstOrDefaultAsync(o => o.OpeningId == request.OpeningId);
        if (opening == null) return NotFound("Opening not found.");
        if (opening.Status != "OPEN") return BadRequest("Applications are not accepted for this opening.");

        // Prevent duplicate application to the same opening.
        var dup = await _context.Applications
            .AnyAsync(a => a.CandidateId == request.CandidateId && a.OpeningId == request.OpeningId && a.Status != "WITHDRAWN");
        if (dup) return Conflict("Candidate has already applied to this opening.");

        var application = new Application
        {
            CandidateId = request.CandidateId,
            OpeningId = request.OpeningId,
            Source = request.Source,
            CurrentStage = "APPLIED",
            Status = "ACTIVE",
        };
        _context.Applications.Add(application);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetApplication), new { id = application.ApplicationId }, application);
    }

    [HttpPost("applications/{id:int}/move")]
    public async Task<IActionResult> MoveApplication(int id, [FromBody] MoveApplicationRequest request)
    {
        if (!await _access.CanMoveApplicationAsync(id)) return Forbid();
        var app = await _context.Applications.FirstOrDefaultAsync(a => a.ApplicationId == id);
        if (app == null) return NotFound();

        var validStages = new[] { "APPLIED", "SCREENING", "INTERVIEW", "ASSESSMENT", "OFFER", "HIRED", "REJECTED", "WITHDRAWN" };
        if (!validStages.Contains(request.Stage)) return BadRequest($"Invalid stage: {request.Stage}");

        var from = app.CurrentStage;
        app.CurrentStage = request.Stage;
        if (request.Stage == "REJECTED") app.Status = "REJECTED";
        if (request.Stage == "WITHDRAWN") app.Status = "WITHDRAWN";
        if (request.Stage == "HIRED") app.Status = "HIRED";
        app.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _context.ApplicationStageHistories.Add(new ApplicationStageHistory
        {
            ApplicationId = id,
            FromStage = from,
            ToStage = request.Stage,
            ActorEmployeeId = _access.GetCurrentEmployeeId(),
            Comment = request.Comment,
        });
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("applications/{id:int}/history")]
    public async Task<ActionResult<List<ApplicationStageHistory>>> GetApplicationHistory(int id)
    {
        if (!await _access.CanViewApplicationAsync(id)) return Forbid();
        return await _context.ApplicationStageHistories
            .AsNoTracking()
            .Where(h => h.ApplicationId == id)
            .OrderBy(h => h.CreatedAt)
            .ToListAsync();
    }

    // ---- Interviews ----

    [HttpGet("interviews")]
    public async Task<ActionResult<List<Interview>>> GetInterviews([FromQuery] int? applicationId = null)
    {
        var query = _context.Interviews.AsNoTracking().AsQueryable();
        if (applicationId.HasValue) query = query.Where(i => i.ApplicationId == applicationId.Value);
        return await query.OrderBy(i => i.ScheduledStart).ToListAsync();
    }

    [HttpPost("interviews")]
    public async Task<ActionResult<Interview>> CreateInterview([FromBody] CreateInterviewRequest request)
    {
        if (!await _access.CanViewApplicationAsync(request.ApplicationId)) return Forbid();
        var interview = new Interview
        {
            ApplicationId = request.ApplicationId,
            InterviewType = request.InterviewType ?? "HR",
            ScheduledStart = request.ScheduledStart,
            ScheduledEnd = request.ScheduledEnd,
            Location = request.Location,
            Status = "SCHEDULED",
            OrganizerEmployeeId = _access.GetCurrentEmployeeId(),
        };
        _context.Interviews.Add(interview);
        await _context.SaveChangesAsync();

        if (request.ParticipantEmployeeIds != null)
        {
            foreach (var empId in request.ParticipantEmployeeIds)
            {
                _context.InterviewParticipants.Add(new InterviewParticipant
                {
                    InterviewId = interview.InterviewId,
                    EmployeeId = empId,
                    Role = "INTERVIEWER",
                });
            }
            await _context.SaveChangesAsync();
        }

        return CreatedAtAction(nameof(GetInterviews), new { id = interview.InterviewId }, interview);
    }

    [HttpPost("interviews/{id:int}/evaluations")]
    public async Task<ActionResult<InterviewEvaluation>> SubmitEvaluation(int id, [FromBody] SubmitEvaluationRequest request)
    {
        if (!await _access.CanSubmitEvaluationAsync(id)) return Forbid();
        var evaluatorId = _access.GetCurrentEmployeeId();
        if (evaluatorId == null) return Unauthorized("No linked employee profile.");

        var existing = await _context.InterviewEvaluations
            .FirstOrDefaultAsync(e => e.InterviewId == id && e.EvaluatorEmployeeId == evaluatorId.Value);
        if (existing != null) return Conflict("You have already submitted an evaluation for this interview.");

        var evaluation = new InterviewEvaluation
        {
            InterviewId = id,
            EvaluatorEmployeeId = evaluatorId.Value,
            CommunicationScore = request.CommunicationScore,
            ExperienceScore = request.ExperienceScore,
            RoleFitScore = request.RoleFitScore,
            Recommendation = request.Recommendation ?? "Neutral",
            Comments = request.Comments,
        };
        _context.InterviewEvaluations.Add(evaluation);
        await _context.SaveChangesAsync();
        return Ok(evaluation);
    }

    // ---- Offers ----

    [HttpGet("offers")]
    public async Task<ActionResult<List<Offer>>> GetOffers([FromQuery] string? status = null)
    {
        var query = _context.Offers.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(o => o.Status == status);
        return await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
    }

    [HttpGet("offers/{id:int}")]
    public async Task<ActionResult<Offer>> GetOffer(int id)
    {
        if (!await _access.CanViewOfferAsync(id)) return Forbid();
        var o = await _context.Offers.AsNoTracking().FirstOrDefaultAsync(x => x.OfferId == id);
        if (o == null) return NotFound();
        return o;
    }

    [HttpPost("offers")]
    public async Task<ActionResult<Offer>> CreateOffer([FromBody] CreateOfferRequest request)
    {
        if (!await _access.CanCreateOfferAsync(request.ApplicationId)) return Forbid();
        var app = await _context.Applications.FirstOrDefaultAsync(a => a.ApplicationId == request.ApplicationId);
        if (app == null) return NotFound();

        var offer = new Offer
        {
            ApplicationId = request.ApplicationId,
            PositionId = request.PositionId,
            ProposedStartDate = request.ProposedStartDate,
            Salary = request.Salary,
            Currency = request.Currency,
            EmploymentType = request.EmploymentType,
            Status = "DRAFT",
            CreatedByEmployeeId = _access.GetCurrentEmployeeId(),
            ExpiresAt = request.ExpiresAt,
        };
        _context.Offers.Add(offer);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetOffer), new { id = offer.OfferId }, offer);
    }

    [HttpPost("offers/{id:int}/accept")]
    public async Task<IActionResult> AcceptOffer(int id)
    {
        if (!await _access.CanViewOfferAsync(id)) return Forbid();
        var offer = await _context.Offers.FirstOrDefaultAsync(o => o.OfferId == id);
        if (offer == null) return NotFound();
        if (offer.Status != "SENT" && offer.Status != "APPROVED") return BadRequest("Only sent/approved offers can be accepted.");

        offer.Status = "ACCEPTED";
        offer.AcceptedAt = DateTime.UtcNow;
        offer.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("offers/{id:int}/decline")]
    public async Task<IActionResult> DeclineOffer(int id, [FromBody] DeclineOfferRequest request)
    {
        if (!await _access.CanViewOfferAsync(id)) return Forbid();
        var offer = await _context.Offers.FirstOrDefaultAsync(o => o.OfferId == id);
        if (offer == null) return NotFound();

        offer.Status = "DECLINED";
        offer.DeclinedAt = DateTime.UtcNow;
        offer.DeclineReason = request.Reason;
        offer.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ---- Hire conversion ----

    [HttpPost("applications/{id:int}/hire")]
    public async Task<ActionResult<HireResult>> Hire(int id)
    {
        if (!await _access.CanHireAsync(id)) return Forbid();
        try
        {
            var result = await _hire.ConvertToEmployeeAsync(id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    // ---- Onboarding ----

    [HttpGet("onboarding")]
    public async Task<ActionResult<List<OnboardingProcess>>> GetOnboarding([FromQuery] string? status = null)
    {
        var query = _context.OnboardingProcesses.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(p => p.Status == status);
        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    [HttpGet("onboarding/{id:int}/tasks")]
    public async Task<ActionResult<List<OnboardingTask>>> GetOnboardingTasks(int id)
    {
        if (!await _access.CanManageOnboardingAsync(id)) return Forbid();
        return await _context.OnboardingTasks
            .AsNoTracking()
            .Where(t => t.OnboardingProcessId == id)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
    }

    [HttpPost("onboarding/{id:int}/tasks")]
    public async Task<ActionResult<OnboardingTask>> AddOnboardingTask(int id, [FromBody] AddOnboardingTaskRequest request)
    {
        if (!await _access.CanManageOnboardingAsync(id)) return Forbid();
        var task = new OnboardingTask
        {
            OnboardingProcessId = id,
            Title = request.Title,
            Description = request.Description,
            OwnerEmployeeId = request.OwnerEmployeeId,
            DueDate = request.DueDate,
            Category = request.Category ?? "HR",
            SortOrder = request.SortOrder ?? 0,
        };
        _context.OnboardingTasks.Add(task);
        await _context.SaveChangesAsync();
        return Ok(task);
    }

    [HttpPost("onboarding/tasks/{id:int}/complete")]
    public async Task<IActionResult> CompleteOnboardingTask(int id)
    {
        var task = await _context.OnboardingTasks.FirstOrDefaultAsync(t => t.OnboardingTaskId == id);
        if (task == null) return NotFound();
        if (!await _access.CanManageOnboardingAsync(task.OnboardingProcessId)) return Forbid();

        task.CompletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

// ---- DTOs ----

public sealed class CreateRequisitionRequest
{
    public int PositionId { get; set; }
    public int? DepartmentId { get; set; }
    public int? WorkLocationId { get; set; }
    public int? HiringManagerEmployeeId { get; set; }
    public int Headcount { get; set; } = 1;
    public string? Reason { get; set; }
    public string? Justification { get; set; }
    public DateTime? TargetStartDate { get; set; }
    public string? Priority { get; set; }
}

public sealed class CreateOpeningRequest
{
    public int RequisitionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? TitleLao { get; set; }
    public string? Summary { get; set; }
    public string? Responsibilities { get; set; }
    public string? Requirements { get; set; }
}

public sealed class CreateCandidateRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? FirstNameLao { get; set; }
    public string? LastNameLao { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? CurrentLocation { get; set; }
    public string? CurrentCompany { get; set; }
    public string? CurrentTitle { get; set; }
    public string? Summary { get; set; }
    public string? Source { get; set; }
}

public sealed class CreateApplicationRequest
{
    public int CandidateId { get; set; }
    public int OpeningId { get; set; }
    public string? Source { get; set; }
}

public sealed class MoveApplicationRequest
{
    public string Stage { get; set; } = string.Empty;
    public string? Comment { get; set; }
}

public sealed class CreateInterviewRequest
{
    public int ApplicationId { get; set; }
    public string? InterviewType { get; set; }
    public DateTime? ScheduledStart { get; set; }
    public DateTime? ScheduledEnd { get; set; }
    public string? Location { get; set; }
    public List<int>? ParticipantEmployeeIds { get; set; }
}

public sealed class SubmitEvaluationRequest
{
    public int CommunicationScore { get; set; } = 3;
    public int ExperienceScore { get; set; } = 3;
    public int RoleFitScore { get; set; } = 3;
    public string? Recommendation { get; set; }
    public string? Comments { get; set; }
}

public sealed class CreateOfferRequest
{
    public int ApplicationId { get; set; }
    public int PositionId { get; set; }
    public DateTime? ProposedStartDate { get; set; }
    public decimal? Salary { get; set; }
    public string? Currency { get; set; }
    public string? EmploymentType { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public sealed class DeclineOfferRequest
{
    public string? Reason { get; set; }
}

public sealed class AddOnboardingTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? OwnerEmployeeId { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Category { get; set; }
    public int? SortOrder { get; set; }
}
