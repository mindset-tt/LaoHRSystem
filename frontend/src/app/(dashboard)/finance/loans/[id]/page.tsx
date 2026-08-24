'use client';

import { useEffect, useState } from 'react';
import { useParams } from 'next/navigation';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { PageHeader } from '@/components/ui/PageHeader';
import { ErrorState } from '@/components/ui/EmptyState';
import { useToast } from '@/components/ui/Toast';
import { loansApi, LOAN_STATUSES, type LoanDetail } from '@/lib/endpoints';
import styles from '../../page.module.css';

export default function LoanDetailPage() {
    const params = useParams<{ id: string }>();
    const loanId = Number(params.id);
    const { t } = useLanguage();
    const toast = useToast();
    const [loan, setLoan] = useState<LoanDetail | null>(null);
    const [loading, setLoading] = useState(true);
    const [acting, setActing] = useState(false);
    const [repaymentAmount, setRepaymentAmount] = useState('');
    const [repaymentNotes, setRepaymentNotes] = useState('');
    const [repaymentDate, setRepaymentDate] = useState(new Date().toISOString().slice(0, 10));

    useEffect(() => {
        if (!loanId) return;
        let cancelled = false;
        setLoading(true);
        loansApi.get(loanId)
            .then((d) => { if (!cancelled) setLoan(d); })
            .catch((err) => {
                console.error(err);
                toast.error(t.finance.loans.messages.errorAction);
            })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [loanId, t.finance.loans.messages.errorAction, toast]);

    async function refresh() {
        const updated = await loansApi.get(loanId);
        setLoan(updated);
    }

    async function doAction(action: 'approve' | 'reject' | 'activate' | 'cancel') {
        if (!loan) return;
        setActing(true);
        try {
            if (action === 'approve') await loansApi.approve(loan.loanId);
            else if (action === 'reject') await loansApi.reject(loan.loanId);
            else if (action === 'activate') await loansApi.activate(loan.loanId);
            else await loansApi.cancel(loan.loanId);

            const messageKey =
                action === 'approve' ? t.finance.loans.messages.approved :
                action === 'reject' ? t.finance.loans.messages.rejected :
                action === 'activate' ? t.finance.loans.messages.activated :
                t.finance.loans.messages.cancelled;
            toast.success(messageKey);
            await refresh();
        } catch (err) {
            console.error(err);
            toast.error(t.finance.loans.messages.errorAction);
        } finally {
            setActing(false);
        }
    }

    async function recordRepayment() {
        if (!loan) return;
        const amount = Number(repaymentAmount);
        if (!amount || amount <= 0) {
            toast.error(t.finance.loans.messages.errorRepay);
            return;
        }
        setActing(true);
        try {
            await loansApi.recordRepayment(loan.loanId, {
                repaidAt: new Date(repaymentDate).toISOString(),
                amountLak: amount,
                notes: repaymentNotes || undefined,
            });
            toast.success(t.finance.loans.messages.repaid);
            setRepaymentAmount('');
            setRepaymentNotes('');
            await refresh();
        } catch (err) {
            console.error(err);
            toast.error(t.finance.loans.messages.errorRepay);
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
                    { label: t.finance.loans.title, href: '/finance/loans' },
                    { label: '…' },
                ]}
            />
        );
    }

    if (!loan) {
        return (
            <div className={styles.page}>
                <Card>
                    <ErrorState
                        title="Loan not found"
                        description="The loan you tried to view does not exist or has been deleted."
                    />
                </Card>
            </div>
        );
    }

    const statusLabel = (s: string) =>
        (t.finance.loans.status as Record<string, string>)[s.toLowerCase()] ?? s;

    const formatLak = (n: number) => new Intl.NumberFormat('en-US').format(n) + ' ₭';

    return (
        <div className={styles.page}>
            <PageHeader
                title={loan.purpose || loan.loanNumber}
                subtitle={loan.loanNumber}
                breadcrumbs={[
                    { label: t.nav.dashboard, href: '/' },
                    { label: t.finance.loans.title, href: '/finance/loans' },
                    { label: loan.loanNumber },
                ]}
                actions={
                    <div className={styles.headerActions}>
                        <span className={`${styles.badge} ${styles[`loan_${loan.status.toLowerCase()}`]}`}>
                            {statusLabel(loan.status)}
                        </span>
                        {loan.status === 'DRAFT' && (
                            <>
                                <Button variant="ghost" onClick={() => doAction('reject')} loading={acting}>
                                    {t.finance.loans.actions.reject}
                                </Button>
                                <Button onClick={() => doAction('approve')} loading={acting}>
                                    {t.finance.loans.actions.approve}
                                </Button>
                            </>
                        )}
                        {loan.status === 'APPROVED' && (
                            <Button onClick={() => doAction('activate')} loading={acting}>
                                {t.finance.loans.actions.activate}
                            </Button>
                        )}
                        {(loan.status === 'DRAFT' || loan.status === 'APPROVED') && (
                            <Button variant="ghost" onClick={() => doAction('cancel')} loading={acting}>
                                {t.finance.loans.actions.cancel}
                            </Button>
                        )}
                    </div>
                }
            />

            <div className={styles.detailLayout}>
                <div className={styles.detailMain}>
                    <Card>
                        <h3 className={styles.sectionTitle}>{t.finance.loans.repayments.title}</h3>
                        {loan.repayments.length === 0 ? (
                            <p className={styles.muted}>{t.finance.loans.repayments.empty}</p>
                        ) : (
                            <ul className={styles.repaymentList}>
                                {loan.repayments.map(r => (
                                    <li key={r.repaymentId} className={styles.repayment}>
                                        <span className={styles.repaymentDate}>
                                            {new Date(r.repaidAt).toLocaleDateString()}
                                        </span>
                                        <span className={styles.repaymentNotes}>{r.notes ?? '—'}</span>
                                        <span className={styles.repaymentAmount}>{formatLak(r.amountLak)}</span>
                                    </li>
                                ))}
                            </ul>
                        )}

                        {loan.status === 'ACTIVE' && (
                            <div className={styles.commentForm}>
                                <h4 style={{ margin: '1rem 0 0.5rem' }}>{t.finance.loans.actions.recordRepayment}</h4>
                                <div className={styles.filters}>
                                    <input
                                        type="date"
                                        value={repaymentDate}
                                        onChange={(e) => setRepaymentDate(e.target.value)}
                                        className={styles.searchInput}
                                        style={{ maxWidth: 180 }}
                                    />
                                    <input
                                        type="number"
                                        placeholder={t.finance.loans.repayments.amount}
                                        value={repaymentAmount}
                                        onChange={(e) => setRepaymentAmount(e.target.value)}
                                        className={styles.searchInput}
                                        style={{ maxWidth: 180 }}
                                    />
                                </div>
                                <textarea
                                    className={styles.textarea}
                                    placeholder={t.finance.loans.repayments.notes}
                                    value={repaymentNotes}
                                    onChange={(e) => setRepaymentNotes(e.target.value)}
                                    rows={2}
                                    style={{ marginTop: '0.5rem', width: '100%' }}
                                />
                                <Button
                                    onClick={recordRepayment}
                                    loading={acting}
                                    disabled={!repaymentAmount}
                                    style={{ marginTop: '0.5rem' }}
                                >
                                    {t.finance.loans.actions.recordRepayment}
                                </Button>
                            </div>
                        )}
                    </Card>
                </div>

                <div className={styles.detailSidebar}>
                    <Card>
                        <h3 className={styles.sectionTitle}>Details</h3>
                        <dl className={styles.dl}>
                            <dt>{t.finance.loans.fields.employee}</dt>
                            <dd>{loan.employeeName ?? '—'}</dd>
                            <dt>{t.finance.loans.fields.type}</dt>
                            <dd>{loan.loanType === 'ADVANCE' ? t.finance.loans.type.advance : t.finance.loans.type.loan}</dd>
                            <dt>{t.finance.loans.fields.principalLak}</dt>
                            <dd>{formatLak(loan.principalAmountLak)}</dd>
                            <dt>{t.finance.loans.fields.interestRate}</dt>
                            <dd>{loan.interestRate}%</dd>
                            <dt>{t.finance.loans.fields.installments}</dt>
                            <dd>{loan.installmentsPaid}/{loan.installments}</dd>
                            <dt>{t.finance.loans.fields.installmentAmount}</dt>
                            <dd>{formatLak(loan.installmentAmount)}</dd>
                            <dt>{t.finance.loans.fields.repaid}</dt>
                            <dd>{formatLak(loan.repaidAmount)}</dd>
                            <dt>{t.finance.loans.fields.remaining}</dt>
                            <dd>{formatLak(loan.remainingAmount)}</dd>
                            <dt>{t.finance.loans.fields.startDate}</dt>
                            <dd>{new Date(loan.startDate).toLocaleDateString()}</dd>
                            {loan.endDate && (
                                <>
                                    <dt>{t.finance.loans.fields.endDate}</dt>
                                    <dd>{new Date(loan.endDate).toLocaleDateString()}</dd>
                                </>
                            )}
                            <dt>{t.finance.loans.fields.approver}</dt>
                            <dd>{loan.approverName ?? '—'}</dd>
                        </dl>
                    </Card>
                </div>
            </div>
        </div>
    );
}
