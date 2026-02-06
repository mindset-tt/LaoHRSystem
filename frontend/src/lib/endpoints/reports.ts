import { apiClient } from '../apiClient';

export const reportsApi = {
    /**
     * Download NSSF Report PDF
     */
    /**
     * Download NSSF Report PDF
     */
    downloadNssfReport: async (periodId: number): Promise<{ blob: Blob, filename: string }> => {
        return apiClient.getBlobWithFilename(`/api/reports/nssf/${periodId}`);
    },

    /**
     * Download NSSF Package (Zip)
     */
    downloadNssfPackage: async (periodId: number): Promise<{ blob: Blob, filename: string }> => {
        return apiClient.getBlobWithFilename(`/api/reports/nssf/zip/${periodId}`);
    },
};

export default reportsApi;
