'use client';

import { useEffect, useState } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { PageHeader } from '@/components/ui/PageHeader';
import { Skeleton } from '@/components/ui/Skeleton';
import { useToast } from '@/components/ui/Toast';
import { financeAccountingApi, type ApAgingDto } from '@/lib/endpoints/financeAccounting';

export default function FinanceDashboardPage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [aging, setAging] = useState<ApAgingDto | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let cancelled = false;
        financeAccountingApi.getAging()
            .then((a) => { if (!cancelled) setAging(a); })
            .catch((err) => { console.error(err); toast.error(t.common.error); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [t.common.error, toast]);

    if (loading) return <Skeleton />;

    const cards: { label: string; value: number }[] = [
        { label: 'AP Current', value: aging?.current ?? 0 },
        { label: 'AP 1–30 days', value: aging?.days1To30 ?? 0 },
        { label: 'AP 31–60 days', value: aging?.days31To60 ?? 0 },
        { label: 'AP 61–90 days', value: aging?.days61To90 ?? 0 },
        { label: 'AP Over 90 days', value: aging?.over90 ?? 0 },
        { label: 'AP Total Outstanding', value: aging?.total ?? 0 },
    ];

    return (
        <div>
            <PageHeader title="Finance" subtitle="Accounts payable overview" />
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(220px, 1fr))', gap: '1rem' }}>
                {cards.map((c) => (
                    <Card key={c.label}>
                        <div style={{ fontSize: '0.85rem', opacity: 0.7 }}>{c.label}</div>
                        <div style={{ fontSize: '1.75rem', fontWeight: 600 }}>{c.value.toLocaleString()}</div>
                    </Card>
                ))}
            </div>
        </div>
    );
}
