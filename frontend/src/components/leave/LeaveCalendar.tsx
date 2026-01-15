'use client';

import { useState, useEffect, useCallback, useMemo } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Skeleton } from '@/components/ui/Skeleton';
import { leaveApi } from '@/lib/endpoints';
import type { LeaveCalendarItem } from '@/lib/types';
import styles from './LeaveCalendar.module.css';

const LEAVE_COLORS: Record<string, { dot: string; text: string }> = {
    ANNUAL: { dot: '#6366f1', text: 'ພັກປະຈຳປີ' },
    SICK: { dot: '#ef4444', text: 'ພັກປ່ວຍ' },
    PERSONAL: { dot: '#f59e0b', text: 'ກິດສ່ວນຕົວ' },
    MATERNITY: { dot: '#ec4899', text: 'ລາຄອດ' },
    PATERNITY: { dot: '#8b5cf6', text: 'ລາເມຍຄອດ' },
    UNPAID: { dot: '#6b7280', text: 'ບໍ່ມີເງິນ' },
};

/**
 * Leave Calendar Component
 * Shows a monthly calendar view with leave requests marked (same style as attendance calendar)
 */
export function LeaveCalendar() {
    const { language } = useLanguage();
    const [currentDate, setCurrentDate] = useState(new Date());
    const [leaves, setLeaves] = useState<LeaveCalendarItem[]>([]);
    const [loading, setLoading] = useState(true);

    const year = currentDate.getFullYear();
    const month = currentDate.getMonth();

    const loadLeaves = useCallback(async () => {
        setLoading(true);
        try {
            const data = await leaveApi.getCalendar(year, month + 1);
            setLeaves(data);
        } catch (err) {
            console.error('Failed to load calendar:', err);
        } finally {
            setLoading(false);
        }
    }, [year, month]);

    useEffect(() => {
        loadLeaves();
    }, [loadLeaves]);

    // Calendar days logic (same as attendance page)
    const calendarDays = useMemo(() => {
        const firstDay = new Date(year, month, 1);
        const lastDay = new Date(year, month + 1, 0);
        const daysInMonth = lastDay.getDate();
        const startDayOfWeek = firstDay.getDay();

        const days: { date: Date; isCurrentMonth: boolean; leaves: LeaveCalendarItem[] }[] = [];

        // Previous month days
        for (let i = startDayOfWeek - 1; i >= 0; i--) {
            const date = new Date(year, month, -i);
            days.push({ date, isCurrentMonth: false, leaves: [] });
        }

        // Current month days
        for (let day = 1; day <= daysInMonth; day++) {
            const date = new Date(year, month, day);
            // Normalize to date string (YYYY-MM-DD) to avoid timezone issues
            const dateStr = `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;

            const dayLeaves = leaves.filter(leave => {
                // Extract date parts only (ignore time/timezone)
                const startStr = leave.startDate.split('T')[0];
                const endStr = leave.endDate.split('T')[0];
                return dateStr >= startStr && dateStr <= endStr;
            });
            days.push({ date, isCurrentMonth: true, leaves: dayLeaves });
        }

        // Next month days to fill the grid
        const remaining = 42 - days.length;
        for (let i = 1; i <= remaining; i++) {
            const date = new Date(year, month + 1, i);
            days.push({ date, isCurrentMonth: false, leaves: [] });
        }

        return days;
    }, [year, month, leaves]);

    const navigateMonth = (direction: -1 | 1) => {
        setCurrentDate(new Date(year, month + direction, 1));
    };

    const isToday = (date: Date) => date.toDateString() === new Date().toDateString();

    return (
        <div className={styles.container}>
            {/* Calendar Navigation */}
            <Card>
                <div className={styles.calendarHeader}>
                    <button className={styles.navBtn} onClick={() => navigateMonth(-1)}>
                        <ChevronLeftIcon />
                    </button>
                    <h2 className={styles.monthTitle}>
                        {new Intl.DateTimeFormat(language === 'lo' ? 'lo-LA' : 'en-US', { month: 'long', year: 'numeric' }).format(currentDate)}
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
                                const d = new Date(2024, 0, 7 + i); // Sunday start
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
                                const dayIsToday = isToday(day.date);
                                const isWeekend = day.date.getDay() === 0 || day.date.getDay() === 6;

                                return (
                                    <div
                                        key={index}
                                        className={`
                                            ${styles.dayCell}
                                            ${!day.isCurrentMonth ? styles.otherMonth : ''}
                                            ${dayIsToday ? styles.today : ''}
                                            ${isWeekend ? styles.weekend : ''}
                                        `}
                                    >
                                        <span className={styles.dayNumber}>{day.date.getDate()}</span>
                                        {day.leaves.length > 0 && (
                                            <div className={styles.leaveIndicators}>
                                                {day.leaves.slice(0, 3).map((leave, i) => (
                                                    <div key={i} className={styles.leaveIndicator}>
                                                        <span
                                                            className={styles.statusDot}
                                                            style={{ backgroundColor: LEAVE_COLORS[leave.leaveType]?.dot || '#6b7280' }}
                                                        />
                                                        <span className={styles.leaveName}>
                                                            {leave.employeeName?.split(' ')[0]}
                                                        </span>
                                                    </div>
                                                ))}
                                                {day.leaves.length > 3 && (
                                                    <span className={styles.moreCount}>+{day.leaves.length - 3}</span>
                                                )}
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
                {Object.entries(LEAVE_COLORS).map(([type, { dot, text }]) => (
                    <div key={type} className={styles.legendItem}>
                        <span className={styles.statusDot} style={{ backgroundColor: dot }} />
                        <span>{language === 'lo' ? text : type.charAt(0) + type.slice(1).toLowerCase()}</span>
                    </div>
                ))}
            </div>
        </div>
    );
}

// Icons
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

export default LeaveCalendar;
