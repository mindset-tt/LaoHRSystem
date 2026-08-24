'use client';

import { useEffect, useState } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Skeleton } from '@/components/ui/Skeleton';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { useToast } from '@/components/ui/Toast';
import { approvalsApi } from '@/lib/endpoints';
import type { ApprovalInboxItem } from '@/lib/endpoints';
import styles from './page.module.css';

/**
 * My Approvals Page — MSS: pending approvals where the current user is the
 * resolved approver. Phase 3C2.
 */
export default function MyApprovalsPage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [loading, setLoading] = useState(true);
    const [items, setItems] = useState<ApprovalInboxItem[]>([]);
    const [error, setError] = useState<string | null>(null);

    const load = async () => {
        setLoading(true);
        try {
            const data = await approvalsApi.getMyPending();
            setItems(data);
        } catch {
            setError(t.common.error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        load();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    const handleApprove = async (id: number) => {
        try {
            await approvalsApi.approve(id);
            toast.success(t.common.saved);
            await load();
        } catch {
            toast.error(t.common.error);
        }
    };

    const handleReject = async (id: number) => {
        try {
            await approvalsApi.reject(id);
            toast.success(t.common.saved);
            await load();
        } catch {
            toast.error(t.common.error);
        }
    };

    if (loading) {
        return (
            <div className={styles.page}>
                <Skeleton width={200} height={32} />
                <Card><Skeleton width="100%" height={200} /></Card>
            </div>
        );
    }

    return (
        <div className={styles.page}>
            <PageHeader
                title={t.sidebar.myApprovals}
                breadcrumbs={[{ label: t.nav.dashboard, href: '/' }, { label: t.sidebar.myApprovals }]}
            />

            {error && <Card><p className={styles.error}>{error}</p></Card>}

            {!error && items.length === 0 && (
                <Card><p className={styles.empty}>No pending approvals.</p></Card>
            )}

            {items.length > 0 && (
                <Card noPadding>
                    <div className={styles.list}>
                        {items.map((item) => (
                            <div key={item.approvalRequestId} className={styles.row}>
                                <div className={styles.info}>
                                    <span className={styles.type}>{item.requestType}</span>
                                    <span className={styles.requester}>
                                        {item.requesterName ?? `#${item.requesterEmployeeId}`}
                                    </span>
                                    <span className={styles.date}>
                                        {new Date(item.createdAt).toLocaleDateString()}
                                    </span>
                                </div>
                                <div className={styles.actions}>
                                    <Button
                                        variant="primary"
                                        size="sm"
                                        onClick={() => handleApprove(item.approvalRequestId)}
                                    >
                                        Approve
                                    </Button>
                                    <Button
                                        variant="danger"
                                        size="sm"
                                        onClick={() => handleReject(item.approvalRequestId)}
                                    >
                                        Reject
                                    </Button>
                                </div>
                            </div>
                        ))}
                    </div>
                </Card>
            )}
        </div>
    );
}
