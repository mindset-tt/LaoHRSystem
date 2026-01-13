'use client';

import Link from 'next/link';
import { useAuth } from '@/components/providers/AuthProvider';
import { EmployeeForm } from '@/components/forms/EmployeeForm';
import { isHROrAdmin } from '@/lib/permissions';
import { useRouter } from 'next/navigation';
import { useEffect } from 'react';
import styles from './page.module.css';

/**
 * New Employee Page
 * Create a new employee
 */
export default function NewEmployeePage() {
    const { role, loading } = useAuth();
    const router = useRouter();
    const canAdd = isHROrAdmin(role);

    useEffect(() => {
        if (!loading && !canAdd) {
            router.push('/employees');
        }
    }, [loading, canAdd, router]);

    if (loading || !canAdd) {
        return null;
    }

    return (
        <div className={styles.page}>
            {/* Header */}
            <div className={styles.header}>
                <div className={styles.breadcrumbs}>
                    <Link href="/employees" className={styles.breadcrumbLink}>
                        Employees
                    </Link>
                    <span className={styles.breadcrumbSeparator}>/</span>
                    <span className={styles.breadcrumbCurrent}>New Employee</span>
                </div>
            </div>

            <h1 className={styles.title}>Add New Employee</h1>
            <p className={styles.subtitle}>
                Fill in the information below to create a new employee record
            </p>

            <EmployeeForm />
        </div>
    );
}
