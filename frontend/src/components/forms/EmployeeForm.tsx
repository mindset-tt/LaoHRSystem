'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
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
            newErrors.employeeCode = 'Employee code is required';
        }
        if (!formData.laoName.trim()) {
            newErrors.laoName = 'Lao name is required';
        }
        if (!formData.departmentId) {
            newErrors.departmentId = 'Department is required';
        }
        if (!formData.baseSalary || parseFloat(formData.baseSalary) <= 0) {
            newErrors.baseSalary = 'Base salary must be greater than 0';
        }
        if (formData.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
            newErrors.email = 'Invalid email format';
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
                salaryCurrency: 'LAK',
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
            setError('Failed to save employee. Please try again.');
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
                    <h3 className={styles.sectionTitle}>Basic Information</h3>
                    <div className={styles.row}>
                        <Input
                            label="Employee Code *"
                            value={formData.employeeCode}
                            onChange={(e) => handleChange('employeeCode', e.target.value)}
                            error={errors.employeeCode}
                            placeholder="e.g. EMP0001"
                            disabled={isEditing}
                        />
                        <Input
                            label="Lao Name *"
                            value={formData.laoName}
                            onChange={(e) => handleChange('laoName', e.target.value)}
                            error={errors.laoName}
                            placeholder="ຊື່ ນາມສະກຸນ"
                        />
                    </div>
                    <div className={styles.row}>
                        <Input
                            label="English Name"
                            value={formData.englishName}
                            onChange={(e) => handleChange('englishName', e.target.value)}
                            placeholder="Full name in English"
                        />
                        <div className={styles.field}>
                            <label className={styles.label}>Gender</label>
                            <select
                                className={styles.select}
                                value={formData.gender}
                                onChange={(e) => handleChange('gender', e.target.value)}
                            >
                                <option value="">Select gender</option>
                                <option value="Male">Male</option>
                                <option value="Female">Female</option>
                            </select>
                        </div>
                    </div>
                    <div className={styles.row}>
                        <Input
                            label="Date of Birth"
                            type="date"
                            value={formData.dateOfBirth}
                            onChange={(e) => handleChange('dateOfBirth', e.target.value)}
                        />
                        <Input
                            label="Dependents"
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
                    <h3 className={styles.sectionTitle}>Contact Information</h3>
                    <div className={styles.row}>
                        <Input
                            label="Email"
                            type="email"
                            value={formData.email}
                            onChange={(e) => handleChange('email', e.target.value)}
                            error={errors.email}
                            placeholder="name@company.com"
                        />
                        <Input
                            label="Phone"
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
                    <h3 className={styles.sectionTitle}>Employment Details</h3>
                    <div className={styles.row}>
                        <div className={styles.field}>
                            <label className={styles.label}>Department *</label>
                            <select
                                className={`${styles.select} ${errors.departmentId ? styles.selectError : ''}`}
                                value={formData.departmentId}
                                onChange={(e) => handleChange('departmentId', e.target.value)}
                            >
                                <option value="">Select department</option>
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
                            label="Job Title"
                            value={formData.jobTitle}
                            onChange={(e) => handleChange('jobTitle', e.target.value)}
                            placeholder="e.g. Senior Developer"
                        />
                    </div>
                    <div className={styles.row}>
                        <Input
                            label="Hire Date"
                            type="date"
                            value={formData.hireDate}
                            onChange={(e) => handleChange('hireDate', e.target.value)}
                        />
                        <Input
                            label="Base Salary (LAK) *"
                            type="number"
                            min="0"
                            value={formData.baseSalary}
                            onChange={(e) => handleChange('baseSalary', e.target.value)}
                            error={errors.baseSalary}
                            placeholder="e.g. 10000000"
                        />
                    </div>
                </div>
            </Card>

            {/* Bank Information */}
            <Card>
                <div className={styles.section}>
                    <h3 className={styles.sectionTitle}>Bank Information</h3>
                    <div className={styles.row}>
                        <Input
                            label="Bank Name"
                            value={formData.bankName}
                            onChange={(e) => handleChange('bankName', e.target.value)}
                            placeholder="e.g. BCEL, LDB"
                        />
                        <Input
                            label="Bank Account Number"
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
                    Cancel
                </Button>
                <Button type="submit" loading={loading}>
                    {isEditing ? 'Update Employee' : 'Create Employee'}
                </Button>
            </div>
        </form>
    );
}

export default EmployeeForm;
