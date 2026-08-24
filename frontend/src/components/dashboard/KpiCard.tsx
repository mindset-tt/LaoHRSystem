'use client';

import type { ReactNode } from 'react';
import styles from './KpiCard.module.css';

export interface KpiCardProps {
    label: string;
    value: number | null;
    unit?: string;
    delta?: number | null;
    comparisonLabel?: string;
    icon?: ReactNode;
    loading?: boolean;
}

/**
 * Phase 3C3 — reusable KPI card. Shows a value with optional delta and unit.
 */
export function KpiCard({ label, value, unit, delta, comparisonLabel, icon, loading }: KpiCardProps) {
    return (
        <div className={styles.card}>
            <div className={styles.header}>
                <span className={styles.label}>{label}</span>
                {icon && <span className={styles.icon}>{icon}</span>}
            </div>
            <div className={styles.value}>
                {loading ? '—' : value === null ? '—' : formatNumber(value)}
                {unit && !loading && value !== null && <span className={styles.unit}>{unit}</span>}
            </div>
            {delta !== null && delta !== undefined && (
                <div className={`${styles.delta} ${delta >= 0 ? styles.up : styles.down}`}>
                    {delta >= 0 ? '▲' : '▼'} {formatNumber(Math.abs(delta))}
                    {comparisonLabel && <span className={styles.comparison}>{comparisonLabel}</span>}
                </div>
            )}
        </div>
    );
}

function formatNumber(n: number): string {
    if (Number.isInteger(n)) return n.toLocaleString();
    return n.toLocaleString(undefined, { maximumFractionDigits: 2 });
}
