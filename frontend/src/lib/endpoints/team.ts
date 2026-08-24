/**
 * Team API Endpoints
 * Phase 3C2B — manager self-service (MSS) team leave + attendance.
 */

import { apiClient } from '../apiClient';

export interface TeamLeaveItem {
    leaveId: number;
    employeeId: number;
    employeeName?: string | null;
    departmentName?: string | null;
    jobTitle?: string | null;
    leaveType: string;
    startDate: string;
    endDate: string;
    totalDays: number;
    status: string;
}

export interface TeamAttendanceItem {
    attendanceId: number;
    employeeId: number;
    employeeName?: string | null;
    attendanceDate: string;
    clockIn?: string | null;
    clockOut?: string | null;
    status: string;
    isLate: boolean;
    isEarlyLeave: boolean;
    workHours?: number | null;
}

export const teamApi = {
    /** Team leave for the current manager's direct reports. */
    getTeamLeave: async (params: { status?: string; from?: string; to?: string } = {}): Promise<TeamLeaveItem[]> => {
        const sp = new URLSearchParams();
        if (params.status) sp.set('status', params.status);
        if (params.from) sp.set('from', params.from);
        if (params.to) sp.set('to', params.to);
        const q = sp.toString();
        return apiClient.get<TeamLeaveItem[]>(`/api/team/leave${q ? `?${q}` : ''}`);
    },

    /** Team attendance for the current manager's direct reports. */
    getTeamAttendance: async (params: { date?: string; from?: string; to?: string } = {}): Promise<TeamAttendanceItem[]> => {
        const sp = new URLSearchParams();
        if (params.date) sp.set('date', params.date);
        if (params.from) sp.set('from', params.from);
        if (params.to) sp.set('to', params.to);
        const q = sp.toString();
        return apiClient.get<TeamAttendanceItem[]>(`/api/team/attendance${q ? `?${q}` : ''}`);
    },
};

export default teamApi;
