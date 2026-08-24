// Phase 4A — Back Office API client (Procurement, Inventory, Assets, Contracts,
// Service Requests, Budgets). Mirrors the conventions of finance.ts / projects.ts.

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

// ---- Supplier ----
export interface SupplierListItem {
    supplierId: number;
    supplierCode: string;
    name: string;
    legalName?: string | null;
    phone?: string | null;
    email?: string | null;
    country?: string | null;
    status: string;
    createdAt: string;
}

export interface SupplierDetail extends SupplierListItem {
    taxId?: string | null;
    registrationNumber?: string | null;
    address?: string | null;
    provinceId?: number | null;
    districtId?: number | null;
    bankName?: string | null;
    bankAccount?: string | null;
    paymentTerms?: string | null;
    updatedAt: string;
}

export interface CreateSupplierInput {
    name: string;
    legalName?: string | null;
    taxId?: string | null;
    registrationNumber?: string | null;
    phone?: string | null;
    email?: string | null;
    address?: string | null;
    country?: string | null;
    provinceId?: number | null;
    districtId?: number | null;
    bankName?: string | null;
    bankAccount?: string | null;
    paymentTerms?: string | null;
}

// ---- Purchase Request ----
export interface PurchaseRequestItemDto {
    purchaseRequestItemId: number;
    description: string;
    itemId?: number | null;
    quantity: number;
    unit?: string | null;
    estimatedUnitPrice: number;
    estimatedAmount: number;
    preferredSupplierId?: number | null;
    notes?: string | null;
}

export interface PurchaseRequestListItem {
    purchaseRequestId: number;
    requestNumber: string;
    requestedByEmployeeId: number;
    requestedByName?: string | null;
    departmentId?: number | null;
    departmentName?: string | null;
    requiredDate?: string | null;
    purpose?: string | null;
    status: string;
    totalEstimatedAmount: number;
    currency: string;
    createdAt: string;
}

export interface PurchaseRequestDetail extends PurchaseRequestListItem {
    projectId?: number | null;
    costCenterId?: number | null;
    items: PurchaseRequestItemDto[];
}

export interface CreatePurchaseRequestInput {
    departmentId?: number | null;
    projectId?: number | null;
    costCenterId?: number | null;
    requiredDate?: string | null;
    purpose?: string | null;
    currency: string;
    items: { description: string; itemId?: number | null; quantity: number; unit?: string | null; estimatedUnitPrice: number; preferredSupplierId?: number | null; notes?: string | null }[];
}

// ---- Purchase Order ----
export interface PurchaseOrderListItem {
    purchaseOrderId: number;
    poNumber: string;
    supplierId: number;
    supplierName?: string | null;
    requestId?: number | null;
    currency: string;
    orderDate: string;
    expectedDate?: string | null;
    status: string;
    total: number;
    createdAt: string;
}

export interface PurchaseOrderItemDto {
    purchaseOrderItemId: number;
    description: string;
    itemId?: number | null;
    quantity: number;
    unit?: string | null;
    unitPrice: number;
    taxRate: number;
    lineTotal: number;
    notes?: string | null;
}

export interface PurchaseOrderDetail extends PurchaseOrderListItem {
    paymentTerms?: string | null;
    subtotal: number;
    tax: number;
    items: PurchaseOrderItemDto[];
}

export interface CreatePurchaseOrderInput {
    supplierId: number;
    requestId?: number | null;
    currency: string;
    expectedDate?: string | null;
    paymentTerms?: string | null;
    items: { description: string; itemId?: number | null; quantity: number; unit?: string | null; unitPrice: number; taxRate: number; notes?: string | null }[];
}

// ---- Goods Receipt ----
export interface GoodsReceiptListItem {
    goodsReceiptId: number;
    receiptNumber: string;
    purchaseOrderId: number;
    poNumber?: string | null;
    receivedByEmployeeId: number;
    receivedByName?: string | null;
    receivedDate: string;
    warehouseId?: number | null;
    status: string;
}

