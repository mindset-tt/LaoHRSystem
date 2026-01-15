'use client';

import { useEffect, useState, useCallback } from 'react';
import { useAuth } from '@/components/providers/AuthProvider';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card, CardTitle } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Skeleton, SkeletonTable } from '@/components/ui/Skeleton';
import { MaskedField } from '@/components/ui/MaskedField';
import { NewPeriodModal } from '@/components/forms/NewPeriodModal';
import { payrollApi, reportsApi } from '@/lib/endpoints';
import { formatPayrollPeriod } from '@/lib/datetime';
import { isHROrAdmin } from '@/lib/permissions';
import type { PayrollPeriod, SalarySlip } from '@/lib/types';
import styles from './page.module.css';

/**
 * Payroll Page
 * Manage payroll periods, run calculations, and view salary slips
 */
export default function PayrollPage() {
    const { role } = useAuth();
    const { t } = useLanguage();
    const [loading, setLoading] = useState(true);
    const [periods, setPeriods] = useState<PayrollPeriod[]>([]);
    const [selectedPeriod, setSelectedPeriod] = useState<PayrollPeriod | null>(null);
    const [slips, setSlips] = useState<SalarySlip[]>([]);
    const [actionLoading, setActionLoading] = useState(false);
    const [showNewPeriodModal, setShowNewPeriodModal] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const canManage = isHROrAdmin(role);

    const loadPeriods = useCallback(async () => {
        try {
            const data = await payrollApi.getPeriods();
            setPeriods(data);
            if (!selectedPeriod && data.length > 0) {
                setSelectedPeriod(data[0]);
            }
        } catch (err) {
            console.error('Failed to load payroll periods:', err);
            setError('Failed to load payroll periods');
        } finally {
            setLoading(false);
        }
    }, [selectedPeriod]);

    useEffect(() => {
        loadPeriods();
    }, [loadPeriods]);

    useEffect(() => {
        const loadSlips = async () => {
            if (!selectedPeriod) return;
            try {
                const data = await payrollApi.getSlips(selectedPeriod.periodId);
                setSlips(data);
            } catch (err) {
                console.error('Failed to load slips:', err);
                setError('Failed to load salary slips');
            }
        };

        loadSlips();
    }, [selectedPeriod]);

    const handleCreatePeriod = async (year: number, month: number) => {
        try {
            await payrollApi.createPeriod({ year, month });
            await loadPeriods();
            setShowNewPeriodModal(false);
        } catch (err) {
            console.error('Failed to create period:', err);
            throw err;
        }
    };

    const handleRunPayroll = async () => {
        if (!selectedPeriod) return;
        setActionLoading(true);
        try {
            setError(null);
            await payrollApi.runPayroll(selectedPeriod.periodId);
            await Promise.all([
                payrollApi.getSlips(selectedPeriod.periodId).then(setSlips),
                loadPeriods()
            ]);
        } catch (err) {
            console.error('Failed to run payroll:', err);
            setError(err instanceof Error ? err.message : 'Failed to run payroll');
        } finally {
            setActionLoading(false);
        }
    };

    const handleDownloadPdf = async (slipId: number) => {
        try {
            const blob = await payrollApi.downloadPdf(slipId);
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `payslip-${slipId}.pdf`;
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(url);
            document.body.removeChild(a);
        } catch (err) {
            console.error('Failed to download PDF:', err);
            setError('Failed to download payslip PDF');
        }
    };

    const handleDownloadAll = async () => {
        if (!selectedPeriod) return;
        try {
            const blob = await payrollApi.exportPayroll(selectedPeriod.periodId);
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `payroll-export-${selectedPeriod.periodId}.xlsx`;
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(url);
            document.body.removeChild(a);
        } catch (err) {
            console.error('Failed to export payroll:', err);
            setError('Failed to export payroll data');
        }
    };

    const formatCurrency = (amount: number, currency: string = 'LAK') => {
        // Handle null/undefined/empty currency by defaulting to LAK
        const code = currency || 'LAK';

        const formatter = new Intl.NumberFormat('en-US', {
            style: 'decimal',
            minimumFractionDigits: code === 'LAK' ? 0 : 2,
            maximumFractionDigits: code === 'LAK' ? 0 : 2,
        });

        let symbol = '₭';
        if (code === 'USD') symbol = '$';
        else if (code === 'THB') symbol = '฿';
        else if (code === 'CNY') symbol = '¥';
        else if (code !== 'LAK') symbol = code; // Show code if unknown generic

        return `${symbol} ${formatter.format(amount)}`;
    };

    return (
        <div className={styles.page}>
            {/* ... keeping previous sections ... */}
            <div className={styles.header}>
                <div>
                    <h1 className={styles.title}>{t.payroll.title}</h1>
                    <p className={styles.subtitle}>
                        {t.payroll.subtitle}
                    </p>
                </div>
                {canManage && (
                    <div className={styles.headerActions}>
                        {selectedPeriod?.status === 'DRAFT' && slips.length > 0 && (
                            <Button
                                variant="secondary"
                                leftIcon={<PlayIcon />}
                                onClick={handleRunPayroll}
                                loading={actionLoading}
                            >
                                {t.payroll.runPayroll}
                            </Button>
                        )}
                        <Button leftIcon={<PlusIcon />} onClick={() => setShowNewPeriodModal(true)}>
                            {t.payroll.newPeriod}
                        </Button>
                    </div>
                )}
            </div>

            {/* ... Error, Period Selector, Summary moved to ... */}

            {/* (Hiding lines 136-237 for brevity as they are unchanged) */}

            {selectedPeriod && (
                <Card noPadding>
                    <div className={styles.tableHeader}>
                        <CardTitle>{t.payroll.table.title}</CardTitle>
                        {slips.length > 0 && (
                            <Button
                                variant="secondary"
                                size="sm"
                                leftIcon={<DownloadIcon />}
                                onClick={handleDownloadAll}
                            >
                                {t.payroll.table.exportAll}
                            </Button>
                        )}
                    </div>

                    {loading ? (
                        <div className={styles.loading}>
                            <SkeletonTable rows={5} columns={5} />
                        </div>
                    ) : slips.length === 0 ? (
                        <div className={styles.emptyState}>
                            <WalletIcon />
                            <p>{t.payroll.table.empty}</p>
                            {selectedPeriod.status === 'DRAFT' && (
                                <div className={styles.emptyStateActions}>
                                    <p>{t.payroll.table.runToGenerate}</p>
                                    <Button
                                        onClick={handleRunPayroll}
                                        loading={actionLoading}
                                        leftIcon={<PlayIcon />}
                                    >
                                        {t.payroll.runPayroll}
                                    </Button>
                                </div>
                            )}
                        </div>
                    ) : (
                        <div className={styles.tableWrapper}>
                            <table className={styles.table}>
                                <thead>
                                    <tr>
                                        <th>{t.payroll.table.headers.employee}</th>
                                        <th>Contract</th>
                                        <th>{t.payroll.table.headers.gross} (LAK)</th>
                                        <th>{t.payroll.table.headers.deductions} (LAK)</th>
                                        <th>{t.payroll.table.headers.net}</th>
                                        <th>{t.payroll.table.headers.status}</th>
                                        <th></th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {slips.map((slip) => (
                                        <tr key={slip.slipId}>
                                            <td>
                                                <div className={styles.employeeCell}>
                                                    <span className={styles.employeeName}>
                                                        {slip.employee?.englishName || slip.employee?.laoName}
                                                    </span>
                                                    <span className={styles.employeeCode}>
                                                        {slip.employee?.employeeCode}
                                                    </span>
                                                </div>
                                            </td>
                                            <td>
                                                {slip.contractCurrency && slip.contractCurrency !== 'LAK' ? (
                                                    <span title={`Rate: ${slip.exchangeRateUsed}`}>
                                                        {formatCurrency(slip.baseSalaryOriginal, slip.contractCurrency)}
                                                    </span>
                                                ) : (
                                                    <span className={styles.textMuted}>LAK</span>
                                                )}
                                            </td>
                                            <td>
                                                <MaskedField value={formatCurrency(slip.grossIncome)} visibleChars={8} />
                                            </td>
                                            <td>
                                                <MaskedField
                                                    value={formatCurrency(slip.nssfEmployeeDeduction + slip.taxDeduction)}
                                                    visibleChars={8}
                                                />
                                            </td>
                                            <td>
                                                <div className={styles.netSalaryCell}>
                                                    <MaskedField value={formatCurrency(slip.netSalary)} visibleChars={8} />
                                                    {slip.paymentCurrency && slip.paymentCurrency !== 'LAK' && (
                                                        <div className={styles.subText}>
                                                            {formatCurrency(slip.netSalaryOriginal, slip.paymentCurrency)}
                                                        </div>
                                                    )}
                                                </div>
                                            </td>
                                            <td>
                                                <span className={`${styles.status} ${styles[slip.status.toLowerCase()]}`}>
                                                    {slip.status}
                                                </span>
                                            </td>
                                            <td>
                                                <button
                                                    className={styles.viewBtn}
                                                    onClick={() => handleDownloadPdf(slip.slipId)}
                                                >
                                                    <DownloadIcon />
                                                    {t.payroll.table.viewPdf}
                                                </button>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    )}
                </Card>
            )}

            {/* New Period Modal */}
            <NewPeriodModal
                isOpen={showNewPeriodModal}
                onClose={() => setShowNewPeriodModal(false)}
                onSubmit={handleCreatePeriod}
            />
        </div>
    );
}

// Summary Card Component
function SummaryCard({
    label,
    value,
    icon,
}: {
    label: string;
    value: string;
    icon: React.ReactNode;
}) {
    return (
        <Card className={styles.summaryCard}>
            <div className={styles.summaryContent}>
                <div className={styles.summaryIcon}>{icon}</div>
                <div className={styles.summaryInfo}>
                    <span className={styles.summaryLabel}>{label}</span>
                    <span className={styles.summaryValue}>{value}</span>
                </div>
            </div>
        </Card>
    );
}

// Icons
function PlusIcon() {
    return (
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <line x1="12" y1="5" x2="12" y2="19" />
            <line x1="5" y1="12" x2="19" y2="12" />
        </svg>
    );
}

function PlayIcon() {
    return (
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <polygon points="5 3 19 12 5 21 5 3" />
        </svg>
    );
}

function DownloadIcon() {
    return (
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
            <polyline points="7 10 12 15 17 10" />
            <line x1="12" y1="15" x2="12" y2="3" />
        </svg>
    );
}

function DollarIcon() {
    return (
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <line x1="12" y1="1" x2="12" y2="23" />
            <path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6" />
        </svg>
    );
}

function MinusCircleIcon() {
    return (
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <circle cx="12" cy="12" r="10" />
            <line x1="8" y1="12" x2="16" y2="12" />
        </svg>
    );
}

function WalletIcon() {
    return (
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <path d="M21 12V7H5a2 2 0 0 1 0-4h14v4" />
            <path d="M3 5v14a2 2 0 0 0 2 2h16v-5" />
            <path d="M18 12a2 2 0 0 0 0 4h4v-4h-4z" />
        </svg>
    );
}

function UsersIcon() {
    return (
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
            <circle cx="9" cy="7" r="4" />
            <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
            <path d="M16 3.13a4 4 0 0 1 0 7.75" />
        </svg>
    );
}
