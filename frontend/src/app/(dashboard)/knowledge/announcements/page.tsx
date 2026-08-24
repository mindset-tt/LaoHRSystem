'use client';

import React from 'react';
import { useEffect, useMemo, useState } from 'react';
import Link from 'next/link';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { PageHeader } from '@/components/ui/PageHeader';
import { Pagination } from '@/components/ui/Pagination';
import { Select } from '@/components/ui/Select';
import { useToast } from '@/components/ui/Toast';
import { announcementsApi, ANNOUNCEMENT_SEVERITIES, type AnnouncementListItem } from '@/lib/endpoints';
import type { PaginatedResponse } from '@/lib/types/pagination';
import styles from '../page.module.css';

const SEVERITY_OPTIONS = ['', ...ANNOUNCEMENT_SEVERITIES] as const;

export default function AnnouncementsPage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [pageData, setPageData] = useState<PaginatedResponse<AnnouncementListItem>>({
        items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false,
    });
    const [loading, setLoading] = useState(true);
    const [severity, setSeverity] = useState<string>('');
    const [unreadOnly, setUnreadOnly] = useState(false);
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(25);

    useEffect(() => {
        let cancelled = false;
        React.startTransition(() => {
            setLoading(true);
        });
        announcementsApi.list({
            severity: severity || undefined,
            unreadOnly: unreadOnly || undefined,
            page,
            pageSize,
        })
            .then((p) => { if (!cancelled) setPageData(p); })
            .catch((err) => {
                console.error(err);
                toast.error(t.knowledge.announcements.messages.errorUpdate);
            })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [severity, unreadOnly, page, pageSize, t.knowledge.announcements.messages.errorUpdate, toast]);

    const severityLabel = (s: string) =>
        (t.knowledge.announcements.severity as Record<string, string>)[s.toLowerCase()] ?? s;

    const headerActions = useMemo(() => (
        <Link href="/knowledge/announcements/new">
            <Button>{t.knowledge.announcements.newAnnouncement}</Button>
        </Link>
    ), [t]);

    return (
        <div className={styles.page}>
            <PageHeader
                title={t.knowledge.announcements.title}
                subtitle={t.knowledge.announcements.subtitle}
                breadcrumbs={[
                    { label: t.nav.dashboard, href: '/' },
                    { label: t.knowledge.announcements.title },
                ]}
                actions={headerActions}
            />

            <Card className={styles.filtersCard}>
                <div className={styles.filters}>
                    <Select
                        value={severity}
                        onChange={(e) => setSeverity(e.target.value)}
                        options={SEVERITY_OPTIONS.map(s => ({
                            value: s,
                            label: s === '' ? t.knowledge.announcements.fields.severity : severityLabel(s),
                        }))}
                    />
                    <label className={styles.unreadDot + ' ' + styles.unreadDot} style={{ display: 'inline-flex', alignItems: 'center', gap: 6 }}>
                        <input
                            type="checkbox"
                            checked={unreadOnly}
                            onChange={(e) => setUnreadOnly(e.target.checked)}
                        />
                        Unread only
                    </label>
                </div>
            </Card>

            {loading ? (
                <Card>…</Card>
            ) : pageData.items.length === 0 ? (
                <Card>
                    <div style={{ padding: 'var(--space-4)', textAlign: 'center' }}>
                        <h3>{t.knowledge.announcements.empty.title}</h3>
                        <p style={{ color: 'var(--text-muted)' }}>{t.knowledge.announcements.empty.description}</p>
                    </div>
                </Card>
            ) : (
                <div className={styles.announcementList}>
                    {pageData.items.map(a => (
                        <div
                            key={a.announcementId}
                            className={`${styles.announcementCard} ${styles[`is_${a.severity.toLowerCase()}`]}`}
                        >
                            <div className={styles.announcementHeader}>
                                <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
                                    {!a.isRead && <span className={styles.unreadDot} aria-label="Unread" />}
                                    <Link href={`/knowledge/announcements/${a.announcementId}`} className={styles.announcementTitle}>
                                        {a.title}
                                    </Link>
                                    {a.isPinned && <span className={styles.pinnedBadge}>📌 Pinned</span>}
                                </div>
                                <span className={`${styles.severityChip} ${styles[`severity_${a.severity.toLowerCase()}`]}`}>
                                    {severityLabel(a.severity)}
                                </span>
                            </div>
                            <div className={styles.announcementMeta}>
                                <span>{a.authorName ?? '—'}</span>
                                <span>·</span>
                                <span>{new Date(a.createdAt).toLocaleString()}</span>
                            </div>
                        </div>
                    ))}
                </div>
            )}

            {!loading && pageData.totalItems > 0 && (
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
        </div>
    );
}
