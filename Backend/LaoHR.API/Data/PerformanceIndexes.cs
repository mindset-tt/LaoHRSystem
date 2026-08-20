using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;

namespace LaoHR.API.Data;

/// <summary>
/// Phase 0.1 — performance indexes for the hot read paths.
///
/// The existing EF migrations are SQL Server-flavored while the runtime uses
/// Npgsql, so we can't run them through Migrate(). This helper applies raw
/// SQL on startup with IF NOT EXISTS so it's a no-op if the indexes already
/// exist. It's deliberately not an EF Migration so it does not need a
/// Designer.cs or a model snapshot to match.
/// </summary>
public static class PerformanceIndexes
{
    private static readonly string[] Statements =
    {
        // Attendance — list/calendar by date is the dashboard hot path.
        "CREATE INDEX IF NOT EXISTS \"IX_Attendances_AttendanceDate\" ON \"Attendances\" (\"AttendanceDate\");",

        // SalarySlip — payroll reports always filter by PeriodId.
        "CREATE INDEX IF NOT EXISTS \"IX_SalarySlips_PeriodId\" ON \"SalarySlips\" (\"PeriodId\");",

        // LeaveRequest — date-range + status filter on calendar + approval queues.
        "CREATE INDEX IF NOT EXISTS \"IX_LeaveRequests_StartDate_EndDate_Status\" ON \"LeaveRequests\" (\"StartDate\", \"EndDate\", \"Status\");",

        // Employee — every HR list filters by IsActive and DepartmentId.
        "CREATE INDEX IF NOT EXISTS \"IX_Employees_IsActive_DepartmentId\" ON \"Employees\" (\"IsActive\", \"DepartmentId\");",

        // AuditLog — security/audit timeline queries are always by Timestamp.
        "CREATE INDEX IF NOT EXISTS \"IX_AuditLogs_Timestamp\" ON \"AuditLogs\" (\"Timestamp\");",
        "CREATE INDEX IF NOT EXISTS \"IX_AuditLogs_EntityName_Timestamp\" ON \"AuditLogs\" (\"EntityName\", \"Timestamp\");",

        // Phase 2 — Project workspace hot paths.
        "CREATE INDEX IF NOT EXISTS \"IX_Projects_Status\" ON \"Projects\" (\"Status\");",
        "CREATE INDEX IF NOT EXISTS \"IX_ProjectTasks_Project_Status\" ON \"ProjectTasks\" (\"ProjectId\", \"Status\");",
        "CREATE INDEX IF NOT EXISTS \"IX_ProjectTasks_Assignee\" ON \"TaskAssignees\" (\"EmployeeId\");",
        "CREATE INDEX IF NOT EXISTS \"IX_ProjectTasks_DueDate\" ON \"ProjectTasks\" (\"DueDate\");",
        "CREATE INDEX IF NOT EXISTS \"IX_ActivityLogs_Project_CreatedAt\" ON \"ActivityLogs\" (\"ProjectId\", \"CreatedAt\" DESC);"
    };

    public static void Apply(LaoHRDbContext db)
    {
        foreach (var sql in Statements)
        {
            db.Database.ExecuteSqlRaw(sql);
        }
    }
}
