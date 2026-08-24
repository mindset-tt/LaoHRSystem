'use client';

import React, { useEffect, useState, useCallback } from 'react';
import { useAuth } from '@/components/providers/AuthProvider';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Skeleton } from '@/components/ui/Skeleton';
import { workScheduleApi } from '@/lib/endpoints';
import { isHROrAdmin } from '@/lib/permissions';
import type { WorkSchedule, SaturdayWorkType } from '@/lib/types';
import styles from './page.module.css';

const WEEKDAYS = [
    { key: 'monday', en: 'Monday', lo: 'ຈັນ' },
    { key: 'tuesday', en: 'Tuesday', lo: 'ອັງຄານ' },
    { key: 'wednesday', en: 'Wednesday', lo: 'ພຸດ' },
    { key: 'thursday', en: 'Thursday', lo: 'ພະຫັດ' },
    { key: 'friday', en: 'Friday', lo: 'ສຸກ' },
] as const;

const SATURDAY_WEEKS = [
    { value: '1', en: '1st', lo: 'ທີ 1' },
    { value: '2', en: '2nd', lo: 'ທີ 2' },
    { value: '3', en: '3rd', lo: 'ທີ 3' },
    { value: '4', en: '4th', lo: 'ທີ 4' },
];

/**
 * Work Schedule Settings Page
 * Configure working days, hours, and Saturday configuration
 */
