'use client';

import { useEffect, useState } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Skeleton } from '@/components/ui/Skeleton';
import { PageHeader } from '@/components/ui/PageHeader';
import { KpiCard } from '@/components/dashboard/KpiCard';
import { BreakdownList } from '@/components/dashboard/BreakdownList';
import { analyticsApi } from '@/lib/endpoints';
import type { HrDashboard } from '@/lib/endpoints';
import styles from './page.module.css';

/**
 * HR Dashboard — workforce overview. Phase 3C3.
 */
export default function HrDashboardPage() {
    const { t } = useLanguage();
    const [data, setData] = useState<HrDashboard | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;
        analyticsApi.getHr()
            .then(d => { if (!cancelled) setData(d); })
            .catch(() => { if (!cancelled) setError(t.common.error); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [t.common.error]);

    if (loading) {
        return (
            <div className={styles.page}>
                <Skeleton width={200} height={32} />
                <div className={styles.grid}><Skeleton width="100%" height={120} /><Skeleton width="100%" height={120} /></div>
            </div>
        );
    }

    if (error || !data) {
        return (
            <div className={styles.page}>
                <PageHeader title="HR Dashboard" />
                <Card><p className={styles.error}>{error ?? t.common.error}</p></Card>
            </div>
        );
    }

    return (
        <div className={styles.page}>
            <PageHeader
                title="HR Dashboard"
                breadcrumbs={[{ label: t.nav.dashboard, href: '/' }, { label: 'HR' }]}
            />

            <div className={styles.grid}>
                {data.kpis.map(kpi => (
                    <KpiCard key={kpi.id} label={kpi.label} value={kpi.value} unit={kpi.unit} />
                ))}
            </div>

            <div className={styles.columns}>
                <Card header={<h3 className={styles.sectionTitle}>Headcount by Department</h3>}>
                    <BreakdownList title="" items={data.headcountByDepartment} />
                </Card>
                <Card header={<h3 className={styles.sectionTitle}>Headcount by Location</h3>}>
                    <BreakdownList title="" items={data.headcountByLocation} />
                </Card>
            </div>

            <div className={styles.columns}>
                <Card header={<h3 className={styles.sectionTitle}>Headcount by Position</h3>}>
                    <BreakdownList title="" items={data.headcountByPosition} />
                </Card>
                <Card header={<h3 className={styles.sectionTitle}>Leave Days by Type</h3>}>
                    <BreakdownList title="" items={data.leaveByType} />
                </Card>
            </div>
        </div>
    );
}
