/**
 * Work Schedule & Holidays API endpoints
 */
import { apiClient } from '../apiClient';
import type { WorkSchedule, UpdateWorkSchedule, Holiday, CreateHoliday, UpdateHoliday, WorkDayBreakdown } from '../types';
import type { PaginatedResponse } from '../types/pagination';

export const workScheduleApi = {
    /**
     * Get the work schedule
     */
    get: async (): Promise<WorkSchedule> => {
        return apiClient.get<WorkSchedule>('/api/settings/work-schedule');
    },

    /**
     * Update the work schedule
     */
    update: async (schedule: UpdateWorkSchedule): Promise<WorkSchedule> => {
        return apiClient.put<WorkSchedule>('/api/settings/work-schedule', schedule);
    },

    /**
     * Calculate work days between two dates
     */
    calculateWorkDays: async (startDate: string, endDate: string): Promise<WorkDayBreakdown> => {
        const params = new URLSearchParams({ startDate, endDate });
        return apiClient.get<WorkDayBreakdown>(`/api/settings/workdays/calculate?${params}`);
    },
};

export const holidaysApi = {
    /**
     * Get holidays (paged), optionally filtered by year.
     */
    getAll: async (year?: number, page = 1, pageSize = 50): Promise<PaginatedResponse<Holiday>> => {
        const params = new URLSearchParams();
        if (year !== undefined) params.set('year', year.toString());
        params.set('page', page.toString());
        params.set('pageSize', pageSize.toString());
        return apiClient.get<PaginatedResponse<Holiday>>(`/api/holidays?${params.toString()}`);
    },

    /**
     * Get a specific holiday
     */
    get: async (id: number): Promise<Holiday> => {
        return apiClient.get<Holiday>(`/api/holidays/${id}`);
    },

    /**
     * Create a new holiday
     */
    create: async (holiday: CreateHoliday): Promise<Holiday> => {
        return apiClient.post<Holiday>('/api/holidays', holiday);
    },

    /**
     * Update a holiday
     */
    update: async (id: number, holiday: UpdateHoliday): Promise<Holiday> => {
        return apiClient.put<Holiday>(`/api/holidays/${id}`, holiday);
    },

    /**
     * Delete a holiday (soft delete)
     */
    delete: async (id: number): Promise<void> => {
        return apiClient.delete<void>(`/api/holidays/${id}`);
    },

    /**
     * Seed default Laos holidays
     */
    seedDefaults: async (): Promise<{ message: string }> => {
        return apiClient.post<{ message: string }>('/api/holidays/seed-defaults');
    },
};