export default function WorkScheduleSettingsPage() {
    const { role } = useAuth();
    const { language } = useLanguage();
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [schedule, setSchedule] = useState<WorkSchedule | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);

    const canEdit = isHROrAdmin(role);

    const loadSchedule = useCallback(async () => {
        try {
            setError(null);
            const data = await workScheduleApi.get();
            setSchedule(data);
        } catch (err) {
            console.error('Failed to load schedule:', err);
            setError(language === 'lo' ? 'ບໍ່ສາມາດໂຫຼດຂໍ້ມູນໄດ້' : 'Failed to load schedule');
        } finally {
            setLoading(false);
        }
    }, [language]);

    useEffect(() => {
        React.startTransition(() => {
            loadSchedule();
        });
    }, [loadSchedule]);

    const handleFieldChange = <K extends keyof WorkSchedule>(field: K, value: WorkSchedule[K]) => {
        if (!schedule) return;
        setSchedule({ ...schedule, [field]: value });
    };

    const handleSaturdayWeekToggle = (week: string) => {
        if (!schedule) return;
        const currentWeeks = schedule.saturdayWeeks?.split(',').filter(w => w) || [];
        const newWeeks = currentWeeks.includes(week)
            ? currentWeeks.filter(w => w !== week)
            : [...currentWeeks, week];
        setSchedule({ ...schedule, saturdayWeeks: newWeeks.join(',') });
    };

    const handleSaturdayTypeChange = (type: SaturdayWorkType) => {
        if (!schedule) return;
        const saturday = type !== 'NONE';
        const hours = type === 'FULL' ? 8 : type === 'HALF' ? 4 : 0;
        setSchedule({
            ...schedule,
            saturdayWorkType: type,
            saturday,
            saturdayHours: hours,
        });
    };

    const handleSave = async () => {
        if (!schedule) return;

        setSaving(true);
        setError(null);
        setSuccess(null);

        try {
            await workScheduleApi.update({
                monday: schedule.monday,
                tuesday: schedule.tuesday,
                wednesday: schedule.wednesday,
                thursday: schedule.thursday,
                friday: schedule.friday,
                saturday: schedule.saturday,
                sunday: schedule.sunday,
                workStartTime: schedule.workStartTime,
                workEndTime: schedule.workEndTime,
                breakStartTime: schedule.breakStartTime,
                breakEndTime: schedule.breakEndTime,
                lateThresholdMinutes: schedule.lateThresholdMinutes,
                saturdayWorkType: schedule.saturdayWorkType,
                saturdayHours: schedule.saturdayHours,
                saturdayWeeks: schedule.saturdayWeeks,
                saturdayStartTime: schedule.saturdayStartTime,
                saturdayEndTime: schedule.saturdayEndTime,
                standardMonthlyHours: schedule.standardMonthlyHours,
                dailyWorkHours: schedule.dailyWorkHours,
            });
            setSuccess(language === 'lo' ? 'ບັນທຶກສຳເລັດ' : 'Schedule updated successfully');
        } catch (err) {
            console.error('Failed to save:', err);
            setError(language === 'lo' ? 'ບໍ່ສາມາດບັນທຶກໄດ້' : 'Failed to save changes');
        } finally {
            setSaving(false);
        }
    };

    if (!canEdit) {
        return (
            <div className={styles.container}>
                <Card>
                    <div className={styles.noAccess}>
                        <p>{language === 'lo' ? 'ທ່ານບໍ່ມີສິດເຂົ້າເຖິງໜ້ານີ້' : 'You do not have access to this page'}</p>
                    </div>
                </Card>
            </div>
        );
    }

    return (
        <div className={styles.container}>
            <div className={styles.header}>
                <h1 className={styles.title}>
                    {language === 'lo' ? 'ຕັ້ງຄ່າຕາຕະລາງການເຮັດວຽກ' : 'Work Schedule Settings'}
                </h1>
                <p className={styles.subtitle}>
                    {language === 'lo'
                        ? 'ກຳນົດວັນເຮັດວຽກ, ເວລາ ແລະ ຕັ້ງຄ່າວັນເສົາ'
                        : 'Configure working days, hours, and Saturday schedule'}
                </p>
            </div>

            {/* Alerts */}
            {error && (
                <div className={styles.errorAlert}>
                    {error}
                    <button onClick={() => setError(null)}>×</button>
                </div>
            )}
            {success && (
                <div className={styles.successAlert}>
                    {success}
                    <button onClick={() => setSuccess(null)}>×</button>
                </div>
            )}

            {loading ? (
                <div className={styles.loading}>
                    <Skeleton height={400} />
                </div>
            ) : schedule && (
                <div className={styles.content}>
                    {/* Standard Monthly Hours (Laos Law) */}
                    <Card>
                        <h2 className={styles.sectionTitle}>
                            {language === 'lo' ? 'ຊົ່ວໂມງມາດຕະຖານ (ກົດໝາຍແຮງງານ)' : 'Standard Hours (Labor Law)'}
                        </h2>
                        <div className={styles.standardHours}>
                            <div className={styles.field}>
                                <label className={styles.label}>
                                    {language === 'lo' ? 'ຊົ່ວໂມງ/ເດືອນ' : 'Hours/Month'}
                                </label>
                                <input
                                    type="number"
                                    className={styles.input}
                                    value={schedule.standardMonthlyHours}
                                    onChange={(e) => handleFieldChange('standardMonthlyHours', Number(e.target.value))}
                                    min={0}
                                    max={200}
                                />
                            </div>
                            <div className={styles.field}>
                                <label className={styles.label}>
                                    {language === 'lo' ? 'ຊົ່ວໂມງ/ວັນ' : 'Hours/Day'}
                                </label>
                                <input
                                    type="number"
                                    className={styles.input}
                                    value={schedule.dailyWorkHours}
                                    onChange={(e) => handleFieldChange('dailyWorkHours', Number(e.target.value))}
                                    min={0}
                                    max={12}
                                />
                            </div>
                            <div className={styles.lawNote}>
                                <span>📋</span>
                                {language === 'lo'
                                    ? 'ກົດໝາຍແຮງງານລາວ: 160 ຊົ່ວໂມງ/ເດືອນ (20 ວັນ × 8 ຊົ່ວໂມງ)'
                                    : 'Laos Labor Law: 160 hours/month (20 days × 8 hours)'}
                            </div>
                        </div>
                    </Card>

                    {/* Working Days (Mon-Fri) */}
                    <Card>
                        <h2 className={styles.sectionTitle}>
                            {language === 'lo' ? 'ວັນເຮັດວຽກ (ຈັນ-ສຸກ)' : 'Working Days (Mon-Fri)'}
                        </h2>
                        <div className={styles.daysGrid}>
                            {WEEKDAYS.map(({ key, en, lo }) => (
                                <label key={key} className={styles.dayToggle}>
                                    <input
                                        type="checkbox"
                                        checked={schedule[key as keyof WorkSchedule] as boolean}
                                        onChange={() => handleFieldChange(key as keyof WorkSchedule, !schedule[key as keyof WorkSchedule] as never)}
                                    />
                                    <span className={styles.dayLabel}>
                                        {language === 'lo' ? lo : en}
                                    </span>
                                </label>
                            ))}
                        </div>
                    </Card>

                    {/* Saturday Configuration */}
                    <Card>
                        <h2 className={styles.sectionTitle}>
                            {language === 'lo' ? 'ຕັ້ງຄ່າວັນເສົາ' : 'Saturday Configuration'}
                        </h2>

                        <div className={styles.saturdayConfig}>
                            <div className={styles.fieldGroup}>
                                <label className={styles.label}>
                                    {language === 'lo' ? 'ປະເພດວັນເສົາ' : 'Saturday Work Type'}
                                </label>
                                <div className={styles.radioGroup}>
                                    <label className={styles.radioOption}>
                                        <input
                                            type="radio"
                                            name="saturdayType"
                                            checked={schedule.saturdayWorkType === 'NONE'}
                                            onChange={() => handleSaturdayTypeChange('NONE')}
                                        />
                                        <span>{language === 'lo' ? 'ບໍ່ເຮັດວຽກ' : 'No Work'}</span>
                                    </label>
                                    <label className={styles.radioOption}>
                                        <input
                                            type="radio"
                                            name="saturdayType"
                                            checked={schedule.saturdayWorkType === 'HALF'}
                                            onChange={() => handleSaturdayTypeChange('HALF')}
                                        />
                                        <span>{language === 'lo' ? 'ເຄິ່ງມື້ (4 ຊມ)' : 'Half Day (4 hrs)'}</span>
                                    </label>
                                    <label className={styles.radioOption}>
                                        <input
                                            type="radio"
                                            name="saturdayType"
                                            checked={schedule.saturdayWorkType === 'FULL'}
                                            onChange={() => handleSaturdayTypeChange('FULL')}
                                        />
                                        <span>{language === 'lo' ? 'ເຕັມມື້ (8 ຊມ)' : 'Full Day (8 hrs)'}</span>
                                    </label>
                                </div>
                            </div>

                            {schedule.saturdayWorkType !== 'NONE' && (
                                <>
                                    <div className={styles.fieldGroup}>
                                        <label className={styles.label}>
                                            {language === 'lo' ? 'ວັນເສົາໃດ?' : 'Which Saturdays?'}
                                        </label>
                                        <div className={styles.weeksGrid}>
                                            {SATURDAY_WEEKS.map(({ value, en, lo }) => (
                                                <label key={value} className={styles.weekToggle}>
                                                    <input
                                                        type="checkbox"
                                                        checked={schedule.saturdayWeeks?.includes(value)}
                                                        onChange={() => handleSaturdayWeekToggle(value)}
                                                    />
                                                    <span>{language === 'lo' ? lo : en}</span>
                                                </label>
                                            ))}
                                            <label className={styles.weekToggle}>
                                                <input
                                                    type="checkbox"
                                                    checked={schedule.saturdayWeeks === 'ALL'}
                                                    onChange={() => handleFieldChange('saturdayWeeks', schedule.saturdayWeeks === 'ALL' ? '' : 'ALL')}
                                                />
                                                <span>{language === 'lo' ? 'ທຸກເສົາ' : 'All'}</span>
                                            </label>
                                        </div>
                                    </div>

                                    <div className={styles.hoursGrid}>
                                        <div className={styles.field}>
                                            <label className={styles.label}>
                                                {language === 'lo' ? 'ເວລາເລີ່ມ (ເສົາ)' : 'Saturday Start'}
                                            </label>
                                            <input
                                                type="time"
                                                className={styles.input}
                                                value={schedule.saturdayStartTime?.substring(0, 5) || schedule.workStartTime?.substring(0, 5) || '08:00'}
                                                onChange={(e) => handleFieldChange('saturdayStartTime', e.target.value + ':00')}
                                            />
                                        </div>
                                        <div className={styles.field}>
                                            <label className={styles.label}>
                                                {language === 'lo' ? 'ເວລາສິ້ນສຸດ (ເສົາ)' : 'Saturday End'}
                                            </label>
                                            <input
                                                type="time"
                                                className={styles.input}
                                                value={schedule.saturdayEndTime?.substring(0, 5) || (schedule.saturdayWorkType === 'HALF' ? '12:00' : schedule.workEndTime?.substring(0, 5) || '17:00')}
                                                onChange={(e) => handleFieldChange('saturdayEndTime', e.target.value + ':00')}
                                            />
                                        </div>
                                    </div>
                                </>
                            )}
                        </div>
                    </Card>

                    {/* Work Hours (Mon-Fri) */}
                    <Card>
                        <h2 className={styles.sectionTitle}>
                            {language === 'lo' ? 'ເວລາເຮັດວຽກ (ຈັນ-ສຸກ)' : 'Work Hours (Mon-Fri)'}
                        </h2>
                        <div className={styles.hoursGrid}>
                            <div className={styles.field}>
                                <label className={styles.label}>
                                    {language === 'lo' ? 'ເລີ່ມເຮັດວຽກ' : 'Work Start'}
                                </label>
                                <input
                                    type="time"
                                    className={styles.input}
                                    value={schedule.workStartTime?.substring(0, 5) || '08:00'}
                                    onChange={(e) => handleFieldChange('workStartTime', e.target.value + ':00')}
                                />
                            </div>
                            <div className={styles.field}>
                                <label className={styles.label}>
                                    {language === 'lo' ? 'ເລີກວຽກ' : 'Work End'}
                                </label>
                                <input
                                    type="time"
                                    className={styles.input}
                                    value={schedule.workEndTime?.substring(0, 5) || '17:00'}
                                    onChange={(e) => handleFieldChange('workEndTime', e.target.value + ':00')}
                                />
                            </div>
                            <div className={styles.field}>
                                <label className={styles.label}>
                                    {language === 'lo' ? 'ພັກຜ່ອນ (ເລີ່ມ)' : 'Break Start'}
                                </label>
                                <input
                                    type="time"
                                    className={styles.input}
                                    value={schedule.breakStartTime?.substring(0, 5) || '12:00'}
                                    onChange={(e) => handleFieldChange('breakStartTime', e.target.value + ':00')}
                                />
                            </div>
                            <div className={styles.field}>
                                <label className={styles.label}>
                                    {language === 'lo' ? 'ພັກຜ່ອນ (ສິ້ນສຸດ)' : 'Break End'}
                                </label>
                                <input
                                    type="time"
                                    className={styles.input}
                                    value={schedule.breakEndTime?.substring(0, 5) || '13:00'}
                                    onChange={(e) => handleFieldChange('breakEndTime', e.target.value + ':00')}
                                />
                            </div>
                            <div className={styles.field}>
                                <label className={styles.label}>
                                    {language === 'lo' ? 'ມາຊ້າໄດ້ (ນາທີ)' : 'Late Threshold (min)'}
                                </label>
                                <input
                                    type="number"
                                    className={styles.input}
                                    value={schedule.lateThresholdMinutes}
                                    onChange={(e) => handleFieldChange('lateThresholdMinutes', Number(e.target.value))}
                                    min={0}
                                    max={60}
                                />
                            </div>
                        </div>
                    </Card>

                    {/* Summary */}
                    <Card>
                        <h2 className={styles.sectionTitle}>
                            {language === 'lo' ? 'ສະຫຼຸບ' : 'Summary'}
                        </h2>
                        <div className={styles.preview}>
                            <div className={styles.previewItem}>
                                <span className={styles.previewLabel}>
                                    {language === 'lo' ? 'ວັນເຮັດວຽກ:' : 'Working days:'}
                                </span>
                                <span className={styles.previewValue}>
                                    {language === 'lo' ? 'ຈັນ-ສຸກ' : 'Mon-Fri'}
                                    {schedule.saturdayWorkType !== 'NONE' && (
                                        <> + {language === 'lo' ? 'ເສົາ' : 'Sat'}
                                            ({schedule.saturdayWorkType === 'HALF'
                                                ? (language === 'lo' ? 'ເຄິ່ງມື້' : 'Half')
                                                : (language === 'lo' ? 'ເຕັມມື້' : 'Full')})</>
                                    )}
                                </span>
                            </div>
                            <div className={styles.previewItem}>
                                <span className={styles.previewLabel}>
                                    {language === 'lo' ? 'ເວລາເຮັດວຽກ:' : 'Work hours:'}
                                </span>
                                <span className={styles.previewValue}>
                                    {schedule.workStartTime?.substring(0, 5)} - {schedule.workEndTime?.substring(0, 5)}
                                </span>
                            </div>
                            <div className={styles.previewItem}>
                                <span className={styles.previewLabel}>
                                    {language === 'lo' ? 'ຊົ່ວໂມງມາດຕະຖານ:' : 'Standard hours:'}
                                </span>
                                <span className={styles.previewValue}>
                                    {schedule.standardMonthlyHours} {language === 'lo' ? 'ຊມ/ເດືອນ' : 'hrs/month'}
                                </span>
                            </div>
                            {schedule.saturdayWorkType !== 'NONE' && (
                                <div className={styles.previewItem}>
                                    <span className={styles.previewLabel}>
                                        {language === 'lo' ? 'ວັນເສົາ:' : 'Saturdays:'}
                                    </span>
                                    <span className={styles.previewValue}>
                                        {schedule.saturdayWeeks === 'ALL'
                                            ? (language === 'lo' ? 'ທຸກເສົາ' : 'All Saturdays')
                                            : schedule.saturdayWeeks?.split(',').map(w =>
                                                language === 'lo' ? `ທີ ${w}` : `${w}${['st', 'nd', 'rd', 'th'][Number(w) - 1] || 'th'}`
                                            ).join(', ')}
                                    </span>
                                </div>
                            )}
                        </div>
                    </Card>

                    {/* Save Button */}
                    <div className={styles.actions}>
                        <Button onClick={handleSave} loading={saving}>
                            {language === 'lo' ? 'ບັນທຶກ' : 'Save Changes'}
                        </Button>
                    </div>
                </div>
            )}
        </div>
    );
}
