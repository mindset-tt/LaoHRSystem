'use client';

import { useState, useEffect } from 'react';
import { Button } from '@/components/ui/Button';
import { payrollApi, reportsApi } from '@/lib/endpoints';
import type { PayrollPeriod } from '@/lib/types';
import styles from './page.module.css';
import { formatDate } from '@/lib/datetime';

export default function ReportsPage() {
    const [periods, setPeriods] = useState<PayrollPeriod[]>([]);
    const [loading, setLoading] = useState(true);
    const [nssfPeriod, setNssfPeriod] = useState<string>('');
    const [downloading, setDownloading] = useState(false);

    useEffect(() => {
        loadPeriods();
    }, []);

    const loadPeriods = async () => {
        try {
            const data = await payrollApi.getPeriods();
            setPeriods(data);
            if (data.length > 0) {
                setNssfPeriod(data[0].periodId.toString());
            }
        } catch (error) {
            console.error('Failed to load payroll periods:', error);
        } finally {
            setLoading(false);
        }
    };

    const handleDownloadNssf = async () => {
        if (!nssfPeriod) return;

        setDownloading(true);
        try {
            const blob = await reportsApi.downloadNssfReport(parseInt(nssfPeriod));
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `nssf-report-${nssfPeriod}.pdf`;
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(url);
            document.body.removeChild(a);
        } catch (error) {
            console.error('Failed to download NSSF report:', error);
            alert('Failed to download report');
        } finally {
            setDownloading(false);
        }
    };

    return (
        <div className={styles.container}>
            <div className={styles.header}>
                <h1 className={styles.title}>Reports</h1>
                <p className={styles.subtitle}>Generate and download HR and Payroll reports</p>
            </div>

            <div className={styles.grid}>
                {/* NSSF Report Card */}
                <div className={styles.card}>
                    <div className={styles.cardIcon}>
                        <FileTextIcon />
                    </div>
                    <h3 className={styles.cardTitle}>NSSF Report</h3>
                    <p className={styles.cardDescription}>
                        Generate standard NSSF contribution report (PDF) for a specific payroll period.
                    </p>

                    <div className={styles.formGroup}>
                        <label className={styles.label}>Select Period</label>
                        <select
                            className={styles.select}
                            value={nssfPeriod}
                            onChange={(e) => setNssfPeriod(e.target.value)}
                            disabled={loading || periods.length === 0}
                        >
                            {periods.map((period) => (
                                <option key={period.periodId} value={period.periodId}>
                                    {formatDate(period.startDate)} - {formatDate(period.endDate)}
                                </option>
                            ))}
                        </select>
                    </div>

                    <Button
                        onClick={handleDownloadNssf}
                        disabled={downloading || !nssfPeriod}
                        loading={downloading}
                        className={styles.downloadBtn}
                    >
                        <DownloadIcon />
                        Download PDF
                    </Button>
                </div>

                {/* Placeholder for other reports */}
                <div className={styles.card}>
                    <div className={styles.cardIcon}>
                        <ChartIcon />
                    </div>
                    <h3 className={styles.cardTitle}>Employee Statistics</h3>
                    <p className={styles.cardDescription}>
                        Overview of employee demographics, department distribution, and status.
                    </p>
                    <Button variant="secondary" disabled>
                        Coming Soon
                    </Button>
                </div>
            </div>
        </div>
    );
}

function FileTextIcon() {
    return (
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
            <polyline points="14 2 14 8 20 8" />
            <line x1="16" y1="13" x2="8" y2="13" />
            <line x1="16" y1="17" x2="8" y2="17" />
            <polyline points="10 9 9 9 8 9" />
        </svg>
    );
}

function DownloadIcon() {
    return (
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
            <polyline points="7 10 12 15 17 10" />
            <line x1="12" y1="15" x2="12" y2="3" />
        </svg>
    );
}

function ChartIcon() {
    return (
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <line x1="18" y1="20" x2="18" y2="10" />
            <line x1="12" y1="20" x2="12" y2="4" />
            <line x1="6" y1="20" x2="6" y2="14" />
        </svg>
    );
}
