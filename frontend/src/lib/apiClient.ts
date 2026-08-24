/**
 * API Client
 * 
 * Auto-injects access tokens and handles 401-triggered refresh.
 * All requests go through this client for consistent error handling.
 */

import type { ApiError } from './types';

// API base URL - configured for development
const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

// Token storage
let accessToken: string | null = null;
let tokenExpiresAt: Date | null = null;
let refreshToken: string | null = null;

// Initialize from localStorage if available (client-side only)
if (typeof window !== 'undefined') {
    const storedToken = localStorage.getItem('accessToken');
    const storedExpiry = localStorage.getItem('tokenExpiresAt');

    if (storedToken && storedExpiry) {
        const expiryDate = new Date(storedExpiry);
        if (expiryDate > new Date()) {
            accessToken = storedToken;
            tokenExpiresAt = expiryDate;
        } else {
            // Clear expired token
            localStorage.removeItem('accessToken');
            localStorage.removeItem('tokenExpiresAt');
        }
    }

    const storedRefresh = localStorage.getItem('refreshToken');
    const storedRefreshExpiry = localStorage.getItem('refreshTokenExpiresAt');
    if (storedRefresh && storedRefreshExpiry) {
        const expiryDate = new Date(storedRefreshExpiry);
        if (expiryDate > new Date()) {
            refreshToken = storedRefresh;
        } else {
            localStorage.removeItem('refreshToken');
            localStorage.removeItem('refreshTokenExpiresAt');
        }
    }
}

// Refresh promise to prevent multiple simultaneous refreshes
let refreshPromise: Promise<void> | null = null;

/**
 * Set the access + refresh tokens (called after login or rotation).
 * Phase 6c — both tokens are persisted to localStorage and rotated together.
 */
export function setAccessToken(token: string, expiresAt: Date): void {
    accessToken = token;
    tokenExpiresAt = expiresAt;

    if (typeof window !== 'undefined') {
        localStorage.setItem('accessToken', token);
        localStorage.setItem('tokenExpiresAt', expiresAt.toISOString());
    }
}

export function setRefreshToken(token: string, expiresAt: Date): void {
    refreshToken = token;

    if (typeof window !== 'undefined') {
        localStorage.setItem('refreshToken', token);
        localStorage.setItem('refreshTokenExpiresAt', expiresAt.toISOString());
    }
}

/**
 * Clear all tokens (called on logout / refresh-failure).
 */
export function clearAccessToken(): void {
    accessToken = null;
    tokenExpiresAt = null;
    refreshToken = null;

    if (typeof window !== 'undefined') {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('tokenExpiresAt');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('refreshTokenExpiresAt');
    }
}

export function getRefreshToken(): string | null {
    return refreshToken;
}

/**
 * Check if we have a valid token
 */
export function hasValidToken(): boolean {
    if (!accessToken || !tokenExpiresAt) return false;
    // Consider token invalid if it expires in less than 1 minute
    return tokenExpiresAt.getTime() - Date.now() > 60000;
}

export function getAccessToken(): string | null {
    return accessToken;
}

/**
 * Refresh the access + refresh tokens by presenting the current refresh
 * token. Server rotates the refresh token on every use (Phase 6c).
 */
async function refreshAccessToken(): Promise<void> {
    try {
        if (!refreshToken) {
            throw new Error('No refresh token available');
        }
        const response = await fetch(`${API_BASE_URL}/api/auth/refresh`, {
            method: 'POST',
            credentials: 'include',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ refreshToken }),
        });

        if (!response.ok) {
            throw new Error('Token refresh failed');
        }

        const data = await response.json();
        setAccessToken(data.token, new Date(data.expiresAt));
        if (data.refreshToken && data.refreshExpiresAt) {
            setRefreshToken(data.refreshToken, new Date(data.refreshExpiresAt));
        }
    } catch {
        clearAccessToken();
        if (typeof window !== 'undefined') {
            window.location.assign(new URL('/login?expired=true', window.location.origin).href);
        }
        throw new Error('Session expired');
    }
}

/**
 * Ensure we have a valid token, refreshing if necessary
 */
async function ensureValidToken(): Promise<void> {
    if (hasValidToken()) return;

    // If already refreshing, wait for that to complete
    if (refreshPromise) {
        await refreshPromise;
        return;
    }

    // Start refresh
    refreshPromise = refreshAccessToken();
    try {
        await refreshPromise;
    } finally {
        refreshPromise = null;
    }
}

/**
 * API Error class for better error handling
 */
export class ApiClientError extends Error {
    status: number;
    code?: string;
    details?: Record<string, string[]>;

    constructor(message: string, status: number, code?: string, details?: Record<string, string[]>) {
        super(message);
        this.name = 'ApiClientError';
        this.status = status;
        this.code = code;
        this.details = details;
    }
}

interface RequestOptions extends RequestInit {
    /** Skip authentication for this request */
    noAuth?: boolean;
    /** Expected response type */
    responseType?: 'json' | 'blob' | 'text' | 'blobWithHeaders';
}

/**
 * Make an authenticated API request
 */
