/**
 * DateTime Utilities
 * 
 * All times displayed in Asia/Vientiane (ICT, UTC+7).
 * Backend stores UTC; frontend converts for display.
 */

export const LAO_TIMEZONE = 'Asia/Vientiane';

/**
 * Format a UTC date string for display in Lao timezone
 */
export function formatDateTime(utcDate: string | Date): string {
    const date = typeof utcDate === 'string' ? new Date(utcDate) : utcDate;
    return new Intl.DateTimeFormat('en-US', {
        timeZone: LAO_TIMEZONE,
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        hour12: false,
    }).format(date);
}

/**
 * Format a date only (no time)
 */
export function formatDate(utcDate: string | Date): string {
    const date = typeof utcDate === 'string' ? new Date(utcDate) : utcDate;
    return new Intl.DateTimeFormat('en-US', {
        timeZone: LAO_TIMEZONE,
        year: 'numeric',
        month: 'short',
        day: 'numeric',
    }).format(date);
}

/**
 * Format a date in Lao format
 */
export function formatDateLao(utcDate: string | Date): string {
    const date = typeof utcDate === 'string' ? new Date(utcDate) : utcDate;
    return new Intl.DateTimeFormat('lo-LA', {
        timeZone: LAO_TIMEZONE,
        year: 'numeric',
        month: 'long',
        day: 'numeric',
    }).format(date);
}

/**
 * Format time only
 */
export function formatTime(utcDate: string | Date): string {
    const date = typeof utcDate === 'string' ? new Date(utcDate) : utcDate;
    return new Intl.DateTimeFormat('en-US', {
        timeZone: LAO_TIMEZONE,
        hour: '2-digit',
        minute: '2-digit',
        hour12: false,
    }).format(date);
}

/**
 * Format a date for API requests (ISO format)
 */
export function formatForApi(date: Date): string {
    return date.toISOString();
}

/**
 * Get current date in Lao timezone
 */
export function getCurrentLaoDate(): Date {
    return new Date(
        new Date().toLocaleString('en-US', { timeZone: LAO_TIMEZONE })
    );
}

/**
 * Format relative time (e.g., "2 hours ago")
 */
export function formatRelativeTime(utcDate: string | Date): string {
    const date = typeof utcDate === 'string' ? new Date(utcDate) : utcDate;
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffSecs = Math.floor(diffMs / 1000);
    const diffMins = Math.floor(diffSecs / 60);
    const diffHours = Math.floor(diffMins / 60);
    const diffDays = Math.floor(diffHours / 24);

    if (diffSecs < 60) return 'Just now';
    if (diffMins < 60) return `${diffMins} minute${diffMins === 1 ? '' : 's'} ago`;
    if (diffHours < 24) return `${diffHours} hour${diffHours === 1 ? '' : 's'} ago`;
    if (diffDays < 7) return `${diffDays} day${diffDays === 1 ? '' : 's'} ago`;

    return formatDate(date);
}

/**
 * Format month and year for payroll periods
 */
export function formatPayrollPeriod(year: number, month: number): string {
    const date = new Date(year, month - 1);
    return new Intl.DateTimeFormat('en-US', {
        year: 'numeric',
        month: 'long',
    }).format(date);
}

/**
 * Get the start and end of a month
 */
export function getMonthRange(year: number, month: number): { start: Date; end: Date } {
    const start = new Date(year, month - 1, 1);
    const end = new Date(year, month, 0); // Last day of the month
    return { start, end };
}

/**
 * Check if a date is today
 */
export function isToday(utcDate: string | Date): boolean {
    const date = typeof utcDate === 'string' ? new Date(utcDate) : utcDate;
    const today = getCurrentLaoDate();
    return (
        date.getFullYear() === today.getFullYear() &&
        date.getMonth() === today.getMonth() &&
        date.getDate() === today.getDate()
    );
}

/**
 * Calculate work hours between two times
 */
export function calculateWorkHours(clockIn: string | Date, clockOut: string | Date): number {
    const start = typeof clockIn === 'string' ? new Date(clockIn) : clockIn;
    const end = typeof clockOut === 'string' ? new Date(clockOut) : clockOut;
    const diffMs = end.getTime() - start.getTime();
    return Math.round((diffMs / (1000 * 60 * 60)) * 100) / 100;
}
