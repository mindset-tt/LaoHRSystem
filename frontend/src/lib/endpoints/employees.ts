/**
 * Employees API Endpoints
 * CRUD operations for employees and departments
 */

import { apiClient } from '../apiClient';
import type { Employee, Department, CreateEmployeeRequest } from '../types';

export const employeesApi = {
    /**
     * Get all employees
     */
    getAll: async (params?: {
        departmentId?: number;
        isActive?: boolean;
        search?: string;
    }): Promise<Employee[]> => {
        const searchParams = new URLSearchParams();
        if (params?.departmentId) searchParams.set('departmentId', params.departmentId.toString());
        if (params?.isActive !== undefined) searchParams.set('isActive', params.isActive.toString());
        if (params?.search) searchParams.set('search', params.search);

        const query = searchParams.toString();
        return apiClient.get<Employee[]>(`/api/employees${query ? `?${query}` : ''}`);
    },

    /**
     * Get single employee by ID
     */
    getById: async (id: number): Promise<Employee> => {
        return apiClient.get<Employee>(`/api/employees/${id}`);
    },

    /**
     * Create new employee
     */
    create: async (employee: CreateEmployeeRequest): Promise<Employee> => {
        return apiClient.post<Employee>('/api/employees', employee);
    },

    /**
     * Update employee
     */
    update: async (id: number, employee: Partial<Employee>): Promise<Employee> => {
        return apiClient.put<Employee>(`/api/employees/${id}`, employee);
    },

    /**
     * Deactivate employee (soft delete)
     */
    deactivate: async (id: number): Promise<void> => {
        return apiClient.delete(`/api/employees/${id}`);
    },

    /**
     * Upload employee profile photo
     */
    uploadPhoto: async (id: number, file: File): Promise<{ profilePath: string }> => {
        const formData = new FormData();
        formData.append('file', file);

        // Custom fetch for multipart
        const response = await fetch(
            `${process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000'}/api/employees/${id}/photo`,
            {
                method: 'POST',
                body: formData,
                credentials: 'include',
            }
        );

        if (!response.ok) {
            throw new Error('Failed to upload photo');
        }

        return response.json();
    },
};

export const departmentsApi = {
    /**
     * Get all departments
     */
    getAll: async (): Promise<Department[]> => {
        return apiClient.get<Department[]>('/api/departments');
    },

    /**
     * Get department by ID
     */
    getById: async (id: number): Promise<Department> => {
        return apiClient.get<Department>(`/api/departments/${id}`);
    },

    /**
     * Create new department
     */
    create: async (department: Partial<Department>): Promise<Department> => {
        return apiClient.post<Department>('/api/departments', department);
    },
};

export default employeesApi;
