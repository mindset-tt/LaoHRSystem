'use client';

import { useEffect, useState, useCallback } from 'react';
import { useAuth } from '@/components/providers/AuthProvider';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Skeleton } from '@/components/ui/Skeleton';
import { conversionRatesApi } from '@/lib/endpoints';
import { isHROrAdmin } from '@/lib/permissions';
import type { ConversionRate, CurrencyCode } from '@/lib/types';
import styles from './page.module.css';

const CURRENCIES: { code: CurrencyCode; name: string; nameLao: string; symbol: string }[] = [
    { code: 'USD', name: 'US Dollar', nameLao: 'ໂດລາສະຫະລັດ', symbol: '$' },
    { code: 'THB', name: 'Thai Baht', nameLao: 'ບາດໄທ', symbol: '฿' },
    { code: 'CNY', name: 'Chinese Yuan', nameLao: 'ຫຍວນຈີນ', symbol: '¥' },
];

const DEFAULT_RATES: Record<CurrencyCode, number> = {
    USD: 22000,
    THB: 650,
    CNY: 3100,
    LAK: 1,
};

/**
 * Currency Rates Settings Page
 */
export default function CurrencyRatesPage() {
    const { role } = useAuth();
    const { language } = useLanguage();
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [currentRates, setCurrentRates] = useState<ConversionRate[]>([]);
    const [allRates, setAllRates] = useState<ConversionRate[]>([]);
    const [editRates, setEditRates] = useState<Record<string, number>>({});
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);
    const [showHistory, setShowHistory] = useState(false);

    const canEdit = isHROrAdmin(role);

    const loadRates = useCallback(async () => {
        try {
            setError(null);
            const [current, all] = await Promise.all([
                conversionRatesApi.getCurrent(),
                conversionRatesApi.getAll(),
            ]);
            setCurrentRates(current);
            setAllRates(all);

            // Initialize edit rates from current rates or defaults
            const rateMap: Record<string, number> = {};
            CURRENCIES.forEach(cur => {
                const existing = current.find(r => r.fromCurrency === cur.code);
                rateMap[cur.code] = existing?.rate ?? DEFAULT_RATES[cur.code];
            });
            setEditRates(rateMap);
        } catch (err) {
            console.error('Failed to load rates:', err);
            // Initialize with defaults if API fails
            const rateMap: Record<string, number> = {};
            CURRENCIES.forEach(cur => {
                rateMap[cur.code] = DEFAULT_RATES[cur.code];
            });
            setEditRates(rateMap);
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        loadRates();
    }, [loadRates]);

    const handleRateChange = (currency: string, value: string) => {
        const numValue = parseFloat(value) || 0;
        setEditRates(prev => ({ ...prev, [currency]: numValue }));
    };

    const handleSave = async () => {
        setSaving(true);
        setError(null);
        setSuccess(null);

        try {
            for (const currency of CURRENCIES) {
                const newRate = editRates[currency.code];
                const existing = currentRates.find(r => r.fromCurrency === currency.code);

                // Always create new rate to preserve history
                if (!existing || existing.rate !== newRate) {
                    await conversionRatesApi.create({
                        fromCurrency: currency.code,
                        toCurrency: 'LAK',
                        rate: newRate,
                        effectiveDate: new Date().toISOString(),
                    });
                }
            }

            setSuccess(language === 'lo' ? 'ບັນທຶກສຳເລັດ' : 'Rates updated successfully');
            loadRates();
        } catch (err) {
            console.error('Failed to save:', err);
            setError(language === 'lo' ? 'ບໍ່ສາມາດບັນທຶກໄດ້' : 'Failed to save changes');
        } finally {
            setSaving(false);
        }
    };

    const formatDate = (dateStr: string) => {
        return new Date(dateStr).toLocaleDateString(language === 'lo' ? 'lo-LA' : 'en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
        });
    };

    if (!canEdit) {
        return (
            <div className={styles.container}>
                <Card>
                    <div className={styles.noAccess}>
                        <p>{language === 'lo' ? 'ທ່ານບໍ່ມີສິດເຂົ້າເຖິງໜ້ານີ້' : 'You do not have access to this page'}</p>
                    </div>
                </Card>
            </div>
        );
    }

    return (
        <div className={styles.container}>
            <div className={styles.header}>
                <h1 className={styles.title}>
                    {language === 'lo' ? 'ອັດຕາແລກປ່ຽນເງິນ' : 'Currency Exchange Rates'}
                </h1>
                <p className={styles.subtitle}>
                    {language === 'lo'
                        ? 'ກຳນົດອັດຕາແລກປ່ຽນສຳລັບຄຳນວນເງິນເດືອນ'
                        : 'Set exchange rates for payroll calculations'}
                </p>
            </div>

            {/* Payroll Info Note */}
            <Card>
                <div className={styles.infoBox}>
                    <div className={styles.infoIcon}>💡</div>
                    <div>
                        <h3 className={styles.infoTitle}>
                            {language === 'lo' ? 'ວິທີໃຊ້ໃນການຄິດໄລ່ເງິນເດືອນ' : 'How Payroll Uses Exchange Rates'}
                        </h3>
                        <ul className={styles.infoList}>
                            <li>
                                {language === 'lo'
                                    ? 'ເມື່ອຄິດໄລ່ເງິນເດືອນ, ລະບົບຈະໃຊ້ອັດຕາແລກປ່ຽນຕາມວັນທີສິ້ນສຸດຂອງງວດເງິນເດືອນ'
                                    : 'When calculating payroll, the system uses the rate effective on the period end date'}
                            </li>
                            <li>
                                {language === 'lo'
                                    ? 'ຖ້າພະນັກງານມີຖານເງິນເດືອນເປັນ USD ຫຼື THB, ລະບົບຈະແປງເປັນ LAK ກ່ອນຄິດໄລ່'
                                    : 'If employee salary is in USD/THB/CNY, system converts to LAK before calculation'}
                            </li>
                            <li>
                                {language === 'lo'
                                    ? 'ປະຫວັດອັດຕາແລກປ່ຽນຈະຖືກບັນທຶກໄວ້ສຳລັບການກວດສອບ'
                                    : 'Rate history is preserved for audit and tracking purposes'}
                            </li>
                        </ul>
                    </div>
                </div>
            </Card>

            {/* Alerts */}
            {error && (
                <div className={styles.errorAlert}>
                    {error}
                    <button onClick={() => setError(null)}>×</button>
                </div>
            )}
            {success && (
                <div className={styles.successAlert}>
                    {success}
                    <button onClick={() => setSuccess(null)}>×</button>
                </div>
            )}

            {loading ? (
                <Skeleton height={300} />
            ) : (
                <div className={styles.content}>
                    {/* Current Rates */}
                    <Card>
                        <h2 className={styles.sectionTitle}>
                            {language === 'lo' ? 'ອັດຕາແລກປ່ຽນປັດຈຸບັນ' : 'Current Exchange Rates'}
                        </h2>
                        <p className={styles.hint}>
                            {language === 'lo'
                                ? '1 ສະກຸນເງິນ = X ກີບ'
                                : '1 Currency = X LAK'}
                        </p>

                        <div className={styles.ratesGrid}>
                            {CURRENCIES.map(currency => {
                                const current = currentRates.find(r => r.fromCurrency === currency.code);
                                return (
                                    <div key={currency.code} className={styles.rateCard}>
                                        <div className={styles.currencyInfo}>
                                            <span className={styles.currencySymbol}>{currency.symbol}</span>
                                            <div>
                                                <div className={styles.currencyCode}>{currency.code}</div>
                                                <div className={styles.currencyName}>
                                                    {language === 'lo' ? currency.nameLao : currency.name}
                                                </div>
                                                {current && (
                                                    <div className={styles.effectiveDate}>
                                                        {language === 'lo' ? 'ມີຜົນ:' : 'Effective:'} {formatDate(current.effectiveDate)}
                                                    </div>
                                                )}
                                            </div>
                                        </div>
                                        <div className={styles.rateInput}>
                                            <span className={styles.ratePrefix}>1 {currency.code} =</span>
                                            <input
                                                type="number"
                                                className={styles.input}
                                                value={editRates[currency.code] || ''}
                                                onChange={(e) => handleRateChange(currency.code, e.target.value)}
                                                min={0}
                                                step={1}
                                            />
                                            <span className={styles.rateSuffix}>LAK</span>
                                        </div>
                                    </div>
                                );
                            })}
                        </div>
                    </Card>

                    {/* Preview */}
                    <Card>
                        <h2 className={styles.sectionTitle}>
                            {language === 'lo' ? 'ຕົວຢ່າງການແປງ' : 'Conversion Preview'}
                        </h2>
                        <div className={styles.preview}>
                            {CURRENCIES.map(currency => (
                                <div key={currency.code} className={styles.previewItem}>
                                    <span className={styles.previewFrom}>
                                        {currency.symbol}100 {currency.code}
                                    </span>
                                    <span className={styles.previewArrow}>→</span>
                                    <span className={styles.previewTo}>
                                        {(100 * (editRates[currency.code] || 0)).toLocaleString()} LAK
                                    </span>
                                </div>
                            ))}
                        </div>
                    </Card>

                    {/* Save Button */}
                    <div className={styles.actions}>
                        <Button onClick={handleSave} loading={saving}>
                            {language === 'lo' ? 'ບັນທຶກ' : 'Save Changes'}
                        </Button>
                    </div>

                    {/* History Section */}
                    <Card>
                        <div className={styles.historyHeader}>
                            <h2 className={styles.sectionTitle}>
                                {language === 'lo' ? 'ປະຫວັດອັດຕາແລກປ່ຽນ' : 'Exchange Rate History'}
                            </h2>
                            <button
                                className={styles.toggleButton}
                                onClick={() => setShowHistory(!showHistory)}
                            >
                                {showHistory
                                    ? (language === 'lo' ? 'ເຊື່ອງ' : 'Hide')
                                    : (language === 'lo' ? 'ສະແດງ' : 'Show')}
                            </button>
                        </div>

                        {showHistory && (
                            <div className={styles.historyTable}>
                                <table>
                                    <thead>
                                        <tr>
                                            <th>{language === 'lo' ? 'ສະກຸນເງິນ' : 'Currency'}</th>
                                            <th>{language === 'lo' ? 'ອັດຕາ' : 'Rate'}</th>
                                            <th>{language === 'lo' ? 'ວັນທີມີຜົນ' : 'Effective Date'}</th>
                                            <th>{language === 'lo' ? 'ວັນໝົດອາຍຸ' : 'Expiry Date'}</th>
                                            <th>{language === 'lo' ? 'ສະຖານະ' : 'Status'}</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {allRates.length === 0 ? (
                                            <tr>
                                                <td colSpan={5} className={styles.emptyRow}>
                                                    {language === 'lo' ? 'ບໍ່ມີຂໍ້ມູນ' : 'No history available'}
                                                </td>
                                            </tr>
                                        ) : (
                                            allRates.map(rate => (
                                                <tr key={rate.conversionRateId}>
                                                    <td>
                                                        <strong>{rate.fromCurrency}</strong> → {rate.toCurrency}
                                                    </td>
                                                    <td className={styles.rateCell}>
                                                        {rate.rate.toLocaleString()}
                                                    </td>
                                                    <td>{formatDate(rate.effectiveDate)}</td>
                                                    <td>
                                                        {rate.expiryDate
                                                            ? formatDate(rate.expiryDate)
                                                            : (language === 'lo' ? 'ປັດຈຸບັນ' : 'Current')}
                                                    </td>
                                                    <td>
                                                        <span className={`${styles.statusBadge} ${rate.isActive && !rate.expiryDate ? styles.active : styles.expired}`}>
                                                            {rate.isActive && !rate.expiryDate
                                                                ? (language === 'lo' ? 'ໃຊ້ງານ' : 'Active')
                                                                : (language === 'lo' ? 'ໝົດອາຍຸ' : 'Expired')}
                                                        </span>
                                                    </td>
                                                </tr>
                                            ))
                                        )}
                                    </tbody>
                                </table>
                            </div>
                        )}
                    </Card>
                </div>
            )}
        </div>
    );
}
