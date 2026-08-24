'use client';

import React from 'react';
import { useCallback, useEffect, useMemo, useState } from 'react';
import Link from 'next/link';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { PageHeader } from '@/components/ui/PageHeader';
import { DataTable, type DataTableColumn } from '@/components/ui/DataTable';
import { Pagination } from '@/components/ui/Pagination';
import { Select } from '@/components/ui/Select';
import { useToast } from '@/components/ui/Toast';
import { loansApi, LOAN_STATUSES, type LoanListItem } from '@/lib/endpoints';
import type { PaginatedResponse } from '@/lib/types/pagination';
import styles from '../page.module.css';

const STATUS_OPTIONS = ['', ...LOAN_STATUSES] as const;

export default function LoansPage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [pageData, setPageData] = useState<PaginatedResponse<LoanListItem>>({
        items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false,
    });
    const [loading, setLoading] = useState(true);
    const [status, setStatus] = useState<string>('');
    const [mineOnly, setMineOnly] = useState(false);
    const [search, setSearch] = useState('');
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(25);

    useEffect(() => {
        let cancelled = false;
        React.startTransition(() => {
            setLoading(true);
        });
        loansApi.list({
            status: status || undefined,
            mineOnly: mineOnly || undefined,
            search: search || undefined,
            page,
            pageSize,
        })
            .then((p) => { if (!cancelled) setPageData(p); })
            .catch((err) => {
                console.error(err);
                toast.error(t.finance.loans.messages.errorAction);
            })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [status, mineOnly, search, page, pageSize, t.finance.loans.messages.errorAction, toast]);

    const statusLabel = useCallback((s: string) =>
        (t.finance.loans.status as Record<string, string>)[s.toLowerCase()] ?? s, [t]);

    const formatLak = (n: number) => new Intl.NumberFormat('en-US').format(n) + ' ₭';

    const columns: DataTableColumn<LoanListItem>[] = useMemo(() => [
        {
            key: 'number',
            header: t.finance.loans.fields.number,
            sortBy: r => r.loanNumber,
            render: r => <strong>{r.loanNumber}</strong>,
        },
        {
            key: 'employee',
            header: t.finance.loans.fields.employee,
            render: r => r.employeeName ?? '—',
        },
        {
            key: 'purpose',
            header: t.finance.loans.fields.purpose,
            render: r => r.purpose ?? '—',
        },
        {
            key: 'principalLak',
            header: t.finance.loans.fields.principalLak,
            render: r => <span className={styles.amountCell}>{formatLak(r.principalAmountLak)}</span>,
        },
        {
            key: 'remaining',
            header: t.finance.loans.fields.remaining,
            render: r => <span className={styles.amountCell}>{formatLak(r.remainingAmount)}</span>,
        },
        {
            key: 'installments',
            header: t.finance.loans.fields.installments,
            render: r => `${r.installmentsPaid}/${r.installments}`,
        },
        {
            key: 'startDate',
            header: t.finance.loans.fields.startDate,
            render: r => new Date(r.startDate).toLocaleDateString(),
        },
        {
            key: 'status',
            header: t.finance.loans.fields.status,
            render: r => (
                <span className={`${styles.badge} ${styles[`loan_${r.status.toLowerCase()}`]}`}>
                    {statusLabel(r.status)}
                </span>
            ),
        },
    ], [t, statusLabel]);

    return (
        <div className={styles.page}>
            <PageHeader
                title={t.finance.loans.title}
                subtitle={t.finance.loans.subtitle}
                breadcrumbs={[
                    { label: t.nav.dashboard, href: '/' },
                    { label: t.finance.loans.title },
                ]}
                actions={
                    <Link href="/finance/loans/new">
                        <Button>{t.finance.loans.newLoan}</Button>
                    </Link>
                }
            />

            <Card className={styles.filtersCard}>
                <div className={styles.filters}>
                    <Select
                        value={status}
                        onChange={(e) => setStatus(e.target.value)}
                        options={STATUS_OPTIONS.map(s => ({
                            value: s,
                            label: s === '' ? t.finance.loans.fields.status : statusLabel(s),
                        }))}
                    />
                    <input
                        type="search"
                        placeholder={t.projects.searchPlaceholder}
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                        className={styles.searchInput}
                    />
                    <label className={styles.checkbox}>
                        <input
                            type="checkbox"
                            checked={mineOnly}
                            onChange={(e) => setMineOnly(e.target.checked)}
                        />
                        {t.finance.expenses.filters.mineOnly}
                    </label>
                </div>
            </Card>

            <Card noPadding>
                <DataTable<LoanListItem>
                    columns={columns}
                    rows={pageData.items}
                    rowKey={r => r.loanId}
                    isLoading={loading}
                    emptyTitle={t.finance.loans.empty.title}
                    emptyDescription={t.finance.loans.empty.description}
                />
                {!loading && (
                    <Pagination
                        page={pageData.page}
                        pageSize={pageData.pageSize}
                        totalItems={pageData.totalItems}
                        totalPages={pageData.totalPages}
                        hasNext={pageData.hasNext}
                        hasPrevious={pageData.hasPrevious}
                        onPageChange={setPage}
                        onPageSizeChange={setPageSize}
                    />
                )}
            </Card>
        </div>
    );
}
