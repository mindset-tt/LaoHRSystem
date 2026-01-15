using LaoHR.API.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace LaoHR.API.Jobs;

/// <summary>
/// Background service that runs scheduled leave management jobs:
/// - Monthly accrual: Runs on the 1st of each month
/// - Year-end carry-over: Runs on January 1st
/// </summary>
public class LeaveScheduledJobsService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LeaveScheduledJobsService> _logger;
    
    // Check every hour
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);

    public LeaveScheduledJobsService(
        IServiceProvider serviceProvider,
        ILogger<LeaveScheduledJobsService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Leave Scheduled Jobs Service started");

        // Track last run dates to avoid running multiple times
        DateTime? lastMonthlyAccrual = null;
        DateTime? lastYearEndCarryOver = null;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                var currentYear = now.Year;
                var currentMonth = now.Month;
                var currentDay = now.Day;

                // Check if we should run monthly accrual (1st of each month)
                if (currentDay == 1)
                {
                    var accrualKey = new DateTime(currentYear, currentMonth, 1);
                    if (lastMonthlyAccrual != accrualKey)
                    {
                        await RunMonthlyAccrualAsync(currentYear, currentMonth);
                        lastMonthlyAccrual = accrualKey;
                    }
                }

                // Check if we should run year-end carry-over (January 1st)
                if (currentMonth == 1 && currentDay == 1)
                {
                    var carryOverKey = new DateTime(currentYear, 1, 1);
                    if (lastYearEndCarryOver != carryOverKey)
                    {
                        await RunYearEndCarryOverAsync(currentYear - 1, currentYear);
                        lastYearEndCarryOver = carryOverKey;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Leave Scheduled Jobs Service");
            }

            // Wait before next check
            await Task.Delay(CheckInterval, stoppingToken);
        }

        _logger.LogInformation("Leave Scheduled Jobs Service stopped");
    }

    private async Task RunMonthlyAccrualAsync(int year, int month)
    {
        _logger.LogInformation("Running monthly accrual for {Year}-{Month}", year, month);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var leaveService = scope.ServiceProvider.GetRequiredService<ILeaveService>();

            // First, ensure balances are initialized for this year
            await leaveService.InitializeYearlyBalancesAsync(year);

            // Then process monthly accrual
            await leaveService.ProcessMonthlyAccrualAsync(year, month);

            _logger.LogInformation("Monthly accrual completed for {Year}-{Month}", year, month);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run monthly accrual for {Year}-{Month}", year, month);
        }
    }

    private async Task RunYearEndCarryOverAsync(int fromYear, int toYear)
    {
        _logger.LogInformation("Running year-end carry-over from {FromYear} to {ToYear}", fromYear, toYear);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var leaveService = scope.ServiceProvider.GetRequiredService<ILeaveService>();

            // Process carry-over before initializing new year balances
            await leaveService.ProcessYearEndCarryOverAsync(fromYear, toYear);

            // Initialize new year balances
            await leaveService.InitializeYearlyBalancesAsync(toYear);

            _logger.LogInformation("Year-end carry-over completed from {FromYear} to {ToYear}", fromYear, toYear);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run year-end carry-over from {FromYear} to {ToYear}", fromYear, toYear);
        }
    }
}
