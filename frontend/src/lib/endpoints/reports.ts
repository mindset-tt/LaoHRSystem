import { apiClient } from '../apiClient';

export const reportsApi = {
    /**
     * Download NSSF Report PDF
     */
    downloadNssfReport: async (periodId: number): Promise<Blob> => {
        return apiClient.getBlob(`/api/reports/nssf/${periodId}`);
    },

    /**
     * Download NSSF Package (Zip)
     */
    downloadNssfPackage: async (periodId: number): Promise<Blob> => {
        return apiClient.getBlob(`/api/reports/nssf/zip/${periodId}`);
    },
};

export default reportsApi;
