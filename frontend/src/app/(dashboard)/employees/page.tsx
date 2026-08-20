'use client';

import { useEffect, useMemo, useState } from 'react';
import Link from 'next/link';
import { useAuth } from '@/components/providers/AuthProvider';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { SkeletonTable } from '@/components/ui/Skeleton';
import { PageHeader } from '@/components/ui/PageHeader';
import { DataTable, type DataTableColumn } from '@/components/ui/DataTable';
import { Pagination } from '@/components/ui/Pagination';
import { ErrorState } from '@/components/ui/EmptyState';
import { useToast } from '@/components/ui/Toast';
import { employeesApi, departmentsApi } from '@/lib/endpoints';
import type { Employee, Department } from '@/lib/types';
import type { PaginatedResponse } from '@/lib/types/pagination';
import { isHROrAdmin } from '@/lib/permissions';
import styles from './page.module.css';

const DEFAULT_PAGE_SIZE = 25;

/**
 * Employee List Page
 * Displays all employees with search, filter, and server-side pagination.
 */
export default function EmployeesPage() {
    const { role } = useAuth();
    const { t, language } = useLanguage();
    const toast = useToast();

    const [pageData, setPageData] = useState<PaginatedResponse<Employee>>({
        items: [], page: 1, pageSize: DEFAULT_PAGE_SIZE, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false,
    });
    const [departments, setDepartments] = useState<Department[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    // Filters
    const [search, setSearch] = useState('');
    const [departmentFilter, setDepartmentFilter] = useState<string>('');
    const [statusFilter, setStatusFilter] = useState<string>('active');
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);

    const canAddEmployee = isHROrAdmin(role);

    useEffect(() => {
        const loadData = async () => {
            try {
                setLoading(true);
                setError(null);
                const [pageResult, departmentsData] = await Promise.all([
                    employeesApi.getAll({
                        isActive: statusFilter === 'active' ? true : statusFilter === 'inactive' ? false : undefined,
                        page,
                        pageSize,
                    }),
                    departmentsApi.getAll(),
                ]);
                setPageData(pageResult);
                setDepartments(departmentsData);
            } catch (err) {
                console.error('Failed to load employees:', err);
                const msg = 'Failed to load employees. Please try again.';
                setError(msg);
                toast.error(msg);
            } finally {
                setLoading(false);
            }
        };

        loadData();
    }, [statusFilter, page, pageSize, toast]);

    // Client-side refinement over the current page (search + department).
    const employees = pageData.items;
    const filteredEmployees = useMemo(() => {
        return employees.filter((emp) => {
            if (search) {
                const s = search.toLowerCase();
                const matches =
                    emp.laoName.toLowerCase().includes(s) ||
                    emp.englishName?.toLowerCase().includes(s) ||
                    emp.employeeCode.toLowerCase().includes(s);
                if (!matches) return false;
            }
            if (departmentFilter && emp.departmentId !== Number(departmentFilter)) return false;
            if (statusFilter === 'active' && !emp.isActive) return false;
            if (statusFilter === 'inactive' && emp.isActive) return false;
            return true;
        });
    }, [employees, search, departmentFilter, statusFilter]);

    const columns: DataTableColumn<Employee>[] = [
        {
            key: 'employee',
            header: t.employees.table.employee,
            sortBy: e => e.englishName || e.laoName,
            render: (employee) => (
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
            ),
        },
        {
            key: 'department',
            header: t.employees.table.department,
            render: (employee) => (
                <span className={styles.department}>
                    {employee.department?.departmentName || '-'}
                </span>
            ),
        },
        {
            key: 'jobTitle',
            header: t.employees.table.jobTitle,
            render: (employee) => (
                <span className={styles.jobTitle}>{employee.jobTitle || '-'}</span>
            ),
        },
        {
            key: 'status',
            header: t.employees.table.status,
            render: (employee) => (
                <span className={`${styles.status} ${employee.isActive ? styles.active : styles.inactive}`}>
                    {employee.isActive ? t.employees.statusActive : t.employees.statusInactive}
                </span>
            ),
        },
        {
            key: 'actions',
            header: '',
            align: 'right',
            render: (employee) => (
                <Link href={`/employees/${employee.employeeId}`}>
                    <button className={styles.viewButton}>{t.employees.table.view}</button>
                </Link>
            ),
        },
    ];

    return (
        <div className={styles.page}>
            <PageHeader
                title={t.employees.title}
                subtitle={t.employees.subtitle}
                breadcrumbs={[{ label: t.nav.dashboard, href: '/' }, { label: t.employees.title }]}
                actions={canAddEmployee ? (
                    <Link href="/employees/new">
                        <Button leftIcon={<PlusIcon />}>{t.employees.addEmployee}</Button>
                    </Link>
                ) : undefined}
            />

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
                    <Select
                        value={departmentFilter}
                        onChange={(e) => setDepartmentFilter(e.target.value)}
                        options={[
                            { value: '', label: t.employees.filterDepartment },
                            ...departments.map(dept => ({ value: dept.departmentId.toString(), label: dept.departmentName })),
                        ]}
                    />
                    <Select
                        value={statusFilter}
                        onChange={(e) => setStatusFilter(e.target.value)}
                        options={[
                            { value: 'all', label: t.employees.filterStatus },
                            { value: 'active', label: t.employees.statusActive },
                            { value: 'inactive', label: t.employees.statusInactive },
                        ]}
                    />
                </div>
            </Card>

            {error && !loading ? (
                <Card>
                    <ErrorState title="Failed to load" description={error} />
                </Card>
            ) : (
                <Card noPadding>
                    {loading ? (
                        <div className={styles.tableWrapper}>
                            <SkeletonTable rows={5} columns={5} />
                        </div>
                    ) : (
                        <DataTable<Employee>
                            columns={columns}
                            rows={filteredEmployees}
                            rowKey={e => e.employeeId}
                            emptyTitle={t.employees.empty.title}
                            emptyDescription={
                                search || departmentFilter
                                    ? t.employees.empty.description
                                    : t.employees.empty.descriptionInit
                            }
                        />
                    )}
                    {!loading && (
                        <Pagination
                            page={pageData.page}
                            pageSize={pageData.pageSize}
                            totalItems={pageData.totalItems}
                            totalPages={pageData.totalPages}
                            hasNext={pageData.hasNext}
                            hasPrevious={pageData.hasPrevious}
                            onPageChange={setPage}
                            onPageSizeChange={setPageSize}
                        />
                    )}
                </Card>
            )}
        </div>
    );
}

function getInitials(name: string): string {
    return name.split(' ').map((n) => n[0]).slice(0, 2).join('').toUpperCase();
}

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