'use client';

import { useState } from 'react';
import { Button } from '@/components/ui/Button';
import { Modal } from '@/components/ui/Modal';
import { getCurrentLaoDate } from '@/lib/datetime';
import styles from './NewPeriodModal.module.css';

interface NewPeriodModalProps {
    isOpen: boolean;
    onClose: () => void;
    onSubmit: (year: number, month: number) => Promise<void>;
}

/**
 * New Period Modal
 * 
 * Form to create a new payroll period by selecting year and month.
 */
export function NewPeriodModal({
    isOpen,
    onClose,
    onSubmit,
}: NewPeriodModalProps) {
    const today = getCurrentLaoDate();
    const [year, setYear] = useState(today.getFullYear());
    // Default to next month if late in month, otherwise current month
    const [month, setMonth] = useState(today.getMonth() + 1);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');

    const years = [today.getFullYear() - 1, today.getFullYear(), today.getFullYear() + 1];
    const months = [
        { value: 1, label: 'January' },
        { value: 2, label: 'February' },
        { value: 3, label: 'March' },
        { value: 4, label: 'April' },
        { value: 5, label: 'May' },
        { value: 6, label: 'June' },
        { value: 7, label: 'July' },
        { value: 8, label: 'August' },
        { value: 9, label: 'September' },
        { value: 10, label: 'October' },
        { value: 11, label: 'November' },
        { value: 12, label: 'December' },
    ];

    const handleSubmit = async () => {
        setError('');
        setLoading(true);

        try {
            await onSubmit(year, month);
            onClose();
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to create period');
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

    return (
        <Modal
            isOpen={isOpen}
            onClose={handleClose}
            title="New Payroll Period"
            footer={
                <>
                    <Button variant="secondary" onClick={handleClose} disabled={loading}>
                        Cancel
                    </Button>
                    <Button onClick={handleSubmit} loading={loading}>
                        Create Period
                    </Button>
                </>
            }
            size="sm"
        >
            <div className={styles.form}>
                <div className={styles.periodPreview}>
                    <div className={styles.previewLabel}>New Period</div>
                    <div className={styles.previewValue}>
                        {months.find((m) => m.value === month)?.label} {year}
                    </div>
                </div>

                <div className={styles.row}>
                    <div className={styles.field}>
                        <label className={styles.label}>Month</label>
                        <select
                            className={styles.select}
                            value={month}
                            onChange={(e) => setMonth(parseInt(e.target.value))}
                            disabled={loading}
                        >
                            {months.map((m) => (
                                <option key={m.value} value={m.value}>
                                    {m.label}
                                </option>
                            ))}
                        </select>
                    </div>

                    <div className={styles.field}>
                        <label className={styles.label}>Year</label>
                        <select
                            className={styles.select}
                            value={year}
                            onChange={(e) => setYear(parseInt(e.target.value))}
                            disabled={loading}
                        >
                            {years.map((y) => (
                                <option key={y} value={y}>
                                    {y}
                                </option>
                            ))}
                        </select>
                    </div>
                </div>

                {error && <p className={styles.error}>{error}</p>}
            </div>
        </Modal>
    );
}

export default NewPeriodModal;
