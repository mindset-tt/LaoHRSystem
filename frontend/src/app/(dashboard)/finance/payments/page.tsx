'use client';

import { useEffect, useState } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { PageHeader } from '@/components/ui/PageHeader';
import { DataTable, type DataTableColumn } from '@/components/ui/DataTable';
import { Pagination } from '@/components/ui/Pagination';
import { useToast } from '@/components/ui/Toast';
import { financeAccountingApi, type PaymentListItem } from '@/lib/endpoints/financeAccounting';
import type { PaginatedResponse } from '@/lib/types/pagination';

export default function PaymentsPage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [pageData, setPageData] = useState<PaginatedResponse<PaymentListItem>>({
        items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false,
    });
    const [loading, setLoading] = useState(true);
    const [page, setPage] = useState(1);

    useEffect(() => {
        let cancelled = false;
        financeAccountingApi.listPayments({ page, pageSize: 25 })
            .then((p) => { if (!cancelled) setPageData(p); })
            .catch((err) => { console.error(err); toast.error(t.common.error); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [page, t.common.error, toast]);

    const columns: DataTableColumn<PaymentListItem>[] = [
        { key: 'paymentNumber', header: 'Payment', render: (r) => r.paymentNumber },
        { key: 'paymentDate', header: 'Date', render: (r) => r.paymentDate.slice(0, 10) },
        { key: 'paymentMethod', header: 'Method', render: (r) => r.paymentMethod },
        { key: 'amount', header: 'Amount', render: (r) => `${r.amount.toLocaleString()} ${r.currency}` },
        { key: 'status', header: 'Status', render: (r) => r.status },
    ];

    return (
        <div>
            <PageHeader title="Payments" subtitle="Accounts Payable" />
            <Card>
                <DataTable columns={columns} rows={pageData.items} rowKey={(r) => r.paymentId} isLoading={loading} />
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
