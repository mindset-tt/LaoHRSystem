/*
================================================================================
    LAO HR MANAGEMENT SYSTEM - DATABASE SCHEMA
    SQL Server DDL Script (Production-Ready)
================================================================================
    
    PURPOSE:
    Complete database schema for Lao HR Management System with full compliance
    for NSSF (Social Security) and PIT (Personal Income Tax) regulations.
    
    COMPLIANCE NOTES:
    - NSSF Ceiling Base: 4,500,000 LAK (stored in SystemSettings for flexibility)
    - Employee NSSF Contribution: 5.5%
    - Employer NSSF Contribution: 6.0%
    - PIT: Progressive Tax Brackets (Lao standard rates)
    
    CURRENCY HANDLING:
    All monetary values use DECIMAL(18,2) to avoid floating-point errors.
    
    CREATED: 2026-01-10
    VERSION: 1.0
================================================================================
*/

-- ============================================================================
-- SECTION 1: DATABASE CREATION (Optional - uncomment if needed)
-- ============================================================================
/*
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'LaoHRSystem')
BEGIN
    CREATE DATABASE [LaoHRSystem];
END
GO

USE [LaoHRSystem];
GO
*/

-- ============================================================================
-- SECTION 2: DROP EXISTING TABLES (For clean re-deployment)
-- ============================================================================
-- Drop in reverse order of dependencies (child tables first)

IF OBJECT_ID('dbo.SalarySlips', 'U') IS NOT NULL DROP TABLE dbo.SalarySlips;
IF OBJECT_ID('dbo.Attendance', 'U') IS NOT NULL DROP TABLE dbo.Attendance;
IF OBJECT_ID('dbo.PayrollPeriods', 'U') IS NOT NULL DROP TABLE dbo.PayrollPeriods;
IF OBJECT_ID('dbo.Employees', 'U') IS NOT NULL DROP TABLE dbo.Employees;
IF OBJECT_ID('dbo.Departments', 'U') IS NOT NULL DROP TABLE dbo.Departments;
IF OBJECT_ID('dbo.TaxBrackets', 'U') IS NOT NULL DROP TABLE dbo.TaxBrackets;
IF OBJECT_ID('dbo.SystemSettings', 'U') IS NOT NULL DROP TABLE dbo.SystemSettings;
GO

-- ============================================================================
-- SECTION 3: SYSTEM SETTINGS TABLE
-- ============================================================================
-- Purpose: Stores configurable system parameters like NSSF rates and ceiling
-- This allows updating rates without modifying application code

