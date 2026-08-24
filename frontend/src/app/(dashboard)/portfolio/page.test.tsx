import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '@/test/renderWithProviders';
import PortfolioPage from '@/app/(dashboard)/portfolio/page';
import { pmApi } from '@/lib/endpoints';

vi.mock('@/lib/endpoints', () => ({
    pmApi: {
        getPortfolio: vi.fn(),
        getCapacity: vi.fn(),
    },
}));

describe('PortfolioPage', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('renders portfolio rows', async () => {
        (pmApi.getPortfolio as ReturnType<typeof vi.fn>).mockResolvedValue([
            { projectId: 1, code: 'P1', name: 'Alpha', status: 'ACTIVE', priority: 'HIGH', progressPercent: 50, health: 'GREEN', openRiskCount: 0, overdueTaskCount: 0, openIssueCount: 0 },
        ]);

        renderWithProviders(<PortfolioPage />);

        await waitFor(() => expect(screen.getByText(/Alpha/)).toBeInTheDocument());
        expect(screen.getByText('50%')).toBeInTheDocument();
    });

    it('renders empty state', async () => {
        (pmApi.getPortfolio as ReturnType<typeof vi.fn>).mockResolvedValue([]);

        renderWithProviders(<PortfolioPage />);

        await waitFor(() => expect(screen.getByText(/No projects/i)).toBeInTheDocument());
    });

    it('renders error state on API failure', async () => {
        (pmApi.getPortfolio as ReturnType<typeof vi.fn>).mockRejectedValue(new Error('fail'));

        renderWithProviders(<PortfolioPage />);

        await waitFor(() => expect(screen.getByText(/An error occurred/i)).toBeInTheDocument());
    });
});
