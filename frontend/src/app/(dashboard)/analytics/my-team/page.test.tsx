import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '@/test/renderWithProviders';
import ManagerDashboardPage from '@/app/(dashboard)/analytics/my-team/page';
import { analyticsApi } from '@/lib/endpoints';

vi.mock('@/lib/endpoints', () => ({
    analyticsApi: {
        getExecutive: vi.fn(),
        getHr: vi.fn(),
        getManager: vi.fn(),
    },
}));

describe('ManagerDashboardPage', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('renders team KPIs', async () => {
        (analyticsApi.getManager as ReturnType<typeof vi.fn>).mockResolvedValue({
            kpis: [
                { id: 'MSS-DIRECT-REPORTS', label: 'Direct Reports', value: 3, unit: 'people' },
            ],
            teamLeaveByStatus: [],
            teamAttendanceTrend: [],
        });

        renderWithProviders(<ManagerDashboardPage />);

        await waitFor(() => expect(screen.getByText('Direct Reports')).toBeInTheDocument());
        expect(screen.getByText('3')).toBeInTheDocument();
    });

    it('renders error state on API failure', async () => {
        (analyticsApi.getManager as ReturnType<typeof vi.fn>).mockRejectedValue(new Error('fail'));

        renderWithProviders(<ManagerDashboardPage />);

        await waitFor(() => expect(screen.getByText(/An error occurred/i)).toBeInTheDocument());
    });
});
