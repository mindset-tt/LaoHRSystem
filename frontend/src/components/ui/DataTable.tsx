'use client';

import { useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import { EmptyState } from './EmptyState';
import styles from './DataTable.module.css';

export interface DataTableColumn<T> {
    key: string;
    header: string;
    /** When set, rows will only sort when this is provided. */
    sortBy?: keyof T | ((row: T) => string | number | Date | null | undefined);
    align?: 'left' | 'right' | 'center';
    width?: string;
    render: (row: T) => ReactNode;
}

interface DataTableProps<T> {
    columns: DataTableColumn<T>[];
    rows: T[];
    rowKey: (row: T) => string | number;
    isLoading?: boolean;
    emptyTitle?: string;
    emptyDescription?: string;
    emptyAction?: ReactNode;
    onRowClick?: (row: T) => void;
}

type SortDirection = 'asc' | 'desc';

export function DataTable<T>({
    columns,
    rows,
    rowKey,
    isLoading,
    emptyTitle = 'No records yet',
    emptyDescription,
    emptyAction,
    onRowClick,
}: DataTableProps<T>) {
    const [sortKey, setSortKey] = useState<string | null>(null);
    const [sortDir, setSortDir] = useState<SortDirection>('asc');

    const sortedRows = useMemo(() => {
        if (!sortKey) return rows;
        const col = columns.find(c => c.key === sortKey);
        if (!col?.sortBy) return rows;
        const accessor = col.sortBy;
        const copy = [...rows];
        copy.sort((a, b) => {
            const av = typeof accessor === 'function' ? accessor(a) : (a as Record<string, unknown>)[accessor as string];
            const bv = typeof accessor === 'function' ? accessor(b) : (b as Record<string, unknown>)[accessor as string];
            if (av == null && bv == null) return 0;
            if (av == null) return 1;
            if (bv == null) return -1;
            if (av instanceof Date && bv instanceof Date) return av.getTime() - bv.getTime();
            if (typeof av === 'number' && typeof bv === 'number') return av - bv;
            return String(av).localeCompare(String(bv));
        });
        return sortDir === 'desc' ? copy.reverse() : copy;
    }, [rows, sortKey, sortDir, columns]);

    function handleHeaderClick(col: DataTableColumn<T>) {
        if (!col.sortBy) return;
        if (sortKey === col.key) {
            setSortDir(prev => (prev === 'asc' ? 'desc' : 'asc'));
        } else {
            setSortKey(col.key);
            setSortDir('asc');
        }
    }

    if (isLoading) {
        return (
            <div className={styles.loading} role="status" aria-live="polite">
                Loading…
            </div>
        );
    }

    if (sortedRows.length === 0) {
        return (
            <EmptyState
                title={emptyTitle}
                description={emptyDescription}
                action={emptyAction}
            />
        );
    }

    return (
        <div className={styles.wrapper}>
            <table className={styles.table}>
                <thead>
                    <tr>
                        {columns.map(col => {
                            const isSorted = sortKey === col.key;
                            const indicator = isSorted ? (sortDir === 'asc' ? '▲' : '▼') : '';
                            return (
                                <th
                                    key={col.key}
                                    className={`${styles.th} ${col.sortBy ? styles.sortable : ''} ${styles[`align-${col.align ?? 'left'}`]}`}
                                    style={col.width ? { width: col.width } : undefined}
                                    onClick={() => handleHeaderClick(col)}
                                    aria-sort={isSorted ? (sortDir === 'asc' ? 'ascending' : 'descending') : 'none'}
                                >
                                    <span>{col.header}</span>
                                    {isSorted && <span className={styles.sortIndicator} aria-hidden>{indicator}</span>}
                                </th>
                            );
                        })}
                    </tr>
                </thead>
                <tbody>
                    {sortedRows.map(row => (
                        <tr
                            key={rowKey(row)}
                            className={onRowClick ? styles.clickable : undefined}
                            onClick={onRowClick ? () => onRowClick(row) : undefined}
                        >
                            {columns.map(col => (
                                <td
                                    key={col.key}
                                    className={`${styles.td} ${styles[`align-${col.align ?? 'left'}`]}`}
                                >
                                    {col.render(row)}
                                </td>
                            ))}
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}