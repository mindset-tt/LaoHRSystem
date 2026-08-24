/**
 * Analytics API Endpoints
 * Phase 3C3 — executive/HR/manager dashboards + domain analytics.
 */

import { apiClient } from '../apiClient';

export interface KpiCard {
    id: string;
    label: string;
    labelLao?: string | null;
    value: number;
    unit: string;
    delta?: number | null;
    comparisonLabel?: string | null;
}

export interface TimeSeriesPoint {
    date: string;
    value: number;
}

export interface BreakdownItem {
    key: string;
    label: string;
    value: number;
    percentage?: number | null;
}

export interface AttentionItem {
    type: string;
    title: string;
    count: number;
    link?: string | null;
}

export interface ExecutiveDashboard {
    kpis: KpiCard[];
    attention: AttentionItem[];
    headcountByDepartment: BreakdownItem[];
    headcountTrend: TimeSeriesPoint[];
    leaveTrend: TimeSeriesPoint[];
    expenseTrend: TimeSeriesPoint[];
}

export interface HrDashboard {
    kpis: KpiCard[];
    headcountByDepartment: BreakdownItem[];
    headcountByLocation: BreakdownItem[];
    headcountByPosition: BreakdownItem[];
    leaveByType: BreakdownItem[];
    newHiresTrend: TimeSeriesPoint[];
}

export interface ManagerDashboard {
    kpis: KpiCard[];
    teamLeaveByStatus: BreakdownItem[];
    teamAttendanceTrend: TimeSeriesPoint[];
}

export interface AttendanceAnalytics {
    kpis: KpiCard[];
    dailyTrend: TimeSeriesPoint[];
    statusBreakdown: BreakdownItem[];
}

export interface LeaveAnalytics {
    kpis: KpiCard[];
    byType: BreakdownItem[];
    byStatus: BreakdownItem[];
    usageTrend: TimeSeriesPoint[];
}

export interface PayrollAnalytics {
    kpis: KpiCard[];
    byDepartment: BreakdownItem[];
    grossTrend: TimeSeriesPoint[];
}

export interface FinanceAnalytics {
    kpis: KpiCard[];
    expenseByCategory: BreakdownItem[];
    loanByStatus: BreakdownItem[];
    expenseTrend: TimeSeriesPoint[];
}

export interface PmAnalytics {
    kpis: KpiCard[];
    projectByStatus: BreakdownItem[];
    taskByStatus: BreakdownItem[];
    riskBySeverity: BreakdownItem[];
    issueByStatus: BreakdownItem[];
}

export const analyticsApi = {
    getExecutive: () => apiClient.get<ExecutiveDashboard>('/api/analytics/executive'),
    getHr: () => apiClient.get<HrDashboard>('/api/analytics/hr'),
    getManager: () => apiClient.get<ManagerDashboard>('/api/analytics/my-team'),
    getAttendance: (params: { range?: string; from?: string; to?: string } = {}) => {
        const sp = new URLSearchParams();
        if (params.range) sp.set('range', params.range);
        if (params.from) sp.set('from', params.from);
        if (params.to) sp.set('to', params.to);
        const q = sp.toString();
        return apiClient.get<AttendanceAnalytics>(`/api/analytics/attendance${q ? `?${q}` : ''}`);
    },
    getLeave: (params: { range?: string; from?: string; to?: string } = {}) => {
        const sp = new URLSearchParams();
        if (params.range) sp.set('range', params.range);
        if (params.from) sp.set('from', params.from);
        if (params.to) sp.set('to', params.to);
        const q = sp.toString();
        return apiClient.get<LeaveAnalytics>(`/api/analytics/leave${q ? `?${q}` : ''}`);
    },
    getPayroll: () => apiClient.get<PayrollAnalytics>('/api/analytics/payroll'),
    getFinance: (params: { range?: string; from?: string; to?: string } = {}) => {
        const sp = new URLSearchParams();
        if (params.range) sp.set('range', params.range);
        if (params.from) sp.set('from', params.from);
        if (params.to) sp.set('to', params.to);
        const q = sp.toString();
        return apiClient.get<FinanceAnalytics>(`/api/analytics/finance${q ? `?${q}` : ''}`);
    },
    getPm: () => apiClient.get<PmAnalytics>('/api/analytics/pm'),
};

export default analyticsApi;
