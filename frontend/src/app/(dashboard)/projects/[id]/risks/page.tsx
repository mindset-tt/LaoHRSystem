'use client';

import React from 'react';
import { useCallback, useEffect, useMemo, useState } from 'react';
import Link from 'next/link';
import { useParams } from 'next/navigation';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { PageHeader } from '@/components/ui/PageHeader';
import { DataTable, type DataTableColumn } from '@/components/ui/DataTable';
import { Pagination } from '@/components/ui/Pagination';
import { Select } from '@/components/ui/Select';
import { useToast } from '@/components/ui/Toast';
import { risksApi } from '@/lib/endpoints';
import type { RiskListItem } from '@/lib/endpoints/projects';
import type { PaginatedResponse } from '@/lib/types/pagination';
import styles from '../page.module.css';

const STATUS_OPTIONS = ['', 'OPEN', 'MITIGATING', 'CLOSED', 'ACCEPTED'];
const PRIORITY_OPTIONS = ['', 'LOW', 'MEDIUM', 'HIGH', 'CRITICAL'];

export default function ProjectRisksPage() {
    const params = useParams<{ id: string }>();
    const projectId = Number(params.id);
    const { t } = useLanguage();
    const toast = useToast();

    const [pageData, setPageData] = useState<PaginatedResponse<RiskListItem>>({
        items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false,
    });
    const [loading, setLoading] = useState(true);
    const [status, setStatus] = useState('');
    const [priority, setPriority] = useState('');
    const [search, setSearch] = useState('');
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(25);

    useEffect(() => {
        if (!projectId) return;
        let cancelled = false;
        React.startTransition(() => {
            setLoading(true);
        });
        risksApi
            .getAll(projectId, {
                status: status || undefined,
                priority: priority || undefined,
                search: search || undefined,
                page,
                pageSize,
            })
            .then((p) => { if (!cancelled) setPageData(p); })
            .catch((err) => {
                console.error(err);
                toast.error(t.common.error);
            })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [projectId, status, priority, search, page, pageSize, t.common.error, toast]);

    const statusLabel = useCallback((s: string) =>
        (t.risks.status as Record<string, string>)[s.toLowerCase()] ?? s, [t]);
    const priorityLabel = useCallback((p: string) =>
        (t.tasks.priority as Record<string, string>)[p.toLowerCase()] ?? p, [t]);

    const scoreClass = (s: number) => {
        if (s >= 15) return styles.scoreCritical;
        if (s >= 8) return styles.scoreHigh;
        if (s >= 4) return styles.scoreMedium;
        return styles.scoreLow;
    };

    const columns: DataTableColumn<RiskListItem>[] = useMemo(() => [
        {
            key: 'title',
            header: t.risks.fields.title,
            sortBy: r => r.title,
            render: r => <strong>{r.title}</strong>,
        },
        {
            key: 'priority',
            header: t.risks.fields.priority,
            render: r => <span className={`${styles.badge} ${styles[`priority_${r.priority.toLowerCase()}`]}`}>{priorityLabel(r.priority)}</span>,
        },
        {
            key: 'likelihood',
            header: t.risks.fields.likelihood,
            render: r => r.likelihood,
        },
        {
            key: 'impact',
            header: t.risks.fields.impact,
            render: r => r.impact,
        },
        {
            key: 'score',
            header: t.risks.fields.score,
            render: r => <span className={`${styles.scoreBadge} ${scoreClass(r.score)}`}>{r.score}</span>,
        },
        {
            key: 'status',
            header: t.risks.fields.status,
            render: r => <span className={`${styles.badge} ${styles[`risk_${r.status.toLowerCase()}`]}`}>{statusLabel(r.status)}</span>,
        },
        {
            key: 'owner',
            header: t.risks.fields.owner,
            render: r => r.ownerName ?? '—',
        },
        {
            key: 'dueDate',
            header: t.risks.fields.dueDate,
            render: r => r.dueDate ? new Date(r.dueDate).toLocaleDateString() : '—',
        },
    ], [t, statusLabel, priorityLabel]);

    return (
        <div className={styles.page}>
            <PageHeader
                title={t.risks.title}
                subtitle={t.risks.subtitle}
                breadcrumbs={[
                    { label: t.nav.dashboard, href: '/' },
                    { label: t.projects.title, href: '/projects' },
                    { label: `Project #${projectId}`, href: `/projects/${projectId}` },
                    { label: t.risks.title },
                ]}
                actions={
                    <div className={styles.headerActions}>
                        <Link href={`/projects/${projectId}`} className={styles.linkButton}>
                            ← {t.projects.detail.overview}
                        </Link>
                        <Link href={`/projects/${projectId}/risks/new`}>
                            <Button>{t.risks.newRisk}</Button>
                        </Link>
                    </div>
                }
            />

            <Card className={styles.filtersCard}>
                <div className={styles.filters}>
                    <Select
                        value={status}
                        onChange={(e) => setStatus(e.target.value)}
                        options={STATUS_OPTIONS.map(s => ({ value: s, label: s === '' ? t.risks.fields.status : statusLabel(s) }))}
                    />
                    <Select
                        value={priority}
                        onChange={(e) => setPriority(e.target.value)}
                        options={PRIORITY_OPTIONS.map(p => ({ value: p, label: p === '' ? t.risks.fields.priority : priorityLabel(p) }))}
                    />
                    <input
                        type="search"
                        placeholder={t.projects.searchPlaceholder}
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                        className={styles.searchInput}
                    />
                </div>
            </Card>

            <Card noPadding>
                <DataTable<RiskListItem>
                    columns={columns}
                    rows={pageData.items}
                    rowKey={r => r.riskId}
                    isLoading={loading}
                    emptyTitle={t.risks.empty.title}
                    emptyDescription={t.risks.empty.description}
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