/**
 * Approvals API Endpoints
 * Phase 3C2 — unified approval inbox (pending approvals, detail, approve/reject).
 */

import { apiClient } from '../apiClient';

export interface ApprovalInboxItem {
    approvalRequestId: number;
    requestType: string;
    entityId: number;
    requesterEmployeeId: number;
    requesterName?: string | null;
    status: string;
    createdAt: string;
}

export interface ApprovalStepDto {
    stepOrder: number;
    resolverType: string;
    approverEmployeeId?: number | null;
    status: string;
    actedAt?: string | null;
    comment?: string | null;
}

export interface ApprovalActionDto {
    actorEmployeeId: number;
    action: string;
    comment?: string | null;
    actedAt: string;
}

export interface ApprovalDetail {
    approvalRequestId: number;
    requestType: string;
    entityId: number;
    requesterEmployeeId: number;
    status: string;
    currentStepIndex: number;
    createdAt: string;
    completedAt?: string | null;
    steps: ApprovalStepDto[];
    history: ApprovalActionDto[];
}

export const approvalsApi = {
    /** Pending approvals where the current user is the resolved approver. */
    getMyPending: async (): Promise<ApprovalInboxItem[]> => {
        return apiClient.get<ApprovalInboxItem[]>('/api/approvals/my-pending');
    },

    /** Pending approval count (for navigation badge). */
    getMyPendingCount: async (): Promise<number> => {
        return apiClient.get<number>('/api/approvals/my-pending/count');
    },

    /** My submitted requests (requester view) — consolidated ESS "My Requests". */
    getMine: async (): Promise<ApprovalInboxItem[]> => {
        return apiClient.get<ApprovalInboxItem[]>('/api/approvals/mine');
    },

    /** Approval detail with steps and history. */
    getById: async (id: number): Promise<ApprovalDetail> => {
        return apiClient.get<ApprovalDetail>(`/api/approvals/${id}`);
    },

    /** Approve the current step. */
    approve: async (id: number, comment?: string): Promise<void> => {
        return apiClient.post<void>(`/api/approvals/${id}/approve`, { comment });
    },

    /** Reject the current step. */
    reject: async (id: number, comment?: string): Promise<void> => {
        return apiClient.post<void>(`/api/approvals/${id}/reject`, { comment });
    },
};

export default approvalsApi;
