'use client';

import { useEffect, useState } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Skeleton } from '@/components/ui/Skeleton';
import { PageHeader } from '@/components/ui/PageHeader';
import { employeesApi } from '@/lib/endpoints';
import type { Employee } from '@/lib/types';
import styles from './page.module.css';

/**
 * My Profile Page — ESS: the current user's own employee profile.
 * Phase 3C2.
 */
export default function MyProfilePage() {
    const { t, language } = useLanguage();
    const [loading, setLoading] = useState(true);
    const [profile, setProfile] = useState<Employee | null>(null);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;
        employeesApi.getMe()
            .then(p => { if (!cancelled) setProfile(p); })
            .catch(() => { if (!cancelled) setError(t.common.error); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [t.common.error]);

    if (loading) {
        return (
            <div className={styles.page}>
                <Skeleton width={200} height={32} />
                <Card><Skeleton width="100%" height={240} /></Card>
            </div>
        );
    }

    if (error || !profile) {
        return (
            <div className={styles.page}>
                <PageHeader title={t.sidebar.myProfile} />
                <Card><p className={styles.error}>{error ?? t.common.error}</p></Card>
            </div>
        );
    }

    const name = language === 'lo' ? profile.laoName : (profile.englishName || profile.laoName);

    return (
        <div className={styles.page}>
            <PageHeader
                title={t.sidebar.myProfile}
                breadcrumbs={[{ label: t.nav.dashboard, href: '/' }, { label: t.sidebar.myProfile }]}
            />

            <Card>
                <div className={styles.header}>
                    <div className={styles.avatar}>{name.charAt(0)}</div>
                    <div>
                        <h2 className={styles.name}>{name}</h2>
                        <p className={styles.code}>{profile.employeeCode}</p>
                    </div>
                </div>
            </Card>

            <div className={styles.grid}>
                <Card header={<h3 className={styles.sectionTitle}>{t.employees.table.employee}</h3>}>
                    <dl className={styles.details}>
                        <Detail label={t.employees.table.jobTitle} value={profile.jobTitle} />
                        <Detail label={t.employees.table.department} value={profile.department?.departmentName} />
                        <Detail label="Email" value={profile.email} />
                        <Detail label="Phone" value={profile.phone} />
                        <Detail label="Gender" value={profile.gender} />
                    </dl>
                </Card>

                <Card header={<h3 className={styles.sectionTitle}>{t.employeeDetail.sections.compensation}</h3>}>
                    <dl className={styles.details}>
                        <Detail label={t.employeeDetail.labels.salaryCurrency} value={profile.salaryCurrency} />
                        <Detail label="Base Salary" value={profile.baseSalary?.toLocaleString()} />
                        <Detail label={t.employeeDetail.labels.nssfId} value={profile.nssfId} />
                        <Detail label={t.employeeDetail.labels.taxId} value={profile.taxId} />
                    </dl>
                </Card>
            </div>
        </div>
    );
}

function Detail({ label, value }: { label: string; value?: string | number | null }) {
    return (
        <div className={styles.detailRow}>
            <dt className={styles.detailLabel}>{label}</dt>
            <dd className={styles.detailValue}>{value ?? '—'}</dd>
        </div>
    );
}
