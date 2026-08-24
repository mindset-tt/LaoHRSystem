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
import { corporateApi, type VisitorDto, type VisitDto } from '@/lib/endpoints/corporate';
import type { PaginatedResponse } from '@/lib/types/pagination';

export default function CorporateVisitorsPage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [visitors, setVisitors] = useState<PaginatedResponse<VisitorDto>>({
        items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false,
    });
    const [visits, setVisits] = useState<VisitDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [page, setPage] = useState(1);
    const [showCreate, setShowCreate] = useState(false);
    const [form, setForm] = useState({ fullName: '', company: '' });
    const [saving, setSaving] = useState(false);

    const load = () => {
        Promise.all([
            corporateApi.listVisitors({ page, pageSize: 25 }),
            corporateApi.listVisits(),
        ])
            .then(([v, visits]) => { setVisitors(v); setVisits(visits); })
            .catch((err) => { console.error(err); toast.error(t.common.error); })
            .finally(() => setLoading(false));
    };

    useEffect(() => { load(); /* eslint-disable-line react-hooks/exhaustive-deps */ }, [page]);

    const create = async () => {
        if (!form.fullName.trim()) { toast.error('Full name is required'); return; }
        setSaving(true);
        try {
            await corporateApi.createVisitor({ fullName: form.fullName, company: form.company || null });
            toast.success('Visitor created');
            setShowCreate(false);
            setForm({ fullName: '', company: '' });
            load();
        } catch (err) { console.error(err); toast.error(t.common.error); }
        finally { setSaving(false); }
    };

    const checkIn = async (id: number) => {
        try { await corporateApi.checkInVisit(id); toast.success('Checked in'); load(); }
        catch (err) { console.error(err); toast.error(t.common.error); }
    };
    const checkOut = async (id: number) => {
        try { await corporateApi.checkOutVisit(id); toast.success('Checked out'); load(); }
        catch (err) { console.error(err); toast.error(t.common.error); }
    };

    const visitorColumns: DataTableColumn<VisitorDto>[] = [
        { key: 'fullName', header: 'Name', render: (r) => r.fullName },
        { key: 'company', header: 'Company', render: (r) => r.company ?? '—' },
        { key: 'phone', header: 'Phone', render: (r) => r.phone ?? '—' },
    ];

    const visitColumns: DataTableColumn<VisitDto>[] = [
        { key: 'visitorName', header: 'Visitor', render: (r) => r.visitorName ?? '—' },
        { key: 'hostName', header: 'Host', render: (r) => r.hostName ?? '—' },
        { key: 'status', header: 'Status', render: (r) => r.status },
        {
            key: 'actions', header: 'Actions', render: (r) => (
                <div style={{ display: 'flex', gap: '0.5rem' }}>
                    {r.status === 'EXPECTED' && <Button size="sm" onClick={() => checkIn(r.visitId)}>Check In</Button>}
                    {r.status === 'CHECKED_IN' && <Button size="sm" onClick={() => checkOut(r.visitId)}>Check Out</Button>}
                </div>
            ),
        },
    ];

    return (
        <div>
            <PageHeader
                title={t.sidebar.visitors}
                subtitle="Visitor management"
                actions={<Button onClick={() => setShowCreate(true)}>New Visitor</Button>}
            />
            <Card>
                <h3 style={{ margin: '0 0 0.5rem' }}>Visits</h3>
                <DataTable columns={visitColumns} rows={visits} rowKey={(r) => r.visitId} isLoading={loading} />
            </Card>
            <Card>
                <h3 style={{ margin: '0 0 0.5rem' }}>Visitors</h3>
                <DataTable columns={visitorColumns} rows={visitors.items} rowKey={(r) => r.visitorId} isLoading={loading} />
                <Pagination page={page} pageSize={25} totalItems={visitors.totalItems} totalPages={visitors.totalPages} hasNext={visitors.hasNext} hasPrevious={visitors.hasPrevious} onPageChange={setPage} />
            </Card>

            <Modal isOpen={showCreate} onClose={() => setShowCreate(false)} title="New Visitor">
                <div style={{ display: 'grid', gap: '0.75rem' }}>
                    <Input label="Full Name" value={form.fullName} onChange={(e) => setForm({ ...form, fullName: e.target.value })} />
                    <Input label="Company" value={form.company} onChange={(e) => setForm({ ...form, company: e.target.value })} />
                    <Button onClick={create} disabled={saving}>{saving ? 'Saving…' : 'Create'}</Button>
                </div>
            </Modal>
        </div>
    );
}
