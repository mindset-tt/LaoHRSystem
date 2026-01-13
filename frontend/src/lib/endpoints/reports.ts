import { apiClient } from '../apiClient';

export const reportsApi = {
    /**
     * Download NSSF Report PDF
     */
    downloadNssfReport: async (periodId: number): Promise<Blob> => {
        return apiClient.getBlob(`/api/reports/nssf/${periodId}`);
    },
};

export default reportsApi;
