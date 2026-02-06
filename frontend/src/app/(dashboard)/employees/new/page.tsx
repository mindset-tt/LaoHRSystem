'use client';

import Link from 'next/link';
import { useAuth } from '@/components/providers/AuthProvider';
import { useLanguage } from '@/components/providers/LanguageProvider';
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
    const { t } = useLanguage();
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
                        {t.employees.title}
                    </Link>
                    <span className={styles.breadcrumbSeparator}>/</span>
                    <span className={styles.breadcrumbCurrent}>{t.employeeForm.createTitle}</span>
                </div>
            </div>

            <h1 className={styles.title}>{t.employeeForm.createTitle}</h1>
            <p className={styles.subtitle}>
                {t.employeeForm.subtitle}
            </p>

            <EmployeeForm />
        </div>
    );
}
