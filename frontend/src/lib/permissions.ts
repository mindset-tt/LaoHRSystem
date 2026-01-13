/**
 * RBAC Permissions Helper
 * 
 * Permissions are loaded at login and refreshed on session renewal.
 * Frontend checks are UX-only; backend authorization is authoritative.
 */

import type { UserRole, Permission } from './types';

/**
 * Permission matrix by role
 */
const ROLE_PERMISSIONS: Record<UserRole, Permission[]> = {
    Admin: [
        'employees.view',
        'employees.create',
        'employees.edit',
        'attendance.view',
        'attendance.edit',
        'leave.view',
        'leave.approve',
        'payroll.view',
        'payroll.run',
        'payroll.export',
        'audit.view',
        'settings.edit',
    ],
    HR: [
        'employees.view',
        'employees.create',
        'employees.edit',
        'attendance.view',
        'attendance.edit',
        'leave.view',
        'leave.approve',
        'payroll.view',
        'payroll.run',
        'payroll.export',
    ],
    Employee: [
        'employees.view', // Self only - enforced by backend
        'attendance.view', // Self only
        'leave.view', // Self only
        'payroll.view', // Self only
        'payroll.export', // Self only
    ],
};

/**
 * Check if a role has a specific permission
 */
export function hasPermission(role: UserRole | undefined, permission: Permission): boolean {
    if (!role) return false;
    return ROLE_PERMISSIONS[role]?.includes(permission) ?? false;
}

/**
 * Check if a role has all of the specified permissions
 */
export function hasAllPermissions(role: UserRole | undefined, permissions: Permission[]): boolean {
    if (!role) return false;
    return permissions.every((p) => hasPermission(role, p));
}

/**
 * Check if a role has any of the specified permissions
 */
export function hasAnyPermission(role: UserRole | undefined, permissions: Permission[]): boolean {
    if (!role) return false;
    return permissions.some((p) => hasPermission(role, p));
}

/**
 * Get all permissions for a role
 */
export function getPermissions(role: UserRole | undefined): Permission[] {
    if (!role) return [];
    return ROLE_PERMISSIONS[role] ?? [];
}

/**
 * Check if user can view all employees or only self
 */
export function canViewAllEmployees(role: UserRole | undefined): boolean {
    return role === 'Admin' || role === 'HR';
}

/**
 * Check if user can edit an employee
 */
export function canEditEmployee(role: UserRole | undefined, employeeId: number, currentUserId?: number): boolean {
    // Admin and HR can edit any employee
    if (role === 'Admin' || role === 'HR') return true;
    // Employees can only edit their own profile (limited fields)
    if (role === 'Employee' && employeeId === currentUserId) return true;
    return false;
}

/**
 * Check if user is an admin
 */
export function isAdmin(role: UserRole | undefined): boolean {
    return role === 'Admin';
}

/**
 * Check if user is HR or Admin
 */
export function isHROrAdmin(role: UserRole | undefined): boolean {
    return role === 'Admin' || role === 'HR';
}
