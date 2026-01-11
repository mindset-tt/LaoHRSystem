import { useState, useEffect } from 'react';
import { authAPI } from '@/lib/api';

interface User {
    username: string;
    role: string;
    displayName: string;
}

export function useAuth() {
    const [user, setUser] = useState<User | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const checkAuth = async () => {
            try {
                const token = localStorage.getItem('authToken');
                if (!token) {
                    setLoading(false);
                    return;
                }

                const userData = await authAPI.me();
                setUser({
                    username: userData.username,
                    role: userData.role,
                    displayName: userData.username
                });
            } catch (error) {
                console.error("Auth check failed", error);
                localStorage.removeItem('authToken');
            } finally {
                setLoading(false);
            }
        };

        checkAuth();
    }, []);

    // Admin = General Manager, has ALL permissions (higher than HR)
    const isAdmin = user?.role === 'Admin';

    // HR can manage employees and leave, but Admin can do everything HR can + more
    const isHR = user?.role === 'HR' || isAdmin;

    // Specific permissions - Admin has everything
    const canManageEmployees = isHR;
    const canManagePayroll = isAdmin; // Only Admin can run payroll
    const canManageLeave = isHR;
    const canManageAttendance = isHR;
    const canManageHolidays = isAdmin; // Only Admin can add/remove holidays
    const canViewReports = isHR;
    const canExportBankFiles = isAdmin; // Only Admin can export bank files
    const canManageSettings = isAdmin; // Only Admin can change system settings

    const logout = () => {
        localStorage.removeItem('authToken');
        document.cookie = "token=; path=/; expires=Thu, 01 Jan 1970 00:00:01 GMT";
        setUser(null);
        window.location.href = '/login';
    };

    return {
        user,
        loading,
        isAdmin,
        isHR,
        canManageEmployees,
        canManagePayroll,
        canManageLeave,
        canManageAttendance,
        canManageHolidays,
        canViewReports,
        canExportBankFiles,
        canManageSettings,
        logout
    };
}
