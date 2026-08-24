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
import { corporateApi, type RoomDto, type FacilityDto } from '@/lib/endpoints/corporate';

const ROOM_TYPES = ['MEETING_ROOM', 'TRAINING_ROOM', 'WORKSPACE', 'CONFERENCE_ROOM', 'OTHER'];

export default function CorporateRoomsPage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [rooms, setRooms] = useState<RoomDto[]>([]);
    const [facilities, setFacilities] = useState<FacilityDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [showCreate, setShowCreate] = useState(false);
    const [form, setForm] = useState({ facilityId: 0, code: '', name: '', roomType: 'MEETING_ROOM', capacity: '' });
    const [saving, setSaving] = useState(false);

    const load = () => {
        corporateApi.listRooms()
            .then(setRooms)
            .catch((err) => { console.error(err); toast.error(t.common.error); })
            .finally(() => setLoading(false));
    };

    useEffect(() => {
        load();
        corporateApi.listFacilities().then(setFacilities).catch(() => {});
        /* eslint-disable-next-line react-hooks/exhaustive-deps */
    }, []);

    const create = async () => {
        if (!form.code.trim() || !form.name.trim()) { toast.error('Code and Name are required'); return; }
        setSaving(true);
        try {
            await corporateApi.createRoom({
                facilityId: form.facilityId,
                code: form.code,
                name: form.name,
                roomType: form.roomType,
                capacity: form.capacity ? Number(form.capacity) : null,
            });
            toast.success('Room created');
            setShowCreate(false);
            setForm({ facilityId: 0, code: '', name: '', roomType: 'MEETING_ROOM', capacity: '' });
            load();
        } catch (err) { console.error(err); toast.error(t.common.error); }
        finally { setSaving(false); }
    };

    const columns: DataTableColumn<RoomDto>[] = [
        { key: 'code', header: 'Code', render: (r) => r.code },
        { key: 'name', header: 'Name', render: (r) => r.name },
        { key: 'roomType', header: 'Type', render: (r) => r.roomType },
        { key: 'capacity', header: 'Capacity', render: (r) => r.capacity ?? '—' },
        { key: 'bookable', header: 'Bookable', render: (r) => (r.bookable ? 'Yes' : 'No') },
    ];

    return (
        <div>
            <PageHeader
                title={t.sidebar.rooms}
                subtitle="Bookable spaces"
                actions={<Button onClick={() => setShowCreate(true)}>New Room</Button>}
            />
            <Card>
                <DataTable columns={columns} rows={rooms} rowKey={(r) => r.roomId} isLoading={loading} />
            </Card>

            <Modal isOpen={showCreate} onClose={() => setShowCreate(false)} title="New Room">
                <div style={{ display: 'grid', gap: '0.75rem' }}>
                    <Select label="Facility" options={facilities.map((f) => ({ value: f.facilityId, label: f.name }))} value={form.facilityId} onChange={(e) => setForm({ ...form, facilityId: Number(e.target.value) })} />
                    <Input label="Code" value={form.code} onChange={(e) => setForm({ ...form, code: e.target.value })} />
                    <Input label="Name" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
                    <Select label="Type" options={ROOM_TYPES.map((r) => ({ value: r, label: r }))} value={form.roomType} onChange={(e) => setForm({ ...form, roomType: e.target.value })} />
                    <Input label="Capacity" type="number" value={form.capacity} onChange={(e) => setForm({ ...form, capacity: e.target.value })} />
                    <Button onClick={create} disabled={saving}>{saving ? 'Saving…' : 'Create'}</Button>
                </div>
            </Modal>
        </div>
    );
}
