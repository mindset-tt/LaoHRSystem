'use client';

import { useEffect, useState } from 'react';
import { useParams } from 'next/navigation';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { PageHeader } from '@/components/ui/PageHeader';
import { ErrorState } from '@/components/ui/EmptyState';
import { announcementsApi, type AnnouncementDetail } from '@/lib/endpoints';
import styles from '../../page.module.css';

export default function AnnouncementDetailPage() {
    const params = useParams<{ id: string }>();
    const announcementId = Number(params.id);
    const { t } = useLanguage();
    const [announcement, setAnnouncement] = useState<AnnouncementDetail | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        if (!announcementId) return;
        let cancelled = false;
        setLoading(true);
        announcementsApi.get(announcementId)
            .then((d) => {
                if (cancelled) return;
                setAnnouncement(d);
                // Mark as read once the detail is loaded.
                if (!d.isRead) {
                    announcementsApi.markRead(announcementId).catch(() => undefined);
                }
            })
            .catch(() => { /* ignored — ErrorState handles missing */ })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [announcementId]);

    if (loading) {
        return (
            <PageHeader
                title="…"
                subtitle="…"
                breadcrumbs={[
                    { label: t.nav.dashboard, href: '/' },
                    { label: t.knowledge.announcements.title, href: '/knowledge/announcements' },
                    { label: '…' },
                ]}
            />
        );
    }

    if (!announcement) {
        return (
            <Card>
                <ErrorState
                    title="Announcement not found"
                    description="The announcement you tried to view does not exist or has been removed."
                />
            </Card>
        );
    }

    const severityLabel = (announcement.severity in t.knowledge.announcements.severity
        ? (t.knowledge.announcements.severity as Record<string, string>)[announcement.severity.toLowerCase()]
        : announcement.severity);

    return (
        <div className={styles.page}>
            <PageHeader
                title={announcement.title}
                subtitle={announcement.authorName ? `${t.knowledge.announcements.fields.author}: ${announcement.authorName}` : undefined}
                breadcrumbs={[
                    { label: t.nav.dashboard, href: '/' },
                    { label: t.knowledge.announcements.title, href: '/knowledge/announcements' },
                    { label: announcement.title },
                ]}
                actions={
                    <span className={`${styles.severityChip} ${styles[`severity_${announcement.severity.toLowerCase()}`]}`}>
                        {severityLabel}
                    </span>
                }
            />

            <Card>
                <div className={styles.announcementMeta} style={{ marginBottom: 'var(--space-3)' }}>
                    <span>{new Date(announcement.createdAt).toLocaleString()}</span>
                    {announcement.publishUntil && (
                        <>
                            <span>·</span>
                            <span>Visible until {new Date(announcement.publishUntil).toLocaleDateString()}</span>
                        </>
                    )}
                </div>
                <p className={styles.announcementBody}>{announcement.body}</p>

                {announcement.titleLao && announcement.bodyLao && (
                    <details style={{ marginTop: 'var(--space-4)' }}>
                        <summary style={{ cursor: 'pointer', color: 'var(--text-muted)' }}>ສະບັບພາສາລາວ</summary>
                        <h3 style={{ marginTop: 'var(--space-2)' }}>{announcement.titleLao}</h3>
                        <p className={styles.announcementBody}>{announcement.bodyLao}</p>
                    </details>
                )}
            </Card>
        </div>
    );
}
