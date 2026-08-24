'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Skeleton } from '@/components/ui/Skeleton';
import { PageHeader } from '@/components/ui/PageHeader';
import { pmApi } from '@/lib/endpoints';
import type { PortfolioItem } from '@/lib/endpoints';
import styles from './page.module.css';

/**
 * Portfolio — cross-project overview. Phase 3C4.
 */
export default function PortfolioPage() {
    const { t } = useLanguage();
    const [items, setItems] = useState<PortfolioItem[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;
        pmApi.getPortfolio()
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
                <PageHeader title="Portfolio" />
                <Card><p className={styles.error}>{error}</p></Card>
            </div>
        );
    }

    return (
        <div className={styles.page}>
            <PageHeader
                title="Portfolio"
                breadcrumbs={[{ label: t.nav.dashboard, href: '/' }, { label: 'Portfolio' }]}
            />

            {items.length === 0 ? (
                <Card><p className={styles.empty}>No projects.</p></Card>
            ) : (
                <Card noPadding>
                    <table className={styles.table}>
                        <thead>
                            <tr>
                                <th>Project</th>
                                <th>Status</th>
                                <th>Health</th>
                                <th>Progress</th>
                                <th>Start</th>
                                <th>Due</th>
                                <th>Risks</th>
                                <th>Overdue</th>
                            </tr>
                        </thead>
                        <tbody>
                            {items.map(p => (
                                <tr key={p.projectId}>
                                    <td>
                                        <Link href={`/projects/${p.projectId}`} className={styles.link}>
                                            {p.code} · {p.name}
                                        </Link>
                                    </td>
                                    <td><span className={styles.badge}>{p.status}</span></td>
                                    <td><span className={`${styles.badge} ${styles[`health_${p.health.toLowerCase()}`]}`}>{p.health}</span></td>
                                    <td>{p.progressPercent}%</td>
                                    <td>{p.startDate ? new Date(p.startDate).toLocaleDateString() : '—'}</td>
                                    <td>{p.dueDate ? new Date(p.dueDate).toLocaleDateString() : '—'}</td>
                                    <td>{p.openRiskCount}</td>
                                    <td>{p.overdueTaskCount}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </Card>
            )}
        </div>
    );
}
