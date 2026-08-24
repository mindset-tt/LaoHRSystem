import { describe, it, expect, vi, beforeEach } from 'vitest';
import { approvalsApi } from '../approvals';
import { notificationsApi } from '../notifications';
import { teamApi } from '../team';
import { employeesApi } from '../employees';

// Mock the apiClient's underlying fetch by stubbing global fetch and setting a token.
import { setAccessToken } from '../../apiClient';

function mockFetch(status: number, body: unknown) {
    return vi.fn().mockResolvedValue({
        ok: status >= 200 && status < 300,
        status,
        text: async () => JSON.stringify(body),
        json: async () => body,
        blob: async () => new Blob(),
        headers: { get: () => null },
    } as unknown as Response);
}

describe('approvalsApi', () => {
    beforeEach(() => {
        setAccessToken('test-token', new Date(Date.now() + 600000));
    });

    it('getMyPending returns inbox items', async () => {
        const items = [{ approvalRequestId: 1, requestType: 'LEAVE', entityId: 10, requesterEmployeeId: 2, status: 'PENDING', createdAt: '2026-01-01' }];
        global.fetch = mockFetch(200, items);
        const result = await approvalsApi.getMyPending();
        expect(result).toEqual(items);
        expect(global.fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/approvals/my-pending'),
            expect.anything(),
        );
    });

    it('getMine returns submitted requests', async () => {
        const items = [{ approvalRequestId: 2, requestType: 'EXPENSE', entityId: 20, requesterEmployeeId: 2, status: 'APPROVED', createdAt: '2026-01-02' }];
        global.fetch = mockFetch(200, items);
        const result = await approvalsApi.getMine();
        expect(result).toEqual(items);
    });
});

describe('notificationsApi', () => {
    beforeEach(() => {
        setAccessToken('test-token', new Date(Date.now() + 600000));
    });

    it('list returns paginated notifications', async () => {
        const page = { items: [{ notificationId: 1, userId: 1, type: 'APPROVAL_APPROVED', title: 'Approved', isRead: false, createdAt: '2026-01-01' }], page: 1, pageSize: 20, totalItems: 1 };
        global.fetch = mockFetch(200, page);
        const result = await notificationsApi.list();
        expect(result.items).toHaveLength(1);
    });

    it('getUnreadCount returns a number', async () => {
        global.fetch = mockFetch(200, 3);
        const result = await notificationsApi.getUnreadCount();
        expect(result).toBe(3);
    });
});

describe('teamApi', () => {
    beforeEach(() => {
        setAccessToken('test-token', new Date(Date.now() + 600000));
    });

    it('getTeamLeave returns team leave items', async () => {
        const items = [{ leaveId: 1, employeeId: 2, leaveType: 'ANNUAL', startDate: '2026-01-01', endDate: '2026-01-02', totalDays: 2, status: 'PENDING' }];
        global.fetch = mockFetch(200, items);
        const result = await teamApi.getTeamLeave();
        expect(result).toEqual(items);
    });

    it('getTeamAttendance returns team attendance items', async () => {
        const items = [{ attendanceId: 1, employeeId: 2, attendanceDate: '2026-01-01', status: 'PRESENT', isLate: false, isEarlyLeave: false }];
        global.fetch = mockFetch(200, items);
        const result = await teamApi.getTeamAttendance();
        expect(result).toEqual(items);
    });
});

describe('employeesApi.getMe', () => {
    beforeEach(() => {
        setAccessToken('test-token', new Date(Date.now() + 600000));
    });

    it('returns the current employee profile', async () => {
        const profile = { employeeId: 1, employeeCode: 'EMP001', laoName: 'Somphon', baseSalary: 100, salaryCurrency: 'LAK', dependentCount: 0, isActive: true, createdAt: '2026-01-01' };
        global.fetch = mockFetch(200, profile);
        const result = await employeesApi.getMe();
        expect(result.employeeId).toBe(1);
    });
});
