/**
 * Attendance API Endpoints
 * Track employee attendance and work hours
 */

import { apiClient } from '../apiClient';
import type { Attendance } from '../types';

export interface AttendanceFilters {
    employeeId?: number;
    startDate?: string;
    endDate?: string;
    status?: 'PRESENT' | 'ABSENT' | 'LEAVE' | 'HOLIDAY';
}

export interface ClockInRequest {
    latitude?: number;
    longitude?: number;
    method?: string;
    notes?: string;
}

export const attendanceApi = {
    /**
     * Get attendance records
     */
    getAll: async (filters?: AttendanceFilters): Promise<Attendance[]> => {
        const params = new URLSearchParams();
        if (filters?.employeeId) params.set('employeeId', filters.employeeId.toString());
        if (filters?.startDate) params.set('startDate', filters.startDate);
        if (filters?.endDate) params.set('endDate', filters.endDate);
        if (filters?.status) params.set('status', filters.status);

        const query = params.toString();
        return apiClient.get<Attendance[]>(`/api/attendance${query ? `?${query}` : ''}`);
    },

    /**
     * Get today's attendance for current user
     */
    getToday: async (): Promise<Attendance | null> => {
        return apiClient.get<Attendance | null>('/api/attendance/today');
    },

    /**
     * Clock in
     */
    clockIn: async (data?: ClockInRequest): Promise<Attendance> => {
        return apiClient.post<Attendance>('/api/attendance/clock-in', data);
    },

    /**
     * Clock out
     */
    clockOut: async (data?: ClockInRequest): Promise<Attendance> => {
        return apiClient.post<Attendance>('/api/attendance/clock-out', data);
    },

    /**
     * Get attendance summary for a period
     */
    getSummary: async (employeeId: number, year: number, month: number): Promise<{
        totalDays: number;
        presentDays: number;
        absentDays: number;
        lateDays: number;
        leaveDays: number;
        totalHours: number;
    }> => {
        return apiClient.get(`/api/attendance/summary?employeeId=${employeeId}&year=${year}&month=${month}`);
    },

    /**
     * Create or update attendance record (admin only)
     */
    upsert: async (attendance: Partial<Attendance>): Promise<Attendance> => {
        return apiClient.post<Attendance>('/api/attendance', attendance);
    },
};

export default attendanceApi;
