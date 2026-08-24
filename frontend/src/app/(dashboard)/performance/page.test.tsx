import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '@/test/renderWithProviders';
import PerformancePage from '@/app/(dashboard)/performance/page';
import { performanceApi } from '@/lib/endpoints';

vi.mock('@/lib/endpoints', () => ({
    performanceApi: {
        getGoals: vi.fn(),
        getReviews: vi.fn(),
        getFeedback: vi.fn(),
        getOneOnOnes: vi.fn(),
    },
}));

describe('PerformancePage', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('renders performance stats and goals', async () => {
        (performanceApi.getGoals as ReturnType<typeof vi.fn>).mockResolvedValue([
            { goalId: 1, employeeId: 1, title: 'Close books', goalType: 'Individual', progressPercent: 50, status: 'ACTIVE', createdAt: '2026-01-01', updatedAt: '2026-01-01' },
        ]);
        (performanceApi.getReviews as ReturnType<typeof vi.fn>).mockResolvedValue([]);
        (performanceApi.getFeedback as ReturnType<typeof vi.fn>).mockResolvedValue([]);
        (performanceApi.getOneOnOnes as ReturnType<typeof vi.fn>).mockResolvedValue([]);

        renderWithProviders(<PerformancePage />);

        await waitFor(() => expect(screen.getByText('Close books')).toBeInTheDocument());
        expect(screen.getByText('Active Goals')).toBeInTheDocument();
    });

    it('renders error state on API failure', async () => {
        (performanceApi.getGoals as ReturnType<typeof vi.fn>).mockRejectedValue(new Error('fail'));
        (performanceApi.getReviews as ReturnType<typeof vi.fn>).mockRejectedValue(new Error('fail'));
        (performanceApi.getFeedback as ReturnType<typeof vi.fn>).mockRejectedValue(new Error('fail'));
        (performanceApi.getOneOnOnes as ReturnType<typeof vi.fn>).mockRejectedValue(new Error('fail'));

        renderWithProviders(<PerformancePage />);

        await waitFor(() => expect(screen.getByText(/An error occurred/i)).toBeInTheDocument());
    });
});
