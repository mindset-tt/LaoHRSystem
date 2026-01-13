import { type ReactNode, type HTMLAttributes } from 'react';
import styles from './Card.module.css';

export interface CardProps extends HTMLAttributes<HTMLDivElement> {
    /** Card variant */
    variant?: 'default' | 'elevated' | 'outlined';
    /** Optional header content */
    header?: ReactNode;
    /** Optional footer content */
    footer?: ReactNode;
    /** Removes default padding */
    noPadding?: boolean;
    /** Makes the card clickable with hover effects */
    interactive?: boolean;
}

/**
 * Card Component
 * Premium container with subtle shadows and optional header/footer
 */
export function Card({
    variant = 'default',
    header,
    footer,
    noPadding = false,
    interactive = false,
    children,
    className = '',
    ...props
}: CardProps) {
    return (
        <div
            className={`
        ${styles.card}
        ${styles[variant]}
        ${noPadding ? styles.noPadding : ''}
        ${interactive ? styles.interactive : ''}
        ${className}
      `.trim()}
            {...props}
        >
            {header && <div className={styles.header}>{header}</div>}
            <div className={noPadding ? '' : styles.body}>{children}</div>
            {footer && <div className={styles.footer}>{footer}</div>}
        </div>
    );
}

/**
 * Card Header Component
 * For use with custom layouts
 */
export function CardHeader({
    children,
    className = '',
    ...props
}: HTMLAttributes<HTMLDivElement>) {
    return (
        <div className={`${styles.header} ${className}`} {...props}>
            {children}
        </div>
    );
}

/**
 * Card Title Component
 */
export function CardTitle({
    children,
    className = '',
    ...props
}: HTMLAttributes<HTMLHeadingElement>) {
    return (
        <h3 className={`${styles.title} ${className}`} {...props}>
            {children}
        </h3>
    );
}

/**
 * Card Description Component
 */
export function CardDescription({
    children,
    className = '',
    ...props
}: HTMLAttributes<HTMLParagraphElement>) {
    return (
        <p className={`${styles.description} ${className}`} {...props}>
            {children}
        </p>
    );
}
