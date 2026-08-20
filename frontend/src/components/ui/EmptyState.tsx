'use client';

import type { ReactNode } from 'react';
import styles from './EmptyState.module.css';

interface EmptyStateProps {
    title: string;
    description?: string;
    icon?: ReactNode;
    action?: ReactNode;
}

export function EmptyState({ title, description, icon, action }: EmptyStateProps) {
    return (
        <div className={styles.empty} role="status">
            {icon && <div className={styles.icon}>{icon}</div>}
            <h3 className={styles.title}>{title}</h3>
            {description && <p className={styles.description}>{description}</p>}
            {action && <div className={styles.action}>{action}</div>}
        </div>
    );
}

interface ErrorStateProps {
    title: string;
    description?: string;
    action?: ReactNode;
}

export function ErrorState({ title, description, action }: ErrorStateProps) {
    return (
        <div className={styles.empty} role="alert">
            <div className={styles.icon} aria-hidden>!</div>
            <h3 className={styles.title}>{title}</h3>
            {description && <p className={styles.description}>{description}</p>}
            {action && <div className={styles.action}>{action}</div>}
        </div>
    );
}
