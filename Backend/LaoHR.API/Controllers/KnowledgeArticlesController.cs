using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;

namespace LaoHR.API.Controllers;

public class KnowledgeCategoryItem
{
    public int KnowledgeCategoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameLao { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public int ArticleCount { get; set; }
}

public class KnowledgeArticleListItem
{
    public int KnowledgeArticleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? TitleLao { get; set; }
    public string Summary { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public int AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class KnowledgeArticleDetail
{
    public int KnowledgeArticleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? TitleLao { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? BodyLao { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public int AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateArticleRequest
{
    public string Title { get; set; } = string.Empty;
    public string? TitleLao { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? BodyLao { get; set; }
    public int CategoryId { get; set; }
    public string Status { get; set; } = "DRAFT";
}

public class UpdateArticleRequest
{
    public string? Title { get; set; }
    public string? TitleLao { get; set; }
    public string? Summary { get; set; }
    public string? Body { get; set; }
    public string? BodyLao { get; set; }
    public int? CategoryId { get; set; }
    public string? Status { get; set; }
}

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class KnowledgeArticlesController : ControllerBase
{
    private readonly LaoHRDbContext _context;

    public KnowledgeArticlesController(LaoHRDbContext context)
    {
        _context = context;
    }

    [HttpGet("categories")]
    public async Task<ActionResult<List<KnowledgeCategoryItem>>> GetCategories()
    {
        // Single round-trip counts.
        var counts = await _context.KnowledgeArticles
            .AsNoTracking()
            .GroupBy(a => a.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.LongCount() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count);

        var items = await _context.KnowledgeCategories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .Select(c => new KnowledgeCategoryItem
            {
                KnowledgeCategoryId = c.KnowledgeCategoryId,
                Code = c.Code,
                Name = c.Name,
                NameLao = c.NameLao,
                Description = c.Description,
                SortOrder = c.SortOrder,
                IsActive = c.IsActive,
                ArticleCount = counts.ContainsKey(c.KnowledgeCategoryId) ? (int)counts[c.KnowledgeCategoryId] : 0,
            })
            .ToListAsync();
        return items;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<KnowledgeArticleListItem>>> GetArticles(
        [FromQuery] int? categoryId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var query = _context.KnowledgeArticles.AsNoTracking().AsQueryable();

        // Non-Admin/HR callers only see published articles.
        var role = User.FindFirstValue(ClaimTypes.Role) ?? User.Identity?.Name;
        var isPrivileged = User.IsInRole("Admin") || User.IsInRole("HR");
        if (!isPrivileged)
            query = query.Where(a => a.Status == "PUBLISHED");

        if (categoryId.HasValue)
            query = query.Where(a => a.CategoryId == categoryId.Value);
        if (!string.IsNullOrEmpty(status) && isPrivileged)
            query = query.Where(a => a.Status == status);
        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower();
            query = query.Where(a =>
                a.Title.ToLower().Contains(s) ||
                a.Summary.ToLower().Contains(s) ||
                a.Body.ToLower().Contains(s));
        }

        var total = await query.LongCountAsync();

        var items = await query
            .OrderByDescending(a => a.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new KnowledgeArticleListItem
            {
                KnowledgeArticleId = a.KnowledgeArticleId,
                Title = a.Title,
                TitleLao = a.TitleLao,
                Summary = a.Summary,
                CategoryId = a.CategoryId,
                CategoryName = a.Category != null ? a.Category.Name : null,
                Status = a.Status,
                ViewCount = a.ViewCount,
                AuthorId = a.AuthorId,
                AuthorName = a.Author != null ? (a.Author.EnglishName ?? a.Author.LaoName) : null,
                PublishedAt = a.PublishedAt,
                UpdatedAt = a.UpdatedAt,
            })
            .ToListAsync();

        return new PaginatedResponse<KnowledgeArticleListItem>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<KnowledgeArticleDetail>> GetArticle(int id)
    {
        var a = await _context.KnowledgeArticles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.KnowledgeArticleId == id);
        if (a == null) return NotFound();

        var isPrivileged = User.IsInRole("Admin") || User.IsInRole("HR");
        if (a.Status != "PUBLISHED" && !isPrivileged)
            return NotFound();

        var category = await _context.KnowledgeCategories
            .Where(c => c.KnowledgeCategoryId == a.CategoryId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync();

        var author = await _context.Employees
            .Where(e => e.EmployeeId == a.AuthorId)
            .Select(e => e.EnglishName ?? e.LaoName)
            .FirstOrDefaultAsync();

        // Lightweight view counter — increment without blocking on read.
        _ = Task.Run(async () =>
        {
            using var scope = HttpContext.RequestServices.CreateScope();
            var db = (LaoHRDbContext)scope.ServiceProvider.GetService(typeof(LaoHRDbContext))!;
            var entity = await db.KnowledgeArticles.FirstOrDefaultAsync(x => x.KnowledgeArticleId == id);
            if (entity != null) { entity.ViewCount++; await db.SaveChangesAsync(); }
        });

        return new KnowledgeArticleDetail
        {
            KnowledgeArticleId = a.KnowledgeArticleId,
            Title = a.Title,
            TitleLao = a.TitleLao,
            Summary = a.Summary,
            Body = a.Body,
            BodyLao = a.BodyLao,
            CategoryId = a.CategoryId,
            CategoryName = category,
            Status = a.Status,
            ViewCount = a.ViewCount + 1,
            AuthorId = a.AuthorId,
            AuthorName = author,
            PublishedAt = a.PublishedAt,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt,
        };
    }

    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<KnowledgeArticle>> CreateArticle([FromBody] CreateArticleRequest request)
    {
        var employeeId = GetCurrentEmployeeId();
        if (employeeId == null) return Forbid();

        var article = new KnowledgeArticle
        {
            Title = request.Title,
            TitleLao = request.TitleLao,
            Summary = request.Summary,
            Body = request.Body,
            BodyLao = request.BodyLao,
            CategoryId = request.CategoryId,
            Status = request.Status,
            AuthorId = employeeId.Value,
            PublishedAt = request.Status == "PUBLISHED" ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _context.KnowledgeArticles.Add(article);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetArticle), new { id = article.KnowledgeArticleId }, article);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> UpdateArticle(int id, [FromBody] UpdateArticleRequest request)
    {
        var a = await _context.KnowledgeArticles.FirstOrDefaultAsync(x => x.KnowledgeArticleId == id);
        if (a == null) return NotFound();

        if (request.Title != null) a.Title = request.Title;
        if (request.TitleLao != null) a.TitleLao = request.TitleLao;
        if (request.Summary != null) a.Summary = request.Summary;
        if (request.Body != null) a.Body = request.Body;
        if (request.BodyLao != null) a.BodyLao = request.BodyLao;
        if (request.CategoryId.HasValue) a.CategoryId = request.CategoryId.Value;

        if (request.Status != null && request.Status != a.Status)
        {
            a.Status = request.Status;
            if (request.Status == "PUBLISHED" && a.PublishedAt == null)
                a.PublishedAt = DateTime.UtcNow;
        }

        a.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> DeleteArticle(int id)
    {
        var a = await _context.KnowledgeArticles.FirstOrDefaultAsync(x => x.KnowledgeArticleId == id);
        if (a == null) return NotFound();
        _context.KnowledgeArticles.Remove(a);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private int? GetCurrentEmployeeId()
    {
        var username = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;
        if (string.IsNullOrEmpty(username)) return null;
        return _context.Users
            .Where(u => u.Username == username)
            .Select(u => u.EmployeeId)
            .FirstOrDefault();
    }
}