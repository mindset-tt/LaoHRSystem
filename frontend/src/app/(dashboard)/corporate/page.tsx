'use client';

import { useEffect, useState } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { PageHeader } from '@/components/ui/PageHeader';
import { Skeleton } from '@/components/ui/Skeleton';
import { useToast } from '@/components/ui/Toast';
import { corporateApi } from '@/lib/endpoints/corporate';

export default function CorporatePage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [counts, setCounts] = useState<Record<string, number> | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let cancelled = false;
        Promise.allSettled([
            corporateApi.listFacilities(),
            corporateApi.listVehicles(),
            corporateApi.listTravelRequests({ pageSize: 1 }),
            corporateApi.listWorkOrders({ pageSize: 1 }),
        ])
            .then(([facilities, vehicles, travel, workOrders]) => {
                if (cancelled) return;
                setCounts({
                    facilities: facilities.status === 'fulfilled' ? facilities.value.length : 0,
                    vehicles: vehicles.status === 'fulfilled' ? vehicles.value.length : 0,
                    travel: travel.status === 'fulfilled' ? travel.value.totalItems : 0,
                    workOrders: workOrders.status === 'fulfilled' ? workOrders.value.totalItems : 0,
                });
            })
            .catch((err) => { console.error(err); toast.error(t.common.error); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [t.common.error, toast]);

    if (loading) return <Skeleton />;

    const cards: { label: string; value: number }[] = [
        { label: 'Facilities', value: counts?.facilities ?? 0 },
        { label: 'Vehicles', value: counts?.vehicles ?? 0 },
        { label: 'Travel Requests', value: counts?.travel ?? 0 },
        { label: 'Work Orders', value: counts?.workOrders ?? 0 },
    ];

    return (
        <div>
            <PageHeader title="Corporate Operations" subtitle="Documents, contracts, facilities, fleet, travel, visitors" />
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
