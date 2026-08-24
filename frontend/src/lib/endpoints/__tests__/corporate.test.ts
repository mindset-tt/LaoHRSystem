import { describe, it, expect, vi, beforeEach } from 'vitest';
import { corporateApi } from '../corporate';
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

describe('corporateApi', () => {
    beforeEach(() => {
        setAccessToken('test-token', new Date(Date.now() + 600000));
    });

    it('listDocuments hits the corporate-documents endpoint', async () => {
        const page = { items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false };
        global.fetch = mockFetch(200, page);
        await corporateApi.listDocuments({ page: 1 });
        expect(global.fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/corporate-documents?page=1'),
            expect.anything(),
        );
    });

    it('listFacilities hits the facilities endpoint', async () => {
        global.fetch = mockFetch(200, []);
        await corporateApi.listFacilities();
        expect(global.fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/facilities'),
            expect.anything(),
        );
    });

    it('listVehicles hits the fleet vehicles endpoint', async () => {
        global.fetch = mockFetch(200, []);
        await corporateApi.listVehicles();
        expect(global.fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/fleet/vehicles'),
            expect.anything(),
        );
    });

    it('listTravelRequests hits the travel endpoint', async () => {
        const page = { items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false };
        global.fetch = mockFetch(200, page);
        await corporateApi.listTravelRequests({ page: 1 });
        expect(global.fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/travel?page=1'),
            expect.anything(),
        );
    });

    it('listVisitors hits the visitors endpoint', async () => {
        const page = { items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false };
        global.fetch = mockFetch(200, page);
        await corporateApi.listVisitors({ page: 1 });
        expect(global.fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/visitors?page=1'),
            expect.anything(),
        );
    });

    it('listWorkOrders hits the work-orders endpoint', async () => {
        const page = { items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false };
        global.fetch = mockFetch(200, page);
        await corporateApi.listWorkOrders({ page: 1 });
        expect(global.fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/work-orders?page=1'),
            expect.anything(),
        );
    });

    it('bookRoom posts to the facilities bookings endpoint', async () => {
        global.fetch = mockFetch(201, { roomBookingId: 1 });
        await corporateApi.bookRoom({ roomId: 1, startAt: '2026-01-01T09:00:00Z', endAt: '2026-01-01T10:00:00Z', title: 'Meeting' });
        expect(global.fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/facilities/bookings'),
            expect.anything(),
        );
    });

    it('bookVehicle posts to the fleet bookings endpoint', async () => {
        global.fetch = mockFetch(201, { vehicleBookingId: 1 });
        await corporateApi.bookVehicle({ vehicleId: 1, startAt: '2026-01-01T09:00:00Z', endAt: '2026-01-01T10:00:00Z', purpose: 'Trip' });
        expect(global.fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/fleet/bookings'),
            expect.anything(),
        );
    });

    it('checkInVisit posts to the visitors check-in endpoint', async () => {
        global.fetch = mockFetch(204, null);
        await corporateApi.checkInVisit(1);
        expect(global.fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/visitors/visits/1/check-in'),
            expect.anything(),
        );
    });
});
