import styles from './Skeleton.module.css';

export interface SkeletonProps {
    /** Width of the skeleton */
    width?: string | number;
    /** Height of the skeleton */
    height?: string | number;
    /** Makes the skeleton circular */
    circle?: boolean;
    /** Additional CSS class */
    className?: string;
}

/**
 * Skeleton Loading Component
 * For perceived performance - shows placeholder while content loads
 */
export function Skeleton({
    width,
    height,
    circle = false,
    className = '',
}: SkeletonProps) {
    return (
        <div
            className={`${styles.skeleton} ${circle ? styles.circle : ''} ${className}`}
            style={{
                width: typeof width === 'number' ? `${width}px` : width,
                height: typeof height === 'number' ? `${height}px` : height,
            }}
            aria-hidden="true"
        />
    );
}

/**
 * Skeleton Text - for text placeholders
 */
export function SkeletonText({
    lines = 3,
    className = '',
}: {
    lines?: number;
    className?: string;
}) {
    return (
        <div className={`${styles.textWrapper} ${className}`}>
            {Array.from({ length: lines }).map((_, i) => (
                <div
                    key={i}
                    className={styles.skeleton}
                    style={{
                        height: '16px',
                        width: i === lines - 1 ? '60%' : '100%',
                    }}
                />
            ))}
        </div>
    );
}

/**
 * Skeleton Avatar - for profile picture placeholders
 */
export function SkeletonAvatar({
    size = 40,
    className = '',
}: {
    size?: number;
    className?: string;
}) {
    return (
        <Skeleton
            width={size}
            height={size}
            circle
            className={className}
        />
    );
}

/**
 * Skeleton Table - for data table placeholders
 */
export function SkeletonTable({
    rows = 5,
    columns = 4,
    className = '',
}: {
    rows?: number;
    columns?: number;
    className?: string;
}) {
    return (
        <div className={`${styles.table} ${className}`}>
            {/* Header */}
            <div className={styles.tableRow}>
                {Array.from({ length: columns }).map((_, i) => (
                    <div key={i} className={styles.tableCell}>
                        <Skeleton height={16} />
                    </div>
                ))}
            </div>
            {/* Rows */}
            {Array.from({ length: rows }).map((_, rowIndex) => (
                <div key={rowIndex} className={styles.tableRow}>
                    {Array.from({ length: columns }).map((_, colIndex) => (
                        <div key={colIndex} className={styles.tableCell}>
                            <Skeleton height={14} width={colIndex === 0 ? '70%' : '50%'} />
                        </div>
                    ))}
                </div>
            ))}
        </div>
    );
}

/**
 * Skeleton Card - for card placeholders
 */
export function SkeletonCard({ className = '' }: { className?: string }) {
    return (
        <div className={`${styles.card} ${className}`}>
            <Skeleton height={120} />
            <div className={styles.cardBody}>
                <Skeleton height={20} width="60%" />
                <SkeletonText lines={2} />
            </div>
        </div>
    );
}
