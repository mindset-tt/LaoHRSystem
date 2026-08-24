// Lightweight Finance API client (Expenses + Employee Loans).
// Mirrors the conventions already used by projects.ts.

import { apiClient } from '../apiClient';
import type { PaginatedResponse } from '../types/pagination';

export const EXPENSE_STATUSES = [
    'DRAFT','SUBMITTED','APPROVED','REJECTED','PAID',
] as const;
export type ExpenseStatus = (typeof EXPENSE_STATUSES)[number];

export const LOAN_STATUSES = [
    'DRAFT','APPROVED','ACTIVE','SETTLED','REJECTED','CANCELLED',
] as const;
export type LoanStatus = (typeof LOAN_STATUSES)[number];

export interface ExpenseCategoryItem {
    expenseCategoryId: number;
    code: string;
    name: string;
    nameLao?: string | null;
    requiresReceipt: boolean;
    defaultLimit?: number | null;
    isActive: boolean;
}

export interface ExpenseListItem {
    expenseId: number;
    expenseNumber: string;
    employeeId: number;
    employeeName?: string | null;
    categoryId: number;
    categoryName?: string | null;
    title: string;
    expenseDate: string;
    currency: string;
    amount: number;
    amountLak: number;
    status: string;
    approverId?: number | null;
    approverName?: string | null;
    createdAt: string;
}

export interface ExpenseDetail extends ExpenseListItem {
    description?: string | null;
    payrollPeriodId?: number | null;
    exchangeRateUsed: number;
    receiptPath?: string | null;
    approvedAt?: string | null;
    approverNotes?: string | null;
    updatedAt: string;
}

export interface CreateExpenseInput {
    // Phase 3C2 — employeeId is resolved server-side from the JWT; optional here.
    employeeId?: number;
    categoryId: number;
    title: string;
    description?: string | null;
    expenseDate: string;
    currency: string;
    amount: number;
    receiptPath?: string | null;
}

export interface UpdateExpenseInput {
    categoryId?: number;
    title?: string;
    description?: string | null;
    expenseDate?: string;
    currency?: string;
    amount?: number;
    receiptPath?: string | null;
    status?: string;
    approverNotes?: string | null;
}

export interface LoanListItem {
    loanId: number;
    loanNumber: string;
    employeeId: number;
    employeeName?: string | null;
    purpose?: string | null;
    currency: string;
    principalAmount: number;
    principalAmountLak: number;
    remainingAmount: number;
    installmentAmount: number;
    installments: number;
    installmentsPaid: number;
    startDate: string;
    expectedEndDate?: string | null;
    status: string;
    approverId?: number | null;
    approverName?: string | null;
    createdAt: string;
}

export interface LoanRepaymentItem {
    repaymentId: number;
    repaidAt: string;
    amountLak: number;
    payrollPeriodId?: number | null;
    notes?: string | null;
}

export interface LoanDetail extends LoanListItem {
    loanType: string;
    interestRate: number;
    repaidAmount: number;
    endDate?: string | null;
    approvedAt?: string | null;
    approverNotes?: string | null;
    updatedAt: string;
    repayments: LoanRepaymentItem[];
}

export interface CreateLoanInput {
    // Phase 3C2 — employeeId is resolved server-side from the JWT; optional here.
    employeeId?: number;
    loanType?: string;
    purpose?: string | null;
    currency?: string;
    principalAmount: number;
    interestRate: number;
    installments: number;
    startDate: string;
}

export interface UpdateLoanInput {
    purpose?: string | null;
    interestRate?: number;
    installments?: number;
    startDate?: string;
    endDate?: string;
    status?: string;
    approverNotes?: string | null;
}

export interface RecordRepaymentInput {
    repaidAt: string;
    amountLak: number;
    payrollPeriodId?: number | null;
    notes?: string | null;
}

function toQuery(params: Record<string, unknown>): string {
    const sp = new URLSearchParams();
    for (const [k, v] of Object.entries(params)) {
        if (v === undefined || v === null || v === '') continue;
        sp.set(k, String(v));
    }
    const q = sp.toString();
    return q ? `?${q}` : '';
}

export const expensesApi = {
    list: (params: Record<string, string | number | boolean | undefined> = {}) =>
        apiClient.get<PaginatedResponse<ExpenseListItem>>(`/api/expenses${toQuery(params)}`),
    get: (id: number) => apiClient.get<ExpenseDetail>(`/api/expenses/${id}`),
    create: (input: CreateExpenseInput) => apiClient.post<ExpenseDetail>('/api/expenses', input),
    update: (id: number, input: UpdateExpenseInput) => apiClient.put<void>(`/api/expenses/${id}`, input),
    delete: (id: number) => apiClient.delete<void>(`/api/expenses/${id}`),
    approve: (id: number, notes?: string) => apiClient.post<void>(`/api/expenses/${id}/approve`, { notes }),
    reject: (id: number, notes?: string) => apiClient.post<void>(`/api/expenses/${id}/reject`, { notes }),
    markPaid: (id: number) => apiClient.post<void>(`/api/expenses/${id}/pay`),
    categories: () => apiClient.get<ExpenseCategoryItem[]>('/api/expenses/categories'),
};

export const loansApi = {
    list: (params: Record<string, string | number | boolean | undefined> = {}) =>
        apiClient.get<PaginatedResponse<LoanListItem>>(`/api/employeeLoans${toQuery(params)}`),
    get: (id: number) => apiClient.get<LoanDetail>(`/api/employeeLoans/${id}`),
    create: (input: CreateLoanInput) => apiClient.post<LoanDetail>('/api/employeeLoans', input),
    update: (id: number, input: UpdateLoanInput) => apiClient.put<void>(`/api/employeeLoans/${id}`, input),
    delete: (id: number) => apiClient.delete<void>(`/api/employeeLoans/${id}`),
    approve: (id: number, notes?: string) => apiClient.post<void>(`/api/employeeLoans/${id}/approve`, { notes }),
    reject: (id: number, notes?: string) => apiClient.post<void>(`/api/employeeLoans/${id}/reject`, { notes }),
    activate: (id: number) => apiClient.post<void>(`/api/employeeLoans/${id}/activate`),
    cancel: (id: number, notes?: string) => apiClient.post<void>(`/api/employeeLoans/${id}/cancel`, { notes }),
    listRepayments: (id: number) => apiClient.get<LoanRepaymentItem[]>(`/api/employeeLoans/${id}/repayments`),
    recordRepayment: (id: number, input: RecordRepaymentInput) =>
        apiClient.post<LoanRepaymentItem>(`/api/employeeLoans/${id}/repayments`, input),
};
