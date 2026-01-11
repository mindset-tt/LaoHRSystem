const API_BASE = 'http://localhost:5000/api';

// Get auth token from localStorage
const getToken = (): string | null => {
    if (typeof window !== 'undefined') {
        return localStorage.getItem('authToken');
    }
    return null;
};

// Generic fetch with auth header
async function fetchAPI<T>(
    endpoint: string,
    options: RequestInit = {}
): Promise<T> {
    const token = getToken();

    const headers: HeadersInit = {
        'Content-Type': 'application/json',
        ...(token ? { 'Authorization': `Bearer ${token}` } : {}),
        ...options.headers,
    };

    const response = await fetch(`${API_BASE}${endpoint}`, {
        ...options,
        headers,
    });

    if (!response.ok) {
        if (response.status === 402) {
            if (typeof window !== 'undefined' && !window.location.pathname.startsWith('/license')) {
                window.location.href = '/license-activated'; // Redirect to activation page
            }
        }
        const error = await response.json().catch(() => ({ message: 'Request failed' }));
        throw new Error(error.message || `HTTP ${response.status}`);
    }

    // Handle empty responses
    const text = await response.text();
    return text ? JSON.parse(text) : null as T;
}

export const licenseAPI = {
    activate: (key: string) => fetchAPI<{ message: string, customer: string }>('/license/activate', {
        method: 'POST',
        body: JSON.stringify({ key })
    }),
    checkStatus: () => fetchAPI<{ isValid: boolean; expiration?: string; customer?: string }>('/license/status'),
};

// Auth API
export const authAPI = {
    login: (username: string, password: string) =>
        fetchAPI<{ token: string; username: string; role: string }>('/auth/login', {
            method: 'POST',
            body: JSON.stringify({ username, password }),
        }),

    me: () => fetchAPI<{ username: string; role: string }>('/auth/me'),
};

