'use client';

import { useEffect, useState } from 'react';
import { useParams } from 'next/navigation';
import Link from 'next/link';
import { useAuth } from '@/components/providers/AuthProvider';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { MaskedField } from '@/components/ui/MaskedField';
import { Skeleton } from '@/components/ui/Skeleton';
import { formatDate } from '@/lib/datetime';
import { isHROrAdmin } from '@/lib/permissions';
import type { Employee } from '@/lib/types';
import styles from './page.module.css';

/**
 * Employee Detail Page
 * Shows comprehensive employee information with tabs
 */
export default function EmployeeDetailPage() {
    const params = useParams();
    const { role } = useAuth();
    const { t } = useLanguage();
    const [employee, setEmployee] = useState<Employee | null>(null);
    const [loading, setLoading] = useState(true);
    const [activeTab, setActiveTab] = useState<'personal' | 'employment' | 'documents'>('personal');

    const employeeId = params.id as string;
    const canEdit = isHROrAdmin(role);

    useEffect(() => {
        const loadEmployee = async () => {
            // Simulated data - will connect to API
            await new Promise((resolve) => setTimeout(resolve, 500));

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
                department: { departmentId: 1, departmentName: 'Engineering', isActive: true, createdAt: '' },
                jobTitle: 'Senior Developer',
                hireDate: '2020-03-01',
                baseSalary: 15000000,
                salaryCurrency: 'LAK',
                bankName: 'BCEL',
                bankAccount: '0102030405060708',
                nssfId: 'NSSF-12345678',
                taxId: 'TAX-87654321',
                dependentCount: 2,
                isActive: true,
                createdAt: '2020-03-01T00:00:00Z',
                updatedAt: '2024-01-10T08:30:00Z',
            });

            setLoading(false);
        };

        loadEmployee();
    }, [employeeId]);

    if (loading) {
        return (
            <div className={styles.page}>
                <div className={styles.header}>
                    <Skeleton width={200} height={32} />
                    <Skeleton width={100} height={40} />
                </div>
                <Card>
                    <div className={styles.profileSkeleton}>
                        <Skeleton width={100} height={100} circle />
                        <div className={styles.profileSkeletonInfo}>
                            <Skeleton width={200} height={24} />
                            <Skeleton width={150} height={16} />
                        </div>
                    </div>
                </Card>
            </div>
        );
    }

    if (!employee) {
        return (
            <div className={styles.page}>
                <div className={styles.notFound}>
                    <h2>{t.employeeDetail.notFound}</h2>
                    <Link href="/employees">
                        <Button>{t.employeeDetail.back}</Button>
                    </Link>
                </div>
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
                    <span className={styles.breadcrumbCurrent}>{employee.englishName}</span>
                </div>
                {canEdit && (
                    <Link href={`/employees/${employeeId}/edit`}>
                        <Button variant="secondary" leftIcon={<EditIcon />}>
                            {t.employeeDetail.edit}
                        </Button>
                    </Link>
                )}
            </div>

            {/* Profile Card */}
            <Card>
                <div className={styles.profile}>
                    <div className={styles.avatar}>
                        {getInitials(employee.englishName || employee.laoName)}
                    </div>
                    <div className={styles.profileInfo}>
                        <h1 className={styles.name}>{employee.englishName || employee.laoName}</h1>
                        <p className={styles.laoName}>{employee.laoName}</p>
                        <div className={styles.meta}>
                            <span className={styles.code}>{employee.employeeCode}</span>
                            <span className={styles.dot}>•</span>
                            <span className={styles.department}>{employee.department?.departmentName}</span>
                            <span className={styles.dot}>•</span>
                            <span className={styles.jobTitle}>{employee.jobTitle}</span>
                            <span className={`${styles.status} ${employee.isActive ? styles.active : styles.inactive}`}>
                                {employee.isActive ? t.employeeDetail.status.active : t.employeeDetail.status.inactive}
                            </span>
                        </div>
                    </div>
                </div>
            </Card>

            {/* Tabs */}
            <div className={styles.tabs}>
                <button
                    className={`${styles.tab} ${activeTab === 'personal' ? styles.activeTab : ''}`}
                    onClick={() => setActiveTab('personal')}
                >
                    {t.employeeDetail.tabs.personal}
                </button>
                <button
                    className={`${styles.tab} ${activeTab === 'employment' ? styles.activeTab : ''}`}
                    onClick={() => setActiveTab('employment')}
                >
                    {t.employeeDetail.tabs.employment}
                </button>
                <button
                    className={`${styles.tab} ${activeTab === 'documents' ? styles.activeTab : ''}`}
                    onClick={() => setActiveTab('documents')}
                >
                    {t.employeeDetail.tabs.documents}
                </button>
            </div>

            {/* Tab Content */}
            <Card>
                <div className={styles.tabContent}>
                    {activeTab === 'personal' && (
                        <div className={styles.infoGrid}>
                            <InfoItem label={t.employeeForm.fields.email} value={employee.email || '-'} />
                            <InfoItem label={t.employeeForm.fields.phone} value={employee.phone || '-'} />
                            <InfoItem label={t.employeeForm.fields.dob} value={employee.dateOfBirth ? formatDate(employee.dateOfBirth) : '-'} />
                            <InfoItem label={t.employeeForm.fields.gender} value={employee.gender || '-'} />
                            <InfoItem label={t.employeeForm.fields.dependents} value={employee.dependentCount.toString()} />
                            <div />
                            {/* Sensitive Data - Masked */}
                            <div className={styles.sensitiveSection}>
                                <h3 className={styles.sectionTitle}>{t.employeeDetail.sections.financial}</h3>
                                <div className={styles.sensitiveGrid}>
                                    <MaskedField label={t.employeeForm.fields.bankAccount} value={employee.bankAccount || ''} />
                                    <MaskedField label={t.employeeDetail.labels.nssfId} value={employee.nssfId || ''} />
                                    <MaskedField label={t.employeeDetail.labels.taxId} value={employee.taxId || ''} />
                                </div>
                            </div>
                        </div>
                    )}

                    {activeTab === 'employment' && (
                        <div className={styles.infoGrid}>
                            <InfoItem label={t.employeeForm.fields.employeeCode} value={employee.employeeCode} />
                            <InfoItem label={t.employeeForm.fields.department} value={employee.department?.departmentName || '-'} />
                            <InfoItem label={t.employeeForm.fields.jobTitle} value={employee.jobTitle || '-'} />
                            <InfoItem label={t.employeeForm.fields.hireDate} value={employee.hireDate ? formatDate(employee.hireDate) : '-'} />
                            <InfoItem label={t.employees.table.status} value={employee.isActive ? t.employeeDetail.status.active : t.employeeDetail.status.inactive} />
                            <div />
                            {/* Salary - Masked */}
                            <div className={styles.sensitiveSection}>
                                <h3 className={styles.sectionTitle}>{t.employeeDetail.sections.compensation}</h3>
                                <div className={styles.sensitiveGrid}>
                                    <MaskedField
                                        label={t.employeeForm.fields.baseSalary}
                                        value={`${employee.baseSalary.toLocaleString()} ${employee.salaryCurrency}`}
                                    />
                                </div>
                            </div>
                        </div>
                    )}

                    {activeTab === 'documents' && (
                        <div className={styles.documentsTab}>
                            <p className={styles.emptyDocuments}>
                                {t.employeeDetail.documents.empty}
                            </p>
                            {canEdit && (
                                <Button variant="secondary" leftIcon={<UploadIcon />}>
                                    {t.employeeDetail.documents.upload}
                                </Button>
                            )}
                        </div>
                    )}
                </div>
            </Card>
        </div >
    );
}

function InfoItem({ label, value }: { label: string; value: string }) {
    return (
        <div className={styles.infoItem}>
            <span className={styles.infoLabel}>{label}</span>
            <span className={styles.infoValue}>{value}</span>
        </div>
    );
}

function getInitials(name: string): string {
    return name
        .split(' ')
        .map((n) => n[0])
        .slice(0, 2)
        .join('')
        .toUpperCase();
}

// Icons
function EditIcon() {
    return (
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
            <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
        </svg>
    );
}

function UploadIcon() {
    return (
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
            <polyline points="17 8 12 3 7 8" />
            <line x1="12" y1="3" x2="12" y2="15" />
        </svg>
    );
}
