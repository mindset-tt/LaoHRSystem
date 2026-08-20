'use client';

import Link from 'next/link';
import { useEffect, useMemo, useState } from 'react';
import { useParams } from 'next/navigation';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { PageHeader } from '@/components/ui/PageHeader';
import { DataTable, type DataTableColumn } from '@/components/ui/DataTable';
import { Pagination } from '@/components/ui/Pagination';
import { Select } from '@/components/ui/Select';
import { useToast } from '@/components/ui/Toast';
import { issuesApi } from '@/lib/endpoints';
import type { IssueListItem } from '@/lib/endpoints/projects';
import type { PaginatedResponse } from '@/lib/types/pagination';
import styles from '../page.module.css';

const STATUS_OPTIONS = ['', 'TODO', 'IN_PROGRESS', 'BLOCKED', 'REVIEW', 'DONE', 'CANCELLED'];
const PRIORITY_OPTIONS = ['', 'LOW', 'MEDIUM', 'HIGH', 'CRITICAL'];

export default function ProjectIssuesPage() {
    const params = useParams<{ id: string }>();
    const projectId = Number(params.id);
    const { t } = useLanguage();
    const toast = useToast();

    const [pageData, setPageData] = useState<PaginatedResponse<IssueListItem>>({
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
        setLoading(true);
        issuesApi
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

    const statusLabel = (s: string) =>
        (t.issues.status as Record<string, string>)[s.toLowerCase()] ?? s;
    const priorityLabel = (p: string) =>
        (t.tasks.priority as Record<string, string>)[p.toLowerCase()] ?? p;

    const columns: DataTableColumn<IssueListItem>[] = useMemo(() => [
        {
            key: 'title',
            header: t.issues.fields.title,
            sortBy: r => r.title,
            render: r => (
                <Link href={`/projects/${projectId}/issues/${r.issueId}`} className={styles.issueTitleLink}>
                    {r.title}
                </Link>
            ),
        },
        {
            key: 'status',
            header: t.issues.fields.status,
            render: r => <span className={`${styles.badge} ${styles[`status_${r.status.toLowerCase()}`]}`}>{statusLabel(r.status)}</span>,
        },
        {
            key: 'priority',
            header: t.issues.fields.priority,
            render: r => <span className={`${styles.badge} ${styles[`priority_${r.priority.toLowerCase()}`]}`}>{priorityLabel(r.priority)}</span>,
        },
        {
            key: 'assignee',
            header: t.issues.fields.assignee,
            render: r => r.assigneeName ?? '—',
        },
        {
            key: 'linkedTask',
            header: t.issues.fields.linkedTask,
            render: r => r.taskNumber
                ? <Link href={`/projects/${projectId}/projecttasks/${r.taskId}`} className={styles.taskLink}>{r.taskNumber}</Link>
                : '—',
        },
        {
            key: 'dueDate',
            header: t.issues.fields.dueDate,
            render: r => r.dueDate
                ? <span className={r.isOverdue ? styles.overdue : undefined}>{new Date(r.dueDate).toLocaleDateString()}</span>
                : '—',
        },
        {
            key: 'comments',
            header: t.tasks.comments.title,
            render: r => r.commentCount,
        },
    ], [projectId, statusLabel, priorityLabel, t]);

    return (
        <div className={styles.page}>
            <PageHeader
                title={t.issues.title}
                subtitle={t.issues.subtitle}
                breadcrumbs={[
                    { label: t.nav.dashboard, href: '/' },
                    { label: t.projects.title, href: '/projects' },
                    { label: `Project #${projectId}`, href: `/projects/${projectId}` },
                    { label: t.issues.title },
                ]}
                actions={
                    <Link href={`/projects/${projectId}`} className={styles.linkButton}>
                        ← {t.projects.detail.overview}
                    </Link>
                }
            />

            <Card className={styles.filtersCard}>
                <div className={styles.filters}>
                    <Select
                        value={status}
                        onChange={(e) => setStatus(e.target.value)}
                        options={STATUS_OPTIONS.map(s => ({ value: s, label: s === '' ? t.tasks.filterStatus : statusLabel(s) }))}
                    />
                    <Select
                        value={priority}
                        onChange={(e) => setPriority(e.target.value)}
                        options={PRIORITY_OPTIONS.map(p => ({ value: p, label: p === '' ? t.tasks.filterPriority : priorityLabel(p) }))}
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
                <DataTable<IssueListItem>
                    columns={columns}
                    rows={pageData.items}
                    rowKey={r => r.issueId}
                    isLoading={loading}
                    emptyTitle={t.issues.empty.title}
                    emptyDescription={t.issues.empty.description}
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