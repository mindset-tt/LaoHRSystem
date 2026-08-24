import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '@/test/renderWithProviders';
import MyProfilePage from '@/app/(dashboard)/my-profile/page';
import { employeesApi } from '@/lib/endpoints';

vi.mock('@/lib/endpoints', () => ({
    employeesApi: { getMe: vi.fn() },
}));

describe('MyProfilePage', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('renders the current employee profile', async () => {
        (employeesApi.getMe as ReturnType<typeof vi.fn>).mockResolvedValue({
            employeeId: 1,
            employeeCode: 'EMP001',
            laoName: 'Somphon Khamsouk',
            englishName: 'Somphon Khamsouk',
            jobTitle: 'Software Engineer',
            baseSalary: 8000000,
            salaryCurrency: 'LAK',
            dependentCount: 0,
            isActive: true,
            createdAt: '2026-01-01',
        });

        renderWithProviders(<MyProfilePage />);

        await waitFor(() => expect(screen.getByText('Somphon Khamsouk')).toBeInTheDocument());
        expect(screen.getByText('EMP001')).toBeInTheDocument();
    });

    it('renders error state on API failure', async () => {
        (employeesApi.getMe as ReturnType<typeof vi.fn>).mockRejectedValue(new Error('fail'));

        renderWithProviders(<MyProfilePage />);

        await waitFor(() => expect(screen.getByText(/An error occurred/i)).toBeInTheDocument());
    });
});
