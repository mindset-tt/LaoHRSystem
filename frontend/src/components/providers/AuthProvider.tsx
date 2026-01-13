'use client';

import {
    createContext,
    useContext,
    useState,
    useCallback,
    useEffect,
    type ReactNode,
} from 'react';
import { useRouter } from 'next/navigation';
import type { UserInfo, UserRole, LoginRequest, LoginResponse, Permission } from '@/lib/types';
import { apiClient, setAccessToken, clearAccessToken, hasValidToken, getAccessToken } from '@/lib/apiClient';
import { hasPermission, getPermissions } from '@/lib/permissions';

interface AuthContextType {
    /** Current user info */
    user: UserInfo | null;
    /** Whether authentication is being checked */
    loading: boolean;
    /** Whether user is authenticated */
    isAuthenticated: boolean;
    /** User's role */
    role: UserRole | undefined;
    /** User's permissions */
    permissions: Permission[];
    /** Login function */
    login: (credentials: LoginRequest) => Promise<void>;
    /** Logout function */
    logout: () => void;
    /** Check if user has a specific permission */
    can: (permission: Permission) => boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

/**
 * AuthProvider Component
 * 
 * Manages authentication state, token storage, and provides auth context.
 * Permissions are loaded at login and refreshed on session renewal.
 */
export function AuthProvider({ children }: { children: ReactNode }) {
    const [user, setUser] = useState<UserInfo | null>(null);
    const [loading, setLoading] = useState(true);
    const router = useRouter();

    const role = user?.role;
    const permissions = role ? getPermissions(role) : [];
    const isAuthenticated = !!user && hasValidToken();

    /**
     * Check if user has a specific permission
     */
    const can = useCallback(
        (permission: Permission) => hasPermission(role, permission),
        [role]
    );

    /**
     * Login function
     */
    const login = useCallback(
        async (credentials: LoginRequest) => {
            const response = await apiClient.post<LoginResponse>('/api/auth/login', credentials, {
                noAuth: true,
            });

            // Store token in memory
            setAccessToken(response.token, new Date(response.expiresAt));

            // Set user info
            setUser({
                username: response.username,
                role: response.role,
                displayName: response.displayName,
            });

            // Redirect to dashboard
            router.push('/');
        },
        [router]
    );

    /**
     * Logout function
     */
    const logout = useCallback(() => {
        clearAccessToken();
        setUser(null);
        router.push('/login');
    }, [router]);

    /**
     * Check authentication on mount
     */
    useEffect(() => {
        const checkAuth = async () => {
            // Skip if no valid token
            if (!hasValidToken()) {
                // Try refresh one time on startup (if we have a cookie)
                try {
                    // We can't confirm cookie existence easily in client JS if httpOnly
                    // But we can try the refresh endpoint once
                    await apiClient.post('/api/auth/refresh', undefined, { noAuth: true });
                    const userInfo = await apiClient.get<UserInfo>('/api/auth/me');
                    setUser(userInfo);
                } catch (e) {
                    clearAccessToken();
                } finally {
                    setLoading(false);
                }
                return;
            }

            try {
                const userInfo = await apiClient.get<UserInfo>('/api/auth/me');
                setUser(userInfo);
            } catch (e) {
                // Token invalid, clear it
                clearAccessToken();
            } finally {
                setLoading(false);
            }
        };

        checkAuth();
    }, []);

    return (
        <AuthContext.Provider
            value={{
                user,
                loading,
                isAuthenticated,
                role,
                permissions,
                login,
                logout,
                can,
            }}
        >
            {children}
        </AuthContext.Provider>
    );
}

/**
 * Hook to access auth context
 */
export function useAuth(): AuthContextType {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error('useAuth must be used within an AuthProvider');
    }
    return context;
}

/**
 * Hook to require authentication
 * Redirects to login if not authenticated
 */
export function useRequireAuth(): AuthContextType {
    const auth = useAuth();
    const router = useRouter();

    useEffect(() => {
        if (!auth.loading && !auth.isAuthenticated) {
            router.push('/login');
        }
    }, [auth.loading, auth.isAuthenticated, router]);

    return auth;
}

/**
 * Hook to require a specific permission
 * Redirects to 403 page if permission not granted
 */
export function useRequirePermission(permission: Permission): AuthContextType {
    const auth = useRequireAuth();
    const router = useRouter();

    useEffect(() => {
        if (!auth.loading && auth.isAuthenticated && !auth.can(permission)) {
            router.push('/403');
        }
        // Using specific auth properties instead of entire object to avoid unnecessary rerenders
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [auth.loading, auth.isAuthenticated, auth.can, permission, router]);

    return auth;
}
