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
import { expensesApi, EXPENSE_STATUSES, type ExpenseCategoryItem, type ExpenseListItem } from '@/lib/endpoints';
import type { PaginatedResponse } from '@/lib/types/pagination';
import styles from '../page.module.css';

const STATUS_OPTIONS = ['', ...EXPENSE_STATUSES] as const;

export default function ExpensesPage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [pageData, setPageData] = useState<PaginatedResponse<ExpenseListItem>>({
        items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false,
    });
    const [categories, setCategories] = useState<ExpenseCategoryItem[]>([]);
    const [loading, setLoading] = useState(true);
    const [status, setStatus] = useState<string>('');
    const [categoryId, setCategoryId] = useState<string>('');
    const [mineOnly, setMineOnly] = useState(false);
    const [search, setSearch] = useState('');
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(25);

    useEffect(() => {
        expensesApi.categories().then(setCategories).catch(() => undefined);
    }, []);

    useEffect(() => {
        let cancelled = false;
        React.startTransition(() => {
            setLoading(true);
        });
        expensesApi.list({
            status: status || undefined,
            categoryId: categoryId ? Number(categoryId) : undefined,
            mineOnly: mineOnly || undefined,
            search: search || undefined,
            page,
            pageSize,
        })
            .then((p) => { if (!cancelled) setPageData(p); })
            .catch((err) => {
                console.error(err);
                toast.error(t.finance.expenses.messages.errorAction);
            })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [status, categoryId, mineOnly, search, page, pageSize, t.finance.expenses.messages.errorAction, toast]);

    const statusLabel = useCallback((s: string) =>
        (t.finance.expenses.status as Record<string, string>)?.[s.toLowerCase()] ?? s, [t]);

    const formatLak = (n: number) => new Intl.NumberFormat('en-US').format(n) + ' ₭';

    const columns: DataTableColumn<ExpenseListItem>[] = useMemo(() => [
        {
            key: 'number',
            header: t.finance.expenses.fields.number,
            sortBy: r => r.expenseNumber,
            render: r => <strong>{r.expenseNumber}</strong>,
        },
        {
            key: 'title',
            header: t.finance.expenses.fields.title,
            render: r => r.title,
        },
        {
            key: 'category',
            header: t.finance.expenses.fields.category,
            render: r => r.categoryName ?? '—',
        },
        {
            key: 'employee',
            header: t.finance.expenses.fields.employee,
            render: r => r.employeeName ?? '—',
        },
        {
            key: 'date',
            header: t.finance.expenses.fields.date,
            render: r => new Date(r.expenseDate).toLocaleDateString(),
        },
        {
            key: 'amount',
            header: t.finance.expenses.fields.amount,
            render: r => (
                <span className={styles.amountCell}>
                    {new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(r.amount)} {r.currency}
                </span>
            ),
        },
        {
            key: 'amountLak',
            header: t.finance.expenses.fields.amountLak,
            render: r => <span className={styles.amountCell}>{formatLak(r.amountLak)}</span>,
        },
        {
            key: 'status',
            header: t.finance.expenses.fields.status,
            render: r => (
                <span className={`${styles.badge} ${styles[`expense_${r.status.toLowerCase()}`]}`}>
                    {statusLabel(r.status)}
                </span>
            ),
        },
    ], [t, statusLabel]);

    return (
        <div className={styles.page}>
            <PageHeader
                title={t.finance.expenses.title}
                subtitle={t.finance.expenses.subtitle}
                breadcrumbs={[
                    { label: t.nav.dashboard, href: '/' },
                    { label: t.finance.expenses.title },
                ]}
                actions={
                    <Link href="/finance/expenses/new">
                        <Button>{t.finance.expenses.newExpense}</Button>
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
                            label: s === '' ? t.finance.expenses.fields.status : statusLabel(s),
                        }))}
                    />
                    <Select
                        value={categoryId}
                        onChange={(e) => setCategoryId(e.target.value)}
                        options={[
                            { value: '', label: t.finance.expenses.fields.category },
                            ...categories.map(c => ({ value: String(c.expenseCategoryId), label: c.name })),
                        ]}
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
                <DataTable<ExpenseListItem>
                    columns={columns}
                    rows={pageData.items}
                    rowKey={r => r.expenseId}
                    isLoading={loading}
                    emptyTitle={t.finance.expenses.empty.title}
                    emptyDescription={t.finance.expenses.empty.description}
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
