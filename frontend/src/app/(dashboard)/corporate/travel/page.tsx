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
import { corporateApi, type TravelRequestDto } from '@/lib/endpoints/corporate';
import type { PaginatedResponse } from '@/lib/types/pagination';

export default function CorporateTravelPage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [pageData, setPageData] = useState<PaginatedResponse<TravelRequestDto>>({
        items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false,
    });
    const [loading, setLoading] = useState(true);
    const [page, setPage] = useState(1);
    const [showCreate, setShowCreate] = useState(false);
    const [form, setForm] = useState({ purpose: '', destination: '', departureDate: '', returnDate: '', estimatedCost: '' });
    const [saving, setSaving] = useState(false);

    const load = () => {
        corporateApi.listTravelRequests({ page, pageSize: 25 })
            .then((p) => setPageData(p))
            .catch((err) => { console.error(err); toast.error(t.common.error); })
            .finally(() => setLoading(false));
    };

    useEffect(() => { load(); /* eslint-disable-line react-hooks/exhaustive-deps */ }, [page]);

    const create = async () => {
        if (!form.purpose.trim() || !form.destination.trim()) { toast.error('Purpose and Destination are required'); return; }
        if (form.returnDate && form.departureDate && form.returnDate < form.departureDate) { toast.error('Return date must be on or after departure'); return; }
        setSaving(true);
        try {
            await corporateApi.createTravelRequest({
                purpose: form.purpose,
                destination: form.destination,
                departureDate: form.departureDate,
                returnDate: form.returnDate,
                estimatedCost: form.estimatedCost ? Number(form.estimatedCost) : null,
            });
            toast.success('Travel request created');
            setShowCreate(false);
            setForm({ purpose: '', destination: '', departureDate: '', returnDate: '', estimatedCost: '' });
            load();
        } catch (err) { console.error(err); toast.error(t.common.error); }
        finally { setSaving(false); }
    };

    const columns: DataTableColumn<TravelRequestDto>[] = [
        { key: 'travelNumber', header: 'Number', render: (r) => r.travelNumber },
        { key: 'destination', header: 'Destination', render: (r) => r.destination },
        { key: 'purpose', header: 'Purpose', render: (r) => r.purpose },
        { key: 'departureDate', header: 'Departure', render: (r) => r.departureDate.slice(0, 10) },
        { key: 'returnDate', header: 'Return', render: (r) => r.returnDate.slice(0, 10) },
        { key: 'status', header: 'Status', render: (r) => r.status },
    ];

    return (
        <div>
            <PageHeader
                title={t.sidebar.travel}
                subtitle="Business travel requests"
                actions={<Button onClick={() => setShowCreate(true)}>New Travel Request</Button>}
            />
            <Card>
                <DataTable columns={columns} rows={pageData.items} rowKey={(r) => r.travelRequestId} isLoading={loading} />
                <Pagination page={page} pageSize={25} totalItems={pageData.totalItems} totalPages={pageData.totalPages} hasNext={pageData.hasNext} hasPrevious={pageData.hasPrevious} onPageChange={setPage} />
            </Card>

            <Modal isOpen={showCreate} onClose={() => setShowCreate(false)} title="New Travel Request">
                <div style={{ display: 'grid', gap: '0.75rem' }}>
                    <Input label="Purpose" value={form.purpose} onChange={(e) => setForm({ ...form, purpose: e.target.value })} />
                    <Input label="Destination" value={form.destination} onChange={(e) => setForm({ ...form, destination: e.target.value })} />
                    <Input label="Departure Date" type="date" value={form.departureDate} onChange={(e) => setForm({ ...form, departureDate: e.target.value })} />
                    <Input label="Return Date" type="date" value={form.returnDate} onChange={(e) => setForm({ ...form, returnDate: e.target.value })} />
                    <Input label="Estimated Cost" type="number" value={form.estimatedCost} onChange={(e) => setForm({ ...form, estimatedCost: e.target.value })} />
                    <Button onClick={create} disabled={saving}>{saving ? 'Saving…' : 'Create'}</Button>
                </div>
            </Modal>
        </div>
    );
}
