import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '@/test/renderWithProviders';
import MyTeamPage from '@/app/(dashboard)/my-team/page';
import { organizationApi, teamApi } from '@/lib/endpoints';

vi.mock('@/lib/endpoints', () => ({
    organizationApi: { getMyTeam: vi.fn() },
    teamApi: { getTeamLeave: vi.fn(), getTeamAttendance: vi.fn() },
}));

describe('MyTeamPage', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('renders direct reports', async () => {
        (organizationApi.getMyTeam as ReturnType<typeof vi.fn>).mockResolvedValue([
            { employeeId: 2, employeeCode: 'EMP002', laoName: 'Davanh', englishName: 'Davanh', jobTitle: 'HR Officer', isActive: true },
        ]);

        renderWithProviders(<MyTeamPage />);

        await waitFor(() => expect(screen.getByText('Davanh')).toBeInTheDocument());
    });

    it('renders empty team state', async () => {
        (organizationApi.getMyTeam as ReturnType<typeof vi.fn>).mockResolvedValue([]);

        renderWithProviders(<MyTeamPage />);

        await waitFor(() => expect(screen.getByText(/no direct reports/i)).toBeInTheDocument());
    });

    it('renders error state on API failure', async () => {
        (organizationApi.getMyTeam as ReturnType<typeof vi.fn>).mockRejectedValue(new Error('fail'));

        renderWithProviders(<MyTeamPage />);

        await waitFor(() => expect(screen.getByText(/Failed to load team data/i)).toBeInTheDocument());
    });
});
