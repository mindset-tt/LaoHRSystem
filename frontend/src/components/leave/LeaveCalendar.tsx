'use client';

import { useState, useEffect, useCallback, useMemo } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { leaveApi } from '@/lib/endpoints';
import type { LeaveCalendarItem } from '@/lib/types';
import styles from './LeaveCalendar.module.css';

interface LeaveCalendarProps {
    onClose?: () => void;
}

const WEEKDAYS = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
const WEEKDAYS_LAO = ['ອາທິດ', 'ຈັນ', 'ອັງຄານ', 'ພຸດ', 'ພະຫັດ', 'ສຸກ', 'ເສົາ'];

const LEAVE_COLORS: Record<string, string> = {
    ANNUAL: '#6366f1',
    SICK: '#ef4444',
    PERSONAL: '#f59e0b',
    MATERNITY: '#ec4899',
    PATERNITY: '#8b5cf6',
    UNPAID: '#6b7280',
};

/**
 * Leave Calendar Component
 * Shows a monthly calendar view with leave requests marked
 */
export function LeaveCalendar({ onClose }: LeaveCalendarProps) {
    const { language } = useLanguage();
    const [currentDate, setCurrentDate] = useState(new Date());
    const [leaves, setLeaves] = useState<LeaveCalendarItem[]>([]);
    const [loading, setLoading] = useState(true);
    const [selectedDay, setSelectedDay] = useState<number | null>(null);

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

    // Calculate calendar grid
    const calendarDays = useMemo(() => {
        const firstDay = new Date(year, month, 1);
        const lastDay = new Date(year, month + 1, 0);
        const startPadding = firstDay.getDay();
        const totalDays = lastDay.getDate();

        const days: (number | null)[] = [];

        // Add padding for days before the 1st
        for (let i = 0; i < startPadding; i++) {
            days.push(null);
        }

        // Add all days of the month
        for (let i = 1; i <= totalDays; i++) {
            days.push(i);
        }

        return days;
    }, [year, month]);

    // Get leaves for a specific day
    const getLeavesForDay = useCallback((day: number) => {
        const date = new Date(year, month, day);
        return leaves.filter(leave => {
            const start = new Date(leave.startDate);
            const end = new Date(leave.endDate);
            return date >= start && date <= end;
        });
    }, [leaves, year, month]);

    const goToPrevMonth = () => {
        setCurrentDate(new Date(year, month - 1, 1));
        setSelectedDay(null);
    };

    const goToNextMonth = () => {
        setCurrentDate(new Date(year, month + 1, 1));
        setSelectedDay(null);
    };

    const goToToday = () => {
        setCurrentDate(new Date());
        setSelectedDay(null);
    };

    const monthName = currentDate.toLocaleString(language === 'lo' ? 'lo-LA' : 'en-US', { month: 'long', year: 'numeric' });
    const weekdays = language === 'lo' ? WEEKDAYS_LAO : WEEKDAYS;

    const selectedDayLeaves = selectedDay ? getLeavesForDay(selectedDay) : [];

    return (
        <div className={styles.container}>
            <div className={styles.header}>
                <div className={styles.navigation}>
                    <button className={styles.navButton} onClick={goToPrevMonth}>
                        ←
                    </button>
                    <h2 className={styles.monthTitle}>{monthName}</h2>
                    <button className={styles.navButton} onClick={goToNextMonth}>
                        →
                    </button>
                </div>
                <button className={styles.todayButton} onClick={goToToday}>
                    {language === 'lo' ? 'ມື້ນີ້' : 'Today'}
                </button>
            </div>

            <div className={styles.calendar}>
                <div className={styles.weekdays}>
                    {weekdays.map((day) => (
                        <div key={day} className={styles.weekday}>{day}</div>
                    ))}
                </div>

                <div className={styles.days}>
                    {calendarDays.map((day, index) => {
                        if (day === null) {
                            return <div key={`empty-${index}`} className={styles.emptyDay} />;
                        }

                        const dayLeaves = getLeavesForDay(day);
                        const isToday =
                            day === new Date().getDate() &&
                            month === new Date().getMonth() &&
                            year === new Date().getFullYear();
                        const isSelected = day === selectedDay;

                        return (
                            <div
                                key={day}
                                className={`${styles.day} ${isToday ? styles.today : ''} ${isSelected ? styles.selected : ''}`}
                                onClick={() => setSelectedDay(day)}
                            >
                                <span className={styles.dayNumber}>{day}</span>
                                {dayLeaves.length > 0 && (
                                    <div className={styles.leaveIndicators}>
                                        {dayLeaves.slice(0, 3).map((leave, i) => (
                                            <div
                                                key={i}
                                                className={styles.leaveIndicator}
                                                style={{ backgroundColor: LEAVE_COLORS[leave.leaveType] || '#6b7280' }}
                                                title={`${leave.employeeName} - ${leave.leaveType}`}
                                            />
                                        ))}
                                        {dayLeaves.length > 3 && (
                                            <span className={styles.moreCount}>+{dayLeaves.length - 3}</span>
                                        )}
                                    </div>
                                )}
                            </div>
                        );
                    })}
                </div>
            </div>

            {selectedDay && selectedDayLeaves.length > 0 && (
                <div className={styles.dayDetails}>
                    <h3 className={styles.detailsTitle}>
                        {language === 'lo' ? 'ວັນທີ' : 'Date'}: {selectedDay}/{month + 1}/{year}
                    </h3>
                    <div className={styles.leaveList}>
                        {selectedDayLeaves.map((leave) => (
                            <div key={leave.leaveId} className={styles.leaveItem}>
                                <div
                                    className={styles.leaveColor}
                                    style={{ backgroundColor: LEAVE_COLORS[leave.leaveType] }}
                                />
                                <div className={styles.leaveInfo}>
                                    <div className={styles.employeeName}>{leave.employeeName}</div>
                                    <div className={styles.leaveType}>
                                        {leave.leaveType}
                                        {leave.isHalfDay && ` (${leave.halfDayType})`}
                                    </div>
                                </div>
                                <div className={`${styles.status} ${styles[leave.status.toLowerCase()]}`}>
                                    {leave.status}
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
            )}

            {/* Legend */}
            <div className={styles.legend}>
                {Object.entries(LEAVE_COLORS).map(([type, color]) => (
                    <div key={type} className={styles.legendItem}>
                        <div className={styles.legendColor} style={{ backgroundColor: color }} />
                        <span>{type}</span>
                    </div>
                ))}
            </div>
        </div>
    );
}

export default LeaveCalendar;
