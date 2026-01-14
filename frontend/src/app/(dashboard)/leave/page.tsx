'use client';

import { useEffect, useState, useCallback, useMemo } from 'react';
import { useAuth } from '@/components/providers/AuthProvider';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Skeleton } from '@/components/ui/Skeleton';
import { LeaveRequestForm } from '@/components/forms/LeaveRequestForm';
import { LeaveCalendar } from '@/components/leave/LeaveCalendar';
import { leaveApi } from '@/lib/endpoints';
import { formatDate, formatRelativeTime } from '@/lib/datetime';
import { isHROrAdmin } from '@/lib/permissions';
import type { LeaveRequest, CreateLeaveRequest } from '@/lib/types';
import styles from './page.module.css';

/**
 * Leave Management Page
 * Shows leave requests, balances, and approval workflow
 */
export default function LeavePage() {
    const { role } = useAuth();
    const { t } = useLanguage();
    const [loading, setLoading] = useState(true);
    const [activeTab, setActiveTab] = useState<'my-leave' | 'approvals' | 'calendar'>('my-leave');
    const [leaveRequests, setLeaveRequests] = useState<LeaveRequest[]>([]);
    const [showRequestModal, setShowRequestModal] = useState(false);
    const [actionLoading, setActionLoading] = useState<number | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [exporting, setExporting] = useState(false);

    const canApprove = isHROrAdmin(role);

    // Leave quotas per person
    const LEAVE_QUOTAS = {
        ANNUAL: 15,
        SICK: 30,
        PERSONAL: 3,
    };

    // Calculate leave balances from loaded leave requests (per person)
    const leaveBalances = useMemo(() => {
        const currentYear = new Date().getFullYear();

        // Filter to approved leaves in current year (used days)
        const approvedLeaves = leaveRequests.filter(
            (r) => r.status === 'APPROVED' && new Date(r.startDate).getFullYear() === currentYear
        );

        const usedAnnual = approvedLeaves.filter(r => r.leaveType === 'ANNUAL').reduce((sum, r) => sum + r.totalDays, 0);
        const usedSick = approvedLeaves.filter(r => r.leaveType === 'SICK').reduce((sum, r) => sum + r.totalDays, 0);
        const usedPersonal = approvedLeaves.filter(r => r.leaveType === 'PERSONAL').reduce((sum, r) => sum + r.totalDays, 0);

        return {
            annual: { used: usedAnnual, total: LEAVE_QUOTAS.ANNUAL },
            sick: { used: usedSick, total: LEAVE_QUOTAS.SICK },
            personal: { used: usedPersonal, total: LEAVE_QUOTAS.PERSONAL },
        };
    }, [leaveRequests]);

    const loadLeaveRequests = useCallback(async () => {
        try {
            setError(null);
            const data = await leaveApi.getAll();
            setLeaveRequests(data);
        } catch (err) {
            console.error('Failed to load leave requests:', err);
            setError(t.common.error);
        } finally {
            setLoading(false);
        }
    }, [t.common.error]);

    useEffect(() => {
        loadLeaveRequests();
    }, [loadLeaveRequests]);

    const handleCreateRequest = async (request: CreateLeaveRequest) => {
        await leaveApi.create(request);
        await loadLeaveRequests();
    };

    const handleApprove = async (leaveId: number) => {
        setActionLoading(leaveId);
        try {
            await leaveApi.approve(leaveId);
            await loadLeaveRequests();
        } catch (err) {
            console.error('Failed to approve leave:', err);
            setError(t.common.error);
        } finally {
            setActionLoading(null);
        }
    };

    const handleReject = async (leaveId: number) => {
        setActionLoading(leaveId);
        try {
            await leaveApi.reject(leaveId);
            await loadLeaveRequests();
        } catch (err) {
            console.error('Failed to reject leave:', err);
            setError(t.common.error);
        } finally {
            setActionLoading(null);
        }
    };

    const handleExport = async () => {
        setExporting(true);
        try {
            const blob = await leaveApi.exportHistory();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `LeaveHistory_${new Date().getFullYear()}.xlsx`;
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(url);
            a.remove();
        } catch (err) {
            console.error('Export failed:', err);
            setError(t.common.error);
        } finally {
            setExporting(false);
        }
    };

    // Filter requests - for now show all for HR, own for employees
    // In production, backend should filter by current user's employee ID
    const myRequests = leaveRequests.filter((r) =>
        !canApprove || r.status !== 'PENDING' || activeTab === 'my-leave'
    );
    const pendingApprovals = leaveRequests.filter((r) => r.status === 'PENDING');

    const getLeaveStatusLabel = (status: string) => {
        const key = status.toLowerCase() as keyof typeof t.leave.status;
        return t.leave.status[key] || status;
    };

    const getLeaveTypeLabel = (type: string) => {
        const key = type.toLowerCase() as keyof typeof t.leave.form.types;
        return t.leave.form.types[key] || type;
    };

    return (
        <div className={styles.page}>
            {/* Header */}
            <div className={styles.header}>
                <div>
                    <h1 className={styles.title}>{t.leave.title}</h1>
                    <p className={styles.subtitle}>
                        {t.leave.subtitle}
                    </p>
                </div>
                <div className={styles.headerActions}>
                    <Button variant="secondary" onClick={handleExport} loading={exporting}>
                        📥 {t.leave.export || 'Export'}
                    </Button>
                    <Button leftIcon={<PlusIcon />} onClick={() => setShowRequestModal(true)}>
                        {t.leave.requestLeave}
                    </Button>
                </div>
            </div>

            {/* Error Alert */}
            {error && (
                <div className={styles.errorAlert}>
                    {error}
                    <button onClick={() => setError(null)}>×</button>
                </div>
            )}

            {/* Leave Balances */}
            <div className={styles.balanceGrid}>
                <BalanceCard
                    type={t.leave.balances.annual}
                    used={leaveBalances.annual.used}
                    total={leaveBalances.annual.total}
                    color="primary"
                    t={t}
                />
                <BalanceCard
                    type={t.leave.balances.sick}
                    used={leaveBalances.sick.used}
                    total={leaveBalances.sick.total}
                    color="warning"
                    t={t}
                />
                <BalanceCard
                    type={t.leave.balances.personal}
                    used={leaveBalances.personal.used}
                    total={leaveBalances.personal.total}
                    color="accent"
                    t={t}
                />
            </div>

            {/* Tabs */}
            <div className={styles.tabs}>
                <button
                    className={`${styles.tab} ${activeTab === 'my-leave' ? styles.activeTab : ''}`}
                    onClick={() => setActiveTab('my-leave')}
                >
                    {t.leave.tabs.myLeave}
                </button>
                {canApprove && (
                    <button
                        className={`${styles.tab} ${activeTab === 'approvals' ? styles.activeTab : ''}`}
                        onClick={() => setActiveTab('approvals')}
                    >
                        {t.leave.tabs.approvals}
                        {pendingApprovals.length > 0 && (
                            <span className={styles.badge}>{pendingApprovals.length}</span>
                        )}
                    </button>
                )}
                <button
                    className={`${styles.tab} ${activeTab === 'calendar' ? styles.activeTab : ''}`}
                    onClick={() => setActiveTab('calendar')}
                >
                    📅 {t.leave.tabs.calendar || 'Calendar'}
                </button>
            </div>

            {/* Calendar View */}
            {activeTab === 'calendar' && (
                <Card>
                    <LeaveCalendar />
                </Card>
            )}

            {/* Leave Requests */}
            {activeTab !== 'calendar' && (
                <Card noPadding>
                    {loading ? (
                        <div className={styles.loading}>
                            <Skeleton height={200} />
                        </div>
                    ) : (
                        <div className={styles.requestList}>
                            {(activeTab === 'my-leave' ? leaveRequests : pendingApprovals).length === 0 ? (
                                <div className={styles.emptyState}>
                                    <CalendarIcon />
                                    <p>{activeTab === 'my-leave' ? t.leave.requests.empty : t.leave.requests.emptyApprovals}</p>
                                </div>
                            ) : (
                                (activeTab === 'my-leave' ? leaveRequests : pendingApprovals).map((request) => (
                                    <div key={request.leaveId} className={styles.requestCard}>
                                        <div className={styles.requestHeader}>
                                            <div className={styles.requestType}>
                                                <span className={`${styles.typeTag} ${styles[request.leaveType.toLowerCase()]}`}>
                                                    {getLeaveTypeLabel(request.leaveType)}
                                                </span>
                                                <span className={styles.requestDays}>
                                                    {request.totalDays} {request.totalDays === 1 ? t.leave.requests.day : t.leave.requests.days}
                                                </span>
                                            </div>
                                            <span className={`${styles.statusTag} ${styles[request.status.toLowerCase()]}`}>
                                                {getLeaveStatusLabel(request.status)}
                                            </span>
                                        </div>

                                        <div className={styles.requestDates}>
                                            <CalendarSmallIcon />
                                            {formatDate(request.startDate)}
                                            {request.startDate !== request.endDate && (
                                                <> → {formatDate(request.endDate)}</>
                                            )}
                                        </div>

                                        {request.reason && (
                                            <p className={styles.requestReason}>{request.reason}</p>
                                        )}

                                        <div className={styles.requestFooter}>
                                            {request.employee && (
                                                <span className={styles.requestEmployee}>
                                                    {request.employee.englishName || request.employee.laoName}
                                                </span>
                                            )}
                                            <span className={styles.requestTime}>
                                                {formatRelativeTime(request.createdAt)}
                                            </span>
                                        </div>

                                        {canApprove && request.status === 'PENDING' && (
                                            <div className={styles.requestActions}>
                                                <Button
                                                    size="sm"
                                                    variant="secondary"
                                                    onClick={() => handleReject(request.leaveId)}
                                                    loading={actionLoading === request.leaveId}
                                                    disabled={actionLoading !== null}
                                                >
                                                    {t.leave.requests.reject}
                                                </Button>
                                                <Button
                                                    size="sm"
                                                    onClick={() => handleApprove(request.leaveId)}
                                                    loading={actionLoading === request.leaveId}
                                                    disabled={actionLoading !== null}
                                                >
                                                    {t.leave.requests.approve}
                                                </Button>
                                            </div>
                                        )}
                                    </div>
                                ))
                            )}
                        </div>
                    )}
                </Card>
            )}

            {/* Request Leave Modal */}
            <LeaveRequestForm
                isOpen={showRequestModal}
                onClose={() => setShowRequestModal(false)}
                onSubmit={handleCreateRequest}
            />
        </div>
    );
}

