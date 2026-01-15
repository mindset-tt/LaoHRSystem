import apiClient from '../apiClient';
import type { ConversionRate, CreateConversionRate, UpdateConversionRate } from '../types';

const BASE_URL = '/api/settings/conversion-rates';

/**
 * Conversion Rates API
 */
export const conversionRatesApi = {
    /**
     * Get all conversion rates
     */
    getAll: () =>
        apiClient.get<ConversionRate[]>(BASE_URL),

    /**
     * Get current active rates
     */
    getCurrent: () =>
        apiClient.get<ConversionRate[]>(`${BASE_URL}/current`),

    /**
     * Get rate for specific currency pair
     */
    getRate: (fromCurrency: string, toCurrency: string = 'LAK') =>
        apiClient.get<ConversionRate>(`${BASE_URL}/${fromCurrency}/to/${toCurrency}`),

    /**
     * Create new conversion rate
     */
    create: (data: CreateConversionRate) =>
        apiClient.post<ConversionRate>(BASE_URL, data),

    /**
     * Update conversion rate
     */
    update: (id: number, data: UpdateConversionRate) =>
        apiClient.put<ConversionRate>(`${BASE_URL}/${id}`, data),

    /**
     * Delete conversion rate
     */
    delete: (id: number) =>
        apiClient.delete(`${BASE_URL}/${id}`),
};
