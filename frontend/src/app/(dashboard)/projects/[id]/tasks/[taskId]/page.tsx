'use client';

import Link from 'next/link';
import { useEffect, useState } from 'react';
import { useParams } from 'next/navigation';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Select } from '@/components/ui/Select';
import { PageHeader } from '@/components/ui/PageHeader';
import { ErrorState, EmptyState } from '@/components/ui/EmptyState';
import { useToast } from '@/components/ui/Toast';
import { projectTasksApi } from '@/lib/endpoints';
import type { TaskDetail } from '@/lib/endpoints/projects';
import styles from './page.module.css';

const STATUS_OPTIONS = ['TODO', 'IN_PROGRESS', 'BLOCKED', 'REVIEW', 'DONE', 'CANCELLED'];

export default function TaskDetailPage() {
    const params = useParams<{ id: string; taskId: string }>();
    const projectId = Number(params.id);
    const taskId = Number(params.taskId);
    const { t } = useLanguage();
    const toast = useToast();

    const [task, setTask] = useState<TaskDetail | null>(null);
    const [loading, setLoading] = useState(true);
    const [comment, setComment] = useState('');
    const [posting, setPosting] = useState(false);
    const [progress, setProgress] = useState(0);

    useEffect(() => {
        if (!projectId || !taskId) return;
        let cancelled = false;
        setLoading(true);
        projectTasksApi.getById(projectId, taskId)
            .then((tr) => {
                if (!cancelled) {
                    setTask(tr);
                    setProgress(tr.progressPercent);
                }
            })
            .catch((err) => {
                console.error(err);
                toast.error(t.tasks.messages.errorUpdate);
            })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [projectId, taskId, t.tasks.messages.errorUpdate, toast]);

    async function updateStatus(newStatus: string) {
        if (!task) return;
        try {
            await projectTasksApi.update(projectId, taskId, { status: newStatus });
            const updated = await projectTasksApi.getById(projectId, taskId);
            setTask(updated);
            setProgress(updated.progressPercent);
            toast.success(t.tasks.messages.statusChanged);
        } catch (err) {
            console.error(err);
            toast.error(t.tasks.messages.errorUpdate);
        }
    }

    async function updateProgress(value: number) {
        if (!task) return;
        setProgress(value);
        try {
            await projectTasksApi.update(projectId, taskId, { progressPercent: value });
        } catch (err) {
            console.error(err);
            toast.error(t.tasks.messages.errorUpdate);
        }
    }

    async function postComment() {
        if (!task || !comment.trim()) return;
        setPosting(true);
        try {
            await projectTasksApi.addComment(projectId, taskId, comment.trim());
            setComment('');
            const updated = await projectTasksApi.getById(projectId, taskId);
            setTask(updated);
        } catch (err) {
            console.error(err);
            toast.error(t.tasks.messages.errorComment);
        } finally {
            setPosting(false);
        }
    }

    if (loading) {
        return <PageHeader title="…" subtitle="…" breadcrumbs={[{ label: t.nav.dashboard, href: '/' }, { label: t.projects.title, href: '/projects' }, { label: '…' }]} />;
    }

    if (!task) {
        return (
            <div className={styles.page}>
                <Card>
                    <ErrorState title="Task not found" description="The task you tried to view does not exist or has been deleted." />
                </Card>
            </div>
        );
    }

    const statusLabel = (s: string) => (t.tasks.status as Record<string, string>)[s.toLowerCase()] ?? s;
    const priorityLabel = (p: string) => (t.tasks.priority as Record<string, string>)[p.toLowerCase()] ?? p;

    return (
        <div className={styles.page}>
            <PageHeader
                title={task.title}
                subtitle={task.taskNumber ? `Task ${task.taskNumber}` : undefined}
                breadcrumbs={[
                    { label: t.nav.dashboard, href: '/' },
                    { label: t.projects.title, href: '/projects' },
                    { label: `Project #${projectId}`, href: `/projects/${projectId}` },
                    { label: task.taskNumber ?? `#${taskId}` },
                ]}
                actions={
                    <span className={`${styles.badge} ${styles[`status_${task.status.toLowerCase()}`]}`}>
                        {statusLabel(task.status)}
                    </span>
                }
            />

            <div className={styles.grid}>
                <div className={styles.main}>
                    <Card>
                        <h3 className={styles.sectionTitle}>{t.tasks.fields.description}</h3>
                        {task.description ? (
                            <p className={styles.description}>{task.description}</p>
                        ) : (
                            <p className={styles.muted}>—</p>
                        )}
                    </Card>

                    <Card>
                        <h3 className={styles.sectionTitle}>{t.tasks.comments.title} ({task.comments.length})</h3>
                        {task.comments.length === 0 ? (
                            <EmptyState title={t.tasks.comments.empty} />
                        ) : (
                            <ul className={styles.commentList}>
                                {task.comments.map(c => (
                                    <li key={c.commentId} className={styles.comment}>
                                        <div className={styles.commentHeader}>
                                            <strong>{c.authorName}</strong>
                                            <span className={styles.commentTime}>{new Date(c.createdAt).toLocaleString()}</span>
                                        </div>
                                        <p className={styles.commentBody}>{c.body}</p>
                                    </li>
                                ))}
                            </ul>
                        )}

                        <div className={styles.commentForm}>
                            <textarea
                                className={styles.textarea}
                                placeholder={t.tasks.comments.placeholder}
                                value={comment}
                                onChange={(e) => setComment(e.target.value)}
                                rows={3}
                            />
                            <Button onClick={postComment} loading={posting} disabled={!comment.trim()}>
                                {t.tasks.comments.submit}
                            </Button>
                        </div>
                    </Card>
                </div>

                <div className={styles.sidebar}>
                    <Card>
                        <h3 className={styles.sectionTitle}>{t.tasks.fields.status}</h3>
                        <Select
                            value={task.status}
                            onChange={(e) => updateStatus(e.target.value)}
                            options={STATUS_OPTIONS.map(s => ({ value: s, label: statusLabel(s) }))}
                        />
                    </Card>

                    <Card>
                        <h3 className={styles.sectionTitle}>{t.tasks.fields.priority}</h3>
                        <span className={`${styles.badge} ${styles[`priority_${task.priority.toLowerCase()}`]}`}>{priorityLabel(task.priority)}</span>
                    </Card>

                    <Card>
                        <h3 className={styles.sectionTitle}>{t.tasks.fields.progress}</h3>
                        <input
                            type="range"
                            min={0}
                            max={100}
                            value={progress}
                            onChange={(e) => setProgress(Number(e.target.value))}
                            onMouseUp={() => updateProgress(progress)}
                            onTouchEnd={() => updateProgress(progress)}
                            className={styles.slider}
                        />
                        <div className={styles.progressValue}>{t.tasks.progress.replace('{percent}', String(progress))}</div>
                    </Card>

                    <Card>
                        <h3 className={styles.sectionTitle}>{t.tasks.fields.assignees}</h3>
                        {task.assignees.length === 0 ? (
                            <p className={styles.muted}>{t.tasks.fields.none}</p>
                        ) : (
                            <ul className={styles.assigneeList}>
                                {task.assignees.map(a => (
                                    <li key={a.employeeId} className={styles.assigneeItem}>
                                        <span className={styles.assigneeAvatar}>{initials(a.employeeName)}</span>
                                        <span>{a.employeeName}</span>
                                    </li>
                                ))}
                            </ul>
                        )}
                    </Card>

                    <Card>
                        <h3 className={styles.sectionTitle}>Dates</h3>
                        <dl className={styles.dl}>
                            <dt>{t.tasks.fields.startDate}</dt>
                            <dd>{task.startDate ? new Date(task.startDate).toLocaleDateString() : '—'}</dd>
                            <dt>{t.tasks.fields.dueDate}</dt>
                            <dd>{task.dueDate ? new Date(task.dueDate).toLocaleDateString() : '—'}</dd>
                        </dl>
                    </Card>
                </div>
            </div>
        </div>
    );
}

function initials(name: string): string {
    return name.split(' ').map(n => n[0]).slice(0, 2).join('').toUpperCase();
}