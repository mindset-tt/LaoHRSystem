'use client';

import React from 'react';
import { useEffect, useState } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Skeleton } from '@/components/ui/Skeleton';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { useToast } from '@/components/ui/Toast';
import { notificationsApi } from '@/lib/endpoints';
import type { NotificationItem } from '@/lib/endpoints';
import styles from './page.module.css';

/**
 * Notifications Page — in-app notification center. Phase 3C2.
 */
export default function NotificationsPage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [loading, setLoading] = useState(true);
    const [items, setItems] = useState<NotificationItem[]>([]);
    const [error, setError] = useState<string | null>(null);

    const load = async () => {
        setLoading(true);
        try {
            const data = await notificationsApi.list({ pageSize: 50 });
            setItems(data.items);
        } catch {
            setError(t.common.error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        React.startTransition(() => { load(); });
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    const handleMarkRead = async (id: number) => {
        try {
            await notificationsApi.markRead(id);
            setItems(prev => prev.map(n => n.notificationId === id ? { ...n, isRead: true } : n));
        } catch {
            toast.error(t.common.error);
        }
    };

    const handleMarkAllRead = async () => {
        try {
            await notificationsApi.markAllRead();
            setItems(prev => prev.map(n => ({ ...n, isRead: true })));
            toast.success(t.common.saved);
        } catch {
            toast.error(t.common.error);
        }
    };

    if (loading) {
        return (
            <div className={styles.page}>
                <Skeleton width={200} height={32} />
                <Card><Skeleton width="100%" height={200} /></Card>
            </div>
        );
    }

    return (
        <div className={styles.page}>
            <PageHeader
                title={t.sidebar.notifications}
                breadcrumbs={[{ label: t.nav.dashboard, href: '/' }, { label: t.sidebar.notifications }]}
                actions={
                    items.some(n => !n.isRead) ? (
                        <Button variant="secondary" size="sm" onClick={handleMarkAllRead}>
                            Mark all read
                        </Button>
                    ) : undefined
                }
            />

            {error && <Card><p className={styles.error}>{error}</p></Card>}

            {!error && items.length === 0 && (
                <Card><p className={styles.empty}>No notifications.</p></Card>
            )}

            {items.length > 0 && (
                <Card noPadding>
                    <div className={styles.list}>
                        {items.map((n) => (
                            <button
                                key={n.notificationId}
                                className={`${styles.row} ${n.isRead ? styles.read : styles.unread}`}
                                onClick={() => !n.isRead && handleMarkRead(n.notificationId)}
                            >
                                <div className={styles.content}>
                                    <span className={styles.title}>{n.title}</span>
                                    {n.message && <span className={styles.message}>{n.message}</span>}
                                    <span className={styles.date}>
                                        {new Date(n.createdAt).toLocaleString()}
                                    </span>
                                </div>
                                {!n.isRead && <span className={styles.dot} aria-label="unread" />}
                            </button>
                        ))}
                    </div>
                </Card>
            )}
        </div>
    );
}
