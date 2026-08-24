'use client';

import React from 'react';
import Link from 'next/link';
import { useEffect, useState } from 'react';
import { useParams } from 'next/navigation';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Select } from '@/components/ui/Select';
import { PageHeader } from '@/components/ui/PageHeader';
import { ErrorState } from '@/components/ui/EmptyState';
import { useToast } from '@/components/ui/Toast';
import { issuesApi } from '@/lib/endpoints';
import type { IssueDetail } from '@/lib/endpoints/projects';
import styles from '../../page.module.css';

const STATUS_OPTIONS = ['TODO', 'IN_PROGRESS', 'BLOCKED', 'REVIEW', 'DONE', 'CANCELLED'];

export default function IssueDetailPage() {
    const params = useParams<{ id: string; issueId: string }>();
    const projectId = Number(params.id);
    const issueId = Number(params.issueId);
    const { t } = useLanguage();
    const toast = useToast();

    const [issue, setIssue] = useState<IssueDetail | null>(null);
    const [loading, setLoading] = useState(true);
    const [comment, setComment] = useState('');
    const [posting, setPosting] = useState(false);

    useEffect(() => {
        if (!projectId || !issueId) return;
        let cancelled = false;
        React.startTransition(() => {
            setLoading(true);
        });
        issuesApi
            .getById(projectId, issueId)
            .then((tr) => { if (!cancelled) setIssue(tr); })
            .catch((err) => {
                console.error(err);
                toast.error(t.issues.messages.errorUpdate);
            })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [projectId, issueId, t.issues.messages.errorUpdate, toast]);

    async function updateStatus(newStatus: string) {
        if (!issue) return;
        try {
            await issuesApi.update(projectId, issueId, { status: newStatus });
            const updated = await issuesApi.getById(projectId, issueId);
            setIssue(updated);
            toast.success(t.issues.messages.updated);
        } catch (err) {
            console.error(err);
            toast.error(t.issues.messages.errorUpdate);
        }
    }

    async function postComment() {
        if (!issue || !comment.trim()) return;
        setPosting(true);
        try {
            await issuesApi.addComment(projectId, issueId, comment.trim());
            setComment('');
            const updated = await issuesApi.getById(projectId, issueId);
            setIssue(updated);
        } catch (err) {
            console.error(err);
            toast.error(t.issues.messages.errorComment);
        } finally {
            setPosting(false);
        }
    }

    if (loading) {
        return (
            <PageHeader
                title="…"
                subtitle="…"
                breadcrumbs={[
                    { label: t.nav.dashboard, href: '/' },
                    { label: t.projects.title, href: '/projects' },
                    { label: '…' },
                ]}
            />
        );
    }

    if (!issue) {
        return (
            <div className={styles.page}>
                <Card>
                    <ErrorState
                        title="Issue not found"
                        description="The issue you tried to view does not exist or has been deleted."
                    />
                </Card>
            </div>
        );
    }

    const statusLabel = (s: string) =>
        (t.issues.status as Record<string, string>)[s.toLowerCase()] ?? s;
    const priorityLabel = (p: string) =>
        (t.tasks.priority as Record<string, string>)[p.toLowerCase()] ?? p;

    return (
        <div className={styles.page}>
            <PageHeader
                title={issue.title}
                subtitle={`Issue #${issue.issueId}`}
                breadcrumbs={[
                    { label: t.nav.dashboard, href: '/' },
                    { label: t.projects.title, href: '/projects' },
                    { label: `Project #${projectId}`, href: `/projects/${projectId}` },
                    { label: t.issues.title, href: `/projects/${projectId}/issues` },
                    { label: `#${issueId}` },
                ]}
                actions={
                    <span className={`${styles.badge} ${styles[`status_${issue.status.toLowerCase()}`]}`}>
                        {statusLabel(issue.status)}
                    </span>
                }
            />

            <div className={styles.issueLayout}>
                <div className={styles.issueMain}>
                    <Card>
                        <h3 className={styles.sectionTitle}>{t.issues.fields.description}</h3>
                        {issue.description ? (
                            <p className={styles.description}>{issue.description}</p>
                        ) : (
                            <p className={styles.muted}>—</p>
                        )}
                    </Card>

                    <Card>
                        <h3 className={styles.sectionTitle}>
                            {t.issues.comments.title} ({issue.comments.length})
                        </h3>
                        {issue.comments.length === 0 ? (
                            <p className={styles.muted}>{t.issues.comments.empty}</p>
                        ) : (
                            <ul className={styles.commentList}>
                                {issue.comments.map(c => (
                                    <li key={c.issueCommentId} className={styles.comment}>
                                        <div className={styles.commentHeader}>
                                            <strong>{c.authorName}</strong>
                                            <span className={styles.commentTime}>
                                                {new Date(c.createdAt).toLocaleString()}
                                            </span>
                                        </div>
                                        <p className={styles.commentBody}>{c.body}</p>
                                    </li>
                                ))}
                            </ul>
                        )}

                        <div className={styles.commentForm}>
                            <textarea
                                className={styles.textarea}
                                placeholder={t.issues.comments.placeholder}
                                value={comment}
                                onChange={(e) => setComment(e.target.value)}
                                rows={3}
                            />
                            <Button onClick={postComment} loading={posting} disabled={!comment.trim()}>
                                {t.issues.comments.submit}
                            </Button>
                        </div>
                    </Card>
                </div>

                <div className={styles.issueSidebar}>
                    <Card>
                        <h3 className={styles.sectionTitle}>{t.issues.fields.status}</h3>
                        <Select
                            value={issue.status}
                            onChange={(e) => updateStatus(e.target.value)}
                            options={STATUS_OPTIONS.map(s => ({ value: s, label: statusLabel(s) }))}
                        />
                    </Card>

                    <Card>
                        <h3 className={styles.sectionTitle}>{t.issues.fields.priority}</h3>
                        <span className={`${styles.badge} ${styles[`priority_${issue.priority.toLowerCase()}`]}`}>
                            {priorityLabel(issue.priority)}
                        </span>
                    </Card>

                    <Card>
                        <h3 className={styles.sectionTitle}>Details</h3>
                        <dl className={styles.dl}>
                            <dt>{t.issues.fields.reporter}</dt>
                            <dd>{issue.reporterName ?? '—'}</dd>
                            <dt>{t.issues.fields.assignee}</dt>
                            <dd>{issue.assigneeName ?? t.issues.fields.none}</dd>
                            <dt>{t.issues.fields.linkedTask}</dt>
                            <dd>
                                {issue.taskId ? (
                                    <Link href={`/projects/${projectId}/projecttasks/${issue.taskId}`} className={styles.taskLink}>
                                        {issue.taskNumber ?? `#${issue.taskId}`}
                                    </Link>
                                ) : t.issues.fields.none}
                            </dd>
                            <dt>{t.issues.fields.dueDate}</dt>
                            <dd>
                                {issue.dueDate
                                    ? <span className={issue.dueDate < new Date().toISOString() && issue.status !== 'DONE' && issue.status !== 'CANCELLED' ? styles.overdue : undefined}>
                                        {new Date(issue.dueDate).toLocaleDateString()}
                                    </span>
                                    : t.issues.fields.none}
                            </dd>
                            {issue.resolvedAt && (
                                <>
                                    <dt>Resolved</dt>
                                    <dd>{new Date(issue.resolvedAt).toLocaleDateString()}</dd>
                                </>
                            )}
                        </dl>
                    </Card>
                </div>
            </div>
        </div>
    );
}