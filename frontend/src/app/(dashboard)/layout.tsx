'use client';

import { useRequireAuth } from '@/components/providers/AuthProvider';
import { Sidebar } from '@/components/layout/Sidebar';
import { Header } from '@/components/layout/Header';
import { SkeletonCard } from '@/components/ui/Skeleton';
import styles from './layout.module.css';

/**
 * Dashboard Layout
 * 
 * Main application shell with sidebar, header, and content area.
 * This metadata is defined once here for all dashboard routes (noindex).
 */
export default function DashboardLayout({
    children,
}: {
    children: React.ReactNode;
}) {
    const { loading, isAuthenticated } = useRequireAuth();

    // Show loading skeleton while checking auth
    if (loading) {
        return (
            <div className={styles.loadingContainer}>
                <div className={styles.loadingContent}>
                    <SkeletonCard />
                    <SkeletonCard />
                </div>
            </div>
        );
    }

    // Don't render anything if not authenticated (redirect will happen)
    if (!isAuthenticated) {
        return null;
    }

    return (
        <div className={styles.layout}>
            <Sidebar />
            <main className={styles.main}>
                <Header />
                <div className={styles.content}>
                    {children}
                </div>
            </main>
        </div>
    );
}
