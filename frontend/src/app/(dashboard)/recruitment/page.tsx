'use client';

import { useEffect, useState } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Skeleton } from '@/components/ui/Skeleton';
import { PageHeader } from '@/components/ui/PageHeader';
import { recruitmentApi } from '@/lib/endpoints';
import type { JobRequisition, JobOpening, Candidate, Offer } from '@/lib/endpoints';
import styles from './page.module.css';

/**
 * Recruitment Dashboard — HR/recruiter overview. Phase 3C5.
 */
export default function RecruitmentPage() {
    const { t } = useLanguage();
    const [requisitions, setRequisitions] = useState<JobRequisition[]>([]);
    const [openings, setOpenings] = useState<JobOpening[]>([]);
    const [candidates, setCandidates] = useState<Candidate[]>([]);
    const [offers, setOffers] = useState<Offer[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;
        Promise.all([
            recruitmentApi.getRequisitions(),
            recruitmentApi.getOpenings('OPEN'),
            recruitmentApi.getCandidates(),
            recruitmentApi.getOffers(),
        ])
            .then(([r, o, c, of]) => {
                if (cancelled) return;
                setRequisitions(r);
                setOpenings(o);
                setCandidates(c);
                setOffers(of);
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
                <PageHeader title="Recruitment" />
                <Card><p className={styles.error}>{error}</p></Card>
            </div>
        );
    }

    const openRequisitions = requisitions.filter(r => r.status === 'APPROVED' || r.status === 'OPEN').length;
    const pendingOffers = offers.filter(o => o.status === 'SENT' || o.status === 'PENDING_APPROVAL').length;

    return (
        <div className={styles.page}>
            <PageHeader
                title="Recruitment"
                breadcrumbs={[{ label: t.nav.dashboard, href: '/' }, { label: 'Recruitment' }]}
            />

            <div className={styles.grid}>
                <Card><div className={styles.stat}><span className={styles.statValue}>{openRequisitions}</span><span className={styles.statLabel}>Open Requisitions</span></div></Card>
                <Card><div className={styles.stat}><span className={styles.statValue}>{openings.length}</span><span className={styles.statLabel}>Open Openings</span></div></Card>
                <Card><div className={styles.stat}><span className={styles.statValue}>{candidates.length}</span><span className={styles.statLabel}>Candidates</span></div></Card>
                <Card><div className={styles.stat}><span className={styles.statValue}>{pendingOffers}</span><span className={styles.statLabel}>Pending Offers</span></div></Card>
            </div>

            <Card header={<h3 className={styles.sectionTitle}>Open Requisitions</h3>}>
                {requisitions.filter(r => r.status === 'APPROVED' || r.status === 'OPEN').length === 0 ? (
                    <p className={styles.empty}>No open requisitions.</p>
                ) : (
                    <ul className={styles.list}>
                        {requisitions.filter(r => r.status === 'APPROVED' || r.status === 'OPEN').map(r => (
                            <li key={r.requisitionId} className={styles.row}>
                                <span className={styles.name}>{r.requisitionNumber}</span>
                                <span className={styles.badge}>{r.status}</span>
                            </li>
                        ))}
                    </ul>
                )}
            </Card>
        </div>
    );
}
