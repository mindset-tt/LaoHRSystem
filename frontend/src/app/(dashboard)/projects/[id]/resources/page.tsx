'use client';

import Link from 'next/link';
import { useEffect, useMemo, useState } from 'react';
import { useParams } from 'next/navigation';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { PageHeader } from '@/components/ui/PageHeader';
import { DataTable, type DataTableColumn } from '@/components/ui/DataTable';
import { Pagination } from '@/components/ui/Pagination';
import { Select } from '@/components/ui/Select';
import { useToast } from '@/components/ui/Toast';
import { resourcesApi } from '@/lib/endpoints';
import type { ResourceListItem } from '@/lib/endpoints/projects';
import type { PaginatedResponse } from '@/lib/types/pagination';
import styles from '../page.module.css';

const ROLE_OPTIONS = ['', 'LEAD', 'CONTRIBUTOR', 'REVIEWER', 'ADVISOR'];

export default function ProjectResourcesPage() {
    const params = useParams<{ id: string }>();
    const projectId = Number(params.id);
    const { t } = useLanguage();
    const toast = useToast();

    const [pageData, setPageData] = useState<PaginatedResponse<ResourceListItem>>({
        items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false,
    });
    const [loading, setLoading] = useState(true);
    const [role, setRole] = useState('');
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(25);

    useEffect(() => {
        if (!projectId) return;
        let cancelled = false;
        setLoading(true);
        resourcesApi
            .getAll(projectId, { role: role || undefined, page, pageSize })
            .then((p) => { if (!cancelled) setPageData(p); })
            .catch((err) => {
                console.error(err);
                toast.error(t.common.error);
            })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [projectId, role, page, pageSize, t.common.error, toast]);

    const roleLabel = (r: string) =>
        (t.resources.role as Record<string, string>)[r.toLowerCase()] ?? r;

    const columns: DataTableColumn<ResourceListItem>[] = useMemo(() => [
        {
            key: 'employee',
            header: t.resources.fields.employee,
            render: r => <strong>{r.employeeName}</strong>,
        },
        {
            key: 'role',
            header: t.resources.fields.role,
            render: r => <span className={styles.resourceRole}>{roleLabel(r.role)}</span>,
        },
        {
            key: 'allocation',
            header: t.resources.fields.allocation,
            render: r => (
                <span className={styles.allocBar}>
                    <span className={styles.allocTrack}>
                        <span className={styles.allocFill} style={{ width: `${r.allocationPercent}%` }} />
                    </span>
                    <span className={styles.allocLabel}>{r.allocationPercent}%</span>
                </span>
            ),
        },
        {
            key: 'startDate',
            header: t.resources.fields.startDate,
            render: r => r.startDate ? new Date(r.startDate).toLocaleDateString() : '—',
        },
        {
            key: 'endDate',
            header: t.resources.fields.endDate,
            render: r => r.endDate ? new Date(r.endDate).toLocaleDateString() : '—',
        },
    ], [roleLabel, t]);

    return (
        <div className={styles.page}>
            <PageHeader
                title={t.resources.title}
                subtitle={t.resources.subtitle}
                breadcrumbs={[
                    { label: t.nav.dashboard, href: '/' },
                    { label: t.projects.title, href: '/projects' },
                    { label: `Project #${projectId}`, href: `/projects/${projectId}` },
                    { label: t.resources.title },
                ]}
                actions={
                    <Link href={`/projects/${projectId}`} className={styles.linkButton}>
                        ← {t.projects.detail.overview}
                    </Link>
                }
            />

            <Card className={styles.filtersCard}>
                <div className={styles.filters}>
                    <Select
                        value={role}
                        onChange={(e) => setRole(e.target.value)}
                        options={ROLE_OPTIONS.map(r => ({ value: r, label: r === '' ? t.resources.fields.role : roleLabel(r) }))}
                    />
                </div>
            </Card>

            <Card noPadding>
                <DataTable<ResourceListItem>
                    columns={columns}
                    rows={pageData.items}
                    rowKey={r => r.resourceId}
                    isLoading={loading}
                    emptyTitle={t.resources.empty.title}
                    emptyDescription={t.resources.empty.description}
                />
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
        </div>
    );
}