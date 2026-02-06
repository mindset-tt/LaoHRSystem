'use client';

import { useEffect, useState, useMemo, useCallback } from 'react';
import { useAuth } from '@/components/providers/AuthProvider';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Skeleton } from '@/components/ui/Skeleton';
import { attendanceApi } from '@/lib/endpoints';
import { formatDate, formatTime, getCurrentLaoDate } from '@/lib/datetime';
import type { Attendance } from '@/lib/types';
import styles from './page.module.css';

/**
 * Attendance Page
 * Shows attendance records with calendar view and list toggle
 * Allows employees to clock in/out
 */
export default function AttendancePage() {
    const { role } = useAuth();
    const { t, language } = useLanguage();
    const [view, setView] = useState<'calendar' | 'list'>('calendar');
    const [loading, setLoading] = useState(true);
    const [selectedDate, setSelectedDate] = useState(getCurrentLaoDate());
    const [attendanceRecords, setAttendanceRecords] = useState<Attendance[]>([]);
    const [todayAttendance, setTodayAttendance] = useState<Attendance | null>(null);
    const [currentTime, setCurrentTime] = useState<string>('');
    const [actionLoading, setActionLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    // Unused for now but will be used for filtering employees
    void role;
    const currentMonth = selectedDate.getMonth();
    const currentYear = selectedDate.getFullYear();

    // Update current time every second
    useEffect(() => {
        const timer = setInterval(() => {
            const now = new Date();
            setCurrentTime(now.toLocaleTimeString('en-US', { hour12: false }));
        }, 1000);
        return () => clearInterval(timer);
    }, []);

    const loadData = useCallback(async () => {
        try {
            setError(null);
            const startDate = new Date(currentYear, currentMonth, 1).toISOString();
            const endDate = new Date(currentYear, currentMonth + 1, 0).toISOString();

            const [records, today] = await Promise.all([
                attendanceApi.getAll({ startDate, endDate }),
                attendanceApi.getToday(),
            ]);

            setAttendanceRecords(records);
            setTodayAttendance(today);
        } catch (err) {
            console.error('Failed to load attendance:', err);
            setError(t.common.error);
        } finally {
            setLoading(false);
        }
    }, [currentMonth, currentYear, t.common.error]);

    useEffect(() => {
        loadData();
    }, [loadData]);

    const handleClockIn = async () => {
        setActionLoading(true);
        try {
            setError(null);
            // Request geolocation if possible
            let location = {};
            try {
                const position = await new Promise<GeolocationPosition>((resolve, reject) => {
                    navigator.geolocation.getCurrentPosition(resolve, reject, {
                        timeout: 5000,
                    });
                });
                location = {
                    latitude: position.coords.latitude,
                    longitude: position.coords.longitude,
                };
            } catch (e) {
                console.warn('Geolocation failed or denied', e);
            }

            await attendanceApi.clockIn({ ...location, method: 'WEB' });
            await loadData();
        } catch (err) {
            console.error('Clock in failed:', err);
            setError(err instanceof Error ? err.message : t.common.error);
        } finally {
            setActionLoading(false);
        }
    };

    const handleClockOut = async () => {
        setActionLoading(true);
        try {
            setError(null);
            let location = {};
            try {
                const position = await new Promise<GeolocationPosition>((resolve, reject) => {
                    navigator.geolocation.getCurrentPosition(resolve, reject);
                });
                location = {
                    latitude: position.coords.latitude,
                    longitude: position.coords.longitude,
                };
            } catch (e) {
                console.warn('Geolocation failed or denied', e);
            }

            await attendanceApi.clockOut({ ...location, method: 'WEB' });
            await loadData();
        } catch (err) {
            console.error('Clock out failed:', err);
            setError(err instanceof Error ? err.message : t.common.error);
        } finally {
            setActionLoading(false);
        }
    };

    // Calendar days logic
    const calendarDays = useMemo(() => {
        const firstDay = new Date(currentYear, currentMonth, 1);
        const lastDay = new Date(currentYear, currentMonth + 1, 0);
        const daysInMonth = lastDay.getDate();
        const startDayOfWeek = firstDay.getDay();

        const days: { date: Date; isCurrentMonth: boolean; attendance?: Attendance }[] = [];

        // Previous month days
        for (let i = startDayOfWeek - 1; i >= 0; i--) {
            const date = new Date(currentYear, currentMonth, -i);
            days.push({ date, isCurrentMonth: false });
        }

        // Current month days
        for (let day = 1; day <= daysInMonth; day++) {
            const date = new Date(currentYear, currentMonth, day);
            const attendance = attendanceRecords.find(
                (a) => new Date(a.attendanceDate).getDate() === day
            );
            days.push({ date, isCurrentMonth: true, attendance });
        }

        // Next month days to fill the grid
        const remaining = 42 - days.length;
        for (let i = 1; i <= remaining; i++) {
            const date = new Date(currentYear, currentMonth + 1, i);
            days.push({ date, isCurrentMonth: false });
        }

        return days;
    }, [currentYear, currentMonth, attendanceRecords]);

    const navigateMonth = (direction: -1 | 1) => {
        setSelectedDate(new Date(currentYear, currentMonth + direction, 1));
    };

    return (
        <div className={styles.page}>
            {/* Header */}
            <div className={styles.header}>
                <div>
                    <h1 className={styles.title}>{t.attendance.title}</h1>
                    <p className={styles.subtitle}>
                        {t.attendance.subtitle}
                    </p>
                </div>
                <div className={styles.viewToggle}>
                    <button
                        className={`${styles.toggleBtn} ${view === 'calendar' ? styles.active : ''}`}
                        onClick={() => setView('calendar')}
                    >
                        <CalendarIcon />
                        {t.attendance.viewCalendar}
                    </button>
                    <button
                        className={`${styles.toggleBtn} ${view === 'list' ? styles.active : ''}`}
                        onClick={() => setView('list')}
                    >
                        <ListIcon />
                        {t.attendance.viewList}
                    </button>
                </div>
            </div>

            {/* Error Alert */}
            {error && (
                <div className={styles.errorAlert}>
                    {error}
                    <button onClick={() => setError(null)}>×</button>
                </div>
            )}

            {/* Today's Status Card */}
            <div className={styles.statusCard}>
                <div className={styles.statusInfo}>
                    <h2 className={styles.statusTitle}>{t.attendance.todayStatus}</h2>
                    <span className={styles.statusText}>
                        {formatDate(new Date().toISOString())}
                    </span>
                    {todayAttendance ? (
                        <div className={styles.attendanceDetails}>
                            {todayAttendance.clockIn && (
                                <span className={styles.statusText}>
                                    {t.attendance.clockedIn}: {formatTime(todayAttendance.clockIn)}
                                </span>
                            )}
                            {todayAttendance.clockOut && (
                                <span className={styles.statusText}>
                                    • {t.attendance.clockedOut}: {formatTime(todayAttendance.clockOut)}
                                </span>
                            )}
                        </div>
                    ) : (
                        <span className={styles.statusText}>{t.attendance.notCheckedIn}</span>
                    )}
                </div>

                <div className={styles.clockActions}>
                    <span className={styles.timeDisplay}>{currentTime}</span>
                    <Button
                        onClick={handleClockIn}
                        disabled={!!todayAttendance?.clockIn || actionLoading}
                        loading={actionLoading && !todayAttendance?.clockIn}
                        variant={todayAttendance?.clockIn ? "secondary" : "primary"}
                    >
                        {t.attendance.clockIn}
                    </Button>
                    <Button
                        onClick={handleClockOut}
                        disabled={!todayAttendance?.clockIn || !!todayAttendance?.clockOut || actionLoading}
                        loading={actionLoading && !!todayAttendance?.clockIn && !todayAttendance?.clockOut}
                        variant={(!todayAttendance?.clockIn || !!todayAttendance?.clockOut) ? "secondary" : "primary"}
                    >
                        {t.attendance.clockOut}
                    </Button>
                </div>
            </div>

            {view === 'calendar' ? (
                <>
                    {/* Calendar Navigation */}
                    <Card>
                        <div className={styles.calendarHeader}>
                            <button className={styles.navBtn} onClick={() => navigateMonth(-1)}>
                                <ChevronLeftIcon />
                            </button>
                            <h2 className={styles.monthTitle}>
                                {new Intl.DateTimeFormat(language === 'lo' ? 'lo-LA' : 'en-US', { month: 'long', year: 'numeric' }).format(selectedDate)}
                            </h2>
                            <button className={styles.navBtn} onClick={() => navigateMonth(1)}>
                                <ChevronRightIcon />
                            </button>
                        </div>
                    </Card>

                    {/* Calendar Grid */}
                    <Card noPadding>
                        {loading ? (
                            <div className={styles.loadingCalendar}>
                                <Skeleton height={400} />
                            </div>
                        ) : (
                            <div className={styles.calendar}>
                                {/* Weekday headers */}
                                <div className={styles.weekdays}>
                                    {Array.from({ length: 7 }).map((_, i) => {
                                        const date = new Date(2024, 0, i); // Jan 2024 starts on Monday? No, Jan 1 2024 is Monday. I need Sunday start.
                                        // Jan 7 2024 is Sunday.
                                        const d = new Date(2024, 0, 7 + i);
                                        return (
                                            <div key={i} className={styles.weekday}>
                                                {new Intl.DateTimeFormat(language === 'lo' ? 'lo-LA' : 'en-US', { weekday: 'short' }).format(d)}
                                            </div>
                                        );
                                    })}
                                </div>

                                {/* Days grid */}
                                <div className={styles.daysGrid}>
                                    {calendarDays.map((day, index) => {
                                        const isToday =
                                            day.date.toDateString() === getCurrentLaoDate().toDateString();
                                        const isWeekend = day.date.getDay() === 0 || day.date.getDay() === 6;

                                        return (
                                            <div
                                                key={index}
                                                className={`
                          ${styles.dayCell}
                          ${!day.isCurrentMonth ? styles.otherMonth : ''}
                          ${isToday ? styles.today : ''}
                          ${isWeekend ? styles.weekend : ''}
                        `}
                                            >
                                                <span className={styles.dayNumber}>{day.date.getDate()}</span>
                                                {day.attendance && (
                                                    <div className={styles.attendanceIndicator}>
                                                        <span
                                                            className={`${styles.statusDot} ${day.attendance.isLate ? styles.late : styles.present
                                                                }`}
                                                        />
                                                        <span className={styles.clockTime}>
                                                            {formatTime(day.attendance.clockIn!)}
                                                        </span>
                                                    </div>
                                                )}
                                            </div>
                                        );
                                    })}
                                </div>
                            </div>
                        )}
                    </Card>

                    {/* Legend */}
                    <div className={styles.legend}>
                        <div className={styles.legendItem}>
                            <span className={`${styles.statusDot} ${styles.present}`} />
                            <span>{t.attendance.legend.present}</span>
                        </div>
                        <div className={styles.legendItem}>
                            <span className={`${styles.statusDot} ${styles.late}`} />
                            <span>{t.attendance.legend.late}</span>
                        </div>
                        <div className={styles.legendItem}>
                            <span className={`${styles.statusDot} ${styles.absent}`} />
                            <span>{t.attendance.legend.absent}</span>
                        </div>
                        <div className={styles.legendItem}>
                            <span className={`${styles.statusDot} ${styles.leave}`} />
                            <span>{t.attendance.legend.leave}</span>
                        </div>
                    </div>
                </>
            ) : (
                /* List View */
                <Card noPadding>
                    {loading ? (
                        <div className={styles.loadingList}>
                            <Skeleton height={300} />
                        </div>
                    ) : (
                        <div className={styles.tableWrapper}>
                            <table className={styles.table}>
                                <thead>
                                    <tr>
                                        <th>{t.attendance.table.date}</th>
                                        <th>{t.attendance.table.clockIn}</th>
                                        <th>{t.attendance.table.clockOut}</th>
                                        <th>{t.attendance.table.workHours}</th>
                                        <th>{t.attendance.table.status}</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {attendanceRecords.length === 0 ? (
                                        <tr>
                                            <td colSpan={5} style={{ textAlign: 'center', padding: '2rem' }}>
                                                {t.attendance.table.empty}
                                            </td>
                                        </tr>
                                    ) : (
                                        attendanceRecords.slice().reverse().map((record) => (
                                            <tr key={record.attendanceId}>
                                                <td>{formatDate(record.attendanceDate)}</td>
                                                <td>
                                                    <span className={record.isLate ? styles.lateText : ''}>
                                                        {record.clockIn ? formatTime(record.clockIn) : '-'}
                                                    </span>
                                                    {record.isLate && <span className={styles.lateTag}>{t.attendance.legend.late}</span>}
                                                </td>
                                                <td>
                                                    {record.clockOut ? formatTime(record.clockOut) : '-'}
                                                    {record.isEarlyLeave && <span className={styles.earlyTag}>{t.attendance.early}</span>}
                                                </td>
                                                <td>{record.workHours?.toFixed(1) || '-'} {t.attendance.hrs}</td>
                                                <td>
                                                    <span className={`${styles.status} ${styles[record.status.toLowerCase()]}`}>
                                                        {t.attendance.legend[record.status.toLowerCase() as keyof typeof t.attendance.legend] || record.status}
                                                    </span>
                                                </td>
                                            </tr>
                                        ))
                                    )}
                                </tbody>
                            </table>
                        </div>
                    )}
                </Card>
            )}
        </div>
    );
}

// Icons
function CalendarIcon() {
    return (
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
            <line x1="16" y1="2" x2="16" y2="6" />
            <line x1="8" y1="2" x2="8" y2="6" />
            <line x1="3" y1="10" x2="21" y2="10" />
        </svg>
    );
}

function ListIcon() {
    return (
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <line x1="8" y1="6" x2="21" y2="6" />
            <line x1="8" y1="12" x2="21" y2="12" />
            <line x1="8" y1="18" x2="21" y2="18" />
            <line x1="3" y1="6" x2="3.01" y2="6" />
            <line x1="3" y1="12" x2="3.01" y2="12" />
            <line x1="3" y1="18" x2="3.01" y2="18" />
        </svg>
    );
}

function ChevronLeftIcon() {
    return (
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <polyline points="15 18 9 12 15 6" />
        </svg>
    );
}

function ChevronRightIcon() {
    return (
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <polyline points="9 18 15 12 9 6" />
        </svg>
    );
}
