'use client';

import { useEffect, useState, useMemo } from 'react';
import Link from 'next/link';
import { useAuth } from '@/components/providers/AuthProvider';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { SkeletonTable } from '@/components/ui/Skeleton';
import { employeesApi, departmentsApi } from '@/lib/endpoints';
import type { Employee, Department } from '@/lib/types';
import { isHROrAdmin } from '@/lib/permissions';
import styles from './page.module.css';

/**
 * Employee List Page
 * Displays all employees with search, filter, and pagination
 */
export default function EmployeesPage() {
    const { role } = useAuth();
    const { t, language } = useLanguage();
    const [employees, setEmployees] = useState<Employee[]>([]);
    const [departments, setDepartments] = useState<Department[]>([]);
    const [loading, setLoading] = useState(true);
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    const [error, setError] = useState<string | null>(null);

    // Filters
    const [search, setSearch] = useState('');
    const [departmentFilter, setDepartmentFilter] = useState<string>('');
    const [statusFilter, setStatusFilter] = useState<string>('active');

    const canAddEmployee = isHROrAdmin(role);

    useEffect(() => {
        const loadData = async () => {
            try {
                setError(null);
                const [employeesData, departmentsData] = await Promise.all([
                    employeesApi.getAll({ isActive: statusFilter === 'active' ? true : statusFilter === 'inactive' ? false : undefined }),
                    departmentsApi.getAll(),
                ]);
                setEmployees(employeesData);
                setDepartments(departmentsData);
            } catch (err) {
                console.error('Failed to load employees:', err);
                setError('Failed to load employees. Please try again.');
                // Fallback to mock data for demo
                setDepartments([
                    { departmentId: 1, departmentName: 'Engineering', departmentCode: 'ENG', isActive: true, createdAt: '' },
                    { departmentId: 2, departmentName: 'Human Resources', departmentCode: 'HR', isActive: true, createdAt: '' },
                    { departmentId: 3, departmentName: 'Finance', departmentCode: 'FIN', isActive: true, createdAt: '' },
                ]);
                setEmployees([
                    { employeeId: 1, employeeCode: 'EMP0001', laoName: 'ສົມສັກ ສີສະຫວັດ', englishName: 'Somsak Sisavad', departmentId: 1, department: { departmentId: 1, departmentName: 'Engineering', isActive: true, createdAt: '' }, jobTitle: 'Senior Developer', baseSalary: 15000000, salaryCurrency: 'LAK', isActive: true, dependentCount: 2, createdAt: '' },
                    { employeeId: 2, employeeCode: 'EMP0002', laoName: 'ນາງ ສີສົມມະໄລ', englishName: 'Sisommalai Wong', departmentId: 2, department: { departmentId: 2, departmentName: 'Human Resources', isActive: true, createdAt: '' }, jobTitle: 'HR Manager', baseSalary: 12000000, salaryCurrency: 'LAK', isActive: true, dependentCount: 1, createdAt: '' },
                ]);
            } finally {
                setLoading(false);
            }
        };

        loadData();
    }, [statusFilter]);

    // Filtered employees
    const filteredEmployees = useMemo(() => {
        return employees.filter((emp) => {
            // Search filter
            if (search) {
                const searchLower = search.toLowerCase();
                const matchesSearch =
                    emp.laoName.toLowerCase().includes(searchLower) ||
                    emp.englishName?.toLowerCase().includes(searchLower) ||
                    emp.employeeCode.toLowerCase().includes(searchLower);
                if (!matchesSearch) return false;
            }

            // Department filter
            if (departmentFilter && emp.departmentId !== parseInt(departmentFilter)) {
                return false;
            }

            // Status filter
            if (statusFilter === 'active' && !emp.isActive) return false;
            if (statusFilter === 'inactive' && emp.isActive) return false;

            return true;
        });
    }, [employees, search, departmentFilter, statusFilter]);

    return (
        <div className={styles.page}>
            {/* Header */}
            <div className={styles.header}>
                <div>
                    <h1 className={styles.title}>{t.employees.title}</h1>
                    <p className={styles.subtitle}>
                        {t.employees.subtitle}
                    </p>
                </div>
                {canAddEmployee && (
                    <Link href="/employees/new">
                        <Button leftIcon={<PlusIcon />}>{t.employees.addEmployee}</Button>
                    </Link>
                )}
            </div>

            {/* Filters */}
            <Card className={styles.filtersCard}>
                <div className={styles.filters}>
                    <div className={styles.searchWrapper}>
                        <Input
                            placeholder={t.employees.searchPlaceholder}
                            value={search}
                            onChange={(e) => setSearch(e.target.value)}
                            leftIcon={<SearchIcon />}
                        />
                    </div>
                    <select
                        className={styles.select}
                        value={departmentFilter}
                        onChange={(e) => setDepartmentFilter(e.target.value)}
                    >
                        <option value="">{t.employees.filterDepartment}</option>
                        {departments.map((dept) => (
                            <option key={dept.departmentId} value={dept.departmentId}>
                                {dept.departmentName}
                            </option>
                        ))}
                    </select>
                    <select
                        className={styles.select}
                        value={statusFilter}
                        onChange={(e) => setStatusFilter(e.target.value)}
                    >
                        <option value="all">{t.employees.filterStatus}</option>
                        <option value="active">{t.employees.statusActive}</option>
                        <option value="inactive">{t.employees.statusInactive}</option>
                    </select>
                </div>
            </Card>

            {/* Employee Table */}
            <Card noPadding>
                {loading ? (
                    <div className={styles.tableWrapper}>
                        <SkeletonTable rows={5} columns={5} />
                    </div>
                ) : filteredEmployees.length === 0 ? (
                    <div className={styles.emptyState}>
                        <div className={styles.emptyIcon}>
                            <UsersIcon />
                        </div>
                        <h3 className={styles.emptyTitle}>{t.employees.empty.title}</h3>
                        <p className={styles.emptyDescription}>
                            {search || departmentFilter
                                ? t.employees.empty.description
                                : t.employees.empty.descriptionInit}
                        </p>
                    </div>
                ) : (
                    <div className={styles.tableWrapper}>
                        <table className={styles.table}>
                            <thead>
                                <tr>
                                    <th>{t.employees.table.employee}</th>
                                    <th>{t.employees.table.department}</th>
                                    <th>{t.employees.table.jobTitle}</th>
                                    <th>{t.employees.table.status}</th>
                                    <th></th>
                                </tr>
                            </thead>
                            <tbody>
                                {filteredEmployees.map((employee) => (
                                    <tr key={employee.employeeId}>
                                        <td>
                                            <div className={styles.employeeCell}>
                                                <div className={styles.avatar}>
                                                    {getInitials(employee.englishName || employee.laoName)}
                                                </div>
                                                <div className={styles.employeeInfo}>
                                                    <span className={styles.employeeName}>
                                                        {language === 'lo' ? employee.laoName : (employee.englishName || employee.laoName)}
                                                    </span>
                                                    <span className={styles.employeeCode}>
                                                        {employee.employeeCode}
                                                    </span>
                                                </div>
                                            </div>
                                        </td>
                                        <td>
                                            <span className={styles.department}>
                                                {employee.department?.departmentName || '-'}
                                            </span>
                                        </td>
                                        <td>
                                            <span className={styles.jobTitle}>
                                                {employee.jobTitle || '-'}
                                            </span>
                                        </td>
                                        <td>
                                            <span
                                                className={`${styles.status} ${employee.isActive ? styles.active : styles.inactive
                                                    }`}
                                            >
                                                {employee.isActive ? t.employees.statusActive : t.employees.statusInactive}
                                            </span>
                                        </td>
                                        <td>
                                            <Link href={`/employees/${employee.employeeId}`}>
                                                <button className={styles.viewButton}>
                                                    {t.employees.table.view}
                                                </button>
                                            </Link>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                )}
            </Card>

            {/* Results count */}
            {!loading && (
                <p className={styles.resultCount}>
                    {t.employees.results
                        .replace('{count}', filteredEmployees.length.toString())
                        .replace('{total}', employees.length.toString())}
                </p>
            )}
        </div>
    );
}

function getInitials(name: string): string {
    return name
        .split(' ')
        .map((n) => n[0])
        .slice(0, 2)
        .join('')
        .toUpperCase();
}

// Icons
function PlusIcon() {
    return (
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <line x1="12" y1="5" x2="12" y2="19" />
            <line x1="5" y1="12" x2="19" y2="12" />
        </svg>
    );
}

function SearchIcon() {
    return (
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <circle cx="11" cy="11" r="8" />
            <line x1="21" y1="21" x2="16.65" y2="16.65" />
        </svg>
    );
}

function UsersIcon() {
    return (
        <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5">
            <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
            <circle cx="9" cy="7" r="4" />
            <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
            <path d="M16 3.13a4 4 0 0 1 0 7.75" />
        </svg>
    );
}
