'use client';

import { useEffect, useState } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Skeleton } from '@/components/ui/Skeleton';
import { PageHeader } from '@/components/ui/PageHeader';
import { performanceApi } from '@/lib/endpoints';
import type { Goal, PerformanceReview, Feedback, OneOnOne } from '@/lib/endpoints';
import styles from './page.module.css';

/**
 * Performance Home — employee/manager performance workspace. Phase 3C6.
 */
export default function PerformancePage() {
    const { t } = useLanguage();
    const [goals, setGoals] = useState<Goal[]>([]);
    const [reviews, setReviews] = useState<PerformanceReview[]>([]);
    const [feedback, setFeedback] = useState<Feedback[]>([]);
    const [oneOnOnes, setOneOnOnes] = useState<OneOnOne[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;
        Promise.all([
            performanceApi.getGoals(),
            performanceApi.getReviews(),
            performanceApi.getFeedback(),
            performanceApi.getOneOnOnes(),
        ])
            .then(([g, r, f, o]) => {
                if (cancelled) return;
                setGoals(g);
                setReviews(r);
                setFeedback(f);
                setOneOnOnes(o);
            })
            .catch(() => { if (!cancelled) setError(t.common.error); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [t.common.error]);

    if (loading) {
        return (
            <div className={styles.page}>
                <Skeleton width={200} height={32} />
                <div className={styles.grid}>
                    <Skeleton width="100%" height={100} />
                    <Skeleton width="100%" height={100} />
                    <Skeleton width="100%" height={100} />
                    <Skeleton width="100%" height={100} />
                </div>
            </div>
        );
    }

    if (error) {
        return (
            <div className={styles.page}>
                <PageHeader title="Performance" />
                <Card><p className={styles.error}>{error}</p></Card>
            </div>
        );
    }

    const activeGoals = goals.filter(g => g.status === 'ACTIVE').length;
    const pendingReviews = reviews.filter(r => r.status === 'SELF_REVIEW' || r.status === 'MANAGER_REVIEW').length;
    const upcomingOneOnOnes = oneOnOnes.filter(o => o.status === 'SCHEDULED').length;

    return (
        <div className={styles.page}>
            <PageHeader
                title="Performance"
                breadcrumbs={[{ label: t.nav.dashboard, href: '/' }, { label: 'Performance' }]}
            />

            <div className={styles.grid}>
                <Card><div className={styles.stat}><span className={styles.statValue}>{activeGoals}</span><span className={styles.statLabel}>Active Goals</span></div></Card>
                <Card><div className={styles.stat}><span className={styles.statValue}>{pendingReviews}</span><span className={styles.statLabel}>Pending Reviews</span></div></Card>
                <Card><div className={styles.stat}><span className={styles.statValue}>{feedback.length}</span><span className={styles.statLabel}>Feedback</span></div></Card>
                <Card><div className={styles.stat}><span className={styles.statValue}>{upcomingOneOnOnes}</span><span className={styles.statLabel}>Upcoming 1:1s</span></div></Card>
            </div>

            <Card header={<h3 className={styles.sectionTitle}>My Goals</h3>}>
                {goals.length === 0 ? (
                    <p className={styles.empty}>No goals.</p>
                ) : (
                    <ul className={styles.list}>
                        {goals.map(g => (
                            <li key={g.goalId} className={styles.row}>
                                <span className={styles.name}>{g.title}</span>
                                <span className={styles.badge}>{g.status}</span>
                            </li>
                        ))}
                    </ul>
                )}
            </Card>
        </div>
    );
}
