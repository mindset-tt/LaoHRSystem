'use client';

import { useEffect, useState } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Skeleton } from '@/components/ui/Skeleton';
import { PageHeader } from '@/components/ui/PageHeader';
import { pmApi } from '@/lib/endpoints';
import type { ResourceWorkload } from '@/lib/endpoints';
import styles from './page.module.css';

/**
 * Capacity / Workload — resource allocation across projects. Phase 3C4.
 */
export default function CapacityPage() {
    const { t } = useLanguage();
    const [items, setItems] = useState<ResourceWorkload[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;
        pmApi.getCapacity()
            .then(d => { if (!cancelled) setItems(d); })
            .catch(() => { if (!cancelled) setError(t.common.error); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [t.common.error]);

    if (loading) {
        return (
            <div className={styles.page}>
                <Skeleton width={200} height={32} />
                <Card><Skeleton width="100%" height={200} /></Card>
            </div>
        );
    }

    if (error) {
        return (
            <div className={styles.page}>
                <PageHeader title="Capacity" />
                <Card><p className={styles.error}>{error}</p></Card>
            </div>
        );
    }

    return (
        <div className={styles.page}>
            <PageHeader
                title="Capacity / Workload"
                breadcrumbs={[{ label: t.nav.dashboard, href: '/' }, { label: 'Capacity' }]}
            />

            {items.length === 0 ? (
                <Card><p className={styles.empty}>No resource allocations.</p></Card>
            ) : (
                <Card noPadding>
                    <table className={styles.table}>
                        <thead>
                            <tr>
                                <th>Employee</th>
                                <th>Allocation</th>
                                <th>Active Projects</th>
                                <th>Status</th>
                            </tr>
                        </thead>
                        <tbody>
                            {items.map(w => (
                                <tr key={w.employeeId}>
                                    <td>{w.employeeName}</td>
                                    <td>{w.totalAllocationPercent}%</td>
                                    <td>{w.activeProjectCount}</td>
                                    <td>
                                        <span className={`${styles.badge} ${w.isOverallocated ? styles.over : styles.ok}`}>
                                            {w.isOverallocated ? 'Overallocated' : 'Balanced'}
                                        </span>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </Card>
            )}
        </div>
    );
}
