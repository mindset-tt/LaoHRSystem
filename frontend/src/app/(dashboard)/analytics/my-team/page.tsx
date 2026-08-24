'use client';

import { useEffect, useState } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Skeleton } from '@/components/ui/Skeleton';
import { PageHeader } from '@/components/ui/PageHeader';
import { KpiCard } from '@/components/dashboard/KpiCard';
import { BreakdownList } from '@/components/dashboard/BreakdownList';
import { analyticsApi } from '@/lib/endpoints';
import type { ManagerDashboard } from '@/lib/endpoints';
import styles from './page.module.css';

/**
 * Manager Dashboard — MSS team overview. Phase 3C3.
 */
export default function ManagerDashboardPage() {
    const { t } = useLanguage();
    const [data, setData] = useState<ManagerDashboard | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;
        analyticsApi.getManager()
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
                <PageHeader title="My Team Dashboard" />
                <Card><p className={styles.error}>{error ?? t.common.error}</p></Card>
            </div>
        );
    }

    return (
        <div className={styles.page}>
            <PageHeader
                title="My Team Dashboard"
                breadcrumbs={[{ label: t.nav.dashboard, href: '/' }, { label: 'My Team' }]}
            />

            <div className={styles.grid}>
                {data.kpis.map(kpi => (
                    <KpiCard key={kpi.id} label={kpi.label} value={kpi.value} unit={kpi.unit} />
                ))}
            </div>

            <Card header={<h3 className={styles.sectionTitle}>Team Leave by Status</h3>}>
                <BreakdownList title="" items={data.teamLeaveByStatus} />
            </Card>
        </div>
    );
}
