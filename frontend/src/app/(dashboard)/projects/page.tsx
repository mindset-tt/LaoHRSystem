'use client';

import React from 'react';
import { useCallback, useEffect, useMemo, useState } from 'react';
import Link from 'next/link';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { PageHeader } from '@/components/ui/PageHeader';
import { DataTable, type DataTableColumn } from '@/components/ui/DataTable';
import { Pagination } from '@/components/ui/Pagination';
import { useToast } from '@/components/ui/Toast';
import { projectsApi } from '@/lib/endpoints';
import type { ProjectListItem } from '@/lib/endpoints/projects';
import type { PaginatedResponse } from '@/lib/types/pagination';
import styles from './page.module.css';

const STATUS_OPTIONS = ['', 'PLANNING', 'ACTIVE', 'ON_HOLD', 'COMPLETED', 'CANCELLED'];
const PRIORITY_OPTIONS = ['', 'LOW', 'MEDIUM', 'HIGH', 'CRITICAL'];

export default function ProjectsPage() {
    const { t } = useLanguage();
    const toast = useToast();

    const [pageData, setPageData] = useState<PaginatedResponse<ProjectListItem>>({
        items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false,
    });
    const [loading, setLoading] = useState(true);
    const [status, setStatus] = useState('');
    const [priority, setPriority] = useState('');
    const [search, setSearch] = useState('');
    const [mineOnly, setMineOnly] = useState(false);
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(25);

    useEffect(() => {
        let cancelled = false;
        React.startTransition(() => {
            setLoading(true);
        });
        projectsApi
            .getAll({ status: status || undefined, priority: priority || undefined, search: search || undefined, mineOnly, page, pageSize })
            .then((res) => { if (!cancelled) setPageData(res); })
            .catch((err) => {
                console.error(err);
                toast.error(t.common.error);
            })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [status, priority, search, mineOnly, page, pageSize, t.common.error, toast]);

    const statusLabel = useCallback((s: string) => (t.projects.status as Record<string, string>)[s.toLowerCase()] ?? s, [t]);
    const priorityLabel = useCallback((p: string) => (t.projects.priority as Record<string, string>)[p.toLowerCase()] ?? p, [t]);

    const columns: DataTableColumn<ProjectListItem>[] = useMemo(() => [
        {
            key: 'code',
            header: t.projects.table.code,
            sortBy: p => p.code,
            render: p => <span className={styles.code}>{p.code}</span>,
        },
        {
            key: 'name',
            header: t.projects.table.name,
            sortBy: p => p.name,
            render: p => (
                <Link href={`/projects/${p.projectId}`} className={styles.nameLink}>
                    <span className={styles.colorDot} style={p.color ? { background: p.color } : undefined} />
                    {p.name}
                </Link>
            ),
        },
        {
            key: 'status',
            header: t.projects.table.status,
            sortBy: p => p.status,
            render: p => <span className={`${styles.badge} ${styles[`status_${p.status.toLowerCase()}`]}`}>{statusLabel(p.status)}</span>,
        },
        {
            key: 'priority',
            header: t.projects.table.priority,
            sortBy: p => p.priority,
            render: p => <span className={`${styles.badge} ${styles[`priority_${p.priority.toLowerCase()}`]}`}>{priorityLabel(p.priority)}</span>,
        },
        {
            key: 'members',
            header: t.projects.table.members,
            align: 'right',
            render: p => <span>{p.memberCount}</span>,
        },
        {
            key: 'progress',
            header: t.projects.table.progress,
            align: 'right',
            render: p => {
                const total = p.openTaskCount + p.doneTaskCount;
                const pct = total === 0 ? 0 : Math.round((p.doneTaskCount / total) * 100);
                return (
                    <div className={styles.progressCell}>
                        <div className={styles.progressBar}><div className={styles.progressFill} style={{ width: `${pct}%` }} /></div>
                        <span className={styles.progressLabel}>{pct}%</span>
                    </div>
                );
            },
        },
        {
            key: 'dueDate',
            header: t.projects.table.dueDate,
            sortBy: p => p.dueDate,
            render: p => p.dueDate ? new Date(p.dueDate).toLocaleDateString() : '—',
        },
    ], [t, statusLabel, priorityLabel]);

    return (
        <div className={styles.page}>
            <PageHeader
                title={t.projects.title}
                subtitle={t.projects.subtitle}
                breadcrumbs={[{ label: t.nav.dashboard, href: '/' }, { label: t.projects.title }]}
                actions={
                    <Link href="/projects/new">
                        <Button>+ {t.projects.newProject}</Button>
                    </Link>
                }
            />

            <Card className={styles.filtersCard}>
                <div className={styles.filters}>
                    <Input
                        placeholder={t.projects.searchPlaceholder}
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                    />
                    <Select
                        value={status}
                        onChange={(e) => setStatus(e.target.value)}
                        options={STATUS_OPTIONS.map(s => ({
                            value: s,
                            label: s === '' ? t.projects.filterStatus : statusLabel(s),
                        }))}
                    />
                    <Select
                        value={priority}
                        onChange={(e) => setPriority(e.target.value)}
                        options={PRIORITY_OPTIONS.map(p => ({
                            value: p,
                            label: p === '' ? t.projects.filterPriority : priorityLabel(p),
                        }))}
                    />
                    <label className={styles.mineOnly}>
                        <input type="checkbox" checked={mineOnly} onChange={(e) => setMineOnly(e.target.checked)} />
                        <span>{t.projects.mineOnly}</span>
                    </label>
                </div>
            </Card>

            <Card noPadding>
                <DataTable<ProjectListItem>
                    columns={columns}
                    rows={pageData.items}
                    rowKey={p => p.projectId}
                    isLoading={loading}
                    emptyTitle={t.projects.empty.title}
                    emptyDescription={t.projects.empty.description}
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