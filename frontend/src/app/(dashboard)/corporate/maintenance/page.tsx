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
import { corporateApi, type WorkOrderDto } from '@/lib/endpoints/corporate';
import type { PaginatedResponse } from '@/lib/types/pagination';

const SOURCE_TYPES = ['FACILITY', 'ROOM', 'ASSET', 'VEHICLE', 'SERVICE_REQUEST'];
const PRIORITIES = ['LOW', 'MEDIUM', 'HIGH', 'URGENT'];

export default function CorporateMaintenancePage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [pageData, setPageData] = useState<PaginatedResponse<WorkOrderDto>>({
        items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false,
    });
    const [loading, setLoading] = useState(true);
    const [page, setPage] = useState(1);
    const [showCreate, setShowCreate] = useState(false);
    const [form, setForm] = useState({ sourceType: 'FACILITY', sourceId: 0, title: '', priority: 'MEDIUM' });
    const [saving, setSaving] = useState(false);

    const load = () => {
        corporateApi.listWorkOrders({ page, pageSize: 25 })
            .then((p) => setPageData(p))
            .catch((err) => { console.error(err); toast.error(t.common.error); })
            .finally(() => setLoading(false));
    };

    useEffect(() => { load(); /* eslint-disable-line react-hooks/exhaustive-deps */ }, [page]);

    const create = async () => {
        if (!form.title.trim()) { toast.error('Title is required'); return; }
        setSaving(true);
        try {
            await corporateApi.createWorkOrder({
                sourceType: form.sourceType,
                sourceId: form.sourceId,
                title: form.title,
                priority: form.priority,
            });
            toast.success('Work order created');
            setShowCreate(false);
            setForm({ sourceType: 'FACILITY', sourceId: 0, title: '', priority: 'MEDIUM' });
            load();
        } catch (err) { console.error(err); toast.error(t.common.error); }
        finally { setSaving(false); }
    };

    const columns: DataTableColumn<WorkOrderDto>[] = [
        { key: 'workOrderNumber', header: 'Number', render: (r) => r.workOrderNumber },
        { key: 'title', header: 'Title', render: (r) => r.title },
        { key: 'sourceType', header: 'Source', render: (r) => r.sourceType },
        { key: 'priority', header: 'Priority', render: (r) => r.priority },
        { key: 'status', header: 'Status', render: (r) => r.status },
    ];

    return (
        <div>
            <PageHeader
                title={t.sidebar.maintenance}
                subtitle="Maintenance work orders"
                actions={<Button onClick={() => setShowCreate(true)}>New Work Order</Button>}
            />
            <Card>
                <DataTable columns={columns} rows={pageData.items} rowKey={(r) => r.workOrderId} isLoading={loading} />
                <Pagination page={page} pageSize={25} totalItems={pageData.totalItems} totalPages={pageData.totalPages} hasNext={pageData.hasNext} hasPrevious={pageData.hasPrevious} onPageChange={setPage} />
            </Card>

            <Modal isOpen={showCreate} onClose={() => setShowCreate(false)} title="New Work Order">
                <div style={{ display: 'grid', gap: '0.75rem' }}>
                    <Select label="Source Type" options={SOURCE_TYPES.map((s) => ({ value: s, label: s }))} value={form.sourceType} onChange={(e) => setForm({ ...form, sourceType: e.target.value })} />
                    <Input label="Source ID" type="number" value={form.sourceId} onChange={(e) => setForm({ ...form, sourceId: Number(e.target.value) })} />
                    <Input label="Title" value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} />
                    <Select label="Priority" options={PRIORITIES.map((p) => ({ value: p, label: p }))} value={form.priority} onChange={(e) => setForm({ ...form, priority: e.target.value })} />
                    <Button onClick={create} disabled={saving}>{saving ? 'Saving…' : 'Create'}</Button>
                </div>
            </Modal>
        </div>
    );
}
