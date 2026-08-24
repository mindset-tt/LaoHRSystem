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
import { corporateApi, type CorporateDocumentDto } from '@/lib/endpoints/corporate';
import type { PaginatedResponse } from '@/lib/types/pagination';

const DOC_TYPES = ['GENERAL', 'POLICY', 'CONTRACT', 'INVOICE', 'QUOTATION', 'RECEIPT', 'CERTIFICATE', 'LICENSE', 'IDENTIFICATION', 'ASSET_DOCUMENT', 'VEHICLE_DOCUMENT', 'TRAVEL_DOCUMENT', 'PROJECT_DOCUMENT'];
const CONFIDENTIALITY = ['INTERNAL', 'CONFIDENTIAL', 'RESTRICTED'];

export default function CorporateDocumentsPage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [pageData, setPageData] = useState<PaginatedResponse<CorporateDocumentDto>>({
        items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false,
    });
    const [loading, setLoading] = useState(true);
    const [page, setPage] = useState(1);
    const [showCreate, setShowCreate] = useState(false);
    const [form, setForm] = useState({ title: '', documentType: 'GENERAL', confidentiality: 'INTERNAL', ownerEntityType: 'GENERAL', ownerEntityId: 0, description: '' });
    const [saving, setSaving] = useState(false);

    const load = () => {
        corporateApi.listDocuments({ page, pageSize: 25 })
            .then((p) => setPageData(p))
            .catch((err) => { console.error(err); toast.error(t.common.error); })
            .finally(() => setLoading(false));
    };

    useEffect(() => { load(); /* eslint-disable-line react-hooks/exhaustive-deps */ }, [page]);

    const create = async () => {
        if (!form.title.trim()) { toast.error('Title is required'); return; }
        setSaving(true);
        try {
            await corporateApi.createDocument({
                title: form.title,
                documentType: form.documentType,
                confidentiality: form.confidentiality,
                ownerEntityType: form.ownerEntityType,
                ownerEntityId: form.ownerEntityId,
                description: form.description || null,
            });
            toast.success('Document created');
            setShowCreate(false);
            setForm({ title: '', documentType: 'GENERAL', confidentiality: 'INTERNAL', ownerEntityType: 'GENERAL', ownerEntityId: 0, description: '' });
            load();
        } catch (err) { console.error(err); toast.error(t.common.error); }
        finally { setSaving(false); }
    };

    const columns: DataTableColumn<CorporateDocumentDto>[] = [
        { key: 'documentNumber', header: 'Number', render: (r) => r.documentNumber },
        { key: 'title', header: 'Title', render: (r) => r.title },
        { key: 'documentType', header: 'Type', render: (r) => r.documentType },
        { key: 'confidentiality', header: 'Classification', render: (r) => r.confidentiality },
        { key: 'currentVersion', header: 'Version', render: (r) => `v${r.currentVersion}` },
        { key: 'expiryDate', header: 'Expiry', render: (r) => (r.expiryDate ? r.expiryDate.slice(0, 10) : '—') },
    ];

    return (
        <div>
            <PageHeader
                title={t.sidebar.documents}
                subtitle="Corporate document management"
                actions={<Button onClick={() => setShowCreate(true)}>New Document</Button>}
            />
            <Card>
                <DataTable columns={columns} rows={pageData.items} rowKey={(r) => r.documentId} isLoading={loading} />
                <Pagination page={page} pageSize={25} totalItems={pageData.totalItems} totalPages={pageData.totalPages} hasNext={pageData.hasNext} hasPrevious={pageData.hasPrevious} onPageChange={setPage} />
            </Card>

            <Modal isOpen={showCreate} onClose={() => setShowCreate(false)} title="New Document">
                <div style={{ display: 'grid', gap: '0.75rem' }}>
                    <Input label="Title" value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} />
                    <Input label="Description" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
                    <Select label="Type" options={DOC_TYPES.map((d) => ({ value: d, label: d }))} value={form.documentType} onChange={(e) => setForm({ ...form, documentType: e.target.value })} />
                    <Select label="Classification" options={CONFIDENTIALITY.map((c) => ({ value: c, label: c }))} value={form.confidentiality} onChange={(e) => setForm({ ...form, confidentiality: e.target.value })} />
                    <Input label="Owner Entity Type" value={form.ownerEntityType} onChange={(e) => setForm({ ...form, ownerEntityType: e.target.value })} />
                    <Input label="Owner Entity ID" type="number" value={form.ownerEntityId} onChange={(e) => setForm({ ...form, ownerEntityId: Number(e.target.value) })} />
                    <Button onClick={create} disabled={saving}>{saving ? 'Saving…' : 'Create'}</Button>
                </div>
            </Modal>
        </div>
    );
}
