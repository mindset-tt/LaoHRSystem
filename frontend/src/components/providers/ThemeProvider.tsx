'use client';

import { createContext, useContext, useEffect, useState, type ReactNode } from 'react';

export type ThemeMode = 'light' | 'dark' | 'system';

interface ThemeContextValue {
    /** The resolved theme actually applied to <html data-theme="...">. */
    theme: 'light' | 'dark';
    /** The user's chosen preference. */
    mode: ThemeMode;
    setMode: (mode: ThemeMode) => void;
}

const ThemeContext = createContext<ThemeContextValue | null>(null);

const STORAGE_KEY = 'laohr:theme';

function getStoredMode(): ThemeMode {
    if (typeof window === 'undefined') return 'system';
    const stored = window.localStorage.getItem(STORAGE_KEY);
    if (stored === 'light' || stored === 'dark' || stored === 'system') return stored;
    return 'system';
}

function resolveTheme(mode: ThemeMode): 'light' | 'dark' {
    if (mode === 'system') {
        if (typeof window === 'undefined') return 'light';
        return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    }
    return mode;
}

function applyTheme(theme: 'light' | 'dark') {
    if (typeof document === 'undefined') return;
    const root = document.documentElement;
    // Both light and dark themes are explicit; CSS uses data-theme overrides
    // so the body does not depend on prefers-color-scheme alone.
    root.setAttribute('data-theme', theme);
}

export function ThemeProvider({ children }: { children: ReactNode }) {
    const [mode, setModeState] = useState<ThemeMode>('system');
    const [theme, setTheme] = useState<'light' | 'dark'>('light');

    // Hydrate from localStorage on mount.
    useEffect(() => {
        const stored = getStoredMode();
        setModeState(stored);
        setTheme(resolveTheme(stored));
    }, []);

    // Apply the resolved theme whenever mode changes.
    useEffect(() => {
        const resolved = resolveTheme(mode);
        setTheme(resolved);
        applyTheme(resolved);
    }, [mode]);

    // Follow system changes when mode is 'system'.
    useEffect(() => {
        if (mode !== 'system' || typeof window === 'undefined') return;
        const mq = window.matchMedia('(prefers-color-scheme: dark)');
        const handler = () => {
            const next = mq.matches ? 'dark' : 'light';
            setTheme(next);
            applyTheme(next);
        };
        mq.addEventListener('change', handler);
        return () => mq.removeEventListener('change', handler);
    }, [mode]);

    const setMode = (next: ThemeMode) => {
        setModeState(next);
        if (typeof window !== 'undefined') {
            window.localStorage.setItem(STORAGE_KEY, next);
        }
    };

    return (
        <ThemeContext.Provider value={{ theme, mode, setMode }}>
            {children}
        </ThemeContext.Provider>
    );
}

export function useTheme(): ThemeContextValue {
    const ctx = useContext(ThemeContext);
    if (!ctx) throw new Error('useTheme must be used inside <ThemeProvider>');
    return ctx;
}
