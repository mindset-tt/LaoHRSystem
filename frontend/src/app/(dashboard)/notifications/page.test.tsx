import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '@/test/renderWithProviders';
import NotificationsPage from '@/app/(dashboard)/notifications/page';
import { notificationsApi } from '@/lib/endpoints';

vi.mock('@/lib/endpoints', () => ({
    notificationsApi: {
        list: vi.fn(),
        markRead: vi.fn(),
        markAllRead: vi.fn(),
    },
}));

describe('NotificationsPage', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('renders notifications', async () => {
        (notificationsApi.list as ReturnType<typeof vi.fn>).mockResolvedValue({
            items: [
                { notificationId: 1, userId: 1, type: 'APPROVAL_APPROVED', title: 'Leave approved', message: 'Your leave was approved', isRead: false, createdAt: '2026-01-01' },
            ],
            page: 1,
            pageSize: 50,
            totalItems: 1,
        });

        renderWithProviders(<NotificationsPage />);

        await waitFor(() => expect(screen.getByText('Leave approved')).toBeInTheDocument());
        expect(screen.getByText('Your leave was approved')).toBeInTheDocument();
    });

    it('renders empty state', async () => {
        (notificationsApi.list as ReturnType<typeof vi.fn>).mockResolvedValue({ items: [], page: 1, pageSize: 50, totalItems: 0 });

        renderWithProviders(<NotificationsPage />);

        await waitFor(() => expect(screen.getByText(/No notifications/i)).toBeInTheDocument());
    });

    it('renders error state on API failure', async () => {
        (notificationsApi.list as ReturnType<typeof vi.fn>).mockRejectedValue(new Error('fail'));

        renderWithProviders(<NotificationsPage />);

        await waitFor(() => expect(screen.getByText(/An error occurred/i)).toBeInTheDocument());
    });
});
