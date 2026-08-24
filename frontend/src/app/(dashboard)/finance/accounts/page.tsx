'use client';

import { useEffect, useState } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { PageHeader } from '@/components/ui/PageHeader';
import { DataTable, type DataTableColumn } from '@/components/ui/DataTable';
import { useToast } from '@/components/ui/Toast';
import { financeAccountingApi, type AccountDto } from '@/lib/endpoints/financeAccounting';

export default function AccountsPage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [accounts, setAccounts] = useState<AccountDto[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let cancelled = false;
        financeAccountingApi.listAccounts()
            .then((a) => { if (!cancelled) setAccounts(a); })
            .catch((err) => { console.error(err); toast.error(t.common.error); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [t.common.error, toast]);

    const columns: DataTableColumn<AccountDto>[] = [
        { key: 'accountCode', header: 'Code', render: (r) => r.accountCode },
        { key: 'name', header: 'Name', render: (r) => r.name },
        { key: 'accountType', header: 'Type', render: (r) => r.accountType },
        { key: 'isPostingAccount', header: 'Posting', render: (r) => (r.isPostingAccount ? 'Yes' : 'No') },
        { key: 'isActive', header: 'Active', render: (r) => (r.isActive ? 'Yes' : 'No') },
    ];

    return (
        <div>
            <PageHeader title="Chart of Accounts" subtitle="Accounting" />
            <Card>
                <DataTable columns={columns} rows={accounts} rowKey={(r) => r.accountId} isLoading={loading} />
            </Card>
        </div>
    );
}
