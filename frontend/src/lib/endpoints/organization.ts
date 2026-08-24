/**
 * Organization API Endpoints
 * Phase 3C1 — department tree, manager chain, direct reports, My Team.
 */

import { apiClient } from '../apiClient';

export interface DepartmentNode {
    departmentId: number;
    departmentName: string;
    departmentNameEn?: string;
    departmentCode?: string;
    managerEmployeeId?: number;
    managerName?: string;
    directHeadcount: number;
    children: DepartmentNode[];
}

export interface EmployeeSummary {
    employeeId: number;
    employeeCode: string;
    laoName: string;
    englishName?: string;
    jobTitle?: string;
    departmentId?: number;
    departmentName?: string;
    positionId?: number;
    positionTitle?: string;
    workLocationId?: number;
    workLocationName?: string;
    email?: string;
    phone?: string;
    isActive: boolean;
}

export const organizationApi = {
    /** Full department tree (root departments with nested children). */
    getTree: async (): Promise<DepartmentNode[]> => {
        return apiClient.get<DepartmentNode[]>('/api/organization/tree');
    },

    /** Manager chain for an employee. */
    getManagerChain: async (employeeId: number): Promise<EmployeeSummary[]> => {
        return apiClient.get<EmployeeSummary[]>(`/api/organization/employees/${employeeId}/manager-chain`);
    },

    /** Direct reports of an employee. */
    getDirectReports: async (employeeId: number): Promise<EmployeeSummary[]> => {
        return apiClient.get<EmployeeSummary[]>(`/api/organization/employees/${employeeId}/reports`);
    },

    /** My Team — direct reports of the current user's linked employee. */
    getMyTeam: async (): Promise<EmployeeSummary[]> => {
        return apiClient.get<EmployeeSummary[]>('/api/organization/my-team');
    },
};
