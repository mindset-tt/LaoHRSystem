namespace LaoHR.API.Services;

/// <summary>
/// Phase 3C3 — shared date-range semantics for reporting.
///
/// All business dates are computed in the Lao timezone (UTC+07:00). Stored
/// timestamps are UTC; range boundaries are converted to UTC for querying.
/// </summary>
public static class ReportingDateRange
{
    public static readonly TimeSpan LaoOffset = TimeSpan.FromHours(7);

    /// <summary>Returns the Lao-local "today" date.</summary>
    public static DateTime TodayLao()
    {
        return DateTime.UtcNow.Add(LaoOffset).Date;
    }

    /// <summary>Converts a Lao-local date to a UTC instant (start of day).</summary>
    public static DateTime LaoToUtc(DateTime laoDate)
    {
        return DateTime.SpecifyKind(laoDate.Date, DateTimeKind.Unspecified).Subtract(LaoOffset);
    }

    /// <summary>Resolves a named range into (startUtc, endUtc) inclusive.</summary>
    public static (DateTime start, DateTime end) Resolve(string? range, DateTime? from, DateTime? to)
    {
        var today = TodayLao();

        if (from.HasValue && to.HasValue)
        {
            return (LaoToUtc(from.Value.Date), LaoToUtc(to.Value.Date.AddDays(1)).AddTicks(-1));
        }

        switch (range?.ToLowerInvariant())
        {
            case "today":
                return (LaoToUtc(today), LaoToUtc(today.AddDays(1)).AddTicks(-1));
            case "week":
                var weekStart = today.AddDays(-(int)today.DayOfWeek);
                return (LaoToUtc(weekStart), LaoToUtc(weekStart.AddDays(7)).AddTicks(-1));
            case "month":
                var monthStart = new DateTime(today.Year, today.Month, 1);
                return (LaoToUtc(monthStart), LaoToUtc(monthStart.AddMonths(1)).AddTicks(-1));
            case "quarter":
                var qMonth = ((today.Month - 1) / 3) * 3 + 1;
                var qStart = new DateTime(today.Year, qMonth, 1);
                return (LaoToUtc(qStart), LaoToUtc(qStart.AddMonths(3)).AddTicks(-1));
            case "year":
                var yStart = new DateTime(today.Year, 1, 1);
                return (LaoToUtc(yStart), LaoToUtc(yStart.AddYears(1)).AddTicks(-1));
            default:
                // Default: current month.
                var mStart = new DateTime(today.Year, today.Month, 1);
                return (LaoToUtc(mStart), LaoToUtc(mStart.AddMonths(1)).AddTicks(-1));
        }
    }

    /// <summary>Returns the previous period of the same length as the given range.</summary>
    public static (DateTime start, DateTime end) Previous((DateTime start, DateTime end) current)
    {
        var length = current.end - current.start;
        return (current.start.Subtract(length), current.start.AddTicks(-1));
    }
}
