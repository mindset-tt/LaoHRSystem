// Phase 4C — Corporate Operations API client (Documents, Facilities, Rooms,
// Fleet, Travel, Visitors, Work Orders). Mirrors backOffice.ts conventions.

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

// ---- Documents ----
export interface CorporateDocumentDto {
    documentId: number;
    documentNumber: string;
    title: string;
    description?: string | null;
    category?: string | null;
    documentType: string;
    ownerEntityType: string;
    ownerEntityId: number;
    confidentiality: string;
    status: string;
    currentVersion: number;
    documentDate?: string | null;
    expiryDate?: string | null;
    createdAt: string;
}

export interface DocumentVersionDto {
    documentVersionId: number;
    documentId: number;
    versionNumber: number;
    fileName: string;
    mimeType?: string | null;
    fileSize: number;
    storageReference: string;
    checksum?: string | null;
    uploadedAt: string;
}

// ---- Facilities / Rooms ----
export interface FacilityDto {
    facilityId: number;
    facilityCode: string;
    name: string;
    nameLao?: string | null;
    workLocationId?: number | null;
    facilityType: string;
    managerEmployeeId?: number | null;
    address?: string | null;
    status: string;
    notes?: string | null;
}

export interface RoomDto {
    roomId: number;
    facilityId: number;
    code: string;
    name: string;
    roomType: string;
    capacity?: number | null;
    floor?: string | null;
    bookable: boolean;
    isActive: boolean;
}

export interface RoomBookingDto {
    roomBookingId: number;
    bookingNumber: string;
    roomId: number;
    roomName?: string | null;
    bookedByEmployeeId: number;
    bookedByName?: string | null;
    startAt: string;
    endAt: string;
    title: string;
    purpose?: string | null;
    participantCount?: number | null;
    status: string;
}

// ---- Fleet ----
export interface VehicleDto {
    vehicleId: number;
    vehicleCode: string;
    registrationNumber: string;
    assetId?: number | null;
    make?: string | null;
    model?: string | null;
    year?: number | null;
    vin?: string | null;
    vehicleType?: string | null;
    fuelType?: string | null;
    status: string;
    workLocationId?: number | null;
    currentOdometer: number;
}

export interface VehicleBookingDto {
    vehicleBookingId: number;
    bookingNumber: string;
    vehicleId: number;
    registrationNumber?: string | null;
    requesterEmployeeId: number;
    requesterName?: string | null;
    driverEmployeeId?: number | null;
    startAt: string;
    endAt: string;
    purpose: string;
    destination?: string | null;
    projectId?: number | null;
    status: string;
}

// ---- Travel ----
export interface TravelRequestDto {
    travelRequestId: number;
    travelNumber: string;
    employeeId: number;
    employeeName?: string | null;
    departmentId?: number | null;
    projectId?: number | null;
    purpose: string;
    destination: string;
    departureDate: string;
    returnDate: string;
    estimatedCost?: number | null;
    currency?: string | null;
    status: string;
    createdAt: string;
}

// ---- Visitors ----
export interface VisitorDto {
    visitorId: number;
    fullName: string;
    company?: string | null;
    phone?: string | null;
    email?: string | null;
    notes?: string | null;
}

export interface VisitDto {
    visitId: number;
    visitorId: number;
    visitorName?: string | null;
    hostEmployeeId: number;
    hostName?: string | null;
    facilityId?: number | null;
    purpose?: string | null;
    expectedAt?: string | null;
    checkedInAt?: string | null;
    checkedOutAt?: string | null;
    status: string;
}

// ---- Work Orders ----
export interface WorkOrderDto {
    workOrderId: number;
    workOrderNumber: string;
    sourceType: string;
    sourceId: number;
    category?: string | null;
    title: string;
    description?: string | null;
    priority: string;
    status: string;
    assignedEmployeeId?: number | null;
    assignedName?: string | null;
    supplierId?: number | null;
    scheduledAt?: string | null;
    completedAt?: string | null;
    cost?: number | null;
    currency?: string | null;
}

