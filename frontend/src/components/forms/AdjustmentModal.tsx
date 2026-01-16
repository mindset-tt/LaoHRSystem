'use client';

import { useState, useEffect } from 'react';
import { Button } from '@/components/ui/Button';
import { Card } from '@/components/ui/Card';
import { MaskedField } from '@/components/ui/MaskedField';
import { employeesApi, adjustmentApi } from '@/lib/endpoints';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Employee, PayrollAdjustment } from '@/lib/types';

interface AdjustmentModalProps {
    isOpen: boolean;
    onClose: () => void;
    periodId: number;
    periodName: string;
}

export function AdjustmentModal({ isOpen, onClose, periodId, periodName }: AdjustmentModalProps) {
    const { t } = useLanguage();
    const [employees, setEmployees] = useState<Employee[]>([]);
    const [selectedEmployeeId, setSelectedEmployeeId] = useState<number | null>(null);
    const [adjustments, setAdjustments] = useState<PayrollAdjustment[]>([]);
    const [loading, setLoading] = useState(false);
    const [loadingList, setLoadingList] = useState(false);

    // Form State
    const [name, setName] = useState('');
    const [amount, setAmount] = useState('');
    const [type, setType] = useState<'EARNING' | 'DEDUCTION' | 'BONUS'>('EARNING');
    const [isTaxable, setIsTaxable] = useState(true);

    useEffect(() => {
        if (isOpen) {
            loadEmployees();
        }
    }, [isOpen]);

    useEffect(() => {
        if (selectedEmployeeId) {
            loadAdjustments(selectedEmployeeId);
        } else {
            setAdjustments([]);
        }
    }, [selectedEmployeeId, periodId]);

    const loadEmployees = async () => {
        try {
            const data = await employeesApi.getAll();
            setEmployees(data.filter(e => e.isActive));
        } catch (err) {
            console.error('Failed to load employees', err);
        }
    };

    const loadAdjustments = async (empId: number) => {
        setLoadingList(true);
        try {
            const data = await adjustmentApi.getAdjustments(periodId, empId);
            setAdjustments(data);
        } catch (err) {
            console.error('Failed to load adjustments', err);
        } finally {
            setLoadingList(false);
        }
    };

    const handleAdd = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!selectedEmployeeId || !name || !amount) return;

        setLoading(true);
        try {
            await adjustmentApi.create({
                employeeId: selectedEmployeeId,
                periodId: periodId,
                name,
                type,
                amount: parseFloat(amount),
                isTaxable,
                isNssfAssessable: false // Default to false
            });

            // Reset form
            setName('');
            setAmount('');

            // Reload list
            await loadAdjustments(selectedEmployeeId);
        } catch (err) {
            console.error('Failed to add adjustment', err);
        } finally {
            setLoading(false);
        }
    };

    const handleDelete = async (id: number) => {
        if (!confirm('Are you sure?')) return;
        try {
            await adjustmentApi.delete(id);
            if (selectedEmployeeId) loadAdjustments(selectedEmployeeId);
        } catch (err) {
            console.error('Failed to delete', err);
        }
    };

    if (!isOpen) return null;

    return (
        <div style={{
            position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
            backgroundColor: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 50
        }}>
            <Card style={{ width: '800px', maxHeight: '90vh', overflowY: 'auto', display: 'flex', flexDirection: 'column' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '20px' }}>
                    <h2 style={{ fontSize: '1.25rem', fontWeight: 'bold' }}>Manage Adjustments - {periodName}</h2>
                    <button onClick={onClose} style={{ border: 'none', background: 'none', fontSize: '1.5rem', cursor: 'pointer' }}>×</button>
                </div>

                <div style={{ display: 'grid', gridTemplateColumns: '250px 1fr', gap: '20px', minHeight: '400px' }}>
                    {/* Left: Employee List */}
                    <div style={{ borderRight: '1px solid #eee', paddingRight: '10px' }}>
                        <h3 style={{ fontSize: '0.9rem', fontWeight: 600, marginBottom: '10px' }}>Select Employee</h3>
                        <div style={{ display: 'flex', flexDirection: 'column', gap: '5px', maxHeight: '400px', overflowY: 'auto' }}>
                            {employees.map(emp => (
                                <button
                                    key={emp.employeeId}
                                    onClick={() => setSelectedEmployeeId(emp.employeeId)}
                                    style={{
                                        padding: '8px',
                                        textAlign: 'left',
                                        border: '1px solid',
                                        borderColor: selectedEmployeeId === emp.employeeId ? '#6200EE' : '#ddd',
                                        backgroundColor: selectedEmployeeId === emp.employeeId ? '#F3E5F5' : 'white',
                                        borderRadius: '4px',
                                        cursor: 'pointer'
                                    }}
                                >
                                    <div style={{ fontWeight: 500 }}>{emp.englishName || emp.laoName}</div>
                                    <div style={{ fontSize: '0.8rem', color: '#666' }}>{emp.employeeCode}</div>
                                </button>
                            ))}
                        </div>
                    </div>

                    {/* Right: Adjustments */}
                    <div>
                        {!selectedEmployeeId ? (
                            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', height: '100%', color: '#888' }}>
                                Select an employee to manage adjustments
                            </div>
                        ) : (
                            <>
                                {/* Add Form */}
                                <form onSubmit={handleAdd} style={{ display: 'flex', gap: '10px', alignItems: 'flex-end', paddingBottom: '20px', borderBottom: '1px solid #eee', marginBottom: '20px' }}>
                                    <div style={{ flex: 2 }}>
                                        <label style={{ display: 'block', fontSize: '0.8rem', marginBottom: '4px' }}>Name</label>
                                        <input
                                            value={name} onChange={e => setName(e.target.value)}
                                            placeholder="e.g. Fuel, Commission"
                                            style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }}
                                            required
                                        />
                                    </div>
                                    <div style={{ flex: 1 }}>
                                        <label style={{ display: 'block', fontSize: '0.8rem', marginBottom: '4px' }}>Amount</label>
                                        <input
                                            type="number"
                                            value={amount} onChange={e => setAmount(e.target.value)}
                                            placeholder="0"
                                            style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }}
                                            required
                                        />
                                    </div>
                                    <div style={{ width: '120px' }}>
                                        <label style={{ display: 'block', fontSize: '0.8rem', marginBottom: '4px' }}>Type</label>
                                        <select
                                            value={type}
                                            onChange={e => setType(e.target.value as any)}
                                            style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }}
                                        >
                                            <option value="EARNING">Income</option>
                                            <option value="DEDUCTION">Deduction</option>
                                            <option value="BONUS">Bonus</option>
                                        </select>
                                    </div>
                                    {type === 'EARNING' && (
                                        <div style={{ display: 'flex', alignItems: 'center', paddingBottom: '8px' }}>
                                            <input
                                                type="checkbox"
                                                id="taxable"
                                                checked={isTaxable}
                                                onChange={e => setIsTaxable(e.target.checked)}
                                                style={{ marginRight: '5px' }}
                                            />
                                            <label htmlFor="taxable" style={{ fontSize: '0.9rem' }}>Taxable?</label>
                                        </div>
                                    )}
                                    <Button type="submit" loading={loading} disabled={!name || !amount}>Add</Button>
                                </form>

                                {/* List */}
                                {loadingList ? (
                                    <div style={{ padding: '20px', textAlign: 'center' }}>Loading...</div>
                                ) : (
                                    <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
                                        {adjustments.length === 0 && <div style={{ color: '#888', fontStyle: 'italic' }}>No adjustments added.</div>}
                                        {adjustments.map(adj => (
                                            <div key={adj.adjustmentId} style={{
                                                display: 'flex', justifyContent: 'space-between', alignItems: 'center',
                                                padding: '10px', backgroundColor: '#f9f9f9', borderRadius: '6px', borderLeft: `4px solid ${adj.type === 'DEDUCTION' ? 'red' : 'green'}`
                                            }}>
                                                <div>
                                                    <div style={{ fontWeight: 600 }}>{adj.name}</div>
                                                    <div style={{ fontSize: '0.8rem', color: '#666' }}>
                                                        {adj.type} • {adj.type === 'EARNING' ? (adj.isTaxable ? 'Taxable' : 'Non-Taxable') : 'Deduction'}
                                                    </div>
                                                </div>
                                                <div style={{ display: 'flex', gap: '15px', alignItems: 'center' }}>
                                                    <div style={{ fontWeight: 'bold' }}>
                                                        {new Intl.NumberFormat().format(adj.amount)}
                                                    </div>
                                                    <button
                                                        onClick={() => handleDelete(adj.adjustmentId)}
                                                        style={{ color: 'red', border: 'none', background: 'none', cursor: 'pointer' }}
                                                        title="Delete"
                                                    >
                                                        🗑️
                                                    </button>
                                                </div>
                                            </div>
                                        ))}
                                    </div>
                                )}
                            </>
                        )}
                    </div>
                </div>
            </Card>
        </div>
    );
}
