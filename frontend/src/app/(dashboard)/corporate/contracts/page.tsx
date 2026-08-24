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
import { useToast } from '@/components/ui/Toast';
import { backOfficeApi, type ContractDto } from '@/lib/endpoints/backOffice';
import type { PaginatedResponse } from '@/lib/types/pagination';

export default function CorporateContractsPage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [pageData, setPageData] = useState<PaginatedResponse<ContractDto>>({
        items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false,
    });
    const [loading, setLoading] = useState(true);
    const [page, setPage] = useState(1);
    const [showCreate, setShowCreate] = useState(false);
    const [form, setForm] = useState({ title: '', ownerEmployeeId: 0, startDate: '', endDate: '', amount: '' });
    const [saving, setSaving] = useState(false);

    const load = () => {
        backOfficeApi.listContracts({ page, pageSize: 25 })
            .then((p) => setPageData(p))
            .catch((err) => { console.error(err); toast.error(t.common.error); })
            .finally(() => setLoading(false));
    };

    useEffect(() => { load(); /* eslint-disable-line react-hooks/exhaustive-deps */ }, [page]);

    const create = async () => {
        if (!form.title.trim()) { toast.error('Title is required'); return; }
        setSaving(true);
        try {
            await backOfficeApi.createContract({
                title: form.title,
                ownerEmployeeId: form.ownerEmployeeId,
                startDate: form.startDate,
                endDate: form.endDate || null,
                amount: form.amount ? Number(form.amount) : null,
            });
            toast.success('Contract created');
            setShowCreate(false);
            setForm({ title: '', ownerEmployeeId: 0, startDate: '', endDate: '', amount: '' });
            load();
        } catch (err) { console.error(err); toast.error(t.common.error); }
        finally { setSaving(false); }
    };

    const columns: DataTableColumn<ContractDto>[] = [
        { key: 'contractNumber', header: 'Number', render: (r) => r.contractNumber },
        { key: 'title', header: 'Title', render: (r) => r.title },
        { key: 'supplierName', header: 'Supplier', render: (r) => r.supplierName ?? '—' },
        { key: 'endDate', header: 'End Date', render: (r) => (r.endDate ? r.endDate.slice(0, 10) : '—') },
        { key: 'status', header: 'Status', render: (r) => r.status },
    ];

    return (
        <div>
            <PageHeader
                title={t.sidebar.contracts}
                subtitle="Contract lifecycle"
                actions={<Button onClick={() => setShowCreate(true)}>New Contract</Button>}
            />
            <Card>
                <DataTable columns={columns} rows={pageData.items} rowKey={(r) => r.contractId} isLoading={loading} />
                <Pagination page={page} pageSize={25} totalItems={pageData.totalItems} totalPages={pageData.totalPages} hasNext={pageData.hasNext} hasPrevious={pageData.hasPrevious} onPageChange={setPage} />
            </Card>

            <Modal isOpen={showCreate} onClose={() => setShowCreate(false)} title="New Contract">
                <div style={{ display: 'grid', gap: '0.75rem' }}>
                    <Input label="Title" value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} />
                    <Input label="Owner Employee ID" type="number" value={form.ownerEmployeeId} onChange={(e) => setForm({ ...form, ownerEmployeeId: Number(e.target.value) })} />
                    <Input label="Start Date" type="date" value={form.startDate} onChange={(e) => setForm({ ...form, startDate: e.target.value })} />
                    <Input label="End Date" type="date" value={form.endDate} onChange={(e) => setForm({ ...form, endDate: e.target.value })} />
                    <Input label="Amount" type="number" value={form.amount} onChange={(e) => setForm({ ...form, amount: e.target.value })} />
                    <Button onClick={create} disabled={saving}>{saving ? 'Saving…' : 'Create'}</Button>
                </div>
            </Modal>
        </div>
    );
}
