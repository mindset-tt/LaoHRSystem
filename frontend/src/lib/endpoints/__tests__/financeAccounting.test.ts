import { describe, it, expect, vi, beforeEach } from 'vitest';
import { financeAccountingApi } from '../financeAccounting';
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

describe('financeAccountingApi', () => {
    beforeEach(() => {
        setAccessToken('test-token', new Date(Date.now() + 600000));
    });

    it('listInvoices hits the supplier-invoices endpoint', async () => {
        const page = { items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false };
        global.fetch = mockFetch(200, page);
        await financeAccountingApi.listInvoices({ page: 1 });
        expect(global.fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/supplier-invoices?page=1'),
            expect.anything(),
        );
    });

    it('listPayments hits the payments endpoint', async () => {
        const page = { items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false };
        global.fetch = mockFetch(200, page);
        await financeAccountingApi.listPayments({ page: 1 });
        expect(global.fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/payments?page=1'),
            expect.anything(),
        );
    });

    it('listAccounts hits the accounts endpoint', async () => {
        const accounts = [{ accountId: 1, accountCode: '1000', name: 'Cash', accountType: 'ASSET', isPostingAccount: true, isActive: true }];
        global.fetch = mockFetch(200, accounts);
        const result = await financeAccountingApi.listAccounts();
        expect(result).toEqual(accounts);
        expect(global.fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/accounts'),
            expect.anything(),
        );
    });

    it('listJournals hits the journals endpoint', async () => {
        const page = { items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false };
        global.fetch = mockFetch(200, page);
        await financeAccountingApi.listJournals({ page: 1 });
        expect(global.fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/journals?page=1'),
            expect.anything(),
        );
    });

    it('getAging hits the aging endpoint', async () => {
        const aging = { current: 0, days1To30: 0, days31To60: 0, days61To90: 0, over90: 0, total: 0 };
        global.fetch = mockFetch(200, aging);
        const result = await financeAccountingApi.getAging();
        expect(result).toEqual(aging);
        expect(global.fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/supplier-invoices/aging'),
            expect.anything(),
        );
    });

    it('listBankAccounts hits the bank-accounts endpoint', async () => {
        const accounts = [{ bankAccountId: 1, bankName: 'BCEL', accountName: 'Main', accountNumber: '123', currency: 'LAK', isActive: true }];
        global.fetch = mockFetch(200, accounts);
        await financeAccountingApi.listBankAccounts();
        expect(global.fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/bank-accounts'),
            expect.anything(),
        );
    });

    it('postInvoice hits the invoice post endpoint', async () => {
        global.fetch = mockFetch(200, {});
        await financeAccountingApi.postInvoice(1);
        expect(global.fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/supplier-invoices/1/post'),
            expect.anything(),
        );
    });

    it('postPaymentToGl hits the payment post-to-gl endpoint', async () => {
        global.fetch = mockFetch(200, {});
        await financeAccountingApi.postPaymentToGl(1);
        expect(global.fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/payments/1/post-to-gl'),
            expect.anything(),
        );
    });

    it('getTrialBalance hits the trial-balance endpoint', async () => {
        const rows = [{ accountId: 1, accountCode: '1000', accountName: 'Cash', accountType: 'ASSET', debit: 0, credit: 0, balance: 0 }];
        global.fetch = mockFetch(200, rows);
        await financeAccountingApi.getTrialBalance(1);
        expect(global.fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/journals/trial-balance?fiscalPeriodId=1'),
            expect.anything(),
        );
    });
});
