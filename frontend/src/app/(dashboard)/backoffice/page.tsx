'use client';

import { useEffect, useState } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { PageHeader } from '@/components/ui/PageHeader';
import { Skeleton } from '@/components/ui/Skeleton';
import { useToast } from '@/components/ui/Toast';
import { apiClient } from '@/lib/apiClient';

interface BackOfficeKpi {
    pendingApprovals: number;
    openPurchaseRequests: number;
    pendingPurchaseRequests: number;
    openPurchaseOrders: number;
    pendingReceipts: number;
    lowStockItems: number;
    outOfStockItems: number;
    assetsInMaintenance: number;
    contractsExpiring: number;
    openServiceRequests: number;
    myOpenPurchaseRequests: number;
    myOpenServiceRequests: number;
}

export default function BackOfficePage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [kpi, setKpi] = useState<BackOfficeKpi | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let cancelled = false;
        apiClient.get<BackOfficeKpi>('/api/backoffice/command-center')
            .then((d) => { if (!cancelled) setKpi(d); })
            .catch((err) => { console.error(err); toast.error(t.common.error); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [t.common.error, toast]);

    if (loading) return <Skeleton />;

    const cards: { label: string; value: number }[] = [
        { label: 'Pending Approvals', value: kpi?.pendingApprovals ?? 0 },
        { label: 'Open Purchase Requests', value: kpi?.openPurchaseRequests ?? 0 },
        { label: 'Pending Purchase Requests', value: kpi?.pendingPurchaseRequests ?? 0 },
        { label: 'Open Purchase Orders', value: kpi?.openPurchaseOrders ?? 0 },
        { label: 'Pending Receipts', value: kpi?.pendingReceipts ?? 0 },
        { label: 'Low Stock Items', value: kpi?.lowStockItems ?? 0 },
        { label: 'Out of Stock Items', value: kpi?.outOfStockItems ?? 0 },
        { label: 'Assets In Maintenance', value: kpi?.assetsInMaintenance ?? 0 },
        { label: 'Contracts Expiring', value: kpi?.contractsExpiring ?? 0 },
        { label: 'Open Internal Requests', value: kpi?.openServiceRequests ?? 0 },
        { label: 'My Open Purchase Requests', value: kpi?.myOpenPurchaseRequests ?? 0 },
        { label: 'My Open Internal Requests', value: kpi?.myOpenServiceRequests ?? 0 },
    ];

    return (
        <div>
            <PageHeader title="Back Office" subtitle="What needs my attention today" />
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(220px, 1fr))', gap: '1rem' }}>
                {cards.map((c) => (
                    <Card key={c.label}>
                        <div style={{ fontSize: '0.85rem', opacity: 0.7 }}>{c.label}</div>
                        <div style={{ fontSize: '1.75rem', fontWeight: 600 }}>{c.value}</div>
                    </Card>
                ))}
            </div>
        </div>
    );
}
