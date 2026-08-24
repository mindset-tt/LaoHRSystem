'use client';

import { useEffect, useState } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { PageHeader } from '@/components/ui/PageHeader';
import { DataTable, type DataTableColumn } from '@/components/ui/DataTable';
import { Pagination } from '@/components/ui/Pagination';
import { Button } from '@/components/ui/Button';
import { Modal } from '@/components/ui/Modal';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { useToast } from '@/components/ui/Toast';
import { backOfficeApi, type ServiceRequestListItem, type ServiceRequestCategoryDto } from '@/lib/endpoints/backOffice';
import type { PaginatedResponse } from '@/lib/types/pagination';

const PRIORITIES = ['LOW', 'MEDIUM', 'HIGH', 'URGENT'];

export default function CorporateServiceDeskPage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [pageData, setPageData] = useState<PaginatedResponse<ServiceRequestListItem>>({
        items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false,
    });
    const [categories, setCategories] = useState<ServiceRequestCategoryDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [page, setPage] = useState(1);
    const [showCreate, setShowCreate] = useState(false);
    const [form, setForm] = useState({ categoryId: 0, subject: '', description: '', priority: 'MEDIUM' });
    const [saving, setSaving] = useState(false);

    const load = () => {
        backOfficeApi.listServiceRequests({ page, pageSize: 25 })
            .then((p) => setPageData(p))
            .catch((err) => { console.error(err); toast.error(t.common.error); })
            .finally(() => setLoading(false));
    };

    useEffect(() => {
        load();
        backOfficeApi.listServiceRequestCategories().then(setCategories).catch(() => {});
        /* eslint-disable-next-line react-hooks/exhaustive-deps */
    }, [page]);

    const create = async () => {
        if (!form.subject.trim()) { toast.error('Subject is required'); return; }
        setSaving(true);
        try {
            await backOfficeApi.createServiceRequest({
                categoryId: form.categoryId,
                subject: form.subject,
                description: form.description || null,
                priority: form.priority,
            });
            toast.success('Request created');
            setShowCreate(false);
            setForm({ categoryId: 0, subject: '', description: '', priority: 'MEDIUM' });
            load();
        } catch (err) { console.error(err); toast.error(t.common.error); }
        finally { setSaving(false); }
    };

    const columns: DataTableColumn<ServiceRequestListItem>[] = [
        { key: 'requestNumber', header: 'Number', render: (r) => r.requestNumber },
        { key: 'subject', header: 'Subject', render: (r) => r.subject },
        { key: 'categoryName', header: 'Category', render: (r) => r.categoryName ?? '—' },
        { key: 'priority', header: 'Priority', render: (r) => r.priority },
        { key: 'status', header: 'Status', render: (r) => r.status },
    ];

    return (
        <div>
            <PageHeader
                title={t.sidebar.serviceRequests}
                subtitle="Internal service desk"
                actions={<Button onClick={() => setShowCreate(true)}>New Request</Button>}
            />
            <Card>
                <DataTable columns={columns} rows={pageData.items} rowKey={(r) => r.serviceRequestId} isLoading={loading} />
                <Pagination page={page} pageSize={25} totalItems={pageData.totalItems} totalPages={pageData.totalPages} hasNext={pageData.hasNext} hasPrevious={pageData.hasPrevious} onPageChange={setPage} />
            </Card>

            <Modal isOpen={showCreate} onClose={() => setShowCreate(false)} title="New Service Request">
                <div style={{ display: 'grid', gap: '0.75rem' }}>
                    <Select label="Category" options={categories.map((c) => ({ value: c.serviceRequestCategoryId, label: c.name }))} value={form.categoryId} onChange={(e) => setForm({ ...form, categoryId: Number(e.target.value) })} />
                    <Input label="Subject" value={form.subject} onChange={(e) => setForm({ ...form, subject: e.target.value })} />
                    <Input label="Description" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
                    <Select label="Priority" options={PRIORITIES.map((p) => ({ value: p, label: p }))} value={form.priority} onChange={(e) => setForm({ ...form, priority: e.target.value })} />
                    <Button onClick={create} disabled={saving}>{saving ? 'Saving…' : 'Create'}</Button>
                </div>
            </Modal>
        </div>
    );
}
