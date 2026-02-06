'use client';

import { useEffect, useState } from 'react';
import { useAuth } from '@/components/providers/AuthProvider';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { apiClient } from '@/lib/apiClient';
import { Card, CardTitle, CardDescription } from '@/components/ui/Card';
import { Skeleton } from '@/components/ui/Skeleton';
import { formatDate, getCurrentLaoDate } from '@/lib/datetime';
import styles from './page.module.css';

interface DashboardStats {
    totalEmployees: number;
    activeEmployees: number;
    presentToday: number;
    pendingLeave: number;
}

/**
 * Dashboard Home Page
 * Quick overview with stats widgets and recent activity
 */
export default function DashboardPage() {
    const { user } = useAuth();
    const { t, language } = useLanguage();
    const [stats, setStats] = useState<DashboardStats | null>(null);
    const [loading, setLoading] = useState(true);

    const today = getCurrentLaoDate();

    useEffect(() => {
        const loadStats = async () => {
            try {
                const data = await apiClient.get<DashboardStats>('/api/dashboard/stats');
                setStats(data);
            } catch (error) {
                console.error('Failed to load dashboard stats:', error);
            } finally {
                setLoading(false);
            }
        };

        loadStats();
    }, []);

    return (
        <div className={styles.page}>
            {/* Welcome Section */}
            <section className={styles.welcomeSection}>
                <div className={styles.welcomeContent}>
                    <h1 className={styles.welcomeTitle}>
                        {getGreeting(t)}, {user?.displayName || 'User'}
                    </h1>
                    <p className={styles.welcomeSubtitle}>
                        {t.dashboardPage.welcome}, {formatDate(today)}
                    </p>
                </div>
            </section>

            {/* Stats Grid */}
            <section className={styles.statsSection}>
                <div className={styles.statsGrid}>
                    <StatCard
                        title={t.dashboardPage.stats.totalEmployees}
                        value={loading ? null : stats?.totalEmployees ?? 0}
                        icon={<UsersIcon />}
                        color="primary"
                    />
                    <StatCard
                        title={t.dashboardPage.stats.activeEmployees}
                        value={loading ? null : stats?.activeEmployees ?? 0}
                        icon={<UserCheckIcon />}
                        color="success"
                    />
                    <StatCard
                        title={t.dashboardPage.stats.presentToday}
                        value={loading ? null : stats?.presentToday ?? 0}
                        icon={<ClockIcon />}
                        color="accent"
                    />
                    <StatCard
                        title={t.dashboardPage.stats.pendingLeave}
                        value={loading ? null : stats?.pendingLeave ?? 0}
                        icon={<CalendarIcon />}
                        color="warning"
                    />
                </div>
            </section>

            {/* Quick Actions & Activity */}
            <section className={styles.contentSection}>
                <div className={styles.contentGrid}>
                    {/* Quick Actions */}
                    <Card>
                        <div className={styles.cardContent}>
                            <CardTitle>{t.dashboardPage.quickActions.title}</CardTitle>
                            <CardDescription>{t.dashboardPage.quickActions.subtitle}</CardDescription>
                            <div className={styles.quickActions}>
                                <QuickActionButton
                                    icon={<PlusIcon />}
                                    label={t.dashboardPage.quickActions.addEmployee}
                                    href="/employees/new"
                                />
                                <QuickActionButton
                                    icon={<ClockIcon />}
                                    label={t.dashboardPage.quickActions.viewAttendance}
                                    href="/attendance"
                                />
                                <QuickActionButton
                                    icon={<CalendarIcon />}
                                    label={t.dashboardPage.quickActions.manageLeave}
                                    href="/leave"
                                />
                                <QuickActionButton
                                    icon={<WalletIcon />}
                                    label={t.dashboardPage.quickActions.runPayroll}
                                    href="/payroll"
                                />
                            </div>
                        </div>
                    </Card>

                    {/* Recent Activity */}
                    <Card>
                        <div className={styles.cardContent}>
                            <CardTitle>{t.dashboardPage.activity.title}</CardTitle>
                            <CardDescription>{t.dashboardPage.activity.subtitle}</CardDescription>
                            <div className={styles.activityList}>
                                <ActivityItem
                                    title={t.dashboardPage.activity.mock.leave.title}
                                    description={t.dashboardPage.activity.mock.leave.desc}
                                    time="2 hours ago"
                                />
                                <ActivityItem
                                    title={t.dashboardPage.activity.mock.employee.title}
                                    description={t.dashboardPage.activity.mock.employee.desc}
                                    time="Yesterday"
                                />
                                <ActivityItem
                                    title={t.dashboardPage.activity.mock.payroll.title}
                                    description={t.dashboardPage.activity.mock.payroll.desc}
                                    time="3 days ago"
                                />
                            </div>
                        </div>
                    </Card>
                </div>
            </section>
        </div>
    );
}

