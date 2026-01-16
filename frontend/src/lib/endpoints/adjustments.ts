import { apiClient } from '../apiClient';
import type { PayrollAdjustment } from '../types';

export const adjustmentApi = {
    // Get adjustments for a period/employee
    getAdjustments: async (periodId: number, employeeId?: number): Promise<PayrollAdjustment[]> => {
        let url = `/api/PayrollAdjustment?periodId=${periodId}`;
        if (employeeId) url += `&employeeId=${employeeId}`;
        const response = await apiClient.get<PayrollAdjustment[]>(url);
        return response;
    },

    // Create adjustment
    create: async (data: Omit<PayrollAdjustment, 'adjustmentId' | 'createdAt'>): Promise<PayrollAdjustment> => {
        const response = await apiClient.post<PayrollAdjustment>('/api/PayrollAdjustment', data);
        return response;
    },

    // Delete adjustment
    delete: async (adjustmentId: number): Promise<void> => {
        await apiClient.delete(`/api/PayrollAdjustment/${adjustmentId}`);
    },
};