export interface GoodsReceiptDetail extends GoodsReceiptListItem {
    notes?: string | null;
    items: { goodsReceiptItemId: number; purchaseOrderItemId: number; description?: string | null; quantityReceived: number; acceptedQuantity: number; rejectedQuantity: number; notes?: string | null }[];
}

// ---- Inventory ----
export interface InventoryItemDto {
    inventoryItemId: number;
    sku: string;
    name: string;
    nameLao?: string | null;
    description?: string | null;
    categoryId?: number | null;
    unitOfMeasure?: string | null;
    itemType: string;
    trackInventory: boolean;
    reorderLevel?: number | null;
    isActive: boolean;
}

export interface WarehouseDto {
    warehouseId: number;
    code: string;
    name: string;
    nameLao?: string | null;
    workLocationId?: number | null;
    address?: string | null;
    managerEmployeeId?: number | null;
    status: string;
}

export interface StockBalanceDto {
    itemId: number;
    warehouseId: number;
    onHand: number;
}

export interface StockMovementDto {
    stockMovementId: number;
    itemId: number;
    itemName?: string | null;
    warehouseId: number;
    warehouseName?: string | null;
    movementType: string;
    quantity: number;
    referenceType?: string | null;
    referenceId?: number | null;
    occurredAt: string;
    performedByEmployeeId: number;
    notes?: string | null;
}

// ---- Asset ----
export interface AssetListItem {
    assetId: number;
    assetCode: string;
    name: string;
    categoryId?: number | null;
    serialNumber?: string | null;
    acquisitionCost?: number | null;
    currency?: string | null;
    custodianEmployeeId?: number | null;
    custodianName?: string | null;
    status: string;
}

export interface AssetDetail extends AssetListItem {
    purchaseOrderItemId?: number | null;
    purchaseDate?: string | null;
    workLocationId?: number | null;
    assignments: {
        assetAssignmentId: number;
        employeeId: number;
        employeeName?: string | null;
        assignedAt: string;
        returnedAt?: string | null;
        assignedByEmployeeId: number;
        conditionAtAssignment?: string | null;
        conditionAtReturn?: string | null;
    }[];
}

// ---- Contract ----
export interface ContractDto {
    contractId: number;
    contractNumber: string;
    title: string;
    supplierId?: number | null;
    supplierName?: string | null;
    contractType?: string | null;
    ownerEmployeeId: number;
    ownerName?: string | null;
    departmentId?: number | null;
    startDate: string;
    endDate?: string | null;
    amount?: number | null;
    currency?: string | null;
    status: string;
    renewalType?: string | null;
    noticeDate?: string | null;
}

// ---- Service Request ----
export interface ServiceRequestCategoryDto {
    serviceRequestCategoryId: number;
    code: string;
    name: string;
    nameLao?: string | null;
    isActive: boolean;
}

export interface ServiceRequestListItem {
    serviceRequestId: number;
    requestNumber: string;
    requesterEmployeeId: number;
    requesterName?: string | null;
    categoryId: number;
    categoryName?: string | null;
    subject: string;
    priority: string;
    status: string;
    assignedEmployeeId?: number | null;
    assignedName?: string | null;
    dueDate?: string | null;
    createdAt: string;
}

export interface ServiceRequestDetail extends ServiceRequestListItem {
    description?: string | null;
    departmentId?: number | null;
    resolvedAt?: string | null;
}

// ---- Budget ----
export interface BudgetDto {
    budgetId: number;
    fiscalYear: number;
    departmentId?: number | null;
    departmentName?: string | null;
    projectId?: number | null;
    costCenterId?: number | null;
    category: string;
    currency: string;
    approvedAmount: number;
    committedAmount: number;
    actualAmount: number;
    availableAmount: number;
    status: string;
}

