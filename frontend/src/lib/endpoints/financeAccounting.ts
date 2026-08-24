// Phase 4B — Finance + Accounting API client (Supplier Invoices, Payments,
// Bank Accounts, Chart of Accounts, Journals, Fiscal Periods, AP Aging).
// Mirrors the conventions of finance.ts / backOffice.ts.

import { apiClient } from '../apiClient';
import type { PaginatedResponse } from '../types/pagination';

function toQuery(params: Record<string, unknown>): string {
    const sp = new URLSearchParams();
    for (const [k, v] of Object.entries(params)) {
        if (v === undefined || v === null || v === '') continue;
        sp.set(k, String(v));
    }
    const q = sp.toString();
    return q ? `?${q}` : '';
}

// ---- Supplier Invoice ----
export interface SupplierInvoiceListItem {
    supplierInvoiceId: number;
    invoiceNumber: string;
    supplierId: number;
    supplierName?: string | null;
    purchaseOrderId?: number | null;
    invoiceDate: string;
    dueDate?: string | null;
    currency: string;
    totalAmount: number;
    remainingAmount: number;
    status: string;
    matchStatus?: string | null;
}

export interface SupplierInvoiceDetail extends SupplierInvoiceListItem {
    subtotal: number;
    taxAmount: number;
    paidAmount: number;
    costCenterId?: number | null;
    projectId?: number | null;
    departmentId?: number | null;
    lines: {
        supplierInvoiceLineId: number;
        purchaseOrderItemId?: number | null;
        inventoryItemId?: number | null;
        description: string;
        quantity: number;
        unitPrice: number;
        subtotal: number;
        taxAmount: number;
        accountId?: number | null;
        costCenterId?: number | null;
        projectId?: number | null;
    }[];
}

// ---- Payment ----
export interface PaymentListItem {
    paymentId: number;
    paymentNumber: string;
    paymentDate: string;
    paymentMethod: string;
    currency: string;
    amount: number;
    bankAccountId?: number | null;
    status: string;
}

export interface PaymentDetail extends PaymentListItem {
    referenceNumber?: string | null;
    allocations: { paymentAllocationId: number; supplierInvoiceId: number; invoiceNumber?: string | null; amount: number }[];
}

// ---- Bank Account ----
export interface BankAccountDto {
    bankAccountId: number;
    bankName: string;
    accountName: string;
    accountNumber: string;
    currency: string;
    branch?: string | null;
    swift?: string | null;
    isActive: boolean;
    openingBalance?: number | null;
    glAccountId?: number | null;
}

// ---- Account (COA) ----
export interface AccountDto {
    accountId: number;
    accountCode: string;
    name: string;
    nameLao?: string | null;
    accountType: string;
    parentAccountId?: number | null;
    isPostingAccount: boolean;
    isActive: boolean;
    currency?: string | null;
}

// ---- Journal ----
export interface JournalLineDto {
    journalLineId: number;
    accountId: number;
    accountCode?: string | null;
    description?: string | null;
    debit: number;
    credit: number;
    costCenterId?: number | null;
    departmentId?: number | null;
    projectId?: number | null;
}

export interface JournalEntryDto {
    journalEntryId: number;
    journalNumber: string;
    postingDate: string;
    fiscalPeriodId: number;
    sourceType: string;
    sourceId?: number | null;
    description?: string | null;
    status: string;
    currency: string;
    reversesJournalEntryId?: number | null;
    lines: JournalLineDto[];
}

// ---- Fiscal Period ----
export interface FiscalPeriodDto {
    fiscalPeriodId: number;
    fiscalYearId: number;
    periodNumber: number;
    startDate: string;
    endDate: string;
    status: string;
}

export interface FiscalYearDto {
    fiscalYearId: number;
    name: string;
    startDate: string;
    endDate: string;
    status: string;
}

// ---- AP Aging ----
export interface ApAgingDto {
    current: number;
    days1To30: number;
    days31To60: number;
    days61To90: number;
    over90: number;
    total: number;
}

