'use client';

import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState } from 'react';
import type { ReactNode } from 'react';
import styles from './Toast.module.css';

type ToastVariant = 'info' | 'success' | 'warning' | 'error';

interface ToastInput {
    title?: string;
    description?: string;
    variant?: ToastVariant;
    durationMs?: number;
}

interface ToastItem extends Required<Omit<ToastInput, 'description'>> {
    id: number;
    description?: string;
}

interface ToastContextValue {
    show: (input: ToastInput) => number;
    dismiss: (id: number) => void;
    success: (description: string, title?: string) => number;
    error: (description: string, title?: string) => number;
    info: (description: string, title?: string) => number;
    warning: (description: string, title?: string) => number;
}

const ToastContext = createContext<ToastContextValue | null>(null);

const DEFAULT_DURATION_MS = 4000;

export function ToastProvider({ children }: { children: ReactNode }) {
    const [toasts, setToasts] = useState<ToastItem[]>([]);
    const idRef = useRef(0);

    const dismiss = useCallback((id: number) => {
        setToasts(prev => prev.filter(t => t.id !== id));
    }, []);

    const show = useCallback((input: ToastInput): number => {
        idRef.current += 1;
        const id = idRef.current;
        const item: ToastItem = {
            id,
            title: input.title ?? '',
            description: input.description,
            variant: input.variant ?? 'info',
            durationMs: input.durationMs ?? DEFAULT_DURATION_MS,
        };
        setToasts(prev => [...prev, item]);
        return id;
    }, []);

    const variants: Record<ToastVariant, (description: string, title?: string) => number> = useMemo(() => ({
        success: (description, title) => show({ description, title, variant: 'success' }),
        error: (description, title) => show({ description, title, variant: 'error' }),
        info: (description, title) => show({ description, title, variant: 'info' }),
        warning: (description, title) => show({ description, title, variant: 'warning' }),
    }), [show]);

    const value = useMemo<ToastContextValue>(() => ({
        show,
        dismiss,
        ...variants,
    }), [show, dismiss, variants]);

    return (
        <ToastContext.Provider value={value}>
            {children}
            <ToastViewport toasts={toasts} onDismiss={dismiss} />
        </ToastContext.Provider>
    );
}

interface ToastViewportProps {
    toasts: ToastItem[];
    onDismiss: (id: number) => void;
}

function ToastViewport({ toasts, onDismiss }: ToastViewportProps) {
    return (
        <div className={styles.viewport} role="region" aria-label="Notifications">
            {toasts.map(t => (
                <ToastView key={t.id} toast={t} onDismiss={onDismiss} />
            ))}
        </div>
    );
}

function ToastView({ toast, onDismiss }: { toast: ToastItem; onDismiss: (id: number) => void }) {
    useEffect(() => {
        if (toast.durationMs <= 0) return;
        const timer = window.setTimeout(() => onDismiss(toast.id), toast.durationMs);
        return () => window.clearTimeout(timer);
    }, [toast.id, toast.durationMs, onDismiss]);

    return (
        <div className={`${styles.toast} ${styles[toast.variant]}`} role="status">
            <div className={styles.body}>
                {toast.title && <div className={styles.title}>{toast.title}</div>}
                {toast.description && <div className={styles.description}>{toast.description}</div>}
            </div>
            <button
                type="button"
                className={styles.close}
                aria-label="Dismiss notification"
                onClick={() => onDismiss(toast.id)}
            >
                ×
            </button>
        </div>
    );
}

export function useToast(): ToastContextValue {
    const ctx = useContext(ToastContext);
    if (!ctx) {
        throw new Error('useToast must be used within a ToastProvider');
    }
    return ctx;
}