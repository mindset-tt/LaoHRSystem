'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { departmentsApi, employeesApi } from '@/lib/endpoints';
import type { Employee, Department, CreateEmployeeRequest } from '@/lib/types';
import styles from './EmployeeForm.module.css';

interface EmployeeFormProps {
    employee?: Employee;
    onSuccess?: () => void;
}

/**
 * Employee Form Component
 * Reusable form for creating and editing employees
 */
export function EmployeeForm({ employee, onSuccess }: EmployeeFormProps) {
    const router = useRouter();
    const { t } = useLanguage();
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [departments, setDepartments] = useState<Department[]>([]);

    const isEditing = !!employee;

    // Form state
    const [formData, setFormData] = useState({
        employeeCode: employee?.employeeCode || '',
        laoName: employee?.laoName || '',
        englishName: employee?.englishName || '',
        email: employee?.email || '',
        phone: employee?.phone || '',
        dateOfBirth: employee?.dateOfBirth || '',
        gender: employee?.gender || '',
        departmentId: employee?.departmentId?.toString() || '',
        jobTitle: employee?.jobTitle || '',
        hireDate: employee?.hireDate || '',
        baseSalary: employee?.baseSalary?.toString() || '',
        salaryCurrency: employee?.salaryCurrency || 'LAK',
        bankName: employee?.bankName || '',
        bankAccount: employee?.bankAccount || '',
        dependentCount: employee?.dependentCount?.toString() || '0',
    });

    // Validation errors
    const [errors, setErrors] = useState<Record<string, string>>({});

    useEffect(() => {
        const loadDepartments = async () => {
            try {
                const data = await departmentsApi.getAll();
                setDepartments(data);
            } catch {
                // Fallback to mock data
                setDepartments([
                    { departmentId: 1, departmentName: 'Engineering', isActive: true, createdAt: '' },
                    { departmentId: 2, departmentName: 'Human Resources', isActive: true, createdAt: '' },
                    { departmentId: 3, departmentName: 'Finance', isActive: true, createdAt: '' },
                ]);
            }
        };
        loadDepartments();
    }, []);

    const validate = (): boolean => {
        const newErrors: Record<string, string> = {};

        if (!formData.employeeCode.trim()) {
            newErrors.employeeCode = `${t.employeeForm.fields.employeeCode} ${t.employeeForm.validation.required}`;
        }
        if (!formData.laoName.trim()) {
            newErrors.laoName = `${t.employeeForm.fields.laoName} ${t.employeeForm.validation.required}`;
        }
        if (!formData.departmentId) {
            newErrors.departmentId = `${t.employeeForm.fields.department} ${t.employeeForm.validation.required}`;
        }
        if (!formData.baseSalary || parseFloat(formData.baseSalary) <= 0) {
            newErrors.baseSalary = t.employeeForm.validation.salary;
        }
        if (formData.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
            newErrors.email = t.employeeForm.validation.email;
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleChange = (field: string, value: string) => {
        setFormData(prev => ({ ...prev, [field]: value }));
        // Clear error when user types
        if (errors[field]) {
            setErrors(prev => ({ ...prev, [field]: '' }));
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!validate()) return;

        setLoading(true);
        setError(null);

        try {
            const payload: CreateEmployeeRequest = {
                employeeCode: formData.employeeCode,
                laoName: formData.laoName,
                englishName: formData.englishName || undefined,
                email: formData.email || undefined,
                phone: formData.phone || undefined,
                dateOfBirth: formData.dateOfBirth || undefined,
                gender: formData.gender || undefined,
                departmentId: parseInt(formData.departmentId),
                jobTitle: formData.jobTitle || undefined,
                hireDate: formData.hireDate || undefined,
                baseSalary: parseFloat(formData.baseSalary),
                salaryCurrency: formData.salaryCurrency,
                bankName: formData.bankName || undefined,
                bankAccount: formData.bankAccount || undefined,
                dependentCount: parseInt(formData.dependentCount) || 0,
            };

            if (isEditing && employee) {
                await employeesApi.update(employee.employeeId, payload);
            } else {
                await employeesApi.create(payload);
            }

            onSuccess?.();
            router.push('/employees');
        } catch (err) {
            console.error('Failed to save employee:', err);
            setError(t.common.error);
        } finally {
            setLoading(false);
        }
    };

    return (
        <form onSubmit={handleSubmit} className={styles.form}>
            {error && (
                <div className={styles.errorBanner}>
                    {error}
                </div>
            )}

            {/* Basic Information */}
            <Card>
                <div className={styles.section}>
                    <h3 className={styles.sectionTitle}>{t.employeeForm.basicInfo}</h3>
                    <div className={styles.row}>
                        <Input
                            label={`${t.employeeForm.fields.employeeCode} *`}
                            value={formData.employeeCode}
                            onChange={(e) => handleChange('employeeCode', e.target.value)}
                            error={errors.employeeCode}
                            placeholder="e.g. EMP0001"
                            disabled={isEditing}
                        />
                        <Input
                            label={`${t.employeeForm.fields.laoName} *`}
                            value={formData.laoName}
                            onChange={(e) => handleChange('laoName', e.target.value)}
                            error={errors.laoName}
                            placeholder="ຊື່ ນາມສະກຸນ"
                        />
                    </div>
                    <div className={styles.row}>
                        <Input
                            label={t.employeeForm.fields.englishName}
                            value={formData.englishName}
                            onChange={(e) => handleChange('englishName', e.target.value)}
                            placeholder="Full name in English"
                        />
                        <div className={styles.field}>
                            <label className={styles.label}>{t.employeeForm.fields.gender}</label>
                            <select
                                className={styles.select}
                                value={formData.gender}
                                onChange={(e) => handleChange('gender', e.target.value)}
                            >
                                <option value="">{t.employeeForm.fields.selectGender}</option>
                                <option value="Male">{t.employeeForm.fields.male}</option>
                                <option value="Female">{t.employeeForm.fields.female}</option>
                            </select>
                        </div>
                    </div>
                    <div className={styles.row}>
                        <Input
                            label={t.employeeForm.fields.dob}
                            type="date"
                            value={formData.dateOfBirth}
                            onChange={(e) => handleChange('dateOfBirth', e.target.value)}
                        />
                        <Input
                            label={t.employeeForm.fields.dependents}
                            type="number"
                            min="0"
                            value={formData.dependentCount}
                            onChange={(e) => handleChange('dependentCount', e.target.value)}
                        />
                    </div>
                </div>
            </Card>

            {/* Contact Information */}
            <Card>
                <div className={styles.section}>
                    <h3 className={styles.sectionTitle}>{t.employeeForm.contactInfo}</h3>
                    <div className={styles.row}>
                        <Input
                            label={t.employeeForm.fields.email}
                            type="email"
                            value={formData.email}
                            onChange={(e) => handleChange('email', e.target.value)}
                            error={errors.email}
                            placeholder="name@company.com"
                        />
                        <Input
                            label={t.employeeForm.fields.phone}
                            type="tel"
                            value={formData.phone}
                            onChange={(e) => handleChange('phone', e.target.value)}
                            placeholder="+856 20 ..."
                        />
                    </div>
                </div>
            </Card>

            {/* Employment Details */}
            <Card>
                <div className={styles.section}>
                    <h3 className={styles.sectionTitle}>{t.employeeForm.employmentDetails}</h3>
                    <div className={styles.row}>
                        <div className={styles.field}>
                            <label className={styles.label}>{t.employeeForm.fields.department} *</label>
                            <select
                                className={`${styles.select} ${errors.departmentId ? styles.selectError : ''}`}
                                value={formData.departmentId}
                                onChange={(e) => handleChange('departmentId', e.target.value)}
                            >
                                <option value="">{t.employeeForm.fields.selectDepartment}</option>
                                {departments.map((dept) => (
                                    <option key={dept.departmentId} value={dept.departmentId}>
                                        {dept.departmentName}
                                    </option>
                                ))}
                            </select>
                            {errors.departmentId && (
                                <span className={styles.fieldError}>{errors.departmentId}</span>
                            )}
                        </div>
                        <Input
                            label={t.employeeForm.fields.jobTitle}
                            value={formData.jobTitle}
                            onChange={(e) => handleChange('jobTitle', e.target.value)}
                            placeholder="e.g. Senior Developer"
                        />
                    </div>
                    <div className={styles.row}>
                        <Input
                            label={t.employeeForm.fields.hireDate}
                            type="date"
                            value={formData.hireDate}
                            onChange={(e) => handleChange('hireDate', e.target.value)}
                        />
                        <div className={styles.salaryGroup}>
                            <div className={styles.currencyField}>
                                <label className={styles.label}>Currency</label>
                                <select
                                    className={styles.currencySelect}
                                    value={formData.salaryCurrency}
                                    onChange={(e) => handleChange('salaryCurrency', e.target.value)}
                                >
                                    <option value="LAK">LAK (ກີບ)</option>
                                    <option value="USD">USD ($)</option>
                                    <option value="THB">THB (฿)</option>
                                    <option value="CNY">CNY (¥)</option>
                                </select>
                            </div>
                            <Input
                                label={`${t.employeeForm.fields.baseSalary} *`}
                                type="number"
                                min="0"
                                value={formData.baseSalary}
                                onChange={(e) => handleChange('baseSalary', e.target.value)}
                                error={errors.baseSalary}
                                placeholder={formData.salaryCurrency === 'LAK' ? 'e.g. 10,000,000' : 'e.g. 500'}
                            />
                        </div>
                    </div>
                    {/* <div className={styles.row}>
                        <div className={styles.salaryGroup}>
                            <div className={styles.currencyField}>
                                <label className={styles.label}>Currency</label>
                                <select
                                    className={styles.currencySelect}
                                    value={formData.salaryCurrency}
                                    onChange={(e) => handleChange('salaryCurrency', e.target.value)}
                                >
                                    <option value="LAK">LAK (ກີບ)</option>
                                    <option value="USD">USD ($)</option>
                                    <option value="THB">THB (฿)</option>
                                    <option value="CNY">CNY (¥)</option>
                                </select>
                            </div>
                            <Input
                                label={`${t.employeeForm.fields.baseSalary} *`}
                                type="number"
                                min="0"
                                value={formData.baseSalary}
                                onChange={(e) => handleChange('baseSalary', e.target.value)}
                                error={errors.baseSalary}
                                placeholder={formData.salaryCurrency === 'LAK' ? 'e.g. 10,000,000' : 'e.g. 500'}
                            />
                        </div>
                    </div> */}
                </div>
            </Card>

            {/* Bank Information */}
            <Card>
                <div className={styles.section}>
                    <h3 className={styles.sectionTitle}>{t.employeeForm.bankInfo}</h3>
                    <div className={styles.row}>
                        <Input
                            label={t.employeeForm.fields.bankName}
                            value={formData.bankName}
                            onChange={(e) => handleChange('bankName', e.target.value)}
                            placeholder="e.g. BCEL, LDB"
                        />
                        <Input
                            label={t.employeeForm.fields.bankAccount}
                            value={formData.bankAccount}
                            onChange={(e) => handleChange('bankAccount', e.target.value)}
                            placeholder="Account number"
                        />
                    </div>
                </div>
            </Card>

            {/* Actions */}
            <div className={styles.actions}>
                <Button
                    type="button"
                    variant="secondary"
                    onClick={() => router.back()}
                >
                    {t.employeeForm.buttons.cancel}
                </Button>
                <Button type="submit" loading={loading}>
                    {isEditing ? t.employeeForm.buttons.update : t.employeeForm.buttons.create}
                </Button>
            </div>
        </form>
    );
}

export default EmployeeForm;
