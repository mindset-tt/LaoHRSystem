// =============================================================================
// Type Definitions - Matching Backend Entities
// =============================================================================

// -----------------------------------------------------------------------------
// Department
// -----------------------------------------------------------------------------
export interface Department {
    departmentId: number;
    departmentName: string;
    departmentNameEn?: string;
    departmentCode?: string;
    isActive: boolean;
    createdAt: string;
}

// -----------------------------------------------------------------------------
// Employee
// -----------------------------------------------------------------------------
export interface Employee {
    employeeId: number;
    employeeCode: string;
    laoName: string;
    englishName?: string;
    nssfId?: string;
    taxId?: string;
    dateOfBirth?: string;
    gender?: string;
    phone?: string;
    email?: string;
    dependentCount: number;
    salaryCurrency: 'LAK' | 'USD' | 'THB';
    profilePath?: string;
    departmentId?: number;
    department?: Department;
    jobTitle?: string;
    hireDate?: string;
    baseSalary: number;
    bankName?: string;
    bankAccount?: string;
    isActive: boolean;
    createdAt: string;
    updatedAt?: string;
}

export interface CreateEmployeeRequest {
    employeeCode?: string;
    laoName: string;
    englishName?: string;
    nssfId?: string;
    taxId?: string;
    dateOfBirth?: string;
    gender?: string;
    phone?: string;
    email?: string;
    dependentCount?: number;
    salaryCurrency?: 'LAK' | 'USD' | 'THB';
    departmentId?: number;
    jobTitle?: string;
    hireDate?: string;
    baseSalary: number;
    bankName?: string;
    bankAccount?: string;
}

// -----------------------------------------------------------------------------
// Attendance
// -----------------------------------------------------------------------------
export interface Attendance {
    attendanceId: number;
    employeeId: number;
    attendanceDate: string;
    clockIn?: string;
    clockInLatitude?: number;
    clockInLongitude?: number;
    clockInMethod?: string;
    clockOut?: string;
    clockOutLatitude?: number;
    clockOutLongitude?: number;
    clockOutMethod?: string;
    workHours?: number;
    status: 'PRESENT' | 'ABSENT' | 'LEAVE' | 'HOLIDAY';
    isLate: boolean;
    isEarlyLeave: boolean;
    notes?: string;
    employee?: Employee;
}

// -----------------------------------------------------------------------------
// Payroll
// -----------------------------------------------------------------------------
export interface PayrollPeriod {
    periodId: number;
    year: number;
    month: number;
    periodName: string;
    startDate: string;
    endDate: string;
    status: 'DRAFT' | 'PROCESSING' | 'COMPLETED' | 'LOCKED';
    createdAt: string;
}

export interface SalarySlip {
    slipId: number;
    employeeId: number;
    periodId: number;
    baseSalary: number;
    overtimePay: number;
    allowances: number;
    grossIncome: number;
    nssfBase: number;
    nssfEmployeeDeduction: number;
    nssfEmployerContribution: number;
    taxableIncome: number;
    taxDeduction: number;
    otherDeductions: number;
    netSalary: number;
    status: 'CALCULATED' | 'APPROVED' | 'PAID';
    createdAt: string;
    employee?: Employee;
    payrollPeriod?: PayrollPeriod;
}

export interface SalaryCalculation {
    baseSalary: number;
    overtimePay: number;
    allowances: number;
    grossIncome: number;
    nssfBase: number;
    nssfEmployeeDeduction: number;
    nssfEmployerContribution: number;
    taxableIncome: number;
    taxDeduction: number;
    otherDeductions: number;
    netSalary: number;
}

// -----------------------------------------------------------------------------
// Leave
// -----------------------------------------------------------------------------
export interface LeaveRequest {
    leaveId: number;
    employeeId: number;
    leaveType: 'ANNUAL' | 'SICK' | 'PERSONAL' | 'MATERNITY' | 'PATERNITY' | 'UNPAID';
    startDate: string;
    endDate: string;
    totalDays: number;
    reason?: string;
    status: 'PENDING' | 'APPROVED' | 'REJECTED' | 'CANCELLED';
    approvedById?: number;
    approvedAt?: string;
    approverNotes?: string;
    createdAt: string;
    employee?: Employee;
}

export interface CreateLeaveRequest {
    leaveType: LeaveRequest['leaveType'];
    startDate: string;
    endDate: string;
    reason?: string;
}

// -----------------------------------------------------------------------------
// Holiday
// -----------------------------------------------------------------------------
export interface Holiday {
    holidayId: number;
    date: string;
    name: string;
    nameEn?: string;
    year: number;
    isRecurring: boolean;
}

// -----------------------------------------------------------------------------
// Document
// -----------------------------------------------------------------------------
export interface EmployeeDocument {
    documentId: number;
    employeeId: number;
    documentType: 'Contract' | 'ID_Card' | 'Resume' | 'Other';
    fileName: string;
    filePath: string;
    uploadedAt: string;
}

// -----------------------------------------------------------------------------
// Authentication
// -----------------------------------------------------------------------------
export interface LoginRequest {
    username: string;
    password: string;
}

export interface LoginResponse {
    token: string;
    username: string;
    role: UserRole;
    displayName: string;
    expiresAt: string;
}

export interface UserInfo {
    username: string;
    role: UserRole;
    displayName: string;
}

export type UserRole = 'Admin' | 'HR' | 'Employee';

// -----------------------------------------------------------------------------
// Permissions
// -----------------------------------------------------------------------------
export type Permission =
    | 'employees.view'
    | 'employees.create'
    | 'employees.edit'
    | 'attendance.view'
    | 'attendance.edit'
    | 'leave.view'
    | 'leave.approve'
    | 'payroll.view'
    | 'payroll.run'
    | 'payroll.export'
    | 'audit.view'
    | 'settings.edit';

// -----------------------------------------------------------------------------
// API Response Types
// -----------------------------------------------------------------------------
export interface ApiError {
    message: string;
    code?: string;
    details?: Record<string, string[]>;
}

export interface PaginatedResponse<T> {
    data: T[];
    total: number;
    page: number;
    pageSize: number;
    totalPages: number;
}
