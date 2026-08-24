import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '@/test/renderWithProviders';
import ExecutiveDashboardPage from '@/app/(dashboard)/analytics/executive/page';
import { analyticsApi } from '@/lib/endpoints';

vi.mock('@/lib/endpoints', () => ({
    analyticsApi: {
        getExecutive: vi.fn(),
        getHr: vi.fn(),
        getManager: vi.fn(),
    },
}));

describe('ExecutiveDashboardPage', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('renders KPI cards', async () => {
        (analyticsApi.getExecutive as ReturnType<typeof vi.fn>).mockResolvedValue({
            kpis: [
                { id: 'HR-HEADCOUNT-ACTIVE', label: 'Active Headcount', value: 42, unit: 'people' },
            ],
            attention: [],
            headcountByDepartment: [],
            headcountTrend: [],
            leaveTrend: [],
            expenseTrend: [],
        });

        renderWithProviders(<ExecutiveDashboardPage />);

        await waitFor(() => expect(screen.getByText('Active Headcount')).toBeInTheDocument());
        expect(screen.getByText('42')).toBeInTheDocument();
    });

    it('renders attention items', async () => {
        (analyticsApi.getExecutive as ReturnType<typeof vi.fn>).mockResolvedValue({
            kpis: [],
            attention: [{ type: 'APPROVAL', title: 'Pending approvals', count: 5 }],
            headcountByDepartment: [],
            headcountTrend: [],
            leaveTrend: [],
            expenseTrend: [],
        });

        renderWithProviders(<ExecutiveDashboardPage />);

        await waitFor(() => expect(screen.getByText('Pending approvals')).toBeInTheDocument());
        expect(screen.getByText('5')).toBeInTheDocument();
    });

    it('renders error state on API failure', async () => {
        (analyticsApi.getExecutive as ReturnType<typeof vi.fn>).mockRejectedValue(new Error('fail'));

        renderWithProviders(<ExecutiveDashboardPage />);

        await waitFor(() => expect(screen.getByText(/An error occurred/i)).toBeInTheDocument());
    });
});
