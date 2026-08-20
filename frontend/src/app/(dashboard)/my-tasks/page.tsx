'use client';

import Link from 'next/link';
import { useEffect, useMemo, useState } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Select } from '@/components/ui/Select';
import { PageHeader } from '@/components/ui/PageHeader';
import { DataTable, type DataTableColumn } from '@/components/ui/DataTable';
import { Pagination } from '@/components/ui/Pagination';
import { useToast } from '@/components/ui/Toast';
import { myTasksApi } from '@/lib/endpoints';
import type { TaskListItem } from '@/lib/endpoints/projects';
import type { PaginatedResponse } from '@/lib/types/pagination';
import styles from './page.module.css';

const STATUS_OPTIONS = ['', 'TODO', 'IN_PROGRESS', 'BLOCKED', 'REVIEW', 'DONE'];

export default function MyTasksPage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [pageData, setPageData] = useState<PaginatedResponse<TaskListItem>>({
        items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false,
    });
    const [loading, setLoading] = useState(true);
    const [status, setStatus] = useState('');
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(25);

    useEffect(() => {
        let cancelled = false;
        setLoading(true);
        myTasksApi.getAll({ status: status || undefined, page, pageSize })
            .then(p => { if (!cancelled) setPageData(p); })
            .catch(err => { console.error(err); toast.error(t.common.error); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [status, page, pageSize, t.common.error, toast]);

    const statusLabel = (s: string) => (t.tasks.status as Record<string, string>)[s.toLowerCase()] ?? s;
    const priorityLabel = (p: string) => (t.tasks.priority as Record<string, string>)[p.toLowerCase()] ?? p;

    const columns: DataTableColumn<TaskListItem>[] = useMemo(() => [
        {
            key: 'taskNumber',
            header: '#',
            render: r => <span className={styles.taskNumber}>{r.taskNumber ?? `#${r.taskId}`}</span>,
        },
        {
            key: 'project',
            header: 'Project',
            render: r => <Link href={`/projects/${r.projectId}`} className={styles.projectLink}>{r.projectCode} · {r.projectName}</Link>,
        },
        {
            key: 'title',
            header: t.tasks.fields.title,
            sortBy: r => r.title,
            render: r => (
                <Link href={`/projects/${r.projectId}/tasks/${r.taskId}`} className={styles.taskTitle}>
                    {r.title}
                </Link>
            ),
        },
        {
            key: 'status',
            header: t.tasks.fields.status,
            render: r => <span className={`${styles.badge} ${styles[`status_${r.status.toLowerCase()}`]}`}>{statusLabel(r.status)}</span>,
        },
        {
            key: 'priority',
            header: t.tasks.fields.priority,
            render: r => <span className={`${styles.badge} ${styles[`priority_${r.priority.toLowerCase()}`]}`}>{priorityLabel(r.priority)}</span>,
        },
        {
            key: 'dueDate',
            header: t.tasks.fields.dueDate,
            sortBy: r => r.dueDate,
            render: r => r.dueDate ? <span className={r.isOverdue ? styles.overdue : undefined}>{new Date(r.dueDate).toLocaleDateString()}</span> : '—',
        },
    ], [t]);

    return (
        <div className={styles.page}>
            <PageHeader
                title={t.nav.myTasks}
                subtitle={t.tasks.subtitle}
                breadcrumbs={[{ label: t.nav.dashboard, href: '/' }, { label: t.nav.myTasks }]}
            />

            <Card className={styles.filtersCard}>
                <div className={styles.filters}>
                    <Select
                        value={status}
                        onChange={(e) => setStatus(e.target.value)}
                        options={STATUS_OPTIONS.map(s => ({
                            value: s,
                            label: s === '' ? t.tasks.filterStatus : statusLabel(s),
                        }))}
                    />
                </div>
            </Card>

            <Card noPadding>
                <DataTable<TaskListItem>
                    columns={columns}
                    rows={pageData.items}
                    rowKey={r => r.taskId}
                    isLoading={loading}
                    emptyTitle={t.tasks.empty.title}
                    emptyDescription={t.tasks.empty.description}
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