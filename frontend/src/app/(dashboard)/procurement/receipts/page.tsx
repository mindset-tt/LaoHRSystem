'use client';

import { useEffect, useState } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { PageHeader } from '@/components/ui/PageHeader';
import { DataTable, type DataTableColumn } from '@/components/ui/DataTable';
import { Pagination } from '@/components/ui/Pagination';
import { useToast } from '@/components/ui/Toast';
import { backOfficeApi, type GoodsReceiptListItem } from '@/lib/endpoints/backOffice';
import type { PaginatedResponse } from '@/lib/types/pagination';

export default function GoodsReceiptsPage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [pageData, setPageData] = useState<PaginatedResponse<GoodsReceiptListItem>>({
        items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false,
    });
    const [loading, setLoading] = useState(true);
    const [page, setPage] = useState(1);

    useEffect(() => {
        let cancelled = false;
        backOfficeApi.listGoodsReceipts({ page, pageSize: 25 })
            .then((p) => { if (!cancelled) setPageData(p); })
            .catch((err) => { console.error(err); toast.error(t.common.error); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [page, t.common.error, toast]);

    const columns: DataTableColumn<GoodsReceiptListItem>[] = [
        { key: 'receiptNumber', header: 'Receipt Number', render: (r) => r.receiptNumber },
        { key: 'poNumber', header: 'PO', render: (r) => r.poNumber ?? '—' },
        { key: 'receivedByName', header: 'Received By', render: (r) => r.receivedByName ?? '—' },
        { key: 'receivedDate', header: 'Received Date', render: (r) => r.receivedDate.slice(0, 10) },
        { key: 'status', header: 'Status', render: (r) => r.status },
    ];

    return (
        <div>
            <PageHeader title={t.sidebar.goodsReceipts} subtitle={t.sidebar.procurement} />
            <Card>
                <DataTable
                    columns={columns}
                    rows={pageData.items}
                    rowKey={(r) => r.goodsReceiptId}
                    isLoading={loading}
                />
                <Pagination
                    page={page}
                    pageSize={25}
                    totalItems={pageData.totalItems}
                    totalPages={pageData.totalPages}
                    hasNext={pageData.hasNext}
                    hasPrevious={pageData.hasPrevious}
                    onPageChange={setPage}
                />
            </Card>
        </div>
    );
}
