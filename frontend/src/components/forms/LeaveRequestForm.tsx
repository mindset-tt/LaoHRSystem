'use client';

import { useState, useMemo } from 'react';
import { Button } from '@/components/ui/Button';
import { Modal } from '@/components/ui/Modal';
import type { CreateLeaveRequest } from '@/lib/types';
import styles from './LeaveRequestForm.module.css';

interface LeaveRequestFormProps {
    isOpen: boolean;
    onClose: () => void;
    onSubmit: (request: CreateLeaveRequest) => Promise<void>;
}

const LEAVE_TYPES = [
    { value: 'ANNUAL', label: 'Annual Leave' },
    { value: 'SICK', label: 'Sick Leave' },
    { value: 'PERSONAL', label: 'Personal Leave' },
    { value: 'MATERNITY', label: 'Maternity Leave' },
    { value: 'PATERNITY', label: 'Paternity Leave' },
    { value: 'UNPAID', label: 'Unpaid Leave' },
] as const;

/**
 * Leave Request Form Modal
 * 
 * Form for creating new leave requests with date validation
 * and total days calculation.
 */
export function LeaveRequestForm({
    isOpen,
    onClose,
    onSubmit,
}: LeaveRequestFormProps) {
    const [leaveType, setLeaveType] = useState<CreateLeaveRequest['leaveType']>('ANNUAL');
    const [startDate, setStartDate] = useState('');
    const [endDate, setEndDate] = useState('');
    const [reason, setReason] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');

    // Calculate total days
    const totalDays = useMemo(() => {
        if (!startDate || !endDate) return 0;
        const start = new Date(startDate);
        const end = new Date(endDate);
        if (end < start) return 0;
        const diffTime = Math.abs(end.getTime() - start.getTime());
        return Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1;
    }, [startDate, endDate]);

    const handleSubmit = async () => {
        setError('');

        if (!startDate || !endDate) {
            setError('Please select start and end dates');
            return;
        }

        if (new Date(endDate) < new Date(startDate)) {
            setError('End date cannot be before start date');
            return;
        }

        setLoading(true);

        try {
            await onSubmit({
                leaveType,
                startDate,
                endDate,
                reason: reason || undefined,
            });

            // Reset form
            setLeaveType('ANNUAL');
            setStartDate('');
            setEndDate('');
            setReason('');
            onClose();
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to submit request');
        } finally {
            setLoading(false);
        }
    };

    const handleClose = () => {
        if (!loading) {
            setError('');
            onClose();
        }
    };

    // Get today's date in YYYY-MM-DD format
    const today = new Date().toISOString().split('T')[0];

    return (
        <Modal
            isOpen={isOpen}
            onClose={handleClose}
            title="Request Leave"
            footer={
                <>
                    <Button variant="secondary" onClick={handleClose} disabled={loading}>
                        Cancel
                    </Button>
                    <Button onClick={handleSubmit} loading={loading}>
                        Submit Request
                    </Button>
                </>
            }
        >
            <div className={styles.form}>
                <div className={styles.field}>
                    <label className={styles.label}>Leave Type</label>
                    <select
                        className={styles.select}
                        value={leaveType}
                        onChange={(e) => setLeaveType(e.target.value as CreateLeaveRequest['leaveType'])}
                        disabled={loading}
                    >
                        {LEAVE_TYPES.map((type) => (
                            <option key={type.value} value={type.value}>
                                {type.label}
                            </option>
                        ))}
                    </select>
                </div>

                <div className={styles.row}>
                    <div className={styles.field}>
                        <label className={styles.label}>Start Date</label>
                        <input
                            type="date"
                            className={styles.input}
                            value={startDate}
                            onChange={(e) => setStartDate(e.target.value)}
                            min={today}
                            disabled={loading}
                        />
                    </div>

                    <div className={styles.field}>
                        <label className={styles.label}>End Date</label>
                        <input
                            type="date"
                            className={styles.input}
                            value={endDate}
                            onChange={(e) => setEndDate(e.target.value)}
                            min={startDate || today}
                            disabled={loading}
                        />
                    </div>
                </div>

                {totalDays > 0 && (
                    <div className={styles.totalDays}>
                        <div className={styles.totalDaysLabel}>Total Days</div>
                        <div className={styles.totalDaysValue}>{totalDays}</div>
                    </div>
                )}

                <div className={styles.field}>
                    <label className={styles.label}>Reason (Optional)</label>
                    <textarea
                        className={styles.textarea}
                        value={reason}
                        onChange={(e) => setReason(e.target.value)}
                        placeholder="Describe the reason for your leave..."
                        disabled={loading}
                    />
                </div>

                {error && <p className={styles.error}>{error}</p>}
            </div>
        </Modal>
    );
}

export default LeaveRequestForm;
