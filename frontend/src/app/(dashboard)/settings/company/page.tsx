'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { companyApi, addressApi } from '@/lib/endpoints/company';
import type { CompanySetting, Province, District, Village } from '@/lib/types';

import Link from 'next/link';
import { useLanguage } from '@/components/providers/LanguageProvider';
import styles from './page.module.css';

export default function CompanySettingsPage() {
    const router = useRouter();
    const { t, language } = useLanguage();
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);

    // Form State
    const [formData, setFormData] = useState<Partial<CompanySetting>>({});

    // Address Data
    const [provinces, setProvinces] = useState<Province[]>([]);
    const [districts, setDistricts] = useState<District[]>([]);
    const [villages, setVillages] = useState<Village[]>([]);

    useEffect(() => {
        loadInitialData();
    }, []);

    // Load initial data
    const loadInitialData = async () => {
        try {
            setLoading(true);
            const [settingsData, provincesData] = await Promise.all([
                companyApi.getSettings().catch(() => ({}) as CompanySetting), // Handle first time empty
                addressApi.getProvinces()
            ]);

            setFormData(settingsData);
            setProvinces(provincesData || []);

            // Cascade load if data exists
            if (settingsData.provinceId) {
                const districtsData = await addressApi.getDistricts(settingsData.provinceId);
                setDistricts(districtsData);
            }
            if (settingsData.districtId) {
                const villagesData = await addressApi.getVillages(settingsData.districtId);
                setVillages(villagesData);
            }
        } catch (error) {
            console.error('Failed to load settings', error);
        } finally {
            setLoading(false);
        }
    };

    const handleChange = (field: keyof CompanySetting, value: string | number) => {
        setFormData(prev => ({ ...prev, [field]: value }));
    };

    const handleProvinceChange = async (e: React.ChangeEvent<HTMLSelectElement>) => {
        const prId = Number(e.target.value);
        handleChange('provinceId', prId);

        // Reset sub-fields
        handleChange('districtId', 0);
        handleChange('villageId', 0);
        setDistricts([]);
        setVillages([]);

        if (prId) {
            const data = await addressApi.getDistricts(prId);
            setDistricts(data);
        }
    };

    const handleDistrictChange = async (e: React.ChangeEvent<HTMLSelectElement>) => {
        const diId = Number(e.target.value);
        handleChange('districtId', diId);

        // Reset sub-field
        handleChange('villageId', 0);
        setVillages([]);

        if (diId) {
            const data = await addressApi.getVillages(diId);
            setVillages(data);
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            setSaving(true);
            await companyApi.updateSettings(formData);
            alert(t.common.saved);
        } catch (error) {
            console.error('Failed to save settings', error);
            alert(t.common.error);
        } finally {
            setSaving(false);
        }
    };

    if (loading) {
        return <div className="p-8 text-center text-neutral-500">{t.common.loading}</div>;
    }

    return (
        <div className={styles.page}>
            {/* Header */}
            <div className={styles.header}>
                <div className={styles.breadcrumbs}>
                    <span className={styles.breadcrumbLink}>{t.sidebar.settings}</span>
                    <span className={styles.breadcrumbSeparator}>/</span>
                    <span className={styles.breadcrumbCurrent}>{t.sidebar.dashboard}</span>
                </div>
            </div>

            <h1 className={styles.title}>{t.settings.title}</h1>
            <p className={styles.subtitle}>
                {t.settings.subtitle}
            </p>

            <Card className="p-6">
                <form onSubmit={handleSubmit}>
                    {/* General Info */}
                    <div className={styles.section}>
                        <div className={styles.grid2}>
                            <Input
                                label={t.settings.companyNameLao}
                                value={formData.companyNameLao || ''}
                                onChange={(e) => handleChange('companyNameLao', e.target.value)}
                                required
                                placeholder="e.g. ບໍລິສັດ ຕົວຢ່າງ ຈຳກັດ"
                            />
                            <Input
                                label={t.settings.companyNameEn}
                                value={formData.companyNameEn || ''}
                                onChange={(e) => handleChange('companyNameEn', e.target.value)}
                                required
                                placeholder="e.g. Example Company Ltd."
                            />
                            <Input
                                label={t.settings.lssoCode}
                                value={formData.lssoCode || ''}
                                onChange={(e) => handleChange('lssoCode', e.target.value)}
                                placeholder="Social Security Office Code"
                            />
                            <Input
                                label={t.settings.taxRisId}
                                value={formData.taxRisId || ''}
                                onChange={(e) => handleChange('taxRisId', e.target.value)}
                                placeholder="Tax Identification Number"
                            />
                        </div>
                    </div>

                    <div className={styles.section}>
                        <h3 className={styles.sectionTitle}>{t.settings.banking}</h3>
                        <div className={styles.grid2}>
                            <Input
                                label={t.settings.bankName}
                                value={formData.bankName || ''}
                                onChange={(e) => handleChange('bankName', e.target.value)}
                                placeholder="e.g. BCEL"
                            />
                            <Input
                                label={t.settings.bankAccount}
                                value={formData.bankAccountNo || ''}
                                onChange={(e) => handleChange('bankAccountNo', e.target.value)}
                                placeholder="e.g. 16011000..."
                            />
                        </div>
                    </div>

                    <div className={styles.section}>
                        <h3 className={styles.sectionTitle}>{t.settings.contact}</h3>
                        <div className={styles.grid3}>
                            <Input
                                label={t.settings.tel}
                                value={formData.tel || ''}
                                onChange={(e) => handleChange('tel', e.target.value)}
                                placeholder="Landline"
                            />
                            <Input
                                label={t.settings.mobile}
                                value={formData.phone || ''}
                                onChange={(e) => handleChange('phone', e.target.value)}
                                placeholder="Mobile"
                            />
                            <Input
                                label={t.settings.email}
                                type="email"
                                value={formData.email || ''}
                                onChange={(e) => handleChange('email', e.target.value)}
                                placeholder="contact@company.com"
                            />
                        </div>
                    </div>

                    <div className={styles.section}>
                        <h3 className={styles.sectionTitle}>{t.settings.address}</h3>
                        <div className={styles.grid3}>
                            <Select
                                label={t.settings.province}
                                value={formData.provinceId || ''}
                                onChange={handleProvinceChange}
                                options={provinces.map(p => ({
                                    value: p.prId,
                                    label: language === 'lo' ? p.prName : p.prNameEn // Dynamic label based on language
                                }))}
                                placeholder={t.common.select}
                            />
                            <Select
                                label={t.settings.district}
                                value={formData.districtId || ''}
                                onChange={handleDistrictChange}
                                options={districts.map(d => ({
                                    value: d.diId,
                                    label: language === 'lo' ? d.diName : d.diNameEn
                                }))}
                                disabled={!formData.provinceId}
                                placeholder={t.common.select}
                            />
                            <Select
                                label={t.settings.village}
                                value={formData.villageId || ''}
                                onChange={(e) => handleChange('villageId', Number(e.target.value))}
                                options={villages.map(v => ({
                                    value: v.villId,
                                    label: language === 'lo' ? v.villName : v.villNameEn
                                }))}
                                disabled={!formData.districtId}
                                placeholder={t.common.select}
                            />
                        </div>
                    </div>

                    <div className={styles.actions}>
                        <Button type="submit" loading={saving} size="lg">
                            {t.common.save}
                        </Button>
                    </div>
                </form>
            </Card>
        </div>
    );
}