CREATE TABLE dbo.SystemSettings (
    SettingId           INT IDENTITY(1,1) PRIMARY KEY,
    SettingKey          NVARCHAR(100) NOT NULL UNIQUE,
    SettingValue        NVARCHAR(500) NOT NULL,
    SettingDescription  NVARCHAR(500) NULL,
    EffectiveDate       DATE NOT NULL DEFAULT GETDATE(),
    CreatedAt           DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt           DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
GO

-- Insert NSSF and other system settings
INSERT INTO dbo.SystemSettings (SettingKey, SettingValue, SettingDescription, EffectiveDate)
VALUES 
    ('NSSF_CEILING_BASE', '4500000', 'Maximum salary base for NSSF calculation (LAK)', '2026-01-01'),
    ('NSSF_EMPLOYEE_RATE', '0.055', 'Employee NSSF contribution rate (5.5%)', '2026-01-01'),
    ('NSSF_EMPLOYER_RATE', '0.060', 'Employer NSSF contribution rate (6.0%)', '2026-01-01'),
    ('CURRENCY_CODE', 'LAK', 'Primary currency code', '2026-01-01'),
    ('WORK_HOURS_PER_DAY', '8', 'Standard work hours per day', '2026-01-01'),
    ('OVERTIME_RATE', '1.5', 'Overtime pay multiplier', '2026-01-01');
GO

-- ============================================================================
-- SECTION 4: TAX BRACKETS TABLE (Progressive PIT Rates)
-- ============================================================================
-- Purpose: Stores Lao Personal Income Tax (PIT) progressive tax brackets
-- Relationship: Referenced by payroll calculation logic
-- Note: MinAmount is inclusive, MaxAmount is exclusive (except for last bracket)

CREATE TABLE dbo.TaxBrackets (
    BracketId       INT IDENTITY(1,1) PRIMARY KEY,
    MinAmount       DECIMAL(18,2) NOT NULL,                 -- Minimum taxable income (LAK)
    MaxAmount       DECIMAL(18,2) NULL,                     -- Maximum taxable income (NULL = unlimited)
    TaxRate         DECIMAL(5,4) NOT NULL,                  -- Tax rate as decimal (e.g., 0.05 = 5%)
    Description     NVARCHAR(200) NULL,                     -- Human-readable description
    EffectiveFrom   DATE NOT NULL DEFAULT GETDATE(),        -- When this bracket becomes effective
    EffectiveTo     DATE NULL,                              -- When this bracket expires (NULL = active)
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt       DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    
    -- Ensure rates are valid percentages
    CONSTRAINT CHK_TaxBrackets_TaxRate CHECK (TaxRate >= 0 AND TaxRate <= 1),
    -- Ensure min is less than max
    CONSTRAINT CHK_TaxBrackets_Range CHECK (MaxAmount IS NULL OR MinAmount < MaxAmount)
);
GO

-- Create index for efficient bracket lookup
CREATE INDEX IX_TaxBrackets_Amount ON dbo.TaxBrackets (MinAmount, MaxAmount)
WHERE EffectiveTo IS NULL;
GO

-- ============================================================================
-- SEED DATA: Lao Progressive Tax Brackets (Standard Rates)
-- ============================================================================
-- Based on Lao PDR Personal Income Tax rates:
-- 0 - 1,300,000 LAK: 0%
-- 1,300,001 - 5,000,000 LAK: 5%
-- 5,000,001 - 15,000,000 LAK: 10%
-- 15,000,001 - 25,000,000 LAK: 15%
-- 25,000,001 - 65,000,000 LAK: 20%
-- Above 65,000,000 LAK: 25%

INSERT INTO dbo.TaxBrackets (MinAmount, MaxAmount, TaxRate, Description, EffectiveFrom)
VALUES 
    (0, 1300000, 0.0000, 'Tax exempt (0 - 1.3M LAK)', '2026-01-01'),
    (1300000, 5000000, 0.0500, 'Rate 5% (1.3M - 5M LAK)', '2026-01-01'),
    (5000000, 15000000, 0.1000, 'Rate 10% (5M - 15M LAK)', '2026-01-01'),
    (15000000, 25000000, 0.1500, 'Rate 15% (15M - 25M LAK)', '2026-01-01'),
    (25000000, 65000000, 0.2000, 'Rate 20% (25M - 65M LAK)', '2026-01-01'),
    (65000000, NULL, 0.2500, 'Rate 25% (Above 65M LAK)', '2026-01-01');
GO

-- ============================================================================
-- SECTION 5: DEPARTMENTS TABLE
-- ============================================================================
-- Purpose: Stores company departments for organizational structure
-- Relationship: One Department -> Many Employees

CREATE TABLE dbo.Departments (
    DepartmentId    INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentCode  NVARCHAR(20) NOT NULL UNIQUE,           -- Short code (e.g., 'HR', 'FIN')
    DepartmentName  NVARCHAR(200) NOT NULL,                 -- Full department name
    ManagerId       INT NULL,                               -- Reference to Employees (added later via ALTER)
    IsActive        BIT NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt       DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
GO

-- Seed some common departments
INSERT INTO dbo.Departments (DepartmentCode, DepartmentName)
VALUES 
    ('HR', N'Human Resources'),
    ('FIN', N'Finance & Accounting'),
    ('IT', N'Information Technology'),
    ('OPS', N'Operations'),
    ('SALES', N'Sales & Marketing');
GO

-- ============================================================================
-- SECTION 6: EMPLOYEES TABLE
-- ============================================================================
-- Purpose: Core employee master data including personal and employment info
-- Relationships:
--   - Many Employees -> One Department (FK to Departments)
--   - One Employee -> Many Attendance records
--   - One Employee -> Many SalarySlips

CREATE TABLE dbo.Employees (
    EmployeeId          INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeCode        NVARCHAR(20) NOT NULL UNIQUE,       -- Company employee ID (e.g., 'EMP001')
    
    -- Personal Information
    LaoName             NVARCHAR(200) NOT NULL,             -- Full name in Lao script (ພາສາລາວ)
    EnglishName         NVARCHAR(200) NOT NULL,             -- Full name in English
    DateOfBirth         DATE NULL,
    Gender              NCHAR(1) NULL,                      -- 'M' = Male, 'F' = Female
    NationalId          NVARCHAR(50) NULL,                  -- Lao National ID number
    PhoneNumber         NVARCHAR(20) NULL,
    Email               NVARCHAR(200) NULL,
    Address             NVARCHAR(500) NULL,
    
    -- Government/Social Security
    NSSF_ID             NVARCHAR(50) NULL,                  -- National Social Security Fund ID
    TaxId               NVARCHAR(50) NULL,                  -- Personal Tax Identification Number
    
    -- Banking Information (for salary payment)
    BankName            NVARCHAR(200) NULL,                 -- e.g., 'BCEL', 'LDB', 'JDB'
    BankAccountNumber   NVARCHAR(50) NULL,                  -- Bank account number
    BankAccountName     NVARCHAR(200) NULL,                 -- Name as registered with bank
    
    -- Employment Information
    DepartmentId        INT NULL,                           -- FK to Departments
    JobTitle            NVARCHAR(200) NULL,
    EmploymentType      NVARCHAR(50) NOT NULL DEFAULT 'FULL_TIME', -- FULL_TIME, PART_TIME, CONTRACT
    HireDate            DATE NOT NULL,
    TerminationDate     DATE NULL,                          -- NULL = currently employed
    
    -- Salary Information
    BaseSalary          DECIMAL(18,2) NOT NULL DEFAULT 0,   -- Monthly base salary in LAK
    
    -- Status
    IsActive            BIT NOT NULL DEFAULT 1,
    
    -- Audit Fields
    CreatedAt           DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt           DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CreatedBy           NVARCHAR(100) NULL,
    UpdatedBy           NVARCHAR(100) NULL,
    
    -- Constraints
    CONSTRAINT FK_Employees_Department FOREIGN KEY (DepartmentId) 
        REFERENCES dbo.Departments(DepartmentId),
    CONSTRAINT CHK_Employees_Gender CHECK (Gender IN ('M', 'F') OR Gender IS NULL),
    CONSTRAINT CHK_Employees_EmploymentType CHECK (EmploymentType IN ('FULL_TIME', 'PART_TIME', 'CONTRACT')),
    CONSTRAINT CHK_Employees_BaseSalary CHECK (BaseSalary >= 0)
);
GO

-- Indexes for common queries
CREATE INDEX IX_Employees_DepartmentId ON dbo.Employees(DepartmentId);
CREATE INDEX IX_Employees_NSSF_ID ON dbo.Employees(NSSF_ID) WHERE NSSF_ID IS NOT NULL;
CREATE INDEX IX_Employees_IsActive ON dbo.Employees(IsActive);
GO

-- Add self-referencing FK for Department Manager (after Employees table exists)
ALTER TABLE dbo.Departments
ADD CONSTRAINT FK_Departments_Manager 
    FOREIGN KEY (ManagerId) REFERENCES dbo.Employees(EmployeeId);
GO

-- ============================================================================
-- SECTION 7: PAYROLL PERIODS TABLE
-- ============================================================================
-- Purpose: Defines payroll processing periods (usually monthly)
-- Relationship: One PayrollPeriod -> Many SalarySlips

CREATE TABLE dbo.PayrollPeriods (
    PeriodId        INT IDENTITY(1,1) PRIMARY KEY,
    PeriodYear      INT NOT NULL,                           -- e.g., 2026
    PeriodMonth     INT NOT NULL,                           -- 1-12
    PeriodName      NVARCHAR(50) NOT NULL,                  -- e.g., 'January 2026'
    StartDate       DATE NOT NULL,                          -- First day of period
    EndDate         DATE NOT NULL,                          -- Last day of period
    PaymentDate     DATE NULL,                              -- Scheduled payment date
    
    -- Processing Status
    Status          NVARCHAR(20) NOT NULL DEFAULT 'DRAFT', -- DRAFT, PROCESSING, APPROVED, PAID, CLOSED
    ProcessedAt     DATETIME2 NULL,                         -- When payroll was calculated
    ApprovedAt      DATETIME2 NULL,                         -- When payroll was approved
    PaidAt          DATETIME2 NULL,                         -- When payments were made
    ApprovedBy      NVARCHAR(100) NULL,
    
    -- Audit Fields
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt       DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    
    -- Constraints
    CONSTRAINT UQ_PayrollPeriods_YearMonth UNIQUE (PeriodYear, PeriodMonth),
    CONSTRAINT CHK_PayrollPeriods_Month CHECK (PeriodMonth BETWEEN 1 AND 12),
    CONSTRAINT CHK_PayrollPeriods_Year CHECK (PeriodYear BETWEEN 2000 AND 2100),
    CONSTRAINT CHK_PayrollPeriods_DateRange CHECK (StartDate <= EndDate),
    CONSTRAINT CHK_PayrollPeriods_Status CHECK (Status IN ('DRAFT', 'PROCESSING', 'APPROVED', 'PAID', 'CLOSED'))
);
GO

-- Index for period lookups
CREATE INDEX IX_PayrollPeriods_YearMonth ON dbo.PayrollPeriods(PeriodYear, PeriodMonth);
CREATE INDEX IX_PayrollPeriods_Status ON dbo.PayrollPeriods(Status);
GO

-- ============================================================================
-- SECTION 8: SALARY SLIPS TABLE
-- ============================================================================
-- Purpose: Stores calculated salary details for each employee per payroll period
-- Relationships:
--   - Many SalarySlips -> One Employee (FK to Employees)
--   - Many SalarySlips -> One PayrollPeriod (FK to PayrollPeriods)
-- 
-- CALCULATION NOTES:
--   NetSalary = BaseSalary + OvertimePay + Allowances 
--               - NSSF_EmployeeDeduction - TaxDeduction - OtherDeductions

CREATE TABLE dbo.SalarySlips (
    SlipId                      INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId                  INT NOT NULL,               -- FK to Employees
    PeriodId                    INT NOT NULL,               -- FK to PayrollPeriods
    
    -- Earnings (All amounts in LAK)
    BaseSalary                  DECIMAL(18,2) NOT NULL DEFAULT 0,   -- Monthly base salary
    OvertimeHours               DECIMAL(8,2) NOT NULL DEFAULT 0,    -- Total overtime hours worked
    OvertimeRate                DECIMAL(5,2) NOT NULL DEFAULT 1.5,  -- Overtime multiplier
    OvertimePay                 DECIMAL(18,2) NOT NULL DEFAULT 0,   -- Calculated overtime payment
    Allowances                  DECIMAL(18,2) NOT NULL DEFAULT 0,   -- Total allowances
    Bonus                       DECIMAL(18,2) NOT NULL DEFAULT 0,   -- Any bonus payments
    GrossIncome                 DECIMAL(18,2) NOT NULL DEFAULT 0,   -- Total before deductions
    
    -- NSSF Calculations
    NSSF_Base                   DECIMAL(18,2) NOT NULL DEFAULT 0,   -- Salary base for NSSF (capped at ceiling)
    NSSF_EmployeeDeduction      DECIMAL(18,2) NOT NULL DEFAULT 0,   -- Employee portion (5.5%)
    NSSF_EmployerContribution   DECIMAL(18,2) NOT NULL DEFAULT 0,   -- Employer portion (6.0%)
    
    -- Tax Calculations
    TaxableIncome               DECIMAL(18,2) NOT NULL DEFAULT 0,   -- Income subject to PIT
    TaxDeduction                DECIMAL(18,2) NOT NULL DEFAULT 0,   -- Personal Income Tax amount
    
    -- Other Deductions
    OtherDeductions             DECIMAL(18,2) NOT NULL DEFAULT 0,   -- Other deductions (loans, etc.)
    DeductionNotes              NVARCHAR(500) NULL,                 -- Explanation of other deductions
    
    -- Final Amounts
    TotalDeductions             DECIMAL(18,2) NOT NULL DEFAULT 0,   -- Sum of all deductions
    NetSalary                   DECIMAL(18,2) NOT NULL DEFAULT 0,   -- Final take-home pay
    
    -- Payment Information
    PaymentMethod               NVARCHAR(50) NOT NULL DEFAULT 'BANK_TRANSFER', -- BANK_TRANSFER, CASH, CHECK
    PaymentReference            NVARCHAR(100) NULL,                 -- Bank transfer reference
    PaidAt                      DATETIME2 NULL,                     -- Actual payment timestamp
    
    -- Status
    Status                      NVARCHAR(20) NOT NULL DEFAULT 'DRAFT', -- DRAFT, CALCULATED, APPROVED, PAID
    
    -- Audit Fields
    CreatedAt                   DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt                   DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CalculatedBy                NVARCHAR(100) NULL,
    ApprovedBy                  NVARCHAR(100) NULL,
    
    -- Constraints
    CONSTRAINT FK_SalarySlips_Employee FOREIGN KEY (EmployeeId) 
        REFERENCES dbo.Employees(EmployeeId),
    CONSTRAINT FK_SalarySlips_Period FOREIGN KEY (PeriodId) 
        REFERENCES dbo.PayrollPeriods(PeriodId),
    CONSTRAINT UQ_SalarySlips_EmployeePeriod UNIQUE (EmployeeId, PeriodId),
    CONSTRAINT CHK_SalarySlips_Status CHECK (Status IN ('DRAFT', 'CALCULATED', 'APPROVED', 'PAID')),
    CONSTRAINT CHK_SalarySlips_PaymentMethod CHECK (PaymentMethod IN ('BANK_TRANSFER', 'CASH', 'CHECK')),
    CONSTRAINT CHK_SalarySlips_Amounts CHECK (
        BaseSalary >= 0 AND 
        OvertimePay >= 0 AND 
        NSSF_EmployeeDeduction >= 0 AND 
        TaxDeduction >= 0 AND 
        GrossIncome >= 0
    )
);
GO

-- Indexes for common queries
CREATE INDEX IX_SalarySlips_EmployeeId ON dbo.SalarySlips(EmployeeId);
CREATE INDEX IX_SalarySlips_PeriodId ON dbo.SalarySlips(PeriodId);
CREATE INDEX IX_SalarySlips_Status ON dbo.SalarySlips(Status);
GO

-- ============================================================================
-- SECTION 9: ATTENDANCE TABLE
-- ============================================================================
-- Purpose: Tracks daily attendance with clock in/out times and geolocation
-- Relationship: Many Attendance records -> One Employee
-- 
-- GEOLOCATION NOTES:
--   - Latitude range: -90 to +90 degrees
--   - Longitude range: -180 to +180 degrees
--   - Precision of DECIMAL(9,6) gives ~0.1m accuracy

CREATE TABLE dbo.Attendance (
    AttendanceId        INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId          INT NOT NULL,                       -- FK to Employees
    AttendanceDate      DATE NOT NULL,                      -- The calendar date
    
    -- Clock In Details
    ClockIn             DATETIME2 NULL,                     -- Clock in timestamp
    ClockInLatitude     DECIMAL(9,6) NULL,                  -- Geolocation latitude (-90 to +90)
    ClockInLongitude    DECIMAL(10,6) NULL,                 -- Geolocation longitude (-180 to +180)
    ClockInMethod       NVARCHAR(50) NULL,                  -- BIOMETRIC, MOBILE_APP, WEB, MANUAL
    ClockInDeviceId     NVARCHAR(100) NULL,                 -- Device/terminal identifier
    ClockInNote         NVARCHAR(500) NULL,                 -- Any notes for clock in
    
    -- Clock Out Details
    ClockOut            DATETIME2 NULL,                     -- Clock out timestamp
    ClockOutLatitude    DECIMAL(9,6) NULL,                  -- Geolocation latitude
    ClockOutLongitude   DECIMAL(10,6) NULL,                 -- Geolocation longitude
    ClockOutMethod      NVARCHAR(50) NULL,                  -- BIOMETRIC, MOBILE_APP, WEB, MANUAL
    ClockOutDeviceId    NVARCHAR(100) NULL,                 -- Device/terminal identifier
    ClockOutNote        NVARCHAR(500) NULL,                 -- Any notes for clock out
    
    -- Calculated Work Time
    WorkHours           DECIMAL(5,2) NULL,                  -- Total hours worked (calculated)
    OvertimeHours       DECIMAL(5,2) NULL,                  -- Overtime hours (if > standard hours)
    BreakMinutes        INT NULL DEFAULT 60,                -- Break time in minutes
    
    -- Status & Flags
    AttendanceStatus    NVARCHAR(20) NOT NULL DEFAULT 'PRESENT', -- PRESENT, ABSENT, LATE, HALF_DAY, LEAVE
    IsLate              BIT NOT NULL DEFAULT 0,             -- True if clocked in late
    IsEarlyLeave        BIT NOT NULL DEFAULT 0,             -- True if clocked out early
    IsApproved          BIT NOT NULL DEFAULT 0,             -- Manager approval status
    ApprovedBy          NVARCHAR(100) NULL,
    ApprovedAt          DATETIME2 NULL,
    
    -- Audit Fields
    CreatedAt           DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt           DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    
    -- Constraints
    CONSTRAINT FK_Attendance_Employee FOREIGN KEY (EmployeeId) 
        REFERENCES dbo.Employees(EmployeeId),
    CONSTRAINT UQ_Attendance_EmployeeDate UNIQUE (EmployeeId, AttendanceDate),
    CONSTRAINT CHK_Attendance_ClockTimes CHECK (ClockOut IS NULL OR ClockIn <= ClockOut),
    CONSTRAINT CHK_Attendance_Latitude CHECK (
        (ClockInLatitude IS NULL OR (ClockInLatitude >= -90 AND ClockInLatitude <= 90)) AND
        (ClockOutLatitude IS NULL OR (ClockOutLatitude >= -90 AND ClockOutLatitude <= 90))
    ),
    CONSTRAINT CHK_Attendance_Longitude CHECK (
        (ClockInLongitude IS NULL OR (ClockInLongitude >= -180 AND ClockInLongitude <= 180)) AND
        (ClockOutLongitude IS NULL OR (ClockOutLongitude >= -180 AND ClockOutLongitude <= 180))
    ),
    CONSTRAINT CHK_Attendance_Status CHECK (AttendanceStatus IN ('PRESENT', 'ABSENT', 'LATE', 'HALF_DAY', 'LEAVE')),
    CONSTRAINT CHK_Attendance_Method CHECK (
        (ClockInMethod IS NULL OR ClockInMethod IN ('BIOMETRIC', 'MOBILE_APP', 'WEB', 'MANUAL')) AND
        (ClockOutMethod IS NULL OR ClockOutMethod IN ('BIOMETRIC', 'MOBILE_APP', 'WEB', 'MANUAL'))
    )
);
GO

-- Indexes for efficient queries
CREATE INDEX IX_Attendance_EmployeeId ON dbo.Attendance(EmployeeId);
CREATE INDEX IX_Attendance_Date ON dbo.Attendance(AttendanceDate);
CREATE INDEX IX_Attendance_EmployeeDate ON dbo.Attendance(EmployeeId, AttendanceDate);
CREATE INDEX IX_Attendance_Status ON dbo.Attendance(AttendanceStatus);
GO

-- ============================================================================
-- SECTION 10: HELPER VIEWS
-- ============================================================================

-- View: Active employees with department info
CREATE OR ALTER VIEW dbo.vw_ActiveEmployees
AS
SELECT 
    e.EmployeeId,
    e.EmployeeCode,
    e.LaoName,
    e.EnglishName,
    e.NSSF_ID,
    e.BankAccountNumber,
    e.BankName,
    e.BaseSalary,
    e.JobTitle,
    e.HireDate,
    d.DepartmentCode,
    d.DepartmentName
FROM dbo.Employees e
LEFT JOIN dbo.Departments d ON e.DepartmentId = d.DepartmentId
WHERE e.IsActive = 1;
GO

-- View: Current tax brackets
CREATE OR ALTER VIEW dbo.vw_CurrentTaxBrackets
AS
SELECT 
    BracketId,
    MinAmount,
    MaxAmount,
    TaxRate,
    Description,
    CAST(TaxRate * 100 AS DECIMAL(5,2)) AS TaxRatePercent
FROM dbo.TaxBrackets
WHERE EffectiveTo IS NULL
    AND EffectiveFrom <= GETDATE();
GO

-- ============================================================================
-- SECTION 11: STORED PROCEDURES
-- ============================================================================

-- Procedure: Calculate NSSF deduction for an employee
CREATE OR ALTER PROCEDURE dbo.sp_CalculateNSSF
    @BaseSalary DECIMAL(18,2),
    @EmployeeDeduction DECIMAL(18,2) OUTPUT,
    @EmployerContribution DECIMAL(18,2) OUTPUT,
    @NSSFBase DECIMAL(18,2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CeilingBase DECIMAL(18,2);
    DECLARE @EmployeeRate DECIMAL(5,4);
    DECLARE @EmployerRate DECIMAL(5,4);
    
    -- Get current NSSF settings
    SELECT @CeilingBase = CAST(SettingValue AS DECIMAL(18,2))
    FROM dbo.SystemSettings WHERE SettingKey = 'NSSF_CEILING_BASE';
    
    SELECT @EmployeeRate = CAST(SettingValue AS DECIMAL(5,4))
    FROM dbo.SystemSettings WHERE SettingKey = 'NSSF_EMPLOYEE_RATE';
    
    SELECT @EmployerRate = CAST(SettingValue AS DECIMAL(5,4))
    FROM dbo.SystemSettings WHERE SettingKey = 'NSSF_EMPLOYER_RATE';
    
    -- Apply ceiling
    SET @NSSFBase = CASE 
        WHEN @BaseSalary > @CeilingBase THEN @CeilingBase 
        ELSE @BaseSalary 
    END;
    
    -- Calculate contributions
    SET @EmployeeDeduction = ROUND(@NSSFBase * @EmployeeRate, 2);
    SET @EmployerContribution = ROUND(@NSSFBase * @EmployerRate, 2);
END;
GO

-- Procedure: Calculate progressive tax
CREATE OR ALTER PROCEDURE dbo.sp_CalculateProgressiveTax
    @TaxableIncome DECIMAL(18,2),
    @TaxAmount DECIMAL(18,2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    SET @TaxAmount = 0;
    
    DECLARE @RemainingIncome DECIMAL(18,2) = @TaxableIncome;
    DECLARE @PrevMax DECIMAL(18,2) = 0;
    
    -- Calculate tax using progressive brackets
    SELECT @TaxAmount = @TaxAmount + 
        CASE 
            WHEN @TaxableIncome >= ISNULL(MaxAmount, @TaxableIncome + 1) 
                THEN (ISNULL(MaxAmount, @TaxableIncome) - MinAmount) * TaxRate
            WHEN @TaxableIncome > MinAmount 
                THEN (@TaxableIncome - MinAmount) * TaxRate
            ELSE 0
        END
    FROM dbo.TaxBrackets
    WHERE EffectiveTo IS NULL
        AND EffectiveFrom <= GETDATE()
        AND MinAmount < @TaxableIncome
    ORDER BY MinAmount;
    
    SET @TaxAmount = ROUND(@TaxAmount, 2);
END;
GO

-- ============================================================================
-- SECTION 12: SAMPLE DATA (For Testing)
-- ============================================================================

-- Insert sample employees
INSERT INTO dbo.Employees (
    EmployeeCode, LaoName, EnglishName, Gender, NSSF_ID, 
    BankName, BankAccountNumber, BankAccountName,
    DepartmentId, JobTitle, HireDate, BaseSalary
)
VALUES 
    ('EMP001', N'ທ້າວ ສົມພອນ ແກ້ວມະນີ', 'Mr. Somphon Keomanee', 'M', 'NSSF-001-2026',
     'BCEL', '010-12-00-00012345-001', 'SOMPHON KEOMANEE',
     1, 'HR Manager', '2020-01-15', 8000000),
    ('EMP002', N'ນາງ ມະນີວັນ ພູມມະວົງ', 'Ms. Manivanh Phoummavong', 'F', 'NSSF-002-2026',
     'LDB', '212-10-00-00098765-001', 'MANIVANH PHOUMMAVONG',
     2, 'Senior Accountant', '2019-06-01', 6500000),
    ('EMP003', N'ທ້າວ ວິໄລພອນ ສີສຸວັນ', 'Mr. Vilayphon Sisouvanh', 'M', 'NSSF-003-2026',
     'BCEL', '010-12-00-00054321-001', 'VILAYPHON SISOUVANH',
     3, 'IT Developer', '2021-03-10', 5500000);
GO

-- Insert sample payroll period
INSERT INTO dbo.PayrollPeriods (
    PeriodYear, PeriodMonth, PeriodName, StartDate, EndDate, PaymentDate, Status
)
VALUES 
    (2026, 1, 'January 2026', '2026-01-01', '2026-01-31', '2026-01-31', 'DRAFT');
GO

-- Insert sample attendance
INSERT INTO dbo.Attendance (
    EmployeeId, AttendanceDate, 
    ClockIn, ClockInLatitude, ClockInLongitude, ClockInMethod,
    ClockOut, ClockOutLatitude, ClockOutLongitude, ClockOutMethod,
    WorkHours, AttendanceStatus
)
VALUES 
    (1, '2026-01-10', 
     '2026-01-10 08:30:00', 17.9757, 102.6331, 'BIOMETRIC',
     '2026-01-10 17:30:00', 17.9757, 102.6331, 'BIOMETRIC',
     8.00, 'PRESENT'),
    (2, '2026-01-10', 
     '2026-01-10 08:45:00', 17.9640, 102.6133, 'MOBILE_APP',
     '2026-01-10 17:00:00', 17.9640, 102.6133, 'MOBILE_APP',
     7.25, 'PRESENT');
GO

-- ============================================================================
-- SCHEMA DOCUMENTATION
-- ============================================================================
/*
TABLE RELATIONSHIPS DIAGRAM:

    ┌─────────────────┐
    │ SystemSettings  │  (Configuration: NSSF rates, etc.)
    └─────────────────┘

    ┌─────────────────┐
    │  TaxBrackets    │  (Lao PIT progressive rates)
    └─────────────────┘

    ┌─────────────────┐
    │  Departments    │
    └────────┬────────┘
             │ 1:N
             ▼
    ┌─────────────────┐
    │   Employees     │ ─────────────┐
    └────────┬────────┘              │
             │                       │
    ┌────────┴────────┐              │
    │ 1:N             │ 1:N          │ N:1
    ▼                 ▼              │
┌──────────┐    ┌───────────┐       │
│Attendance│    │SalarySlips│◄──────┘
└──────────┘    └─────┬─────┘
                      │ N:1
                      ▼
               ┌──────────────┐
               │PayrollPeriods│
               └──────────────┘

LEGEND:
  1:N = One-to-Many relationship
  N:1 = Many-to-One relationship

KEY COMPLIANCE FEATURES:
  1. NSSF Ceiling: Stored in SystemSettings (4,500,000 LAK)
  2. NSSF Rates: Employee 5.5%, Employer 6.0%
  3. Tax Brackets: 6 progressive levels (0% to 25%)
  4. Currency: All monetary fields use DECIMAL(18,2)
  5. Geolocation: DECIMAL(9,6) for ~0.1m accuracy
*/

PRINT '================================================================================';
PRINT 'Lao HR Management System - Database Schema Created Successfully!';
PRINT '================================================================================';
PRINT '';
PRINT 'Tables Created:';
PRINT '  - dbo.SystemSettings (NSSF rates and system config)';
PRINT '  - dbo.TaxBrackets (Lao PIT progressive rates - SEEDED)';
PRINT '  - dbo.Departments (Organizational structure)';
PRINT '  - dbo.Employees (Core employee data)';
PRINT '  - dbo.PayrollPeriods (Monthly payroll cycles)';
PRINT '  - dbo.SalarySlips (Payroll calculations)';
PRINT '  - dbo.Attendance (Clock in/out with geolocation)';
PRINT '';
PRINT 'Views Created:';
PRINT '  - dbo.vw_ActiveEmployees';
PRINT '  - dbo.vw_CurrentTaxBrackets';
PRINT '';
PRINT 'Stored Procedures Created:';
PRINT '  - dbo.sp_CalculateNSSF';
PRINT '  - dbo.sp_CalculateProgressiveTax';
PRINT '================================================================================';
GO