export const financeAccountingApi = {
    // Supplier Invoices
    listInvoices: (params: Record<string, string | number | boolean | undefined> = {}) =>
        apiClient.get<PaginatedResponse<SupplierInvoiceListItem>>(`/api/supplier-invoices${toQuery(params)}`),
    getInvoice: (id: number) => apiClient.get<SupplierInvoiceDetail>(`/api/supplier-invoices/${id}`),
    createInvoice: (input: Record<string, unknown>) => apiClient.post<SupplierInvoiceListItem>('/api/supplier-invoices', input),
    matchInvoice: (id: number) => apiClient.post<{ matchStatus: string; status: string }>(`/api/supplier-invoices/${id}/match`),
    approveInvoice: (id: number) => apiClient.post<void>(`/api/supplier-invoices/${id}/approve`),
    voidInvoice: (id: number) => apiClient.post<void>(`/api/supplier-invoices/${id}/void`),
    postInvoice: (id: number) => apiClient.post<JournalEntryDto>(`/api/supplier-invoices/${id}/post`),
    getAging: () => apiClient.get<ApAgingDto>('/api/supplier-invoices/aging'),

    // Payments
    listPayments: (params: Record<string, string | number | boolean | undefined> = {}) =>
        apiClient.get<PaginatedResponse<PaymentListItem>>(`/api/payments${toQuery(params)}`),
    getPayment: (id: number) => apiClient.get<PaymentDetail>(`/api/payments/${id}`),
    createPayment: (input: Record<string, unknown>) => apiClient.post<PaymentListItem>('/api/payments', input),
    approvePayment: (id: number) => apiClient.post<void>(`/api/payments/${id}/approve`),
    postPayment: (id: number) => apiClient.post<void>(`/api/payments/${id}/post`),
    postPaymentToGl: (id: number) => apiClient.post<JournalEntryDto>(`/api/payments/${id}/post-to-gl`),

    // Bank Accounts
    listBankAccounts: () => apiClient.get<BankAccountDto[]>('/api/bank-accounts'),
    createBankAccount: (input: Record<string, unknown>) => apiClient.post<BankAccountDto>('/api/bank-accounts', input),

    // Chart of Accounts
    listAccounts: () => apiClient.get<AccountDto[]>('/api/accounts'),
    createAccount: (input: Record<string, unknown>) => apiClient.post<AccountDto>('/api/accounts', input),

    // Journals
    listJournals: (params: Record<string, string | number | boolean | undefined> = {}) =>
        apiClient.get<PaginatedResponse<JournalEntryDto>>(`/api/journals${toQuery(params)}`),
    getJournal: (id: number) => apiClient.get<JournalEntryDto>(`/api/journals/${id}`),
    createJournal: (input: Record<string, unknown>) => apiClient.post<JournalEntryDto>('/api/journals', input),
    postJournal: (id: number) => apiClient.post<void>(`/api/journals/${id}/post`),
    reverseJournal: (id: number) => apiClient.post<JournalEntryDto>(`/api/journals/${id}/reverse`),
    getTrialBalance: (fiscalPeriodId: number) =>
        apiClient.get<{ accountId: number; accountCode: string; accountName: string; accountType: string; debit: number; credit: number; balance: number }[]>(`/api/journals/trial-balance?fiscalPeriodId=${fiscalPeriodId}`),

    // Fiscal Periods
    listFiscalYears: () => apiClient.get<FiscalYearDto[]>('/api/fiscal-periods/years'),
    listFiscalPeriods: (params: Record<string, string | number | boolean | undefined> = {}) =>
        apiClient.get<FiscalPeriodDto[]>(`/api/fiscal-periods${toQuery(params)}`),
    closePeriod: (id: number) => apiClient.post<void>(`/api/fiscal-periods/${id}/close`),
    lockPeriod: (id: number) => apiClient.post<void>(`/api/fiscal-periods/${id}/lock`),
};
