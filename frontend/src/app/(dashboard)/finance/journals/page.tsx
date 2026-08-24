'use client';

import { useEffect, useState } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { PageHeader } from '@/components/ui/PageHeader';
import { DataTable, type DataTableColumn } from '@/components/ui/DataTable';
import { Pagination } from '@/components/ui/Pagination';
import { useToast } from '@/components/ui/Toast';
import { financeAccountingApi, type JournalEntryDto } from '@/lib/endpoints/financeAccounting';
import type { PaginatedResponse } from '@/lib/types/pagination';

export default function JournalsPage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [pageData, setPageData] = useState<PaginatedResponse<JournalEntryDto>>({
        items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false,
    });
    const [loading, setLoading] = useState(true);
    const [page, setPage] = useState(1);

    useEffect(() => {
        let cancelled = false;
        financeAccountingApi.listJournals({ page, pageSize: 25 })
            .then((p) => { if (!cancelled) setPageData(p); })
            .catch((err) => { console.error(err); toast.error(t.common.error); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [page, t.common.error, toast]);

    const columns: DataTableColumn<JournalEntryDto>[] = [
        { key: 'journalNumber', header: 'Journal', render: (r) => r.journalNumber },
        { key: 'postingDate', header: 'Date', render: (r) => r.postingDate.slice(0, 10) },
        { key: 'sourceType', header: 'Source', render: (r) => r.sourceType },
        { key: 'description', header: 'Description', render: (r) => r.description ?? '—' },
        { key: 'status', header: 'Status', render: (r) => r.status },
    ];

    return (
        <div>
            <PageHeader title="Journals" subtitle="General Ledger" />
            <Card>
                <DataTable columns={columns} rows={pageData.items} rowKey={(r) => r.journalEntryId} isLoading={loading} />
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
