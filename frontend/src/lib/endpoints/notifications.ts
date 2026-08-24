/**
 * Notifications API Endpoints
 * Phase 3C2 — in-app notification center.
 */

import { apiClient } from '../apiClient';
import type { PaginatedResponse } from '../types/pagination';

export interface NotificationItem {
    notificationId: number;
    userId: number;
    type: string;
    title: string;
    message?: string | null;
    entityType?: string | null;
    entityId?: number | null;
    isRead: boolean;
    createdAt: string;
    readAt?: string | null;
}

export const notificationsApi = {
    /** My notifications (paged). */
    list: async (params: { unreadOnly?: boolean; page?: number; pageSize?: number } = {}): Promise<PaginatedResponse<NotificationItem>> => {
        const sp = new URLSearchParams();
        if (params.unreadOnly) sp.set('unreadOnly', 'true');
        if (params.page) sp.set('page', params.page.toString());
        if (params.pageSize) sp.set('pageSize', params.pageSize.toString());
        const q = sp.toString();
        return apiClient.get<PaginatedResponse<NotificationItem>>(`/api/notifications${q ? `?${q}` : ''}`);
    },

    /** Unread count (for navigation badge). */
    getUnreadCount: async (): Promise<number> => {
        return apiClient.get<number>('/api/notifications/unread-count');
    },

    /** Mark a single notification as read. */
    markRead: async (id: number): Promise<void> => {
        return apiClient.post<void>(`/api/notifications/${id}/read`);
    },

    /** Mark all notifications as read. */
    markAllRead: async (): Promise<void> => {
        return apiClient.post<void>('/api/notifications/read-all');
    },
};

export default notificationsApi;