async function request<T>(
    endpoint: string,
    options: RequestOptions = {}
): Promise<T> {
    const { noAuth = false, responseType = 'json', ...fetchOptions } = options;

    // Ensure valid token unless explicitly skipped
    if (!noAuth) {
        await ensureValidToken();
    }

    const headers: HeadersInit = {
        'Content-Type': 'application/json',
        ...(fetchOptions.headers || {}),
    };

    // Add authorization header if we have a token
    if (accessToken && !noAuth) {
        (headers as Record<string, string>)['Authorization'] = `Bearer ${accessToken}`;
    }

    const url = endpoint.startsWith('http') ? endpoint : `${API_BASE_URL}${endpoint}`;

    const response = await fetch(url, {
        ...fetchOptions,
        headers,
        credentials: 'include', // Always include cookies for refresh token
    });

    // Handle 401 - attempt token refresh and retry
    if (response.status === 401 && !noAuth) {
        await refreshAccessToken();
        // Retry the request with new token
        (headers as Record<string, string>)['Authorization'] = `Bearer ${accessToken}`;
        const retryResponse = await fetch(url, {
            ...fetchOptions,
            headers,
            credentials: 'include',
        });

        if (!retryResponse.ok) {
            const error = await parseError(retryResponse);
            throw new ApiClientError(error.message, retryResponse.status, error.code, error.details);
        }

        if (responseType === 'blob') return retryResponse.blob() as Promise<T>;
        if (responseType === 'text') return retryResponse.text() as Promise<T>;
        if (responseType === 'blobWithHeaders') {
            const blob = await retryResponse.blob();
            const contentDisposition = retryResponse.headers.get('Content-Disposition');
            let filename = 'download';
            if (contentDisposition) {
                const filenameMatch = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
                if (filenameMatch && filenameMatch[1]) {
                    filename = filenameMatch[1].replace(/['"]/g, '');
                }
            }
            return { blob, filename } as unknown as Promise<T>;
        }
        return retryResponse.json();
    }

    // Handle other errors
    if (!response.ok) {
        const error = await parseError(response);
        throw new ApiClientError(error.message, response.status, error.code, error.details);
    }

    if (responseType === 'blob') return response.blob() as Promise<T>;
    if (responseType === 'text') return response.text() as Promise<T>;
    if (responseType === 'blobWithHeaders') {
        const blob = await response.blob();
        const contentDisposition = response.headers.get('Content-Disposition');
        let filename = 'download';
        if (contentDisposition) {
            const filenameMatch = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
            if (filenameMatch && filenameMatch[1]) {
                filename = filenameMatch[1].replace(/['"]/g, '');
            }
        }
        return { blob, filename } as unknown as Promise<T>;
    }

    // Handle empty responses
    const text = await response.text();
    if (!text) return {} as T;

    return JSON.parse(text);
}

/**
 * Parse error response
 */
async function parseError(response: Response): Promise<ApiError> {
    try {
        const data = await response.json();
        return {
            message: data.message || data.title || 'An error occurred',
            code: data.code,
            details: data.errors,
        };
    } catch {
        return {
            message: getDefaultErrorMessage(response.status),
        };
    }
}

/**
 * Get user-friendly error message for status code
 */
function getDefaultErrorMessage(status: number): string {
    switch (status) {
        case 400:
            return 'Invalid request. Please check your input.';
        case 401:
            return 'Your session has expired. Please log in again.';
        case 403:
            return 'You do not have permission to perform this action.';
        case 404:
            return 'The requested resource was not found.';
        case 409:
            return 'This action conflicts with existing data.';
        case 422:
            return 'The provided data is invalid.';
        case 429:
            return 'Too many requests. Please wait a moment.';
        case 500:
            return 'A server error occurred. Please try again later.';
        default:
            return 'An unexpected error occurred. Please try again.';
    }
}

/**
 * API client with convenience methods
 */
export const apiClient = {
    get: <T>(endpoint: string, options?: RequestOptions) =>
        request<T>(endpoint, { ...options, method: 'GET' }),

    getBlob: (endpoint: string, options?: RequestOptions) =>
        request<Blob>(endpoint, { ...options, method: 'GET', responseType: 'blob' }),

    getBlobWithFilename: (endpoint: string, options?: RequestOptions) => {
        return request<{ blob: Blob, filename: string }>(endpoint, {
            ...options,
            method: 'GET',
            responseType: 'blobWithHeaders'
        });
    },

    post: <T>(endpoint: string, data?: unknown, options?: RequestOptions) =>
        request<T>(endpoint, {
            ...options,
            method: 'POST',
            body: data ? JSON.stringify(data) : undefined,
        }),

    put: <T>(endpoint: string, data?: unknown, options?: RequestOptions) =>
        request<T>(endpoint, {
            ...options,
            method: 'PUT',
            body: data ? JSON.stringify(data) : undefined,
        }),

    patch: <T>(endpoint: string, data?: unknown, options?: RequestOptions) =>
        request<T>(endpoint, {
            ...options,
            method: 'PATCH',
            body: data ? JSON.stringify(data) : undefined,
        }),

    delete: <T>(endpoint: string, options?: RequestOptions) =>
        request<T>(endpoint, { ...options, method: 'DELETE' }),
};

export default apiClient;
