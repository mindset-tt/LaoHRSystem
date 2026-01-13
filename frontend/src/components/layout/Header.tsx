'use client';

import { useAuth } from '@/components/providers/AuthProvider';
import styles from './Header.module.css';

interface HeaderProps {
    /** Page title */
    title?: string;
    /** Breadcrumb items */
    breadcrumbs?: { label: string; href?: string }[];
    /** Actions to show on the right */
    actions?: React.ReactNode;
}

/**
 * Header Component
 * Top bar with title, breadcrumbs, and actions
 */
export function Header({ title, breadcrumbs, actions }: HeaderProps) {
    const { user } = useAuth();

    const greeting = getGreeting();

    return (
        <header className={styles.header}>
            <div className={styles.left}>
                {breadcrumbs && breadcrumbs.length > 0 ? (
                    <nav className={styles.breadcrumbs} aria-label="Breadcrumb">
                        {breadcrumbs.map((crumb, index) => (
                            <span key={index} className={styles.breadcrumbItem}>
                                {index > 0 && <span className={styles.separator}>/</span>}
                                {crumb.href ? (
                                    <a href={crumb.href} className={styles.breadcrumbLink}>
                                        {crumb.label}
                                    </a>
                                ) : (
                                    <span className={styles.breadcrumbCurrent}>{crumb.label}</span>
                                )}
                            </span>
                        ))}
                    </nav>
                ) : title ? (
                    <h1 className={styles.title}>{title}</h1>
                ) : (
                    <div className={styles.greeting}>
                        <span className={styles.greetingText}>{greeting}</span>
                        {user && (
                            <span className={styles.userName}>{user.displayName}</span>
                        )}
                    </div>
                )}
            </div>

            {actions && <div className={styles.actions}>{actions}</div>}
        </header>
    );
}

/**
 * Get greeting based on time of day (Lao timezone)
 */
function getGreeting(): string {
    const hour = new Date().getHours();

    if (hour < 12) return 'Good morning,';
    if (hour < 17) return 'Good afternoon,';
    return 'Good evening,';
}
