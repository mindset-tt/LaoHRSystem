/**
 * Auth API Endpoints
 * Handles login, logout, token refresh, and current user
 */

import { apiClient, setAccessToken, clearAccessToken } from '../apiClient';
import type { LoginRequest, LoginResponse, UserInfo } from '../types';

export const authApi = {
    /**
     * Login with username and password
     */
    login: async (credentials: LoginRequest): Promise<LoginResponse> => {
        const response = await apiClient.post<LoginResponse>(
            '/api/auth/login',
            credentials,
            { noAuth: true }
        );

        // Store the access token
        setAccessToken(response.token, new Date(response.expiresAt));

        return response;
    },

    /**
     * Get current user info
     */
    me: async (): Promise<UserInfo> => {
        return apiClient.get<UserInfo>('/api/auth/me');
    },

    /**
     * Refresh access token using refresh token cookie
     */
    refresh: async (): Promise<LoginResponse> => {
        const response = await apiClient.post<LoginResponse>(
            '/api/auth/refresh',
            undefined,
            { noAuth: true }
        );

        setAccessToken(response.token, new Date(response.expiresAt));

        return response;
    },

    /**
     * Logout - clear tokens
     */
    logout: () => {
        clearAccessToken();
    },
};

export default authApi;