export const corporateApi = {
    // Documents
    listDocuments: (params: Record<string, string | number | boolean | undefined> = {}) =>
        apiClient.get<PaginatedResponse<CorporateDocumentDto>>(`/api/corporate-documents${toQuery(params)}`),
    getDocument: (id: number) => apiClient.get<CorporateDocumentDto>(`/api/corporate-documents/${id}`),
    getDocumentVersions: (id: number) => apiClient.get<DocumentVersionDto[]>(`/api/corporate-documents/${id}/versions`),
    createDocument: (input: Partial<CorporateDocumentDto>) => apiClient.post<CorporateDocumentDto>('/api/corporate-documents', input),
    addDocumentVersion: (id: number, input: { fileName: string; mimeType?: string | null; fileSize: number; storageReference: string; checksum?: string | null }) =>
        apiClient.post<DocumentVersionDto>(`/api/corporate-documents/${id}/versions`, input),

    // Facilities
    listFacilities: () => apiClient.get<FacilityDto[]>('/api/facilities'),
    createFacility: (input: Partial<FacilityDto>) => apiClient.post<FacilityDto>('/api/facilities', input),
    listRooms: (facilityId?: number) => apiClient.get<RoomDto[]>(`/api/facilities/rooms${toQuery({ facilityId })}`),
    createRoom: (input: Partial<RoomDto>) => apiClient.post<RoomDto>('/api/facilities/rooms', input),
    listRoomBookings: (params: Record<string, string | number | boolean | undefined> = {}) =>
        apiClient.get<RoomBookingDto[]>(`/api/facilities/bookings${toQuery(params)}`),
    bookRoom: (input: { roomId: number; startAt: string; endAt: string; title: string; purpose?: string | null; participantCount?: number | null }) =>
        apiClient.post<RoomBookingDto>('/api/facilities/bookings', input),
    cancelRoomBooking: (id: number) => apiClient.post<void>(`/api/facilities/bookings/${id}/cancel`),

    // Fleet
    listVehicles: (status?: string) => apiClient.get<VehicleDto[]>(`/api/fleet/vehicles${toQuery({ status })}`),
    createVehicle: (input: Partial<VehicleDto>) => apiClient.post<VehicleDto>('/api/fleet/vehicles', input),
    listVehicleBookings: (params: Record<string, string | number | boolean | undefined> = {}) =>
        apiClient.get<VehicleBookingDto[]>(`/api/fleet/bookings${toQuery(params)}`),
    bookVehicle: (input: { vehicleId: number; driverEmployeeId?: number | null; startAt: string; endAt: string; purpose: string; destination?: string | null; projectId?: number | null }) =>
        apiClient.post<VehicleBookingDto>('/api/fleet/bookings', input),
    cancelVehicleBooking: (id: number) => apiClient.post<void>(`/api/fleet/bookings/${id}/cancel`),

    // Travel
    listTravelRequests: (params: Record<string, string | number | boolean | undefined> = {}) =>
        apiClient.get<PaginatedResponse<TravelRequestDto>>(`/api/travel${toQuery(params)}`),
    getTravelRequest: (id: number) => apiClient.get<TravelRequestDto>(`/api/travel/${id}`),
    createTravelRequest: (input: { departmentId?: number | null; projectId?: number | null; purpose: string; destination: string; departureDate: string; returnDate: string; estimatedCost?: number | null; currency?: string | null }) =>
        apiClient.post<TravelRequestDto>('/api/travel', input),
    submitTravelRequest: (id: number) => apiClient.post<void>(`/api/travel/${id}/submit`),
    approveTravelRequest: (id: number) => apiClient.post<void>(`/api/travel/${id}/approve`),
    rejectTravelRequest: (id: number) => apiClient.post<void>(`/api/travel/${id}/reject`),

    // Visitors
    listVisitors: (params: Record<string, string | number | boolean | undefined> = {}) =>
        apiClient.get<PaginatedResponse<VisitorDto>>(`/api/visitors${toQuery(params)}`),
    createVisitor: (input: Partial<VisitorDto>) => apiClient.post<VisitorDto>('/api/visitors', input),
    listVisits: (params: Record<string, string | number | boolean | undefined> = {}) =>
        apiClient.get<VisitDto[]>(`/api/visitors/visits${toQuery(params)}`),
    createVisit: (input: { visitorId: number; hostEmployeeId: number; facilityId?: number | null; purpose?: string | null; expectedAt?: string | null }) =>
        apiClient.post<VisitDto>('/api/visitors/visits', input),
    checkInVisit: (id: number) => apiClient.post<void>(`/api/visitors/visits/${id}/check-in`),
    checkOutVisit: (id: number) => apiClient.post<void>(`/api/visitors/visits/${id}/check-out`),

    // Work Orders
    listWorkOrders: (params: Record<string, string | number | boolean | undefined> = {}) =>
        apiClient.get<PaginatedResponse<WorkOrderDto>>(`/api/work-orders${toQuery(params)}`),
    createWorkOrder: (input: Partial<WorkOrderDto>) => apiClient.post<WorkOrderDto>('/api/work-orders', input),
    updateWorkOrder: (id: number, input: { status?: string; assignedEmployeeId?: number | null; priority?: string; cost?: number | null; currency?: string | null }) =>
        apiClient.put<void>(`/api/work-orders/${id}`, input),
};
