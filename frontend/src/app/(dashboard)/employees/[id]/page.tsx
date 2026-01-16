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
import { ConfirmationModal } from '@/components/ui/ConfirmationModal';
import { formatDate } from '@/lib/datetime';
import { isHROrAdmin } from '@/lib/permissions';
import type { Employee } from '@/lib/types';
import { employeesApi } from '@/lib/endpoints/employees';
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
            try {
                if (!employeeId) return;

                const data = await employeesApi.getById(parseInt(employeeId));
                setEmployee(data);
            } catch (err) {
                console.error('Failed to load employee:', err);
                setEmployee(null);
            } finally {
                setLoading(false);
            }
        };

        loadEmployee();
    }, [employeeId]);

    const [documents, setDocuments] = useState<any[]>([
        { id: 1, name: 'Employment Connect.pdf', size: '2.4 MB', date: '2024-01-15', type: 'pdf' },
        { id: 2, name: 'ID Card.jpg', size: '1.8 MB', date: '2024-01-10', type: 'image' },
    ]);
    const [pendingDeleteId, setPendingDeleteId] = useState<number | null>(null);

    const handleUpload = () => {
        // Mock upload
        const input = document.createElement('input');
        input.type = 'file';
        input.onchange = (e) => {
            const file = (e.target as HTMLInputElement).files?.[0];
            if (file) {
                setDocuments([
                    ...documents,
                    {
                        id: Date.now(),
                        name: file.name,
                        size: `${(file.size / 1024 / 1024).toFixed(1)} MB`,
                        date: new Date().toISOString().split('T')[0],
                        type: file.type.includes('image') ? 'image' : 'pdf'
                    }
                ]);
            }
        };
        input.click();
    };

    const handleDelete = () => {
        if (pendingDeleteId) {
            setDocuments(documents.filter(d => d.id !== pendingDeleteId));
            setPendingDeleteId(null);
        }
    };

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
                            <div className={styles.documentsHeader}>
                                {canEdit && (
                                    <Button variant="secondary" leftIcon={<UploadIcon />} onClick={handleUpload}>
                                        {t.employeeDetail.documents.upload}
                                    </Button>
                                )}
                            </div>

                            {documents.length === 0 ? (
                                <p className={styles.emptyDocuments}>
                                    {t.employeeDetail.documents.empty}
                                </p>
                            ) : (
                                <div className={styles.documentsList}>
                                    {documents.map(doc => (
                                        <div key={doc.id} className={styles.documentItem}>
                                            <div className={styles.documentInfo}>
                                                <FileIcon />
                                                <div className={styles.documentMeta}>
                                                    <span className={styles.documentName}>{doc.name}</span>
                                                    <span className={styles.documentDetails}>{doc.size} • {doc.date}</span>
                                                </div>
                                            </div>
                                            <div className={styles.documentActions}>
                                                <Button variant="ghost" size="sm" leftIcon={<DownloadIcon />}>
                                                    Download
                                                </Button>
                                                {canEdit && (
                                                    <Button
                                                        variant="ghost"
                                                        size="sm"
                                                        className={styles.deleteBtn}
                                                        onClick={() => setPendingDeleteId(doc.id)}
                                                    >
                                                        <TrashIcon />
                                                    </Button>
                                                )}
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>
                    )}
                </div>
            </Card>

            <ConfirmationModal
                isOpen={!!pendingDeleteId}
                onClose={() => setPendingDeleteId(null)}
                onConfirm={handleDelete}
                title="Delete Document?"
                message="Are you sure you want to delete this document? This action cannot be undone."
                confirmText="Delete"
                variant="danger"
            />
        </div>
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

function FileIcon() {
    return (
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <path d="M13 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V9z"></path>
            <polyline points="13 2 13 9 20 9"></polyline>
        </svg>
    );
}

function DownloadIcon() {
    return (
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
            <polyline points="7 10 12 15 17 10"></polyline>
            <line x1="12" y1="15" x2="12" y2="3"></line>
        </svg>
    );
}

function TrashIcon() {
    return (
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <polyline points="3 6 5 6 21 6"></polyline>
            <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
        </svg>
    );
}
