'use client';

import { useEffect, useState } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { PageHeader } from '@/components/ui/PageHeader';
import { Skeleton } from '@/components/ui/Skeleton';
import { useToast } from '@/components/ui/Toast';
import { apiClient } from '@/lib/apiClient';

interface AccountingConfig {
    isConfigured: boolean;
    apControlAccountId?: number | null;
    defaultExpenseAccountId?: number | null;
    cashAccountId?: number | null;
    employeePayableAccountId?: number | null;
}

export default function FinanceSettingsPage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [config, setConfig] = useState<AccountingConfig | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let cancelled = false;
        apiClient.get<AccountingConfig>('/api/finance/settings')
            .then((c) => { if (!cancelled) setConfig(c); })
            .catch((err) => { console.error(err); toast.error(t.common.error); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [t.common.error, toast]);

    if (loading) return <Skeleton />;

    const rows: { label: string; value: string }[] = [
        { label: 'Accounting Configured', value: config?.isConfigured ? 'Yes' : 'No' },
        { label: 'AP Control Account', value: config?.apControlAccountId?.toString() ?? 'Missing' },
        { label: 'Default Expense Account', value: config?.defaultExpenseAccountId?.toString() ?? 'Missing' },
        { label: 'Cash Account', value: config?.cashAccountId?.toString() ?? 'Missing' },
        { label: 'Employee Payable Account', value: config?.employeePayableAccountId?.toString() ?? 'Missing' },
    ];

    return (
        <div>
            <PageHeader title="Finance Settings" subtitle="Accounting configuration" />
            <Card>
                <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                    <tbody>
                        {rows.map((r) => (
                            <tr key={r.label} style={{ borderBottom: '1px solid #eee' }}>
                                <td style={{ padding: '0.5rem', opacity: 0.7 }}>{r.label}</td>
                                <td style={{ padding: '0.5rem', fontWeight: 600 }}>{r.value}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </Card>
        </div>
    );
}