// Employees API
export const employeesAPI = {
    getAll: () => fetchAPI<Employee[]>('/employees'),
    getById: (id: number) => fetchAPI<Employee>(`/employees/${id}`),
    create: (data: Partial<Employee>) =>
        fetchAPI<Employee>('/employees', { method: 'POST', body: JSON.stringify(data) }),
    update: (id: number, data: Partial<Employee>) =>
        fetchAPI<Employee>(`/employees/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
    delete: (id: number) =>
        fetchAPI<void>(`/employees/${id}`, { method: 'DELETE' }),
    uploadPhoto: async (id: number, file: File) => {
        const formData = new FormData();
        formData.append('file', file);
        const token = localStorage.getItem('authToken');

        const response = await fetch(`${API_BASE}/employees/${id}/photo`, {
            method: 'POST',
            headers: token ? { 'Authorization': `Bearer ${token}` } : {},
            body: formData
        });

        if (!response.ok) throw new Error('Failed to upload photo');
        return await response.json();
    }
};

// Departments API
export const departmentsAPI = {
    getAll: () => fetchAPI<Department[]>('/departments'),
};

// Attendance API
export const attendanceAPI = {
    clockIn: (employeeId: number, lat?: number, lng?: number) =>
        fetchAPI<AttendanceRecord>('/attendance/clock-in', {
            method: 'POST',
            body: JSON.stringify({ employeeId, latitude: lat, longitude: lng }),
        }),
    clockOut: (employeeId: number, lat?: number, lng?: number) =>
        fetchAPI<AttendanceRecord>('/attendance/clock-out', {
            method: 'POST',
            body: JSON.stringify({ employeeId, latitude: lat, longitude: lng }),
        }),
    getByDate: (date: string) => fetchAPI<AttendanceRecord[]>(`/attendance?date=${date}`),
    getByEmployee: (employeeId: number) => fetchAPI<AttendanceRecord[]>(`/attendance/employee/${employeeId}`),
};

// Payroll API
export const payrollAPI = {
    getPeriods: () => fetchAPI<PayrollPeriod[]>('/payroll/periods'),
    calculatePreview: (baseSalary: number, overtimePay: number = 0, allowances: number = 0, otherDeductions: number = 0) =>
        fetchAPI<PayrollPreview>('/payroll/calculate', {
            method: 'POST',
            body: JSON.stringify({ baseSalary, overtimePay, allowances, otherDeductions }),
        }),
    runPayroll: (periodId: number) =>
        fetchAPI<void>(`/payroll/periods/${periodId}/run`, { method: 'POST' }),
    downloadSlipPdf: async (slipId: number) => {
        const token = getToken();
        const response = await fetch(`${API_BASE}/payroll/slips/${slipId}/pdf`, {
            headers: token ? { 'Authorization': `Bearer ${token}` } : {},
        });
        return response.blob();
        return response.blob();
    },
};

// Bank Transfer API
export const bankTransferAPI = {
    downloadBcel: async (periodId: number) => {
        const token = getToken();
        const response = await fetch(`${API_BASE}/banktransfer/bcel/${periodId}`, {
            headers: token ? { 'Authorization': `Bearer ${token}` } : {},
        });
        if (!response.ok) throw new Error('Failed to download BCEL file');
        return response.blob();
    },
    downloadLdb: async (periodId: number) => {
        const token = getToken();
        const response = await fetch(`${API_BASE}/banktransfer/ldb/${periodId}`, {
            headers: token ? { 'Authorization': `Bearer ${token}` } : {},
        });
        if (!response.ok) throw new Error('Failed to download LDB file');
        return response.blob();
    },
};

// Leave API
export const leaveAPI = {
    getAll: () => fetchAPI<LeaveRequest[]>('/leave'),
    create: (data: Partial<LeaveRequest>) =>
        fetchAPI<LeaveRequest>('/leave', { method: 'POST', body: JSON.stringify(data) }),
    approve: (id: number) =>
        fetchAPI<void>(`/leave/${id}/approve`, { method: 'POST' }),
    reject: (id: number) =>
        fetchAPI<void>(`/leave/${id}/reject`, { method: 'POST' }),
};

// Reports API
export const reportsAPI = {
    downloadNssf: async (periodId: number) => {
        const token = getToken();
        const response = await fetch(`${API_BASE}/reports/nssf/${periodId}`, {
            headers: token ? { 'Authorization': `Bearer ${token}` } : {},
        });
        if (!response.ok) throw new Error('Failed to download NSSF report');
        return response.blob();
    }
};

// Types
export interface Employee {
    id: number;
    laoName: string;
    englishName?: string;
    employeeCode: string;
    departmentId: number;
    department?: Department;
    jobTitle?: string;
    hireDate: string;
    baseSalary: number;
    status: string;
    email?: string;
    phone?: string;
    bankAccount?: string;
    bankName?: string;
    nssfId?: string;
    profilePath?: string;
}

export interface Department {
    id: number;
    departmentNameLo: string;
    departmentNameEn: string;
}

export interface AttendanceRecord {
    id: number;
    employeeId: number;
    date: string;
    clockIn?: string;
    clockOut?: string;
    status: string;
}

export interface PayrollPeriod {
    id: number;
    periodName: string;
    startDate: string;
    endDate: string;
    status: string;
}

export interface PayrollPreview {
    baseSalary: number;
    allowances: number;
    overtimePay: number;
    grossIncome: number;
    nssfBase: number;
    nssfEmployee: number;
    nssfEmployer: number;
    taxableIncome: number;
    incomeTax: number;
    netSalary: number;
}

export interface LeaveRequest {
    id: number;
    employeeId: number;
    leaveType: string;
    startDate: string;
    endDate: string;
    status: string;
    reason?: string;
}

export interface Holiday {
    holidayId: number;
    date: string;
    name: string;
    nameEn?: string;
    year: number;
    isRecurring: boolean;
}

// Holidays API
export const holidaysAPI = {
    getAll: (year?: number) => fetchAPI<Holiday[]>(year ? `/holidays?year=${year}` : '/holidays'),
    create: (data: Partial<Holiday>) => fetchAPI<Holiday>('/holidays', { method: 'POST', body: JSON.stringify(data) }),
    update: (id: number, data: Partial<Holiday>) => fetchAPI<Holiday>(`/holidays/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
    delete: (id: number) => fetchAPI<void>(`/holidays/${id}`, { method: 'DELETE' }),
};

export interface EmployeeDocument {
    documentId: number;
    employeeId: number;
    documentType: string;
    fileName: string;
    filePath: string;
    uploadedAt: string;
}

export const documentsAPI = {
    getByEmployee: (employeeId: number) => fetchAPI<EmployeeDocument[]>(`/documents/employee/${employeeId}`),
    upload: async (employeeId: number, documentType: string, file: File) => {
        const formData = new FormData();
        formData.append('employeeId', employeeId.toString());
        formData.append('documentType', documentType);
        formData.append('file', file);
        const token = localStorage.getItem('authToken');

        const response = await fetch(`${API_BASE}/documents`, {
            method: 'POST',
            headers: token ? { 'Authorization': `Bearer ${token}` } : {},
            body: formData
        });

        if (!response.ok) throw new Error('Failed to upload document');
        return await response.json() as EmployeeDocument;
    },
    delete: (id: number) => fetchAPI<void>(`/documents/${id}`, { method: 'DELETE' }),
};

// Settings API
export interface SystemSettings {
    company_name?: string;
    head_office_address?: string;
    head_office_latitude?: string;
    head_office_longitude?: string;
    geofence_radius?: string;
    zkteco_enabled?: string;
    zkteco_ip?: string;
    zkteco_port?: string;
    exchange_usd_lak?: string;
    exchange_thb_lak?: string;
    nssf_rate?: string;
    [key: string]: string | undefined;
}

export const settingsAPI = {
    getAll: () => fetchAPI<SystemSettings>('/settings'),
    update: (key: string, value: string) =>
        fetchAPI<{ settingKey: string; settingValue: string }>(`/settings/${key}`, {
            method: 'PUT',
            body: JSON.stringify({ value }),
        }),
    updateBatch: async (settings: Partial<SystemSettings>) => {
        const updates = Object.entries(settings).filter(([_, v]) => v !== undefined);
        const results = await Promise.allSettled(updates.map(([key, value]) =>
            fetchAPI(`/settings/${key}`, {
                method: 'PUT',
                body: JSON.stringify({ value }),
            })
        ));

        // Check for any failed updates
        const failed = results.filter(r => r.status === 'rejected');
        if (failed.length > 0) {
            console.error('Some settings failed to save:', failed);
            throw new Error(`Failed to save ${failed.length} settings`);
        }
    },
};
