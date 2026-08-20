using System.ComponentModel.DataAnnotations;

namespace LaoHR.Shared.Pagination;

/// <summary>
/// Standard pagination envelope for list endpoints. The frontend
/// <c>PaginatedResponse&lt;T&gt;</c> type is shaped to match this.
/// </summary>
public sealed class PaginatedQuery
{
    /// <summary>1-based page index.</summary>
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    /// <summary>Items per page. Capped server-side.</summary>
    [Range(1, 200)]
    public int PageSize { get; set; } = 25;

    /// <summary>Optional free-text search term.</summary>
    public string? Search { get; set; }

    /// <summary>Optional sort field (whitelisted by the controller).</summary>
    public string? Sort { get; set; }

    /// <summary>Sort direction: asc | desc.</summary>
    public string? Direction { get; set; }

    public const int MaxPageSize = 200;
}

public sealed class PaginatedResponse<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public long TotalItems { get; init; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;
}
