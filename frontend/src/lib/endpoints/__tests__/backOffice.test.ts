import { describe, it, expect, vi, beforeEach } from 'vitest';
import { backOfficeApi } from '../backOffice';
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

describe('backOfficeApi', () => {
    beforeEach(() => {
        setAccessToken('test-token', new Date(Date.now() + 600000));
    });

    it('listSuppliers hits the suppliers endpoint', async () => {
        const page = { items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false };
        global.fetch = mockFetch(200, page);
        const result = await backOfficeApi.listSuppliers({ page: 1 });
        expect(result).toEqual(page);
        expect(global.fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/suppliers?page=1'),
            expect.anything(),
        );
    });

    it('listPurchaseRequests hits the purchase-requests endpoint', async () => {
        const page = { items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false };
        global.fetch = mockFetch(200, page);
        await backOfficeApi.listPurchaseRequests({ page: 1 });
        expect(global.fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/purchase-requests?page=1'),
            expect.anything(),
        );
    });

    it('listPurchaseOrders hits the purchase-orders endpoint', async () => {
        const page = { items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false };
        global.fetch = mockFetch(200, page);
        await backOfficeApi.listPurchaseOrders({ page: 1 });
        expect(global.fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/purchase-orders?page=1'),
            expect.anything(),
        );
    });

    it('listItems hits the inventory items endpoint', async () => {
        const page = { items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false };
        global.fetch = mockFetch(200, page);
        await backOfficeApi.listItems({ page: 1 });
        expect(global.fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/inventory/items?page=1'),
            expect.anything(),
        );
    });

    it('listAssets hits the assets endpoint', async () => {
        const page = { items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false };
        global.fetch = mockFetch(200, page);
        await backOfficeApi.listAssets({ page: 1 });
        expect(global.fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/assets?page=1'),
            expect.anything(),
        );
    });

    it('listBudgets hits the budgets endpoint', async () => {
        const budgets = [{ budgetId: 1, fiscalYear: 2026, category: 'IT', currency: 'LAK', approvedAmount: 100, reservedAmount: 0, committedAmount: 0, actualAmount: 0, availableAmount: 100, status: 'APPROVED' }];
        global.fetch = mockFetch(200, budgets);
        const result = await backOfficeApi.listBudgets();
        expect(result).toEqual(budgets);
        expect(global.fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/budgets'),
            expect.anything(),
        );
    });
});
