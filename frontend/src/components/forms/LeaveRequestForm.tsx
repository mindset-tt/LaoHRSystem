import { useState, useMemo, useRef } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
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
    'ANNUAL',
    'SICK',
    'PERSONAL',
    'MATERNITY',
    'PATERNITY',
    'UNPAID',
] as const;

/**
 * Leave Request Form Modal
 * 
 * Form for creating new leave requests with half-day option,
 * date validation, attachment upload, and total days calculation.
 */
export function LeaveRequestForm({
    isOpen,
    onClose,
    onSubmit,
}: LeaveRequestFormProps) {
    const { t } = useLanguage();
    const [leaveType, setLeaveType] = useState<CreateLeaveRequest['leaveType']>('ANNUAL');
    const [startDate, setStartDate] = useState('');
    const [endDate, setEndDate] = useState('');
    const [reason, setReason] = useState('');
    const [isHalfDay, setIsHalfDay] = useState(false);
    const [halfDayType, setHalfDayType] = useState<'MORNING' | 'AFTERNOON'>('MORNING');
    const [attachment, setAttachment] = useState<File | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const fileInputRef = useRef<HTMLInputElement>(null);

    // Calculate total days
    const totalDays = useMemo(() => {
        if (isHalfDay) return 0.5;
        if (!startDate || !endDate) return 0;
        const start = new Date(startDate);
        const end = new Date(endDate);
        if (end < start) return 0;
        const diffTime = Math.abs(end.getTime() - start.getTime());
        return Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1;
    }, [startDate, endDate, isHalfDay]);

    const handleSubmit = async () => {
        setError('');

        if (!startDate) {
            setError(t.leave.form.errors.dates);
            return;
        }

        if (!isHalfDay && !endDate) {
            setError(t.leave.form.errors.dates);
            return;
        }

        if (!isHalfDay && new Date(endDate) < new Date(startDate)) {
            setError(t.leave.form.errors.endDate);
            return;
        }

        setLoading(true);

        try {
            await onSubmit({
                leaveType,
                startDate,
                endDate: isHalfDay ? startDate : endDate,
                isHalfDay,
                halfDayType: isHalfDay ? halfDayType : undefined,
                reason: reason || undefined,
                attachment: attachment || undefined,
            });

            // Reset form
            resetForm();
            onClose();
        } catch (err) {
            setError(err instanceof Error ? err.message : t.leave.form.errors.submit);
        } finally {
            setLoading(false);
        }
    };

    const resetForm = () => {
        setLeaveType('ANNUAL');
        setStartDate('');
        setEndDate('');
        setReason('');
        setIsHalfDay(false);
        setHalfDayType('MORNING');
        setAttachment(null);
        if (fileInputRef.current) fileInputRef.current.value = '';
    };

    const handleClose = () => {
        if (!loading) {
            setError('');
            onClose();
        }
    };

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (file) {
            // Limit to 5MB
            if (file.size > 5 * 1024 * 1024) {
                setError('File size must be less than 5MB');
                return;
            }
            setAttachment(file);
        }
    };

    const getLeaveTypeLabel = (type: string) => {
        const key = type.toLowerCase() as keyof typeof t.leave.form.types;
        return t.leave.form.types[key] || type;
    };

    // Get today's date in YYYY-MM-DD format
    const today = new Date().toISOString().split('T')[0];

    return (
        <Modal
            isOpen={isOpen}
            onClose={handleClose}
            title={t.leave.form.title}
            footer={
                <>
                    <Button variant="secondary" onClick={handleClose} disabled={loading}>
                        {t.leave.form.cancel}
                    </Button>
                    <Button onClick={handleSubmit} loading={loading}>
                        {t.leave.form.submit}
                    </Button>
                </>
            }
        >
            <div className={styles.form}>
                <div className={styles.field}>
                    <label className={styles.label}>{t.leave.form.leaveType}</label>
                    <select
                        className={styles.select}
                        value={leaveType}
                        onChange={(e) => setLeaveType(e.target.value as CreateLeaveRequest['leaveType'])}
                        disabled={loading}
                    >
                        {LEAVE_TYPES.map((type) => (
                            <option key={type} value={type}>
                                {getLeaveTypeLabel(type)}
                            </option>
                        ))}
                    </select>
                </div>

                {/* Half-day toggle */}
                <div className={styles.field}>
                    <label className={styles.checkboxLabel}>
                        <input
                            type="checkbox"
                            checked={isHalfDay}
                            onChange={(e) => {
                                setIsHalfDay(e.target.checked);
                                if (e.target.checked) {
                                    setEndDate(startDate);
                                }
                            }}
                            disabled={loading}
                        />
                        <span>{t.leave.form.halfDay || 'Half Day'}</span>
                    </label>
                </div>

                {/* Half-day type selector */}
                {isHalfDay && (
                    <div className={styles.field}>
                        <label className={styles.label}>{t.leave.form.halfDayType || 'Time'}</label>
                        <div className={styles.radioGroup}>
                            <label className={styles.radioLabel}>
                                <input
                                    type="radio"
                                    name="halfDayType"
                                    value="MORNING"
                                    checked={halfDayType === 'MORNING'}
                                    onChange={() => setHalfDayType('MORNING')}
                                    disabled={loading}
                                />
                                <span>{t.leave.form.morning || 'Morning'}</span>
                            </label>
                            <label className={styles.radioLabel}>
                                <input
                                    type="radio"
                                    name="halfDayType"
                                    value="AFTERNOON"
                                    checked={halfDayType === 'AFTERNOON'}
                                    onChange={() => setHalfDayType('AFTERNOON')}
                                    disabled={loading}
                                />
                                <span>{t.leave.form.afternoon || 'Afternoon'}</span>
                            </label>
                        </div>
                    </div>
                )}

                <div className={styles.row}>
                    <div className={styles.field}>
                        <label className={styles.label}>{t.leave.form.startDate}</label>
                        <input
                            type="date"
                            className={styles.input}
                            value={startDate}
                            onChange={(e) => {
                                setStartDate(e.target.value);
                                if (isHalfDay) setEndDate(e.target.value);
                            }}
                            min={today}
                            disabled={loading}
                        />
                    </div>

                    {!isHalfDay && (
                        <div className={styles.field}>
                            <label className={styles.label}>{t.leave.form.endDate}</label>
                            <input
                                type="date"
                                className={styles.input}
                                value={endDate}
                                onChange={(e) => setEndDate(e.target.value)}
                                min={startDate || today}
                                disabled={loading}
                            />
                        </div>
                    )}
                </div>

                {totalDays > 0 && (
                    <div className={styles.totalDays}>
                        <div className={styles.totalDaysLabel}>{t.leave.form.totalDays}</div>
                        <div className={styles.totalDaysValue}>{totalDays}</div>
                    </div>
                )}

                <div className={styles.field}>
                    <label className={styles.label}>{t.leave.form.reason}</label>
                    <textarea
                        className={styles.textarea}
                        value={reason}
                        onChange={(e) => setReason(e.target.value)}
                        placeholder={t.leave.form.reasonPlaceholder}
                        disabled={loading}
                    />
                </div>

                {/* Attachment upload */}
                <div className={styles.field}>
                    <label className={styles.label}>{t.leave.form.attachment || 'Attachment'}</label>
                    <input
                        ref={fileInputRef}
                        type="file"
                        className={styles.fileInput}
                        accept=".pdf,.jpg,.jpeg,.png"
                        onChange={handleFileChange}
                        disabled={loading}
                    />
                    {attachment && (
                        <div className={styles.fileName}>
                            📎 {attachment.name}
                            <button
                                type="button"
                                onClick={() => {
                                    setAttachment(null);
                                    if (fileInputRef.current) fileInputRef.current.value = '';
                                }}
                                className={styles.removeFile}
                            >
                                ×
                            </button>
                        </div>
                    )}
                    <p className={styles.hint}>{t.leave.form.attachmentHint || 'PDF, JPG, PNG (max 5MB)'}</p>
                </div>

                {error && <p className={styles.error}>{error}</p>}
            </div>
        </Modal>
    );
}

export default LeaveRequestForm;
