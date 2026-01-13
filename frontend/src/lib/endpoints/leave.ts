/**
 * Leave API Endpoints
 * Leave requests, approvals, and balance tracking
 */

import { apiClient } from '../apiClient';
import type { LeaveRequest, CreateLeaveRequest } from '../types';

export interface LeaveBalance {
    leaveType: string;
    total: number;
    used: number;
    remaining: number;
}

export interface LeaveFilters {
    employeeId?: number;
    status?: 'PENDING' | 'APPROVED' | 'REJECTED' | 'CANCELLED';
    startDate?: string;
    endDate?: string;
}

export const leaveApi = {
    /**
     * Get leave requests
     */
    getAll: async (filters?: LeaveFilters): Promise<LeaveRequest[]> => {
        const params = new URLSearchParams();
        if (filters?.employeeId) params.set('employeeId', filters.employeeId.toString());
        if (filters?.status) params.set('status', filters.status);
        if (filters?.startDate) params.set('startDate', filters.startDate);
        if (filters?.endDate) params.set('endDate', filters.endDate);

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
     * Create leave request
     */
    create: async (request: CreateLeaveRequest): Promise<LeaveRequest> => {
        return apiClient.post<LeaveRequest>('/api/leave', request);
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
     * Get pending approvals (HR/Admin only)
     */
    getPendingApprovals: async (): Promise<LeaveRequest[]> => {
        return apiClient.get<LeaveRequest[]>('/api/leave/pending');
    },
};

export default leaveApi;