export const backOfficeApi = {
    // Suppliers
    listSuppliers: (params: Record<string, string | number | boolean | undefined> = {}) =>
        apiClient.get<PaginatedResponse<SupplierListItem>>(`/api/suppliers${toQuery(params)}`),
    getSupplier: (id: number) => apiClient.get<SupplierDetail>(`/api/suppliers/${id}`),
    createSupplier: (input: CreateSupplierInput) => apiClient.post<SupplierDetail>('/api/suppliers', input),
    updateSupplier: (id: number, input: Partial<CreateSupplierInput> & { status?: string }) =>
        apiClient.put<void>(`/api/suppliers/${id}`, input),

    // Purchase Requests
    listPurchaseRequests: (params: Record<string, string | number | boolean | undefined> = {}) =>
        apiClient.get<PaginatedResponse<PurchaseRequestListItem>>(`/api/purchase-requests${toQuery(params)}`),
    getPurchaseRequest: (id: number) => apiClient.get<PurchaseRequestDetail>(`/api/purchase-requests/${id}`),
    createPurchaseRequest: (input: CreatePurchaseRequestInput) =>
        apiClient.post<PurchaseRequestListItem>('/api/purchase-requests', input),
    submitPurchaseRequest: (id: number) => apiClient.post<void>(`/api/purchase-requests/${id}/submit`),
    approvePurchaseRequest: (id: number, notes?: string) => apiClient.post<void>(`/api/purchase-requests/${id}/approve`, { notes }),
    rejectPurchaseRequest: (id: number, notes?: string) => apiClient.post<void>(`/api/purchase-requests/${id}/reject`, { notes }),

    // Purchase Orders
    listPurchaseOrders: (params: Record<string, string | number | boolean | undefined> = {}) =>
        apiClient.get<PaginatedResponse<PurchaseOrderListItem>>(`/api/purchase-orders${toQuery(params)}`),
    getPurchaseOrder: (id: number) => apiClient.get<PurchaseOrderDetail>(`/api/purchase-orders/${id}`),
    createPurchaseOrder: (input: CreatePurchaseOrderInput) =>
        apiClient.post<PurchaseOrderListItem>('/api/purchase-orders', input),
    sendPurchaseOrder: (id: number) => apiClient.post<void>(`/api/purchase-orders/${id}/send`),
    cancelPurchaseOrder: (id: number) => apiClient.post<void>(`/api/purchase-orders/${id}/cancel`),

    // Goods Receipts
    listGoodsReceipts: (params: Record<string, string | number | boolean | undefined> = {}) =>
        apiClient.get<PaginatedResponse<GoodsReceiptListItem>>(`/api/goods-receipts${toQuery(params)}`),
    getGoodsReceipt: (id: number) => apiClient.get<GoodsReceiptDetail>(`/api/goods-receipts/${id}`),
    createGoodsReceipt: (input: { purchaseOrderId: number; warehouseId?: number | null; notes?: string | null; items: { purchaseOrderItemId: number; quantityReceived: number; acceptedQuantity: number; rejectedQuantity: number; notes?: string | null }[] }) =>
        apiClient.post<GoodsReceiptListItem>('/api/goods-receipts', input),
    postGoodsReceipt: (id: number) => apiClient.post<void>(`/api/goods-receipts/${id}/post`),

    // Inventory
    listItems: (params: Record<string, string | number | boolean | undefined> = {}) =>
        apiClient.get<PaginatedResponse<InventoryItemDto>>(`/api/inventory/items${toQuery(params)}`),
    getItem: (id: number) => apiClient.get<InventoryItemDto>(`/api/inventory/items/${id}`),
    createItem: (input: Partial<InventoryItemDto>) => apiClient.post<InventoryItemDto>('/api/inventory/items', input),
    listCategories: () => apiClient.get<{ inventoryCategoryId: number; code: string; name: string; nameLao?: string | null; parentCategoryId?: number | null; sortOrder: number; isActive: boolean }[]>('/api/inventory/categories'),
    listWarehouses: () => apiClient.get<WarehouseDto[]>('/api/inventory/warehouses'),
    createWarehouse: (input: Partial<WarehouseDto>) => apiClient.post<WarehouseDto>('/api/inventory/warehouses', input),
    getStockBalances: (params: Record<string, string | number | boolean | undefined> = {}) =>
        apiClient.get<StockBalanceDto[]>(`/api/inventory/stock${toQuery(params)}`),
    listMovements: (params: Record<string, string | number | boolean | undefined> = {}) =>
        apiClient.get<PaginatedResponse<StockMovementDto>>(`/api/inventory/stock/movements${toQuery(params)}`),
    recordMovement: (input: { itemId: number; warehouseId: number; movementType: string; quantity: number; referenceType?: string | null; referenceId?: number | null; notes?: string | null }) =>
        apiClient.post<StockMovementDto>('/api/inventory/stock/movements', input),
    transferStock: (input: { itemId: number; fromWarehouseId: number; toWarehouseId: number; quantity: number; notes?: string | null }) =>
        apiClient.post<void>('/api/inventory/stock/transfer', input),
    adjustStock: (input: { itemId: number; warehouseId: number; quantity: number; reason: string }) =>
        apiClient.post<void>('/api/inventory/stock/adjust', input),

    // Assets
    listAssets: (params: Record<string, string | number | boolean | undefined> = {}) =>
        apiClient.get<PaginatedResponse<AssetListItem>>(`/api/assets${toQuery(params)}`),
    getAsset: (id: number) => apiClient.get<AssetDetail>(`/api/assets/${id}`),
    createAsset: (input: { name: string; categoryId?: number | null; serialNumber?: string | null; purchaseOrderItemId?: number | null; purchaseDate?: string | null; acquisitionCost?: number | null; currency?: string | null; workLocationId?: number | null; custodianEmployeeId?: number | null }) =>
        apiClient.post<AssetListItem>('/api/assets', input),
    assignAsset: (id: number, input: { employeeId: number; conditionAtAssignment?: string | null }) =>
        apiClient.post<void>(`/api/assets/${id}/assign`, input),
    returnAsset: (id: number, notes?: string) => apiClient.post<void>(`/api/assets/${id}/return`, { notes }),
    disposeAsset: (id: number) => apiClient.post<void>(`/api/assets/${id}/dispose`),

    // Contracts
    listContracts: (params: Record<string, string | number | boolean | undefined> = {}) =>
        apiClient.get<PaginatedResponse<ContractDto>>(`/api/contracts${toQuery(params)}`),
    getContract: (id: number) => apiClient.get<ContractDto>(`/api/contracts/${id}`),
    createContract: (input: Partial<ContractDto>) => apiClient.post<ContractDto>('/api/contracts', input),
    terminateContract: (id: number) => apiClient.post<void>(`/api/contracts/${id}/terminate`),

    // Service Requests
    listServiceRequestCategories: () => apiClient.get<ServiceRequestCategoryDto[]>('/api/service-requests/categories'),
    listServiceRequests: (params: Record<string, string | number | boolean | undefined> = {}) =>
        apiClient.get<PaginatedResponse<ServiceRequestListItem>>(`/api/service-requests${toQuery(params)}`),
    getServiceRequest: (id: number) => apiClient.get<ServiceRequestDetail>(`/api/service-requests/${id}`),
    createServiceRequest: (input: { categoryId: number; subject: string; description?: string | null; priority: string; departmentId?: number | null; dueDate?: string | null }) =>
        apiClient.post<ServiceRequestListItem>('/api/service-requests', input),
    updateServiceRequest: (id: number, input: { status?: string; assignedEmployeeId?: number | null; priority?: string; dueDate?: string | null }) =>
        apiClient.put<void>(`/api/service-requests/${id}`, input),

    // Budgets
    listBudgets: (params: Record<string, string | number | boolean | undefined> = {}) =>
        apiClient.get<BudgetDto[]>(`/api/budgets${toQuery(params)}`),
    createBudget: (input: { fiscalYear: number; departmentId?: number | null; projectId?: number | null; costCenterId?: number | null; category: string; currency: string; approvedAmount: number }) =>
        apiClient.post<BudgetDto>('/api/budgets', input),
    listCostCenters: () => apiClient.get<{ costCenterId: number; code: string; name: string; nameLao?: string | null; departmentId?: number | null; status: string }[]>('/api/budgets/cost-centers'),
};
