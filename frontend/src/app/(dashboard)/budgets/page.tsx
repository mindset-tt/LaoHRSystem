'use client';

import { useEffect, useState } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { PageHeader } from '@/components/ui/PageHeader';
import { DataTable, type DataTableColumn } from '@/components/ui/DataTable';
import { useToast } from '@/components/ui/Toast';
import { backOfficeApi, type BudgetDto } from '@/lib/endpoints/backOffice';

export default function BudgetsPage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [budgets, setBudgets] = useState<BudgetDto[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let cancelled = false;
        backOfficeApi.listBudgets()
            .then((b) => { if (!cancelled) setBudgets(b); })
            .catch((err) => { console.error(err); toast.error(t.common.error); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [t.common.error, toast]);

    const columns: DataTableColumn<BudgetDto>[] = [
        { key: 'fiscalYear', header: 'Fiscal Year', render: (r) => r.fiscalYear },
        { key: 'category', header: 'Category', render: (r) => r.category },
        { key: 'approvedAmount', header: 'Approved', render: (r) => r.approvedAmount.toLocaleString() },
        { key: 'committedAmount', header: 'Committed', render: (r) => r.committedAmount.toLocaleString() },
        { key: 'actualAmount', header: 'Actual', render: (r) => r.actualAmount.toLocaleString() },
        { key: 'availableAmount', header: 'Available', render: (r) => r.availableAmount.toLocaleString() },
    ];

    return (
        <div>
            <PageHeader title={t.sidebar.budgets} subtitle={t.sidebar.budgets} />
            <Card>
                <DataTable
                    columns={columns}
                    rows={budgets}
                    rowKey={(r) => r.budgetId}
                    isLoading={loading}
                />
            </Card>
        </div>
    );
}
