import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '@/test/renderWithProviders';
import MyApprovalsPage from '@/app/(dashboard)/my-approvals/page';
import { approvalsApi } from '@/lib/endpoints';

vi.mock('@/lib/endpoints', () => ({
    approvalsApi: {
        getMyPending: vi.fn(),
        approve: vi.fn(),
        reject: vi.fn(),
    },
}));

describe('MyApprovalsPage', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('renders pending requests', async () => {
        (approvalsApi.getMyPending as ReturnType<typeof vi.fn>).mockResolvedValue([
            { approvalRequestId: 1, requestType: 'LEAVE', entityId: 10, requesterEmployeeId: 2, requesterName: 'Somphon', status: 'PENDING', createdAt: '2026-01-01' },
        ]);

        renderWithProviders(<MyApprovalsPage />);

        await waitFor(() => expect(screen.getByText('LEAVE')).toBeInTheDocument());
        expect(screen.getByText('Somphon')).toBeInTheDocument();
    });

    it('renders empty state when no pending approvals', async () => {
        (approvalsApi.getMyPending as ReturnType<typeof vi.fn>).mockResolvedValue([]);

        renderWithProviders(<MyApprovalsPage />);

        await waitFor(() => expect(screen.getByText(/No pending approvals/i)).toBeInTheDocument());
    });

    it('renders error state on API failure', async () => {
        (approvalsApi.getMyPending as ReturnType<typeof vi.fn>).mockRejectedValue(new Error('fail'));

        renderWithProviders(<MyApprovalsPage />);

        await waitFor(() => expect(screen.getByText(/An error occurred/i)).toBeInTheDocument());
    });
});
