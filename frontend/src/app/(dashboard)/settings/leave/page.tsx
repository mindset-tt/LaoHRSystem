'use client';

import { useEffect, useState, useCallback } from 'react';
import { useAuth } from '@/components/providers/AuthProvider';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Skeleton } from '@/components/ui/Skeleton';
import { leaveApi } from '@/lib/endpoints';
import { isHROrAdmin } from '@/lib/permissions';
import type { LeavePolicy } from '@/lib/types';
import styles from './page.module.css';

/**
 * Leave Policy Settings Page
 * Admin-only page for configuring leave policies
 */
export default function LeavePolicySettingsPage() {
    const { role } = useAuth();
    const { t, language } = useLanguage();
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState<number | null>(null);
    const [policies, setPolicies] = useState<LeavePolicy[]>([]);
    const [editedPolicies, setEditedPolicies] = useState<Record<number, Partial<LeavePolicy>>>({});
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);

    const canEdit = isHROrAdmin(role);

    const loadPolicies = useCallback(async () => {
        try {
            setError(null);
            const data = await leaveApi.getPolicies();
            setPolicies(data);
        } catch (err) {
            console.error('Failed to load policies:', err);
            setError('Failed to load leave policies');
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        loadPolicies();
    }, [loadPolicies]);

    const handleChange = (policyId: number, field: keyof LeavePolicy, value: number | boolean) => {
        setEditedPolicies(prev => ({
            ...prev,
            [policyId]: {
                ...prev[policyId],
                [field]: value
            }
        }));
    };

    const handleSave = async (policyId: number) => {
        const changes = editedPolicies[policyId];
        if (!changes) return;

        setSaving(policyId);
        setError(null);
        setSuccess(null);

        try {
            const policy = policies.find(p => p.leavePolicyId === policyId);
            if (!policy) return;

            const updated = await leaveApi.updatePolicy(policyId, {
                annualQuota: changes.annualQuota ?? policy.annualQuota,
                maxCarryOver: changes.maxCarryOver ?? policy.maxCarryOver,
                accrualPerMonth: changes.accrualPerMonth ?? policy.accrualPerMonth,
                requiresAttachment: changes.requiresAttachment ?? policy.requiresAttachment,
                minDaysForAttachment: changes.minDaysForAttachment ?? policy.minDaysForAttachment,
                allowHalfDay: changes.allowHalfDay ?? policy.allowHalfDay,
            });

            setPolicies(prev => prev.map(p => p.leavePolicyId === policyId ? updated : p));
            setEditedPolicies(prev => {
                const newState = { ...prev };
                delete newState[policyId];
                return newState;
            });
            setSuccess(language === 'lo' ? 'ບັນທຶກສຳເລັດ' : 'Policy updated successfully');
        } catch (err) {
            console.error('Failed to save policy:', err);
            setError('Failed to save changes');
        } finally {
            setSaving(null);
        }
    };

    const getValue = (policy: LeavePolicy, field: keyof LeavePolicy) => {
        const edited = editedPolicies[policy.leavePolicyId];
        if (edited && field in edited) {
            return edited[field];
        }
        return policy[field];
    };

    const hasChanges = (policyId: number) => {
        return !!editedPolicies[policyId] && Object.keys(editedPolicies[policyId]).length > 0;
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
                    {language === 'lo' ? 'ຕັ້ງຄ່ານະໂຍບາຍການລາ' : 'Leave Policy Settings'}
                </h1>
                <p className={styles.subtitle}>
                    {language === 'lo' ? 'ຈັດການວັນລາພັກ ແລະ ນະໂຍບາຍຕ່າງໆ' : 'Configure leave quotas and policies'}
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

            {/* Policy Cards */}
            {loading ? (
                <div className={styles.loading}>
                    <Skeleton height={200} />
                    <Skeleton height={200} />
                </div>
            ) : (
                <div className={styles.policiesGrid}>
                    {policies.map(policy => (
                        <Card key={policy.leavePolicyId}>
                            <div className={styles.policyCard}>
                                <div className={styles.policyHeader}>
                                    <h3 className={styles.policyName}>
                                        {policy.leaveTypeLao && language === 'lo'
                                            ? policy.leaveTypeLao
                                            : policy.leaveType}
                                    </h3>
                                    <span className={`${styles.statusBadge} ${policy.isActive ? styles.active : styles.inactive}`}>
                                        {policy.isActive ? (language === 'lo' ? 'ເປີດໃຊ້' : 'Active') : (language === 'lo' ? 'ປິດໃຊ້' : 'Inactive')}
                                    </span>
                                </div>

                                <div className={styles.policyFields}>
                                    {/* Annual Quota */}
                                    <div className={styles.field}>
                                        <label className={styles.label}>
                                            {language === 'lo' ? 'ວັນລາປະຈຳປີ' : 'Annual Quota'}
                                        </label>
                                        <input
                                            type="number"
                                            className={styles.input}
                                            value={getValue(policy, 'annualQuota') as number}
                                            onChange={(e) => handleChange(policy.leavePolicyId, 'annualQuota', parseInt(e.target.value) || 0)}
                                            min={0}
                                        />
                                    </div>

                                    {/* Max Carry Over */}
                                    <div className={styles.field}>
                                        <label className={styles.label}>
                                            {language === 'lo' ? 'ຍົກຍອດສູງສຸດ' : 'Max Carry Over'}
                                        </label>
                                        <input
                                            type="number"
                                            className={styles.input}
                                            value={getValue(policy, 'maxCarryOver') as number}
                                            onChange={(e) => handleChange(policy.leavePolicyId, 'maxCarryOver', parseInt(e.target.value) || 0)}
                                            min={0}
                                        />
                                    </div>

                                    {/* Accrual Per Month */}
                                    <div className={styles.field}>
                                        <label className={styles.label}>
                                            {language === 'lo' ? 'ສະສົມ/ເດືອນ' : 'Accrual/Month'}
                                        </label>
                                        <input
                                            type="number"
                                            className={styles.input}
                                            value={getValue(policy, 'accrualPerMonth') as number}
                                            onChange={(e) => handleChange(policy.leavePolicyId, 'accrualPerMonth', parseFloat(e.target.value) || 0)}
                                            min={0}
                                            step={0.25}
                                        />
                                    </div>

                                    {/* Min Days for Attachment */}
                                    <div className={styles.field}>
                                        <label className={styles.label}>
                                            {language === 'lo' ? 'ຕ້ອງແນບໄຟລ໌(ມື້)' : 'Attach Required (days)'}
                                        </label>
                                        <input
                                            type="number"
                                            className={styles.input}
                                            value={getValue(policy, 'minDaysForAttachment') as number}
                                            onChange={(e) => handleChange(policy.leavePolicyId, 'minDaysForAttachment', parseInt(e.target.value) || 0)}
                                            min={0}
                                        />
                                    </div>

                                    {/* Checkboxes */}
                                    <div className={styles.checkboxGroup}>
                                        <label className={styles.checkboxLabel}>
                                            <input
                                                type="checkbox"
                                                checked={getValue(policy, 'allowHalfDay') as boolean}
                                                onChange={(e) => handleChange(policy.leavePolicyId, 'allowHalfDay', e.target.checked)}
                                            />
                                            <span>{language === 'lo' ? 'ອະນຸຍາດເຄິ່ງມື້' : 'Allow Half Day'}</span>
                                        </label>
                                        <label className={styles.checkboxLabel}>
                                            <input
                                                type="checkbox"
                                                checked={getValue(policy, 'requiresAttachment') as boolean}
                                                onChange={(e) => handleChange(policy.leavePolicyId, 'requiresAttachment', e.target.checked)}
                                            />
                                            <span>{language === 'lo' ? 'ຕ້ອງແນບໄຟລ໌' : 'Requires Attachment'}</span>
                                        </label>
                                    </div>
                                </div>

                                <div className={styles.policyActions}>
                                    <Button
                                        onClick={() => handleSave(policy.leavePolicyId)}
                                        loading={saving === policy.leavePolicyId}
                                        disabled={!hasChanges(policy.leavePolicyId)}
                                    >
                                        {language === 'lo' ? 'ບັນທຶກ' : 'Save'}
                                    </Button>
                                </div>
                            </div>
                        </Card>
                    ))}
                </div>
            )}
        </div>
    );
}
