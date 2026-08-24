'use client';

import type { BreakdownItem } from '@/lib/endpoints';
import styles from './BreakdownList.module.css';

export interface BreakdownListProps {
    title: string;
    items: BreakdownItem[];
    emptyText?: string;
}

/**
 * Phase 3C3 — accessible breakdown list (bar + value + table fallback).
 * Communicates magnitude via bar width AND numeric value (not color alone).
 */
export function BreakdownList({ title, items, emptyText = 'No data' }: BreakdownListProps) {
    const max = items.length > 0 ? Math.max(...items.map(i => i.value), 1) : 1;

    return (
        <div className={styles.container}>
            <h3 className={styles.title}>{title}</h3>
            {items.length === 0 ? (
                <p className={styles.empty}>{emptyText}</p>
            ) : (
                <ul className={styles.list} role="list">
                    {items.map(item => (
                        <li key={item.key} className={styles.row}>
                            <span className={styles.label}>{item.label}</span>
                            <div className={styles.barTrack}>
                                <div
                                    className={styles.bar}
                                    style={{ width: `${(item.value / max) * 100}%` }}
                                    aria-hidden="true"
                                />
                            </div>
                            <span className={styles.value}>{formatNumber(item.value)}</span>
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}

function formatNumber(n: number): string {
    if (Number.isInteger(n)) return n.toLocaleString();
    return n.toLocaleString(undefined, { maximumFractionDigits: 2 });
}
