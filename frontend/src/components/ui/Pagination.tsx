'use client';

import styles from './Pagination.module.css';

interface PaginationProps {
    page: number;
    pageSize: number;
    totalItems: number;
    totalPages: number;
    hasNext: boolean;
    hasPrevious: boolean;
    onPageChange: (page: number) => void;
    onPageSizeChange?: (size: number) => void;
}

const PAGE_SIZES = [10, 25, 50, 100];

export function Pagination({
    page,
    pageSize,
    totalItems,
    totalPages,
    hasNext,
    hasPrevious,
    onPageChange,
    onPageSizeChange,
}: PaginationProps) {
    const start = totalItems === 0 ? 0 : (page - 1) * pageSize + 1;
    const end = Math.min(page * pageSize, totalItems);

    return (
        <div className={styles.bar}>
            <div className={styles.summary}>
                Showing {start.toLocaleString()}–{end.toLocaleString()} of {totalItems.toLocaleString()}
            </div>
            <div className={styles.controls}>
                {onPageSizeChange && (
                    <label className={styles.pageSize}>
                        <span>Rows:</span>
                        <select
                            value={pageSize}
                            onChange={e => onPageSizeChange(Number(e.target.value))}
                        >
                            {PAGE_SIZES.map(size => (
                                <option key={size} value={size}>{size}</option>
                            ))}
                        </select>
                    </label>
                )}
                <button
                    type="button"
                    className={styles.button}
                    onClick={() => onPageChange(page - 1)}
                    disabled={!hasPrevious}
                    aria-label="Previous page"
                >
                    ‹ Prev
                </button>
                <span className={styles.page}>
                    Page {page} of {Math.max(totalPages, 1)}
                </span>
                <button
                    type="button"
                    className={styles.button}
                    onClick={() => onPageChange(page + 1)}
                    disabled={!hasNext}
                    aria-label="Next page"
                >
                    Next ›
                </button>
            </div>
        </div>
    );
}