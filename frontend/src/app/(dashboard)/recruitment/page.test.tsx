import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '@/test/renderWithProviders';
import RecruitmentPage from '@/app/(dashboard)/recruitment/page';
import { recruitmentApi } from '@/lib/endpoints';

vi.mock('@/lib/endpoints', () => ({
    recruitmentApi: {
        getRequisitions: vi.fn(),
        getOpenings: vi.fn(),
        getCandidates: vi.fn(),
        getOffers: vi.fn(),
    },
}));

describe('RecruitmentPage', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('renders recruitment stats', async () => {
        (recruitmentApi.getRequisitions as ReturnType<typeof vi.fn>).mockResolvedValue([
            { requisitionId: 1, requisitionNumber: 'REQ-0001', positionId: 1, requestedByEmployeeId: 1, headcount: 1, reason: 'Growth', priority: 'MEDIUM', status: 'APPROVED', createdAt: '2026-01-01', updatedAt: '2026-01-01' },
        ]);
        (recruitmentApi.getOpenings as ReturnType<typeof vi.fn>).mockResolvedValue([]);
        (recruitmentApi.getCandidates as ReturnType<typeof vi.fn>).mockResolvedValue([]);
        (recruitmentApi.getOffers as ReturnType<typeof vi.fn>).mockResolvedValue([]);

        renderWithProviders(<RecruitmentPage />);

        await waitFor(() => expect(screen.getByText('REQ-0001')).toBeInTheDocument());
        expect(screen.getAllByText('Open Requisitions').length).toBeGreaterThan(0);
    });

    it('renders error state on API failure', async () => {
        (recruitmentApi.getRequisitions as ReturnType<typeof vi.fn>).mockRejectedValue(new Error('fail'));
        (recruitmentApi.getOpenings as ReturnType<typeof vi.fn>).mockRejectedValue(new Error('fail'));
        (recruitmentApi.getCandidates as ReturnType<typeof vi.fn>).mockRejectedValue(new Error('fail'));
        (recruitmentApi.getOffers as ReturnType<typeof vi.fn>).mockRejectedValue(new Error('fail'));

        renderWithProviders(<RecruitmentPage />);

        await waitFor(() => expect(screen.getByText(/An error occurred/i)).toBeInTheDocument());
    });
});