// Balance Card Component
function BalanceCard({
    type,
    used,
    total,
    color,
    t,
}: {
    type: string;
    used: number;
    total: number;
    color: 'primary' | 'warning' | 'accent';
    t: any;
}) {
    const remaining = total - used;
    const percentage = (used / total) * 100;

    return (
        <Card className={styles.balanceCard}>
            <div className={styles.balanceContent}>
                <span className={styles.balanceType}>{type}</span>
                <div className={styles.balanceNumbers}>
                    <span className={styles.balanceRemaining}>{remaining}</span>
                    <span className={styles.balanceTotal}>/ {total} {t.leave.balances.days}</span>
                </div>
                <div className={styles.progressBar}>
                    <div
                        className={`${styles.progressFill} ${styles[color]}`}
                        style={{ width: `${percentage}%` }}
                    />
                </div>
                <span className={styles.balanceUsed}>{used} {t.leave.balances.used}</span>
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

function CalendarIcon() {
    return (
        <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5">
            <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
            <line x1="16" y1="2" x2="16" y2="6" />
            <line x1="8" y1="2" x2="8" y2="6" />
            <line x1="3" y1="10" x2="21" y2="10" />
        </svg>
    );
}

function CalendarSmallIcon() {
    return (
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
            <line x1="16" y1="2" x2="16" y2="6" />
            <line x1="8" y1="2" x2="8" y2="6" />
            <line x1="3" y1="10" x2="21" y2="10" />
        </svg>
    );
}
