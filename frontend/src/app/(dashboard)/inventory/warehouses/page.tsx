'use client';

import { useEffect, useState } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { PageHeader } from '@/components/ui/PageHeader';
import { DataTable, type DataTableColumn } from '@/components/ui/DataTable';
import { useToast } from '@/components/ui/Toast';
import { backOfficeApi, type WarehouseDto } from '@/lib/endpoints/backOffice';

export default function WarehousesPage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [warehouses, setWarehouses] = useState<WarehouseDto[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let cancelled = false;
        backOfficeApi.listWarehouses()
            .then((w) => { if (!cancelled) setWarehouses(w); })
            .catch((err) => { console.error(err); toast.error(t.common.error); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [t.common.error, toast]);

    const columns: DataTableColumn<WarehouseDto>[] = [
        { key: 'code', header: 'Code', render: (r) => r.code },
        { key: 'name', header: 'Name', render: (r) => r.name },
        { key: 'address', header: 'Address', render: (r) => r.address ?? '—' },
        { key: 'status', header: 'Status', render: (r) => r.status },
    ];

    return (
        <div>
            <PageHeader title={t.sidebar.warehouses} subtitle={t.sidebar.inventory} />
            <Card>
                <DataTable
                    columns={columns}
                    rows={warehouses}
                    rowKey={(r) => r.warehouseId}
                    isLoading={loading}
                />
            </Card>
        </div>
    );
}
