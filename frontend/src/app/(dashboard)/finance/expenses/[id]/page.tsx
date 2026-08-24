'use client';

import Link from 'next/link';
import { useEffect, useState } from 'react';
import { useParams } from 'next/navigation';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { PageHeader } from '@/components/ui/PageHeader';
import { ErrorState } from '@/components/ui/EmptyState';
import { useToast } from '@/components/ui/Toast';
import { expensesApi, EXPENSE_STATUSES, type ExpenseDetail } from '@/lib/endpoints';
import styles from '../../page.module.css';

export default function ExpenseDetailPage() {
    const params = useParams<{ id: string }>();
    const expenseId = Number(params.id);
    const { t } = useLanguage();
    const toast = useToast();
    const [expense, setExpense] = useState<ExpenseDetail | null>(null);
    const [loading, setLoading] = useState(true);
    const [acting, setActing] = useState(false);

    useEffect(() => {
        if (!expenseId) return;
        let cancelled = false;
        setLoading(true);
        expensesApi.get(expenseId)
            .then((d) => { if (!cancelled) setExpense(d); })
            .catch((err) => {
                console.error(err);
                toast.error(t.finance.expenses.messages.errorAction);
            })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [expenseId, t.finance.expenses.messages.errorAction, toast]);

    async function refresh() {
        const updated = await expensesApi.get(expenseId);
        setExpense(updated);
    }

    async function doAction(action: 'approve' | 'reject' | 'pay') {
        if (!expense) return;
        setActing(true);
        try {
            if (action === 'approve') await expensesApi.approve(expense.expenseId);
            else if (action === 'reject') await expensesApi.reject(expense.expenseId);
            else await expensesApi.markPaid(expense.expenseId);
            const messageKey =
                action === 'approve' ? t.finance.expenses.messages.approved :
                action === 'reject' ? t.finance.expenses.messages.rejected :
                t.finance.expenses.messages.paid;
            toast.success(messageKey);
            await refresh();
        } catch (err) {
            console.error(err);
            toast.error(t.finance.expenses.messages.errorAction);
        } finally {
            setActing(false);
        }
    }

    if (loading) {
        return (
            <PageHeader
                title="…"
                subtitle="…"
                breadcrumbs={[
                    { label: t.nav.dashboard, href: '/' },
                    { label: t.finance.expenses.title, href: '/finance/expenses' },
                    { label: '…' },
                ]}
            />
        );
    }

    if (!expense) {
        return (
            <div className={styles.page}>
                <Card>
                    <ErrorState
                        title="Expense not found"
                        description="The expense you tried to view does not exist or has been deleted."
                    />
                </Card>
            </div>
        );
    }

    const statusLabel = (s: string) =>
        (t.finance.expenses.status as Record<string, string>)[s.toLowerCase()] ?? s;

    const formatMoney = (n: number, ccy: string) =>
        new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(n) + ' ' + ccy;

    const formatLak = (n: number) => new Intl.NumberFormat('en-US').format(n) + ' ₭';

    return (
        <div className={styles.page}>
            <PageHeader
                title={expense.title}
                subtitle={`${expense.expenseNumber}`}
                breadcrumbs={[
                    { label: t.nav.dashboard, href: '/' },
                    { label: t.finance.expenses.title, href: '/finance/expenses' },
                    { label: expense.expenseNumber },
                ]}
                actions={
                    <div className={styles.headerActions}>
                        <span className={`${styles.badge} ${styles[`expense_${expense.status.toLowerCase()}`]}`}>
                            {statusLabel(expense.status)}
                        </span>
                        {expense.status === 'SUBMITTED' && (
                            <>
                                <Button variant="ghost" onClick={() => doAction('reject')} loading={acting}>
                                    {t.finance.expenses.reject}
                                </Button>
                                <Button onClick={() => doAction('approve')} loading={acting}>
                                    {t.finance.expenses.approve}
                                </Button>
                            </>
                        )}
                        {expense.status === 'APPROVED' && (
                            <Button onClick={() => doAction('pay')} loading={acting}>
                                {t.finance.expenses.markPaid}
                            </Button>
                        )}
                    </div>
                }
            />

            <div className={styles.detailLayout}>
                <div className={styles.detailMain}>
                    <Card>
                        <h3 className={styles.sectionTitle}>{t.finance.expenses.fields.description}</h3>
                        {expense.description ? (
                            <p className={styles.description}>{expense.description}</p>
                        ) : (
                            <p className={styles.muted}>—</p>
                        )}
                    </Card>
                </div>

                <div className={styles.detailSidebar}>
                    <Card>
                        <h3 className={styles.sectionTitle}>Details</h3>
                        <dl className={styles.dl}>
                            <dt>{t.finance.expenses.fields.amount}</dt>
                            <dd>{formatMoney(expense.amount, expense.currency)}</dd>
                            <dt>{t.finance.expenses.fields.amountLak}</dt>
                            <dd>{formatLak(expense.amountLak)}</dd>
                            <dt>{t.finance.expenses.fields.employee}</dt>
                            <dd>{expense.employeeName ?? '—'}</dd>
                            <dt>{t.finance.expenses.fields.category}</dt>
                            <dd>{expense.categoryName ?? '—'}</dd>
                            <dt>{t.finance.expenses.fields.date}</dt>
                            <dd>{new Date(expense.expenseDate).toLocaleDateString()}</dd>
                            <dt>{t.finance.expenses.fields.approver}</dt>
                            <dd>{expense.approverName ?? '—'}</dd>
                            {expense.approvedAt && (
                                <>
                                    <dt>Approved</dt>
                                    <dd>{new Date(expense.approvedAt).toLocaleDateString()}</dd>
                                </>
                            )}
                            {expense.approverNotes && (
                                <>
                                    <dt>{t.finance.expenses.fields.notes}</dt>
                                    <dd>{expense.approverNotes}</dd>
                                </>
                            )}
                        </dl>
                    </Card>
                </div>
            </div>
        </div>
    );
}
