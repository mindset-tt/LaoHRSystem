'use client';

import { useEffect, useState, useCallback } from 'react';
import { useAuth } from '@/components/providers/AuthProvider';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Skeleton } from '@/components/ui/Skeleton';
import { Modal } from '@/components/ui/Modal';
import { holidaysApi } from '@/lib/endpoints';
import { isHROrAdmin } from '@/lib/permissions';
import { formatDate } from '@/lib/datetime';
import type { Holiday, CreateHoliday } from '@/lib/types';
import styles from './page.module.css';

/**
 * Holidays Settings Page
 * Manage company holidays
 */
export default function HolidaysSettingsPage() {
    const { role } = useAuth();
    const { language } = useLanguage();
    const [loading, setLoading] = useState(true);
    const [holidays, setHolidays] = useState<Holiday[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);
    const [showModal, setShowModal] = useState(false);
    const [editingHoliday, setEditingHoliday] = useState<Holiday | null>(null);
    const [saving, setSaving] = useState(false);

    // Form state
    const [formData, setFormData] = useState<CreateHoliday>({
        date: '',
        name: '',
        nameLao: '',
        description: '',
        isRecurring: true,
    });

    const canEdit = isHROrAdmin(role);

    const loadHolidays = useCallback(async () => {
        try {
            setError(null);
            const data = await holidaysApi.getAll();
            setHolidays(data);
        } catch (err) {
            console.error('Failed to load holidays:', err);
            setError(language === 'lo' ? 'ບໍ່ສາມາດໂຫຼດຂໍ້ມູນໄດ້' : 'Failed to load holidays');
        } finally {
            setLoading(false);
        }
    }, [language]);

    useEffect(() => {
        loadHolidays();
    }, [loadHolidays]);

    const handleAdd = () => {
        setEditingHoliday(null);
        setFormData({
            date: '',
            name: '',
            nameLao: '',
            description: '',
            isRecurring: true,
        });
        setShowModal(true);
    };

    const handleEdit = (holiday: Holiday) => {
        setEditingHoliday(holiday);
        setFormData({
            date: holiday.date.split('T')[0],
            name: holiday.name,
            nameLao: holiday.nameLao || '',
            description: holiday.description || '',
            isRecurring: holiday.isRecurring,
        });
        setShowModal(true);
    };

    const handleDelete = async (id: number) => {
        if (!confirm(language === 'lo' ? 'ທ່ານແນ່ໃຈບໍ່ວ່າຕ້ອງການລຶບ?' : 'Are you sure you want to delete?')) {
            return;
        }

        try {
            await holidaysApi.delete(id);
            setHolidays(holidays.filter(h => h.holidayId !== id));
            setSuccess(language === 'lo' ? 'ລຶບສຳເລັດ' : 'Holiday deleted');
        } catch (err) {
            console.error('Failed to delete:', err);
            setError(language === 'lo' ? 'ບໍ່ສາມາດລຶບໄດ້' : 'Failed to delete');
        }
    };

    const handleSave = async () => {
        setSaving(true);
        setError(null);

        try {
            if (editingHoliday) {
                const updated = await holidaysApi.update(editingHoliday.holidayId, {
                    ...formData,
                    isActive: true,
                });
                setHolidays(holidays.map(h => h.holidayId === updated.holidayId ? updated : h));
            } else {
                const created = await holidaysApi.create(formData);
                setHolidays([...holidays, created]);
            }
            setShowModal(false);
            setSuccess(language === 'lo' ? 'ບັນທຶກສຳເລັດ' : 'Saved successfully');
        } catch (err) {
            console.error('Failed to save:', err);
            setError(language === 'lo' ? 'ບໍ່ສາມາດບັນທຶກໄດ້' : 'Failed to save');
        } finally {
            setSaving(false);
        }
    };

    const handleSeedDefaults = async () => {
        try {
            await holidaysApi.seedDefaults();
            await loadHolidays();
            setSuccess(language === 'lo' ? 'ເພີ່ມວັນພັກພື້ນຖານສຳເລັດ' : 'Default holidays added');
        } catch (err) {
            console.error('Failed to seed:', err);
            setError(language === 'lo' ? 'ບໍ່ສາມາດເພີ່ມໄດ້' : 'Failed to seed defaults');
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
                <div>
                    <h1 className={styles.title}>
                        {language === 'lo' ? 'ວັນພັກ' : 'Holidays'}
                    </h1>
                    <p className={styles.subtitle}>
                        {language === 'lo' ? 'ຈັດການວັນພັກຂອງບໍລິສັດ' : 'Manage company holidays'}
                    </p>
                </div>
                <div className={styles.headerActions}>
                    <Button variant="secondary" onClick={handleSeedDefaults}>
                        {language === 'lo' ? 'ເພີ່ມວັນພັກລາວ' : 'Add Lao Holidays'}
                    </Button>
                    <Button onClick={handleAdd}>
                        {language === 'lo' ? '+ ເພີ່ມວັນພັກ' : '+ Add Holiday'}
                    </Button>
                </div>
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

            {/* Holidays Table */}
            <Card noPadding>
                {loading ? (
                    <div className={styles.loading}>
                        <Skeleton height={300} />
                    </div>
                ) : holidays.length === 0 ? (
                    <div className={styles.empty}>
                        <p>{language === 'lo' ? 'ບໍ່ມີວັນພັກ' : 'No holidays configured'}</p>
                        <Button variant="secondary" onClick={handleSeedDefaults}>
                            {language === 'lo' ? 'ເພີ່ມວັນພັກພື້ນຖານ' : 'Add Default Holidays'}
                        </Button>
                    </div>
                ) : (
                    <div className={styles.tableWrapper}>
                        <table className={styles.table}>
                            <thead>
                                <tr>
                                    <th>{language === 'lo' ? 'ວັນທີ' : 'Date'}</th>
                                    <th>{language === 'lo' ? 'ຊື່' : 'Name'}</th>
                                    <th>{language === 'lo' ? 'ຊື່ລາວ' : 'Lao Name'}</th>
                                    <th>{language === 'lo' ? 'ປະຈຳປີ' : 'Recurring'}</th>
                                    <th></th>
                                </tr>
                            </thead>
                            <tbody>
                                {holidays.map(holiday => (
                                    <tr key={holiday.holidayId}>
                                        <td>{formatDate(holiday.date)}</td>
                                        <td>{holiday.name}</td>
                                        <td>{holiday.nameLao || '-'}</td>
                                        <td>
                                            <span className={`${styles.badge} ${holiday.isRecurring ? styles.recurring : ''}`}>
                                                {holiday.isRecurring
                                                    ? (language === 'lo' ? 'ປະຈຳປີ' : 'Yes')
                                                    : (language === 'lo' ? 'ຄັ້ງດຽວ' : 'No')}
                                            </span>
                                        </td>
                                        <td>
                                            <div className={styles.rowActions}>
                                                <button
                                                    className={styles.editBtn}
                                                    onClick={() => handleEdit(holiday)}
                                                >
                                                    {language === 'lo' ? 'ແກ້ໄຂ' : 'Edit'}
                                                </button>
                                                <button
                                                    className={styles.deleteBtn}
                                                    onClick={() => handleDelete(holiday.holidayId)}
                                                >
                                                    {language === 'lo' ? 'ລຶບ' : 'Delete'}
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                )}
            </Card>

            {/* Add/Edit Modal */}
            <Modal
                isOpen={showModal}
                onClose={() => setShowModal(false)}
                title={editingHoliday
                    ? (language === 'lo' ? 'ແກ້ໄຂວັນພັກ' : 'Edit Holiday')
                    : (language === 'lo' ? 'ເພີ່ມວັນພັກ' : 'Add Holiday')}
            >
                <div className={styles.form}>
                    <div className={styles.field}>
                        <label className={styles.label}>
                            {language === 'lo' ? 'ວັນທີ' : 'Date'} *
                        </label>
                        <input
                            type="date"
                            className={styles.input}
                            value={formData.date}
                            onChange={(e) => setFormData({ ...formData, date: e.target.value })}
                            required
                        />
                    </div>
                    <div className={styles.field}>
                        <label className={styles.label}>
                            {language === 'lo' ? 'ຊື່ (ພາສາອັງກິດ)' : 'Name (English)'} *
                        </label>
                        <input
                            type="text"
                            className={styles.input}
                            value={formData.name}
                            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                            placeholder="e.g. New Year's Day"
                            required
                        />
                    </div>
                    <div className={styles.field}>
                        <label className={styles.label}>
                            {language === 'lo' ? 'ຊື່ (ພາສາລາວ)' : 'Name (Lao)'}
                        </label>
                        <input
                            type="text"
                            className={styles.input}
                            value={formData.nameLao}
                            onChange={(e) => setFormData({ ...formData, nameLao: e.target.value })}
                            placeholder="ເຊັ່ນ: ວັນປີໃໝ່"
                        />
                    </div>
                    <div className={styles.field}>
                        <label className={styles.label}>
                            {language === 'lo' ? 'ລາຍລະອຽດ' : 'Description'}
                        </label>
                        <textarea
                            className={styles.textarea}
                            value={formData.description}
                            onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                            rows={2}
                        />
                    </div>
                    <div className={styles.checkboxField}>
                        <label className={styles.checkboxLabel}>
                            <input
                                type="checkbox"
                                checked={formData.isRecurring}
                                onChange={(e) => setFormData({ ...formData, isRecurring: e.target.checked })}
                            />
                            <span>
                                {language === 'lo'
                                    ? 'ວັນພັກປະຈຳປີ (ຊ້ຳກັນທຸກປີ)'
                                    : 'Recurring (repeats every year)'}
                            </span>
                        </label>
                    </div>
                    <div className={styles.modalActions}>
                        <Button variant="secondary" onClick={() => setShowModal(false)}>
                            {language === 'lo' ? 'ຍົກເລີກ' : 'Cancel'}
                        </Button>
                        <Button onClick={handleSave} loading={saving} disabled={!formData.date || !formData.name}>
                            {language === 'lo' ? 'ບັນທຶກ' : 'Save'}
                        </Button>
                    </div>
                </div>
            </Modal>
        </div>
    );
}
