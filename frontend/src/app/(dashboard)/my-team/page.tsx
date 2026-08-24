'use client';

import { useEffect, useState } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Skeleton } from '@/components/ui/Skeleton';
import { PageHeader } from '@/components/ui/PageHeader';
import { organizationApi, teamApi } from '@/lib/endpoints';
import type { EmployeeSummary, TeamLeaveItem, TeamAttendanceItem } from '@/lib/endpoints';
import styles from './page.module.css';

type Tab = 'team' | 'leave' | 'attendance';

/**
 * My Team Page — MSS: direct reports, team leave, and team attendance.
 * Phase 3C2B.
 */
export default function MyTeamPage() {
    const { language } = useLanguage();
    const [tab, setTab] = useState<Tab>('team');
    const [loading, setLoading] = useState(true);
    const [team, setTeam] = useState<EmployeeSummary[]>([]);
    const [leave, setLeave] = useState<TeamLeaveItem[]>([]);
    const [attendance, setAttendance] = useState<TeamAttendanceItem[]>([]);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;
        const load = async () => {
            try {
                if (tab === 'team') {
                    const data = await organizationApi.getMyTeam();
                    if (!cancelled) setTeam(data);
                } else if (tab === 'leave') {
                    const data = await teamApi.getTeamLeave();
                    if (!cancelled) setLeave(data);
                } else {
                    const data = await teamApi.getTeamAttendance();
                    if (!cancelled) setAttendance(data);
                }
            } catch {
                if (!cancelled) setError('Failed to load team data');
            } finally {
                if (!cancelled) setLoading(false);
            }
        };
        load();
        return () => { cancelled = true; };
    }, [tab]);

    const handleTabChange = (next: Tab) => {
        setLoading(true);
        setError(null);
        setTab(next);
    };

    const name = (emp: { englishName?: string | null; laoName?: string }) =>
        language === 'lo' ? (emp.laoName || emp.englishName || '') : (emp.englishName || emp.laoName || '');

    return (
        <div className={styles.page}>
            <PageHeader
                title="My Team"
                breadcrumbs={[{ label: 'Dashboard', href: '/' }, { label: 'My Team' }]}
            />

            <div className={styles.tabs} role="tablist">
                <button
                    role="tab"
                    aria-selected={tab === 'team'}
                    className={`${styles.tab} ${tab === 'team' ? styles.active : ''}`}
                    onClick={() => handleTabChange('team')}
                >
                    Team
                </button>
                <button
                    role="tab"
                    aria-selected={tab === 'leave'}
                    className={`${styles.tab} ${tab === 'leave' ? styles.active : ''}`}
                    onClick={() => handleTabChange('leave')}
                >
                    Leave
                </button>
                <button
                    role="tab"
                    aria-selected={tab === 'attendance'}
                    className={`${styles.tab} ${tab === 'attendance' ? styles.active : ''}`}
                    onClick={() => handleTabChange('attendance')}
                >
                    Attendance
                </button>
            </div>

            {loading ? (
                <Card><Skeleton width="100%" height={200} /></Card>
            ) : error ? (
                <Card><p className={styles.error}>{error}</p></Card>
            ) : tab === 'team' ? (
                team.length === 0 ? (
                    <Card><p className={styles.empty}>You have no direct reports.</p></Card>
                ) : (
                    <Card noPadding>
                        <div className={styles.list}>
                            {team.map((emp) => (
                                <div key={emp.employeeId} className={styles.row}>
                                    <div className={styles.name}>{name(emp)}</div>
                                    <div className={styles.meta}>
                                        {emp.jobTitle && <span>{emp.jobTitle}</span>}
                                        {emp.departmentName && <span>{emp.departmentName}</span>}
                                    </div>
                                    <div className={styles.contact}>{emp.email && <span>{emp.email}</span>}</div>
                                </div>
                            ))}
                        </div>
                    </Card>
                )
            ) : tab === 'leave' ? (
                leave.length === 0 ? (
                    <Card><p className={styles.empty}>No team leave requests.</p></Card>
                ) : (
                    <Card noPadding>
                        <div className={styles.list}>
                            {leave.map((l) => (
                                <div key={l.leaveId} className={styles.row}>
                                    <div className={styles.name}>{l.employeeName ?? `#${l.employeeId}`}</div>
                                    <div className={styles.meta}>
                                        <span>{l.leaveType}</span>
                                        <span>{new Date(l.startDate).toLocaleDateString()} – {new Date(l.endDate).toLocaleDateString()}</span>
                                        <span>{l.totalDays} day(s)</span>
                                    </div>
                                    <span className={styles.badge}>{l.status}</span>
                                </div>
                            ))}
                        </div>
                    </Card>
                )
            ) : (
                attendance.length === 0 ? (
                    <Card><p className={styles.empty}>No team attendance records.</p></Card>
                ) : (
                    <Card noPadding>
                        <div className={styles.list}>
                            {attendance.map((a) => (
                                <div key={a.attendanceId} className={styles.row}>
                                    <div className={styles.name}>{a.employeeName ?? `#${a.employeeId}`}</div>
                                    <div className={styles.meta}>
                                        <span>{new Date(a.attendanceDate).toLocaleDateString()}</span>
                                        {a.clockIn && <span>In: {new Date(a.clockIn).toLocaleTimeString()}</span>}
                                        {a.clockOut && <span>Out: {new Date(a.clockOut).toLocaleTimeString()}</span>}
                                    </div>
                                    <span className={styles.badge}>{a.status}</span>
                                </div>
                            ))}
                        </div>
                    </Card>
                )
            )}
        </div>
    );
}
