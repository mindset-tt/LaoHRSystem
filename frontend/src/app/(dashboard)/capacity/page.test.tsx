import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '@/test/renderWithProviders';
import CapacityPage from '@/app/(dashboard)/capacity/page';
import { pmApi } from '@/lib/endpoints';

vi.mock('@/lib/endpoints', () => ({
    pmApi: {
        getPortfolio: vi.fn(),
        getCapacity: vi.fn(),
    },
}));

describe('CapacityPage', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('renders workload rows with overallocation flag', async () => {
        (pmApi.getCapacity as ReturnType<typeof vi.fn>).mockResolvedValue([
            { employeeId: 1, employeeName: 'Somphon', totalAllocationPercent: 130, isOverallocated: true, activeProjectCount: 2 },
        ]);

        renderWithProviders(<CapacityPage />);

        await waitFor(() => expect(screen.getByText('Somphon')).toBeInTheDocument());
        expect(screen.getByText('130%')).toBeInTheDocument();
        expect(screen.getByText('Overallocated')).toBeInTheDocument();
    });

    it('renders empty state', async () => {
        (pmApi.getCapacity as ReturnType<typeof vi.fn>).mockResolvedValue([]);

        renderWithProviders(<CapacityPage />);

        await waitFor(() => expect(screen.getByText(/No resource allocations/i)).toBeInTheDocument());
    });

    it('renders error state on API failure', async () => {
        (pmApi.getCapacity as ReturnType<typeof vi.fn>).mockRejectedValue(new Error('fail'));

        renderWithProviders(<CapacityPage />);

        await waitFor(() => expect(screen.getByText(/An error occurred/i)).toBeInTheDocument());
    });
});
