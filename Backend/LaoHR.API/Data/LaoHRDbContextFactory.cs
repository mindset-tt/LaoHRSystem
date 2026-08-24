using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using LaoHR.Shared.Data;

namespace LaoHR.API.Data;

/// <summary>
/// Phase 3B — design-time factory so `dotnet ef` can generate PostgreSQL
/// migrations against the LaoHRDbContext (which lives in LaoHR.Shared and has
/// no OnConfiguring provider). Uses Npgsql to match the runtime provider.
/// </summary>
public class LaoHRDbContextFactory : IDesignTimeDbContextFactory<LaoHRDbContext>
{
    public LaoHRDbContext CreateDbContext(string[] args)
    {
        // The legacy timestamp switch must be applied ONLY for `database update`,
        // NOT for `migrations add`:
        //   - `database update` applies seed data containing DateTimeKind.Unspecified
        //     values (e.g. holidays), which Npgsql rejects for `timestamp with time
        //     zone` columns. The switch makes Npgsql accept them.
        //   - `migrations add` must run with the switch OFF so the generated model
        //     matches the existing `timestamp with time zone` chain. With the switch
        //     ON, DateTime maps to `timestamp without time zone` and EF scaffolds
        //     hundreds of non-additive AlterColumn operations (a migration rebase).
        //
        // `dotnet ef` passes empty args to the factory for BOTH commands, so we
        // cannot detect the command from args. Instead, operators set the
        // NPGSQL_LEGACY_TIMESTAMP=1 environment variable when running
        // `dotnet ef database update` (see scripts/validate-postgres.ps1).
        var enableLegacy = string.Equals(
            Environment.GetEnvironmentVariable("NPGSQL_LEGACY_TIMESTAMP"), "1",
            StringComparison.OrdinalIgnoreCase);
        if (enableLegacy)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        var optionsBuilder = new DbContextOptionsBuilder<LaoHRDbContext>();

        // Design-time connection string is sourced from the environment, never
        // hardcoded. `migrations add` does not need a real connection (the model
        // is built from the DbContext), so a placeholder is acceptable there.
        // `database update` requires a real connection — operators pass it via
        // `--connection` or the LAOHR_EF_CONNECTION_STRING environment variable.
        var connectionString = Environment.GetEnvironmentVariable("LAOHR_EF_CONNECTION_STRING")
            ?? "Host=localhost;Database=laohr_design;Username=laohr;Password=";

        // MigrationsAssembly must match the runtime config in Program.cs.
        optionsBuilder.UseNpgsql(connectionString,
            b => b.MigrationsAssembly("LaoHR.API"));
        return new LaoHRDbContext(optionsBuilder.Options);
    }
}