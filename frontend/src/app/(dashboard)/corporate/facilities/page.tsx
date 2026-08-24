'use client';

import { useEffect, useState } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { PageHeader } from '@/components/ui/PageHeader';
import { DataTable, type DataTableColumn } from '@/components/ui/DataTable';
import { Button } from '@/components/ui/Button';
import { Modal } from '@/components/ui/Modal';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { useToast } from '@/components/ui/Toast';
import { corporateApi, type FacilityDto } from '@/lib/endpoints/corporate';

const FACILITY_TYPES = ['OFFICE', 'BUILDING', 'BRANCH', 'WAREHOUSE_SITE', 'OTHER'];

export default function CorporateFacilitiesPage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [facilities, setFacilities] = useState<FacilityDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [showCreate, setShowCreate] = useState(false);
    const [form, setForm] = useState({ facilityCode: '', name: '', facilityType: 'OFFICE' });
    const [saving, setSaving] = useState(false);

    const load = () => {
        corporateApi.listFacilities()
            .then(setFacilities)
            .catch((err) => { console.error(err); toast.error(t.common.error); })
            .finally(() => setLoading(false));
    };

    useEffect(() => { load(); /* eslint-disable-line react-hooks/exhaustive-deps */ }, []);

    const create = async () => {
        if (!form.facilityCode.trim() || !form.name.trim()) { toast.error('Code and Name are required'); return; }
        setSaving(true);
        try {
            await corporateApi.createFacility({ facilityCode: form.facilityCode, name: form.name, facilityType: form.facilityType });
            toast.success('Facility created');
            setShowCreate(false);
            setForm({ facilityCode: '', name: '', facilityType: 'OFFICE' });
            load();
        } catch (err) { console.error(err); toast.error(t.common.error); }
        finally { setSaving(false); }
    };

    const columns: DataTableColumn<FacilityDto>[] = [
        { key: 'facilityCode', header: 'Code', render: (r) => r.facilityCode },
        { key: 'name', header: 'Name', render: (r) => r.name },
        { key: 'facilityType', header: 'Type', render: (r) => r.facilityType },
        { key: 'address', header: 'Address', render: (r) => r.address ?? '—' },
        { key: 'status', header: 'Status', render: (r) => r.status },
    ];

    return (
        <div>
            <PageHeader
                title={t.sidebar.facilities}
                subtitle="Physical operational sites"
                actions={<Button onClick={() => setShowCreate(true)}>New Facility</Button>}
            />
            <Card>
                <DataTable columns={columns} rows={facilities} rowKey={(r) => r.facilityId} isLoading={loading} />
            </Card>

            <Modal isOpen={showCreate} onClose={() => setShowCreate(false)} title="New Facility">
                <div style={{ display: 'grid', gap: '0.75rem' }}>
                    <Input label="Code" value={form.facilityCode} onChange={(e) => setForm({ ...form, facilityCode: e.target.value })} />
                    <Input label="Name" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
                    <Select label="Type" options={FACILITY_TYPES.map((f) => ({ value: f, label: f }))} value={form.facilityType} onChange={(e) => setForm({ ...form, facilityType: e.target.value })} />
                    <Button onClick={create} disabled={saving}>{saving ? 'Saving…' : 'Create'}</Button>
                </div>
            </Modal>
        </div>
    );
}
