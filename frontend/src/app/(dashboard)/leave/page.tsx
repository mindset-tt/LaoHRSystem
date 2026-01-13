'use client';

import { useEffect, useState, useCallback } from 'react';
import { useAuth } from '@/components/providers/AuthProvider';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Skeleton } from '@/components/ui/Skeleton';
import { LeaveRequestForm } from '@/components/forms/LeaveRequestForm';
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
    const [loading, setLoading] = useState(true);
    const [activeTab, setActiveTab] = useState<'my-leave' | 'approvals'>('my-leave');
    const [leaveRequests, setLeaveRequests] = useState<LeaveRequest[]>([]);
    const [showRequestModal, setShowRequestModal] = useState(false);
    const [actionLoading, setActionLoading] = useState<number | null>(null);
    const [error, setError] = useState<string | null>(null);

    const canApprove = isHROrAdmin(role);

    // Leave balances (could be fetched from API if endpoint exists)
    const leaveBalances = {
        annual: { used: 5, total: 15 },
        sick: { used: 2, total: 10 },
        personal: { used: 1, total: 3 },
    };

    const loadLeaveRequests = useCallback(async () => {
        try {
            setError(null);
            const data = await leaveApi.getAll();
            setLeaveRequests(data);
        } catch (err) {
            console.error('Failed to load leave requests:', err);
            setError('Failed to load leave requests. Please try again.');
        } finally {
            setLoading(false);
        }
    }, []);

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
            setError('Failed to approve leave request');
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
            setError('Failed to reject leave request');
        } finally {
            setActionLoading(null);
        }
    };

    // Filter requests - for now show all for HR, own for employees
    // In production, backend should filter by current user's employee ID
    const myRequests = leaveRequests.filter((r) =>
        !canApprove || r.status !== 'PENDING' || activeTab === 'my-leave'
    );
    const pendingApprovals = leaveRequests.filter((r) => r.status === 'PENDING');

    return (
        <div className={styles.page}>
            {/* Header */}
            <div className={styles.header}>
                <div>
                    <h1 className={styles.title}>Leave Management</h1>
                    <p className={styles.subtitle}>
                        Request time off and manage leave approvals
                    </p>
                </div>
                <Button leftIcon={<PlusIcon />} onClick={() => setShowRequestModal(true)}>
                    Request Leave
                </Button>
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
                    type="Annual Leave"
                    used={leaveBalances.annual.used}
                    total={leaveBalances.annual.total}
                    color="primary"
                />
                <BalanceCard
                    type="Sick Leave"
                    used={leaveBalances.sick.used}
                    total={leaveBalances.sick.total}
                    color="warning"
                />
                <BalanceCard
                    type="Personal Leave"
                    used={leaveBalances.personal.used}
                    total={leaveBalances.personal.total}
                    color="accent"
                />
            </div>

            {/* Tabs */}
            {canApprove && (
                <div className={styles.tabs}>
                    <button
                        className={`${styles.tab} ${activeTab === 'my-leave' ? styles.activeTab : ''}`}
                        onClick={() => setActiveTab('my-leave')}
                    >
                        All Requests
                    </button>
                    <button
                        className={`${styles.tab} ${activeTab === 'approvals' ? styles.activeTab : ''}`}
                        onClick={() => setActiveTab('approvals')}
                    >
                        Pending Approvals
                        {pendingApprovals.length > 0 && (
                            <span className={styles.badge}>{pendingApprovals.length}</span>
                        )}
                    </button>
                </div>
            )}

            {/* Leave Requests */}
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
                                <p>No {activeTab === 'my-leave' ? 'leave requests' : 'pending approvals'}</p>
                            </div>
                        ) : (
                            (activeTab === 'my-leave' ? leaveRequests : pendingApprovals).map((request) => (
                                <div key={request.leaveId} className={styles.requestCard}>
                                    <div className={styles.requestHeader}>
                                        <div className={styles.requestType}>
                                            <span className={`${styles.typeTag} ${styles[request.leaveType.toLowerCase()]}`}>
                                                {request.leaveType}
                                            </span>
                                            <span className={styles.requestDays}>{request.totalDays} day(s)</span>
                                        </div>
                                        <span className={`${styles.statusTag} ${styles[request.status.toLowerCase()]}`}>
                                            {request.status}
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
                                                Reject
                                            </Button>
                                            <Button
                                                size="sm"
                                                onClick={() => handleApprove(request.leaveId)}
                                                loading={actionLoading === request.leaveId}
                                                disabled={actionLoading !== null}
                                            >
                                                Approve
                                            </Button>
                                        </div>
                                    )}
                                </div>
                            ))
                        )}
                    </div>
                )}
            </Card>

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
}: {
    type: string;
    used: number;
    total: number;
    color: 'primary' | 'warning' | 'accent';
}) {
    const remaining = total - used;
    const percentage = (used / total) * 100;

    return (
        <Card className={styles.balanceCard}>
            <div className={styles.balanceContent}>
                <span className={styles.balanceType}>{type}</span>
                <div className={styles.balanceNumbers}>
                    <span className={styles.balanceRemaining}>{remaining}</span>
                    <span className={styles.balanceTotal}>/ {total} days</span>
                </div>
                <div className={styles.progressBar}>
                    <div
                        className={`${styles.progressFill} ${styles[color]}`}
                        style={{ width: `${percentage}%` }}
                    />
                </div>
                <span className={styles.balanceUsed}>{used} days used</span>
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
