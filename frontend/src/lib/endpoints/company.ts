import apiClient from '../apiClient';
import type { CompanySetting, District, Province, Village } from '../types';

export const companyApi = {
    getSettings: () =>
        apiClient.get<CompanySetting>('/api/company-settings'),

    updateSettings: (data: Partial<CompanySetting>) =>
        apiClient.put<CompanySetting>('/api/company-settings', data),
};

export const addressApi = {
    getProvinces: () =>
        apiClient.get<Province[]>('/api/address/provinces'),

    getDistricts: (provinceId: number) =>
        apiClient.get<District[]>(`/api/address/districts/${provinceId}`),

    getVillages: (districtId: number) =>
        apiClient.get<Village[]>(`/api/address/villages/${districtId}`),
};
