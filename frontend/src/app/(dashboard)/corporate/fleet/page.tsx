'use client';

import { useEffect, useState } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { PageHeader } from '@/components/ui/PageHeader';
import { DataTable, type DataTableColumn } from '@/components/ui/DataTable';
import { Button } from '@/components/ui/Button';
import { Modal } from '@/components/ui/Modal';
import { Input } from '@/components/ui/Input';
import { useToast } from '@/components/ui/Toast';
import { corporateApi, type VehicleDto } from '@/lib/endpoints/corporate';

export default function CorporateFleetPage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [vehicles, setVehicles] = useState<VehicleDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [showCreate, setShowCreate] = useState(false);
    const [form, setForm] = useState({ vehicleCode: '', registrationNumber: '', make: '', model: '', currentOdometer: 0 });
    const [saving, setSaving] = useState(false);

    const load = () => {
        corporateApi.listVehicles()
            .then(setVehicles)
            .catch((err) => { console.error(err); toast.error(t.common.error); })
            .finally(() => setLoading(false));
    };

    useEffect(() => { load(); /* eslint-disable-line react-hooks/exhaustive-deps */ }, []);

    const create = async () => {
        if (!form.vehicleCode.trim() || !form.registrationNumber.trim()) { toast.error('Code and Registration are required'); return; }
        setSaving(true);
        try {
            await corporateApi.createVehicle({
                vehicleCode: form.vehicleCode,
                registrationNumber: form.registrationNumber,
                make: form.make || null,
                model: form.model || null,
                currentOdometer: form.currentOdometer,
            });
            toast.success('Vehicle created');
            setShowCreate(false);
            setForm({ vehicleCode: '', registrationNumber: '', make: '', model: '', currentOdometer: 0 });
            load();
        } catch (err) { console.error(err); toast.error(t.common.error); }
        finally { setSaving(false); }
    };

    const columns: DataTableColumn<VehicleDto>[] = [
        { key: 'vehicleCode', header: 'Code', render: (r) => r.vehicleCode },
        { key: 'registrationNumber', header: 'Registration', render: (r) => r.registrationNumber },
        { key: 'make', header: 'Make', render: (r) => r.make ?? '—' },
        { key: 'model', header: 'Model', render: (r) => r.model ?? '—' },
        { key: 'currentOdometer', header: 'Odometer', render: (r) => r.currentOdometer },
        { key: 'status', header: 'Status', render: (r) => r.status },
    ];

    return (
        <div>
            <PageHeader
                title={t.sidebar.fleet}
                subtitle="Company vehicles"
                actions={<Button onClick={() => setShowCreate(true)}>New Vehicle</Button>}
            />
            <Card>
                <DataTable columns={columns} rows={vehicles} rowKey={(r) => r.vehicleId} isLoading={loading} />
            </Card>

            <Modal isOpen={showCreate} onClose={() => setShowCreate(false)} title="New Vehicle">
                <div style={{ display: 'grid', gap: '0.75rem' }}>
                    <Input label="Code" value={form.vehicleCode} onChange={(e) => setForm({ ...form, vehicleCode: e.target.value })} />
                    <Input label="Registration Number" value={form.registrationNumber} onChange={(e) => setForm({ ...form, registrationNumber: e.target.value })} />
                    <Input label="Make" value={form.make} onChange={(e) => setForm({ ...form, make: e.target.value })} />
                    <Input label="Model" value={form.model} onChange={(e) => setForm({ ...form, model: e.target.value })} />
                    <Input label="Current Odometer" type="number" value={form.currentOdometer} onChange={(e) => setForm({ ...form, currentOdometer: Number(e.target.value) })} />
                    <Button onClick={create} disabled={saving}>{saving ? 'Saving…' : 'Create'}</Button>
                </div>
            </Modal>
        </div>
    );
}
