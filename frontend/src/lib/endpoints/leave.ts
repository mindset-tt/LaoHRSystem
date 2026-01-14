/**
 * Leave API Endpoints
 * Leave requests, approvals, balance tracking, calendar, and export
 */

import { apiClient } from '../apiClient';
import type { LeaveRequest, CreateLeaveRequest, LeaveBalance, LeavePolicy, LeaveCalendarItem } from '../types';

export interface LeaveFilters {
    employeeId?: number;
    status?: 'PENDING' | 'APPROVED' | 'REJECTED' | 'CANCELLED';
    year?: number;
    month?: number;
}

export const leaveApi = {
    /**
     * Get leave requests
     */
    getAll: async (filters?: LeaveFilters): Promise<LeaveRequest[]> => {
        const params = new URLSearchParams();
        if (filters?.employeeId) params.set('employeeId', filters.employeeId.toString());
        if (filters?.status) params.set('status', filters.status);
        if (filters?.year) params.set('year', filters.year.toString());
        if (filters?.month) params.set('month', filters.month.toString());

        const query = params.toString();
        return apiClient.get<LeaveRequest[]>(`/api/leave${query ? `?${query}` : ''}`);
    },

    /**
     * Get single leave request
     */
    getById: async (id: number): Promise<LeaveRequest> => {
        return apiClient.get<LeaveRequest>(`/api/leave/${id}`);
    },

    /**
     * Create leave request with optional attachment
     */
    create: async (request: CreateLeaveRequest): Promise<LeaveRequest> => {
        const formData = new FormData();
        formData.append('employeeId', (request.employeeId ?? 1).toString());
        formData.append('leaveType', request.leaveType);
        formData.append('startDate', request.startDate);
        formData.append('endDate', request.endDate);
        formData.append('isHalfDay', (request.isHalfDay ?? false).toString());
        if (request.halfDayType) formData.append('halfDayType', request.halfDayType);
        if (request.reason) formData.append('reason', request.reason);
        if (request.attachment) formData.append('attachment', request.attachment);

        const response = await fetch('/api/leave', {
            method: 'POST',
            body: formData,
        });

        if (!response.ok) {
            const error = await response.text();
            throw new Error(error);
        }

        return response.json();
    },

    /**
     * Cancel leave request
     */
    cancel: async (id: number): Promise<void> => {
        return apiClient.post(`/api/leave/${id}/cancel`);
    },

    /**
     * Approve leave request (HR/Admin only)
     */
    approve: async (id: number, notes?: string): Promise<LeaveRequest> => {
        return apiClient.post<LeaveRequest>(`/api/leave/${id}/approve`, { notes });
    },

    /**
     * Reject leave request (HR/Admin only)
     */
    reject: async (id: number, notes?: string): Promise<LeaveRequest> => {
        return apiClient.post<LeaveRequest>(`/api/leave/${id}/reject`, { notes });
    },

    /**
     * Get leave balance for an employee
     */
    getBalance: async (employeeId?: number): Promise<LeaveBalance[]> => {
        const query = employeeId ? `?employeeId=${employeeId}` : '';
        return apiClient.get<LeaveBalance[]>(`/api/leave/balance${query}`);
    },

    /**
     * Get leave calendar data for a month
     */
    getCalendar: async (year: number, month: number): Promise<LeaveCalendarItem[]> => {
        return apiClient.get<LeaveCalendarItem[]>(`/api/leave/calendar?year=${year}&month=${month}`);
    },

    /**
     * Get all leave policies
     */
    getPolicies: async (): Promise<LeavePolicy[]> => {
        return apiClient.get<LeavePolicy[]>('/api/leave/policies');
    },

    /**
     * Update a leave policy (Admin only)
     */
    updatePolicy: async (id: number, policy: Partial<LeavePolicy>): Promise<LeavePolicy> => {
        return apiClient.put<LeavePolicy>(`/api/leave/policies/${id}`, policy);
    },

    /**
     * Export leave history to Excel
     */
    exportHistory: async (year?: number, employeeId?: number): Promise<Blob> => {
        const params = new URLSearchParams();
        if (year) params.set('year', year.toString());
        if (employeeId) params.set('employeeId', employeeId.toString());

        const query = params.toString();
        const response = await fetch(`/api/leave/export${query ? `?${query}` : ''}`, {
            method: 'GET',
        });

        if (!response.ok) throw new Error('Export failed');
        return response.blob();
    },
};

// Re-export types for convenience
export type { LeaveBalance, LeavePolicy, LeaveCalendarItem };

export default leaveApi;