// Stat Card Component
interface StatCardProps {
    title: string;
    value: number | null;
    icon: React.ReactNode;
    color: 'primary' | 'success' | 'accent' | 'warning';
}

function StatCard({ title, value, icon, color }: StatCardProps) {
    return (
        <Card className={styles.statCard}>
            <div className={styles.statContent}>
                <div className={`${styles.statIcon} ${styles[color]}`}>{icon}</div>
                <div className={styles.statInfo}>
                    <span className={styles.statTitle}>{title}</span>
                    {value === null ? (
                        <Skeleton width={60} height={32} />
                    ) : (
                        <span className={styles.statValue}>{value.toLocaleString()}</span>
                    )}
                </div>
            </div>
        </Card>
    );
}

// Quick Action Button
function QuickActionButton({
    icon,
    label,
    href,
}: {
    icon: React.ReactNode;
    label: string;
    href: string;
}) {
    return (
        <a href={href} className={styles.quickActionButton}>
            <span className={styles.quickActionIcon}>{icon}</span>
            <span className={styles.quickActionLabel}>{label}</span>
        </a>
    );
}

// Activity Item
function ActivityItem({
    title,
    description,
    time,
}: {
    title: string;
    description: string;
    time: string;
}) {
    return (
        <div className={styles.activityItem}>
            <div className={styles.activityDot} />
            <div className={styles.activityContent}>
                <span className={styles.activityTitle}>{title}</span>
                <span className={styles.activityDescription}>{description}</span>
                <span className={styles.activityTime}>{time}</span>
            </div>
        </div>
    );
}

// Greeting based on time
function getGreeting(t: any): string {
    const hour = new Date().getHours();

    if (hour < 12) return t.dashboardPage.greetings.morning;
    if (hour < 17) return t.dashboardPage.greetings.afternoon;
    return t.dashboardPage.greetings.evening;
}

// Icons
function UsersIcon() {
    return (
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
            <circle cx="9" cy="7" r="4" />
            <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
            <path d="M16 3.13a4 4 0 0 1 0 7.75" />
        </svg>
    );
}

function UserCheckIcon() {
    return (
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
            <circle cx="8.5" cy="7" r="4" />
            <polyline points="17 11 19 13 23 9" />
        </svg>
    );
}

function ClockIcon() {
    return (
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <circle cx="12" cy="12" r="10" />
            <polyline points="12 6 12 12 16 14" />
        </svg>
    );
}

function CalendarIcon() {
    return (
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
            <line x1="16" y1="2" x2="16" y2="6" />
            <line x1="8" y1="2" x2="8" y2="6" />
            <line x1="3" y1="10" x2="21" y2="10" />
        </svg>
    );
}

function WalletIcon() {
    return (
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <path d="M21 12V7H5a2 2 0 0 1 0-4h14v4" />
            <path d="M3 5v14a2 2 0 0 0 2 2h16v-5" />
            <path d="M18 12a2 2 0 0 0 0 4h4v-4h-4z" />
        </svg>
    );
}

function PlusIcon() {
    return (
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <line x1="12" y1="5" x2="12" y2="19" />
            <line x1="5" y1="12" x2="19" y2="12" />
        </svg>
    );
}
