using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace LaoHR.API.Data;

public static class DbSeeder
{
    public static void Seed(LaoHRDbContext context)
    {
        // Ensure database is created
        context.Database.EnsureCreated();

        // 1. Seed Departments
        if (!context.Departments.Any())
        {
            var departments = new List<Department>
            {
                new Department { DepartmentName = "Human Resources", DepartmentCode = "HR", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Department { DepartmentName = "Information Technology", DepartmentCode = "IT", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Department { DepartmentName = "Finance", DepartmentCode = "FIN", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Department { DepartmentName = "Operations", DepartmentCode = "OPS", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Department { DepartmentName = "Sales & Marketing", DepartmentCode = "MKT", IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            context.Departments.AddRange(departments);
            context.SaveChanges();
        }

        // 2. Seed Employees
        if (!context.Employees.Any())
        {
            var depts = context.Departments.ToList();
            var hrDept = depts.FirstOrDefault(d => d.DepartmentCode == "HR");
            var itDept = depts.FirstOrDefault(d => d.DepartmentCode == "IT");
            var finDept = depts.FirstOrDefault(d => d.DepartmentCode == "FIN");
            var opsDept = depts.FirstOrDefault(d => d.DepartmentCode == "OPS");

            var employees = new List<Employee>
            {
                // Admin / HR Manager
                new Employee 
                { 
                    EmployeeCode = "EMP001", 
                    LaoName = "Somsack Phomvihane", 
                    EnglishName = "Somsack Phomvihane",
                    Gender = "Male",
                    DateOfBirth = new DateTime(1985, 5, 15),
                    Phone = "020-5555-1001",
                    Email = "somsack@laohr.com", // Maps to 'admin' logic if we used email login
                    DepartmentId = hrDept?.DepartmentId,
                    JobTitle = "HR Manager",
                    HireDate = new DateTime(2020, 1, 1),
                    BaseSalary = 15000000,
                    SalaryCurrency = "LAK",
                    DependentCount = 2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                // HR Staff
                new Employee 
                { 
                    EmployeeCode = "EMP002", 
                    LaoName = "Maly Soulivong", 
                    EnglishName = "Maly Soulivong",
                    Gender = "Female",
                    DateOfBirth = new DateTime(1992, 8, 20),
                    Phone = "020-5555-1002",
                    Email = "maly@laohr.com",
                    DepartmentId = hrDept?.DepartmentId,
                    JobTitle = "HR Officer",
                    HireDate = new DateTime(2021, 3, 15),
                    BaseSalary = 8000000,
                    SalaryCurrency = "LAK",
                    DependentCount = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                // IT Manager
                new Employee 
                { 
                    EmployeeCode = "EMP003", 
                    LaoName = "Khamphay Vongsay", 
                    EnglishName = "Khamphay Vongsay",
                    Gender = "Male",
                    DateOfBirth = new DateTime(1988, 11, 10),
                    Phone = "020-5555-2001",
                    Email = "khamphay@laohr.com",
                    DepartmentId = itDept?.DepartmentId,
                    JobTitle = "IT Manager",
                    HireDate = new DateTime(2019, 6, 1),
                    BaseSalary = 25000000,
                    SalaryCurrency = "LAK",
                    DependentCount = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                // Senior Developer
                new Employee 
                { 
                    EmployeeCode = "EMP004", 
                    LaoName = "Somphone Keobounphan", 
                    EnglishName = "Somphone Keobounphan",
                    Gender = "Male",
                    DateOfBirth = new DateTime(1995, 2, 28),
                    Phone = "020-5555-2002",
                    Email = "somphone@laohr.com",
                    DepartmentId = itDept?.DepartmentId,
                    JobTitle = "Senior Developer",
                    HireDate = new DateTime(2022, 1, 10),
                    BaseSalary = 18000000,
                    SalaryCurrency = "LAK",
                    DependentCount = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                // Developer
                new Employee 
                { 
                    EmployeeCode = "EMP005", 
                    LaoName = "Davanh Inthavong", 
                    EnglishName = "Davanh Inthavong",
                    Gender = "Female",
                    DateOfBirth = new DateTime(1998, 7, 12),
                    Phone = "020-5555-2003",
                    Email = "davanh@laohr.com",
                    DepartmentId = itDept?.DepartmentId,
                    JobTitle = "Web Developer",
                    HireDate = new DateTime(2023, 5, 20),
                    BaseSalary = 12000000,
                    SalaryCurrency = "LAK",
                    DependentCount = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                // Accountant
                new Employee 
                { 
                    EmployeeCode = "EMP006", 
                    LaoName = "Bouavan Sithole", 
                    EnglishName = "Bouavan Sithole",
                    Gender = "Female",
                    DateOfBirth = new DateTime(1990, 4, 5),
                    Phone = "020-5555-3001",
                    Email = "bouavan@laohr.com",
                    DepartmentId = finDept?.DepartmentId,
                    JobTitle = "Senior Accountant",
                    HireDate = new DateTime(2018, 9, 1),
                    BaseSalary = 14000000,
                    SalaryCurrency = "LAK",
                    DependentCount = 2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                // Ops Staff
                new Employee 
                { 
                    EmployeeCode = "EMP007", 
                    LaoName = "Somsavath Lengsavad", 
                    EnglishName = "Somsavath Lengsavad",
                    Gender = "Male",
                    DateOfBirth = new DateTime(1980, 10, 30),
                    Phone = "020-5555-4001",
                    Email = "somsavath@laohr.com",
                    DepartmentId = opsDept?.DepartmentId,
                    JobTitle = "Operations Manager",
                    HireDate = new DateTime(2015, 1, 1),
                    BaseSalary = 20000000,
                    SalaryCurrency = "LAK",
                    DependentCount = 3,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };
            context.Employees.AddRange(employees);
            context.SaveChanges();
        }

        // 3. Seed Attendance (Last 30 days)
        if (!context.Attendances.Any())
        {
            var employees = context.Employees.ToList();
            var attendances = new List<Attendance>();
            var today = DateTime.UtcNow.Date;
            var random = new Random();

            foreach (var emp in employees)
            {
                for (int i = 30; i >= 0; i--)
                {
                    var date = today.AddDays(-i);
                    // Skip weekends
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                        continue;

                    // 90% chance of being present, 5% late, 5% leave/absent
                    var roll = random.Next(100);
                    
                    if (roll < 85) // Present on time
                    {
                        var clockIn = date.AddHours(8).AddMinutes(random.Next(0, 25)); // 08:00 - 08:25
                        var clockOut = date.AddHours(17).AddMinutes(random.Next(0, 45)); // 17:00 - 17:45
                        
                        attendances.Add(new Attendance
                        {
                            EmployeeId = emp.EmployeeId,
                            AttendanceDate = date,
                            ClockIn = clockIn,
                            ClockOut = clockOut,
                            ClockInMethod = "WEB",
                            ClockOutMethod = "WEB",
                            Status = "PRESENT",
                            IsLate = false,
                            IsEarlyLeave = false,
                            WorkHours = Math.Round((decimal)(clockOut - clockIn).TotalHours, 2)
                        });
                    }
                    else if (roll < 95) // Late
                    {
                        var clockIn = date.AddHours(8).AddMinutes(random.Next(35, 90)); // 08:35 - 9:30
                        var clockOut = date.AddHours(17).AddMinutes(random.Next(0, 30));
                        
                        attendances.Add(new Attendance
                        {
                            EmployeeId = emp.EmployeeId,
                            AttendanceDate = date,
                            ClockIn = clockIn,
                            ClockOut = clockOut,
                            ClockInMethod = "WEB",
                            ClockOutMethod = "WEB",
                            Status = "LATE",
                            IsLate = true,
                            IsEarlyLeave = false,
                            WorkHours = Math.Round((decimal)(clockOut - clockIn).TotalHours, 2)
                        });
                    }
                    // Else Absent/Leave (don't create record or create as Leave)
                }
            }
            context.Attendances.AddRange(attendances);
            context.SaveChanges();
        }

        // 4. Seed Leave Requests
        if (!context.LeaveRequests.Any())
        {
            var emp1 = context.Employees.FirstOrDefault();
            if (emp1 != null)
            {
                context.LeaveRequests.Add(new LeaveRequest
                {
                    EmployeeId = emp1.EmployeeId,
                    LeaveType = "ANNUAL",
                    StartDate = DateTime.UtcNow.AddDays(10),
                    EndDate = DateTime.UtcNow.AddDays(12),
                    TotalDays = 3,
                    Reason = "Family vacation planned",
                    Status = "PENDING",
                    CreatedAt = DateTime.UtcNow
                });
                
                context.LeaveRequests.Add(new LeaveRequest
                {
                    EmployeeId = emp1.EmployeeId,
                    LeaveType = "SICK",
                    StartDate = DateTime.UtcNow.AddDays(-10),
                    EndDate = DateTime.UtcNow.AddDays(-9),
                    TotalDays = 2,
                    Reason = "Flu",
                    Status = "APPROVED",
                    ApprovedById = 2, // Assuming HR
                    ApprovedAt = DateTime.UtcNow.AddDays(-8),
                    CreatedAt = DateTime.UtcNow.AddDays(-11)
                });
            }
            context.SaveChanges();
        }

        // 5. Seed Payroll Data
        if (!context.PayrollPeriods.Any())
        {
            var employees = context.Employees.ToList();
            var now = DateTime.UtcNow;
            
            // Seed 2 months: Last Month and Current Month
            for (int i = 1; i >= 0; i--) 
            {
                var periodDate = now.AddMonths(-i); // i=1 (Last Month), i=0 (Current Month)
                var year = periodDate.Year;
                var month = periodDate.Month;
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var period = new PayrollPeriod
                {
                    Year = year,
                    Month = month,
                    PeriodName = $"{startDate:MMMM yyyy} Payroll",
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = "COMPLETED", // Mark both as COMPLETED so reports work immediately
                    CreatedAt = startDate
                };
                context.PayrollPeriods.Add(period);
                context.SaveChanges();

                // Generate Slips for this period
                var slips = new List<SalarySlip>();
                foreach (var emp in employees)
                {
                    // Simple calculation logic for seeding
                    var baseSalary = emp.BaseSalary;
                    var gross = baseSalary; 
                    
                    // NSSF 5.5% 
                    var nssfBase = Math.Min(gross, 4500000);
                    var nssfEmployee = nssfBase * 0.055m;
                    var nssfEmployer = nssfBase * 0.060m;
                    
                    // Tax (Simple progressive mock)
                    var taxBase = gross - nssfEmployee;
                    var tax = 0m;
                    if (taxBase > 1300000) tax += (Math.Min(taxBase, 5000000) - 1300000) * 0.05m;
                    if (taxBase > 5000000) tax += (Math.Min(taxBase, 15000000) - 5000000) * 0.10m;
                    if (taxBase > 15000000) tax += (taxBase - 15000000) * 0.15m;
                    
                    slips.Add(new SalarySlip
                    {
                        EmployeeId = emp.EmployeeId,
                        PeriodId = period.PeriodId,
                        BaseSalary = baseSalary,
                        OvertimePay = 0,
                        Allowances = 0,
                        GrossIncome = gross,
                        NssfBase = nssfBase,
                        NssfEmployeeDeduction = nssfEmployee,
                        NssfEmployerContribution = nssfEmployer,
                        TaxableIncome = taxBase,
                        TaxDeduction = tax,
                        OtherDeductions = 0,
                        NetSalary = gross - nssfEmployee - tax,
                        Status = "PAID",
                        CreatedAt = endDate
                    });
                }
                context.SalarySlips.AddRange(slips);
                context.SaveChanges();
            }
        }
            // 6. Seed Address Data (Provinces, Districts, Villages)
        SeedAddresses(context);
    }

    private static void SeedAddresses(LaoHRDbContext context)
    {
        // Only seed if no provinces exist
        if (context.Provinces.Any())
        {
            Console.WriteLine("Address data already exists. Skipping seed.");
            return;
        }

        Console.WriteLine("Seeding Address Data from SQL Script...");
        
        try
        {
            // Path relative to LaoHR.API project execution directory
            // We need to go up from Backend/LaoHR.API to root, then to Database/AddressinLao
            var baseDir = AppContext.BaseDirectory; // This might be in bin/Debug/net...
            // Let's try finding the solution root by traversing up
            var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
            // Navigate up to find 'Database' folder
            string? sqlFilePath = null;
            
            // Search strategy: Check typical relative paths
            string[] possiblePaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "../../Database/AddressinLao/data_sqlserver.sql"), // From Backend/LaoHR.API
                Path.Combine(Directory.GetCurrentDirectory(), "../../../Database/AddressinLao/data_sqlserver.sql"),
                Path.Combine(Directory.GetCurrentDirectory(), "Database/AddressinLao/data_sqlserver.sql"),
                @"C:\Users\khamp\Documents\WorkAtRL\LaoHRSystem\Database\AddressinLao\data_sqlserver.sql" // Fallback absolute path as per user env
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    sqlFilePath = path;
                    break;
                }
            }

            if (sqlFilePath == null || !File.Exists(sqlFilePath))
            {
                Console.WriteLine("Could not find data_sqlserver.sql. Skipping address seed.");
                return;
            }

            var sqlContent = File.ReadAllText(sqlFilePath);
            
            // Split by "GO" command (case insensitive, on its own line)
            // Regex to find GO on a separate line
            var batches = System.Text.RegularExpressions.Regex.Split(sqlContent, @"^\s*GO\s*$", System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            foreach (var batch in batches)
            {
                var cmd = batch.Trim();
                if (!string.IsNullOrWhiteSpace(cmd))
                {
                    context.Database.ExecuteSqlRaw(cmd);
                }
            }
            
            Console.WriteLine("Address data seeded successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding address data: {ex.Message}");
        }
    }
}
