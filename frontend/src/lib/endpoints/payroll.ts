/**
 * Payroll API Endpoints
 * Payroll periods, salary calculation, and payslips
 */

import { apiClient } from '../apiClient';
import type { PayrollPeriod, SalarySlip, SalaryCalculation } from '../types';

export interface CreatePeriodRequest {
    year: number;
    month: number;
}

export interface CalculateRequest {
    employeeId: number;
    year: number;
    month: number;
    overtimePay?: number;
    allowances?: number;
    otherDeductions?: number;
}

export const payrollApi = {
    /**
     * Get all payroll periods
     */
    getPeriods: async (): Promise<PayrollPeriod[]> => {
        return apiClient.get<PayrollPeriod[]>('/api/payroll/periods');
    },

    /**
     * Get single period
     */
    getPeriod: async (id: number): Promise<PayrollPeriod> => {
        return apiClient.get<PayrollPeriod>(`/api/payroll/periods/${id}`);
    },

    /**
     * Create new payroll period
     */
    createPeriod: async (request: CreatePeriodRequest): Promise<PayrollPeriod> => {
        return apiClient.post<PayrollPeriod>('/api/payroll/periods', request);
    },

    /**
     * Run payroll calculation for a period
     */
    runPayroll: async (periodId: number): Promise<SalarySlip[]> => {
        return apiClient.post<SalarySlip[]>(`/api/payroll/periods/${periodId}/run`);
    },

    /**
     * Get salary slips for a period
     */
    getSlips: async (periodId: number): Promise<SalarySlip[]> => {
        return apiClient.get<SalarySlip[]>(`/api/payroll/periods/${periodId}/slips`);
    },

    /**
     * Get single salary slip
     */
    getSlip: async (slipId: number): Promise<SalarySlip> => {
        return apiClient.get<SalarySlip>(`/api/payroll/slips/${slipId}`);
    },

    /**
     * Download payslip as PDF
     */
    downloadPdf: async (slipId: number): Promise<Blob> => {
        return apiClient.getBlob(`/api/payroll/slips/${slipId}/pdf`);
    },

    /**
     * Export payroll period to Excel
     */
    exportPayroll: async (periodId: number): Promise<Blob> => {
        return apiClient.getBlob(`/api/payroll/periods/${periodId}/export`);
    },

    /**
     * Calculate salary preview (without saving)
     */
    calculate: async (request: CalculateRequest): Promise<SalaryCalculation> => {
        return apiClient.post<SalaryCalculation>('/api/payroll/calculate', request);
    },

    /**
     * Approve salary slip
     */
    approveSlip: async (slipId: number): Promise<SalarySlip> => {
        return apiClient.post<SalarySlip>(`/api/payroll/slips/${slipId}/approve`);
    },

    /**
     * Mark slip as paid
     */
    markAsPaid: async (slipId: number): Promise<SalarySlip> => {
        return apiClient.post<SalarySlip>(`/api/payroll/slips/${slipId}/paid`);
    },
};

export default payrollApi;
