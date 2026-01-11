using LaoHR.Bridge.Service.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace LaoHR.Bridge.Service;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private ZkDevice? _device;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ZKTeco Bridge Worker starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                bool isEnabled = false;
                string deviceIp = "192.168.1.201";
                int devicePort = 4370;

                using (var scope = _serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
                    var settingEnabled = await db.SystemSettings.FindAsync("ZKTECO_ENABLED");
                    if (settingEnabled != null && bool.TryParse(settingEnabled.SettingValue, out bool enabled))
                        isEnabled = enabled;

                    var settingIp = await db.SystemSettings.FindAsync("ZK_DEVICE_IP");
                    if (settingIp != null) deviceIp = settingIp.SettingValue;

                    var settingPort = await db.SystemSettings.FindAsync("ZK_DEVICE_PORT");
                    if (settingPort != null && int.TryParse(settingPort.SettingValue, out int p)) devicePort = p;
                }

                if (!isEnabled)
                {
                    if (_device != null && _device.IsConnected)
                    {
                        _logger.LogInformation("ZKTeco disabled in settings. Disconnecting...");
                        _device.Dispose();
                        _device = null;
                    }
                    
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    continue;
                }

                // Enabled
                if (_device == null)
                {
                    _logger.LogInformation($"Initializing ZKTeco Device at {deviceIp}:{devicePort}");
                    _device = new ZkDevice("MainDevice", deviceIp, devicePort);
                    _device.OnAttendance += HandleAttendanceEvents;
                    _device.OnDisconnected += (d) => _logger.LogWarning($"ZKTeco Device disconnected!");
                }

                if (!_device.IsConnected)
                {
                    _logger.LogInformation("Attempting to connect to ZKTeco Device...");
                    if (_device.Connect())
                    {
                        _logger.LogInformation($"Connected to ZKTeco S/N: {_device.SerialNumber}");
                        // Sync users on connect
                        await SyncEmployeesFromDevice();
                    }
                    else
                    {
                        _logger.LogWarning("Failed to connect. Retrying in 30s...");
                        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                        continue;
                    }
                }
                else
                {
                    // Periodic Sync (e.g., every 1 hour)
                    if (DateTime.UtcNow - _lastSync > TimeSpan.FromHours(1))
                    {
                        await SyncEmployeesFromDevice();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in worker loop");
            }
            
            // Healthy pulse
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
    
    private DateTime _lastSync = DateTime.MinValue;

    private async Task SyncEmployeesFromDevice()
    {
        if (_device == null || !_device.IsConnected) return;
        
        try
        {
            _logger.LogInformation("Syncing employees from device...");
            var users = _device.GetAllUsers();
            if (users.Count == 0) return;

            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
                int added = 0;

                foreach (var u in users)
                {
                    var emp = await db.Employees.FirstOrDefaultAsync(e => e.EmployeeCode == u.EnrollNumber);
                    if (emp == null)
                    {
                        // New Employee
                        var newEmp = new Employee
                        {
                            EmployeeCode = u.EnrollNumber,
                            LaoName = string.IsNullOrWhiteSpace(u.Name) ? $"Emp-{u.EnrollNumber}" : u.Name,
                            EnglishName = u.Name,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = u.Enabled,
                            DepartmentId = 1 // Default to Admin or first department
                        };
                        db.Employees.Add(newEmp);
                        added++;
                    }
                    // Optional: Update existing names? Better not overwrite HR data.
                }

                if (added > 0)
                {
                    await db.SaveChangesAsync();
                    _logger.LogInformation($"✅ Auto-added {added} employees from device.");
                }
            }
            _lastSync = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error syncing employees: {ex.Message}");
        }
    }

    private async void HandleAttendanceEvents(ZkDevice device, AttendanceEventArgs args)
    {
        _logger.LogInformation($"Real-time Attendance: ID={args.EnrollNumber} Time={args.EventTime}");

        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
                
                // Find employee by code (Assuming EnrollNumber == EmployeeCode)
                var employee = await context.Employees.FirstOrDefaultAsync(e => e.EmployeeCode == args.EnrollNumber);
                
                if (employee != null)
                {
                   var date = args.EventTime.Date;
                   var existingAtt = await context.Attendances
                       .FirstOrDefaultAsync(a => a.EmployeeId == employee.EmployeeId && a.AttendanceDate == date);

                   if (existingAtt == null)
                   {
                       // Create new record (ClockIn)
                       var att = new Attendance
                       {
                           EmployeeId = employee.EmployeeId,
                           AttendanceDate = date,
                           ClockIn = args.EventTime,
                           Status = "Present"
                       };
                       context.Attendances.Add(att);
                       await context.SaveChangesAsync();
                       _logger.LogInformation($"Recorded ClockIn for {employee.LaoName}");
                   }
                   else
                   {
                       // Update existing record (ClockOut) if it's later than existing ClockIn
                       if (existingAtt.ClockIn.HasValue && (args.EventTime - existingAtt.ClockIn.Value).TotalMinutes > 60)
                       {
                           // Assume it's ClockOut
                           existingAtt.ClockOut = args.EventTime;
                           context.Attendances.Update(existingAtt);
                           await context.SaveChangesAsync();
                           _logger.LogInformation($"Recorded ClockOut for {employee.LaoName}");
                       }
                   }
                }
                else
                {
                    _logger.LogWarning($"Unknown Employee Code: {args.EnrollNumber}");
                }
            }
        }
        catch (Exception ex)
        {
             _logger.LogError($"Error saving attendance: {ex.Message}");
        }
    }

    public override void Dispose()
    {
        _device?.Dispose();
        base.Dispose();
    }
}
