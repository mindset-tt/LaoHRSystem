'use client';

import { useEffect, useState } from 'react';
import { useParams } from 'next/navigation';
import Link from 'next/link';
import { useAuth } from '@/components/providers/AuthProvider';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { EmployeeForm } from '@/components/forms/EmployeeForm';
import { Skeleton } from '@/components/ui/Skeleton';
import { employeesApi } from '@/lib/endpoints';
import { isHROrAdmin } from '@/lib/permissions';
import type { Employee } from '@/lib/types';
import styles from './page.module.css';

/**
 * Edit Employee Page
 * Update an existing employee
 */
export default function EditEmployeePage() {
    const params = useParams();
    const { role, loading: authLoading } = useAuth();
    const { t } = useLanguage();
    const [employee, setEmployee] = useState<Employee | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const employeeId = params.id as string;
    const canEdit = isHROrAdmin(role);

    useEffect(() => {
        const loadEmployee = async () => {
            try {
                const data = await employeesApi.getById(parseInt(employeeId));
                setEmployee(data);
            } catch (err) {
                console.error('Failed to load employee:', err);
                setError('Failed to load employee data');
                // Fallback to mock data for demo
                setEmployee({
                    employeeId: parseInt(employeeId),
                    employeeCode: 'EMP0001',
                    laoName: 'ສົມສັກ ສີສະຫວັດ',
                    englishName: 'Somsak Sisavad',
                    email: 'somsak@laohr.com',
                    phone: '+856 20 5555 1234',
                    dateOfBirth: '1990-05-15',
                    gender: 'Male',
                    departmentId: 1,
                    jobTitle: 'Senior Developer',
                    hireDate: '2020-03-01',
                    baseSalary: 15000000,
                    salaryCurrency: 'LAK',
                    bankName: 'BCEL',
                    bankAccount: '0102030405060708',
                    dependentCount: 2,
                    isActive: true,
                    createdAt: '',
                });
            } finally {
                setLoading(false);
            }
        };

        loadEmployee();
    }, [employeeId]);

    if (authLoading || loading) {
        return (
            <div className={styles.page}>
                <Skeleton width={200} height={32} />
                <Skeleton width={300} height={20} />
                <div style={{ marginTop: 'var(--space-6)' }}>
                    <Skeleton height={400} />
                </div>
            </div>
        );
    }

    if (!canEdit) {
        return (
            <div className={styles.page}>
                <h1>Access Denied</h1>
                <p>You don&apos;t have permission to edit employees.</p>
                <Link href="/employees">Back to Employees</Link>
            </div>
        );
    }

    if (error && !employee) {
        return (
            <div className={styles.page}>
                <h1>Error</h1>
                <p>{error}</p>
                <Link href="/employees">Back to Employees</Link>
            </div>
        );
    }

    return (
        <div className={styles.page}>
            {/* Header */}
            <div className={styles.header}>
                <div className={styles.breadcrumbs}>
                    <Link href="/employees" className={styles.breadcrumbLink}>
                        {t.employees.title}
                    </Link>
                    <span className={styles.breadcrumbSeparator}>/</span>
                    <Link href={`/employees/${employeeId}`} className={styles.breadcrumbLink}>
                        {employee?.englishName || employee?.laoName}
                    </Link>
                    <span className={styles.breadcrumbSeparator}>/</span>
                    <span className={styles.breadcrumbCurrent}>{t.employeeDetail.edit}</span>
                </div>
            </div>

            <h1 className={styles.title}>{t.employeeForm.editTitle}</h1>
            <p className={styles.subtitle}>
                {t.employeeForm.editSubtitle || 'Update employee information'}
            </p>

            {employee && <EmployeeForm employee={employee} />}
        </div>
    );
}
