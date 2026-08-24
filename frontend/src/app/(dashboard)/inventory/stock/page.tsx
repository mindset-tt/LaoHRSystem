'use client';

import { useEffect, useState } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { PageHeader } from '@/components/ui/PageHeader';
import { DataTable, type DataTableColumn } from '@/components/ui/DataTable';
import { useToast } from '@/components/ui/Toast';
import { backOfficeApi, type StockBalanceDto } from '@/lib/endpoints/backOffice';

export default function StockPage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [balances, setBalances] = useState<StockBalanceDto[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let cancelled = false;
        backOfficeApi.getStockBalances()
            .then((b) => { if (!cancelled) setBalances(b); })
            .catch((err) => { console.error(err); toast.error(t.common.error); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [t.common.error, toast]);

    const columns: DataTableColumn<StockBalanceDto>[] = [
        { key: 'itemId', header: 'Item ID', render: (r) => r.itemId },
        { key: 'warehouseId', header: 'Warehouse ID', render: (r) => r.warehouseId },
        { key: 'onHand', header: 'On Hand', render: (r) => r.onHand.toLocaleString() },
    ];

    return (
        <div>
            <PageHeader title={t.sidebar.stock} subtitle={t.sidebar.inventory} />
            <Card>
                <DataTable
                    columns={columns}
                    rows={balances}
                    rowKey={(r) => `${r.itemId}-${r.warehouseId}`}
                    isLoading={loading}
                />
            </Card>
        </div>
    );
}
