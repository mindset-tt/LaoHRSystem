'use client';

import { useEffect, useState } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { PageHeader } from '@/components/ui/PageHeader';
import { DataTable, type DataTableColumn } from '@/components/ui/DataTable';
import { Pagination } from '@/components/ui/Pagination';
import { useToast } from '@/components/ui/Toast';
import { backOfficeApi, type ServiceRequestListItem } from '@/lib/endpoints/backOffice';
import type { PaginatedResponse } from '@/lib/types/pagination';

export default function ServiceRequestsPage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [pageData, setPageData] = useState<PaginatedResponse<ServiceRequestListItem>>({
        items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false,
    });
    const [loading, setLoading] = useState(true);
    const [page, setPage] = useState(1);

    useEffect(() => {
        let cancelled = false;
        backOfficeApi.listServiceRequests({ page, pageSize: 25 })
            .then((p) => { if (!cancelled) setPageData(p); })
            .catch((err) => { console.error(err); toast.error(t.common.error); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [page, t.common.error, toast]);

    const columns: DataTableColumn<ServiceRequestListItem>[] = [
        { key: 'requestNumber', header: 'Number', render: (r) => r.requestNumber },
        { key: 'subject', header: 'Subject', render: (r) => r.subject },
        { key: 'categoryName', header: 'Category', render: (r) => r.categoryName ?? '—' },
        { key: 'priority', header: 'Priority', render: (r) => r.priority },
        { key: 'status', header: 'Status', render: (r) => r.status },
    ];

    return (
        <div>
            <PageHeader title={t.sidebar.serviceRequests} subtitle={t.sidebar.serviceRequests} />
            <Card>
                <DataTable
                    columns={columns}
                    rows={pageData.items}
                    rowKey={(r) => r.serviceRequestId}
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
