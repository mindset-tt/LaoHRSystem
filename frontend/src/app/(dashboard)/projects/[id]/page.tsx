'use client';

import React from 'react';
import Link from 'next/link';
import { useCallback, useEffect, useMemo, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { PageHeader } from '@/components/ui/PageHeader';
import { DataTable, type DataTableColumn } from '@/components/ui/DataTable';
import { Pagination } from '@/components/ui/Pagination';
import { EmptyState, ErrorState } from '@/components/ui/EmptyState';
import { useToast } from '@/components/ui/Toast';
import { projectsApi, projectTasksApi } from '@/lib/endpoints';
import { CommentThread } from '@/components/ui/CommentThread';
import type { ProjectDetail, TaskListItem, ActivityListItem } from '@/lib/endpoints/projects';
import type { PaginatedResponse } from '@/lib/types/pagination';
import styles from './page.module.css';

type Tab = 'board' | 'list' | 'activity' | 'discussion';

const STATUS_COLUMNS: Array<{ key: string; labelKey: string }> = [
    { key: 'TODO', labelKey: 'todo' },
    { key: 'IN_PROGRESS', labelKey: 'in_progress' },
    { key: 'BLOCKED', labelKey: 'blocked' },
    { key: 'REVIEW', labelKey: 'review' },
    { key: 'DONE', labelKey: 'done' },
];

export default function ProjectDetailPage() {
    const params = useParams<{ id: string }>();
    const router = useRouter();
    const projectId = Number(params.id);
    const { t } = useLanguage();
    const toast = useToast();

    const [project, setProject] = useState<ProjectDetail | null>(null);
    const [projectLoading, setProjectLoading] = useState(true);
    const [projectError, setProjectError] = useState<string | null>(null);

    const [tab, setTab] = useState<Tab>('board');
    const [tasksPage, setTasksPage] = useState<PaginatedResponse<TaskListItem>>({
        items: [], page: 1, pageSize: 50, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false,
    });
    const [tasksLoading, setTasksLoading] = useState(true);
    const [taskPage, setTaskPage] = useState(1);

    const [activityPage, setActivityPage] = useState<PaginatedResponse<ActivityListItem>>({
        items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false,
    });
    const [activityLoading, setActivityLoading] = useState(true);

    useEffect(() => {
        if (!projectId) return;
        let cancelled = false;
        React.startTransition(() => {
            setProjectLoading(true);
            setProjectError(null);
        });
        projectsApi
            .getById(projectId)
            .then((p) => { if (!cancelled) setProject(p); })
            .catch((err) => {
                console.error(err);
                const msg = 'Failed to load project';
                if (!cancelled) {
                    setProjectError(msg);
                    toast.error(msg);
                }
            })
            .finally(() => { if (!cancelled) setProjectLoading(false); });
        return () => { cancelled = true; };
    }, [projectId, toast]);

    useEffect(() => {
        if (!projectId) return;
        let cancelled = false;
        React.startTransition(() => {
            setTasksLoading(true);
        });
        projectTasksApi
            .getAll(projectId, { page: taskPage, pageSize: 50 })
            .then((p) => { if (!cancelled) setTasksPage(p); })
            .catch((err) => {
                console.error(err);
                toast.error(t.common.error);
            })
            .finally(() => { if (!cancelled) setTasksLoading(false); });
        return () => { cancelled = true; };
    }, [projectId, taskPage, t.common.error, toast]);

    useEffect(() => {
        if (!projectId) return;
        let cancelled = false;
        React.startTransition(() => {
            setActivityLoading(true);
        });
        projectsApi
            .getActivities(projectId, 1, 25)
            .then((p) => { if (!cancelled) setActivityPage(p); })
            .catch((err) => {
                console.error(err);
                toast.error(t.common.error);
            })
            .finally(() => { if (!cancelled) setActivityLoading(false); });
        return () => { cancelled = true; };
    }, [projectId, t.common.error, toast]);

    const statusLabel = useCallback((s: string) => (t.tasks.status as Record<string, string>)[s.toLowerCase()] ?? s, [t]);
    const priorityLabel = useCallback((p: string) => (t.tasks.priority as Record<string, string>)[p.toLowerCase()] ?? p, [t]);

    // Group tasks by status for the board view.
    const tasksByStatus = useMemo(() => {
        const map: Record<string, TaskListItem[]> = {};
        for (const col of STATUS_COLUMNS) map[col.key] = [];
        for (const task of tasksPage.items) {
            if (!map[task.status]) map[task.status] = [];
            map[task.status].push(task);
        }
        return map;
    }, [tasksPage.items]);

    const taskColumns: DataTableColumn<TaskListItem>[] = useMemo(() => [
        {
            key: 'taskNumber',
            header: '#',
            render: r => <span className={styles.taskNumber}>{r.taskNumber ?? `#${r.taskId}`}</span>,
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
            key: 'assignees',
            header: t.tasks.fields.assignees,
            render: r => r.assignees.length === 0 ? '—' : (
                <div className={styles.assigneeStack}>
                    {r.assignees.slice(0, 3).map(a => (
                        <span key={a.employeeId} className={styles.avatar}>{initials(a.employeeName)}</span>
                    ))}
                    {r.assignees.length > 3 && <span className={styles.avatarMore}>+{r.assignees.length - 3}</span>}
                </div>
            ),
        },
        {
            key: 'dueDate',
            header: t.tasks.fields.dueDate,
            sortBy: r => r.dueDate,
            render: r => r.dueDate ? <span className={r.isOverdue ? styles.overdue : undefined}>{new Date(r.dueDate).toLocaleDateString()}</span> : '—',
        },
    ], [t, statusLabel, priorityLabel]);

    if (projectLoading) {
        return (
            <div className={styles.page}>
                <PageHeader title="…" subtitle="…" breadcrumbs={[{ label: t.nav.dashboard, href: '/' }, { label: t.projects.title, href: '/projects' }, { label: '…' }]} />
            </div>
        );
    }

    if (projectError || !project) {
        return (
            <div className={styles.page}>
                <PageHeader title="Project" breadcrumbs={[{ label: t.nav.dashboard, href: '/' }, { label: t.projects.title, href: '/projects' }, { label: 'Error' }]} />
                <Card>
                    <ErrorState title="Failed to load project" description={projectError ?? 'Project not found'} action={<Link href="/projects"><Button>Back</Button></Link>} />
                </Card>
            </div>
        );
    }

    return (
        <div className={styles.page}>
            <PageHeader
                title={project.name}
                subtitle={project.description ?? undefined}
                breadcrumbs={[
                    { label: t.nav.dashboard, href: '/' },
                    { label: t.projects.title, href: '/projects' },
                    { label: project.code },
                ]}
                actions={
                    <div className={styles.headerActions}>
                        <span className={`${styles.badge} ${styles[`status_${project.status.toLowerCase()}`]}`}>
                            {(t.projects.status as Record<string, string>)[project.status.toLowerCase()] ?? project.status}
                        </span>
                        <Button onClick={() => router.push(`/projects/${projectId}/tasks/new`)}>{t.projects.detail.newTask}</Button>
                    </div>
                }
            />

            {/* Summary card */}
            <Card className={styles.summaryCard}>
                <div className={styles.summaryGrid}>
                    <SummaryItem label={t.projects.table.owner} value={project.ownerName ?? '—'} />
                    <SummaryItem label={t.projects.table.members} value={String(project.members.length)} />
                    <SummaryItem label={t.projects.table.dueDate} value={project.dueDate ? new Date(project.dueDate).toLocaleDateString() : '—'} />
                    <SummaryItem
                        label={t.projects.table.progress}
                        value={`${Object.entries(project.taskCountByStatus).filter(([k]) => k === 'DONE').map(([, v]) => v).reduce((a, b) => a + b)} / ${Object.values(project.taskCountByStatus).reduce((a, b) => a + b, 0)}`}
                    />
                </div>
            </Card>

            {/* Tabs */}
            <div className={styles.tabs}>
                <button className={`${styles.tab} ${tab === 'board' ? styles.tabActive : ''}`} onClick={() => setTab('board')}>
                    {t.projects.detail.tabs.board}
                </button>
                <button className={`${styles.tab} ${tab === 'list' ? styles.tabActive : ''}`} onClick={() => setTab('list')}>
                    {t.projects.detail.tabs.list}
                </button>
                <button className={`${styles.tab} ${tab === 'activity' ? styles.tabActive : ''}`} onClick={() => setTab('activity')}>
                    {t.projects.detail.tabs.timeline} ({activityPage.totalItems})
                </button>
                <button className={`${styles.tab} ${tab === 'discussion' ? styles.tabActive : ''}`} onClick={() => setTab('discussion')}>
                    {t.knowledge.comments.title}
                </button>
            </div>

            {/* Sub-navigation for project workspace slices */}
            <div className={styles.filters}>
                <Link href={`/projects/${projectId}/risks`} className={styles.linkButton}>
                    {t.risks.title} →
                </Link>
                <Link href={`/projects/${projectId}/issues`} className={styles.linkButton}>
                    {t.issues.title} →
                </Link>
                <Link href={`/projects/${projectId}/resources`} className={styles.linkButton}>
                    {t.resources.title} →
                </Link>
            </div>

            {tab === 'board' && (
                <div className={styles.board}>
                    {STATUS_COLUMNS.map(col => (
                        <div key={col.key} className={styles.boardColumn}>
                            <div className={styles.boardColumnHeader}>
                                <span className={styles.boardColumnTitle}>{(t.projects.detail.taskBoard as Record<string, string>)[col.labelKey]}</span>
                                <span className={styles.boardColumnCount}>{tasksByStatus[col.key]?.length ?? 0}</span>
                            </div>
                            <div className={styles.boardColumnBody}>
                                {(tasksByStatus[col.key] ?? []).length === 0 ? (
                                    <div className={styles.boardEmpty}>{(t.projects.detail.taskBoard as Record<string, string>).empty}</div>
                                ) : (
                                    (tasksByStatus[col.key] ?? []).map(task => (
                                        <Link key={task.taskId} href={`/projects/${task.projectId}/tasks/${task.taskId}`} className={styles.taskCard}>
                                            <div className={styles.taskCardTitle}>{task.title}</div>
                                            <div className={styles.taskCardMeta}>
                                                <span className={styles.taskNumber}>{task.taskNumber}</span>
                                                {task.dueDate && (
                                                    <span className={task.isOverdue ? styles.overdue : styles.dueDate}>
                                                        {new Date(task.dueDate).toLocaleDateString()}
                                                    </span>
                                                )}
                                            </div>
                                            {task.assignees.length > 0 && (
                                                <div className={styles.assigneeStack}>
                                                    {task.assignees.slice(0, 3).map(a => (
                                                        <span key={a.employeeId} className={styles.avatar}>{initials(a.employeeName)}</span>
                                                    ))}
                                                </div>
                                            )}
                                        </Link>
                                    ))
                                )}
                            </div>
                        </div>
                    ))}
                </div>
            )}

            {tab === 'list' && (
                <Card noPadding>
                    <DataTable<TaskListItem>
                        columns={taskColumns}
                        rows={tasksPage.items}
                        rowKey={r => r.taskId}
                        isLoading={tasksLoading}
                        emptyTitle={t.tasks.empty.title}
                        emptyDescription={t.tasks.empty.description}
                    />
                    {!tasksLoading && (
                        <Pagination
                            page={tasksPage.page}
                            pageSize={tasksPage.pageSize}
                            totalItems={tasksPage.totalItems}
                            totalPages={tasksPage.totalPages}
                            hasNext={tasksPage.hasNext}
                            hasPrevious={tasksPage.hasPrevious}
                            onPageChange={setTaskPage}
                        />
                    )}
                </Card>
            )}

            {tab === 'activity' && (
                <Card>
                    {activityLoading ? (
                        <div className={styles.loading}>{t.common.loading}</div>
                    ) : activityPage.items.length === 0 ? (
                        <EmptyState title="No activity yet" />
                    ) : (
                        <ul className={styles.activityList}>
                            {activityPage.items.map(a => (
                                <li key={a.activityId} className={styles.activityItem}>
                                    <span className={styles.activityActor}>{a.actorName ?? `User ${a.actorId}`}</span>
                                    <span className={styles.activityAction}>{humanize(a.action)}</span>
                                    {a.taskId && <Link href={`/projects/${a.projectId}/tasks/${a.taskId}`} className={styles.activityTaskRef}>#{a.taskId}</Link>}
                                    <span className={styles.activityTime}>{new Date(a.createdAt).toLocaleString()}</span>
                                </li>
                            ))}
                        </ul>
                    )}
                </Card>
            )}

            {tab === 'discussion' && (
                <Card>
                    <CommentThread entityType="PROJECT" entityId={projectId} title={t.knowledge.comments.title} />
                </Card>
            )}
        </div>
    );
}

function SummaryItem({ label, value }: { label: string; value: string }) {
    return (
        <div className={styles.summaryItem}>
            <div className={styles.summaryLabel}>{label}</div>
            <div className={styles.summaryValue}>{value}</div>
        </div>
    );
}

function initials(name: string): string {
    return name.split(' ').map(n => n[0]).slice(0, 2).join('').toUpperCase();
}

function humanize(action: string): string {
    return action.toLowerCase().replace(/_/g, ' ');
}