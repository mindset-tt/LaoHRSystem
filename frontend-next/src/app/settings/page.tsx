"use client";

import { useState, useEffect, useCallback } from "react";
import { useTranslations } from "next-intl";
import { Globe, Building, Server, Coins, Save, Check, Zap, MapPin, Loader2 } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Switch } from "@/components/ui/switch";
import { Badge } from "@/components/ui/badge";
import { AttendanceMap } from "@/components/map";
import { settingsAPI, SystemSettings } from "@/lib/api";

const languages = [
    { code: 'en', name: 'English', flag: '🇬🇧' },
    { code: 'lo', name: 'ລາວ', flag: '🇱🇦' },
    { code: 'vi', name: 'Tiếng Việt', flag: '🇻🇳' },
    { code: 'zh', name: '中文', flag: '🇨🇳' },
    { code: 'ja', name: '日本語', flag: '🇯🇵' },
];

const cardColors = ["#3b82f6", "#f59e0b", "#10b981", "#8b5cf6", "#ef4444"];

// Default settings
const defaultSettings: SystemSettings = {
    company_name: "Lao HR Company Ltd.",
    head_office_address: "Vientiane, Laos",
    head_office_latitude: "17.9757",
    head_office_longitude: "102.6331",
    geofence_radius: "100",
    zkteco_enabled: "true",
    zkteco_ip: "192.168.1.201",
    zkteco_port: "4370",
    exchange_usd_lak: "22500",
    exchange_thb_lak: "680",
    nssf_rate: "5.5",
};

export default function SettingsPage() {
    const t = useTranslations("settings");

    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [saved, setSaved] = useState(false);
    const [settings, setSettings] = useState<SystemSettings>(defaultSettings);
    const [selectedLang, setSelectedLang] = useState('en');

    // Load settings from backend
    const loadSettings = useCallback(async () => {
        try {
            setLoading(true);
            const data = await settingsAPI.getAll();

            // Normalize keys to lowercase (backend may return UPPERCASE keys)
            const normalizedData: SystemSettings = {};
            for (const [key, value] of Object.entries(data)) {
                normalizedData[key.toLowerCase()] = value;
            }

            setSettings({ ...defaultSettings, ...normalizedData });
        } catch (error) {
            console.error("Failed to load settings:", error);
            // Use defaults on error
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        loadSettings();
        if (typeof document !== 'undefined') {
            const match = document.cookie.match(/locale=([^;]+)/);
            if (match) setSelectedLang(match[1]);
        }
    }, [loadSettings]);

    const changeLanguage = (locale: string) => {
        setSelectedLang(locale);
        document.cookie = `locale=${locale};path=/;max-age=31536000`;
        window.location.reload();
    };

    const updateSetting = (key: keyof SystemSettings, value: string) => {
        setSettings(prev => ({ ...prev, [key]: value }));
    };

    const handleSave = async () => {
        try {
            setSaving(true);
            await settingsAPI.updateBatch(settings);
            setSaved(true);
            setTimeout(() => setSaved(false), 2000);
        } catch (error) {
            console.error("Failed to save settings:", error);
            alert("Failed to save settings");
        } finally {
            setSaving(false);
        }
    };

    const handleLocationChange = (lat: number, lng: number) => {
        setSettings(prev => ({
            ...prev,
            head_office_latitude: lat.toFixed(6),
            head_office_longitude: lng.toFixed(6),
        }));
    };

    const officeLat = parseFloat(settings.head_office_latitude || "17.9757");
    const officeLng = parseFloat(settings.head_office_longitude || "102.6331");

    if (loading) {
        return (
            <div className="flex items-center justify-center h-96">
                <Loader2 className="h-8 w-8 animate-spin text-indigo-500" />
            </div>
        );
    }

    return (
        <div className="space-y-6">
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
                <div>
                    <h1 className="text-2xl font-semibold tracking-tight text-slate-900 dark:text-white">{t("title")}</h1>
                    <p className="text-sm text-slate-600 dark:text-slate-400 mt-1 leading-relaxed">{t("subtitle")}</p>
                </div>
                <Button
                    onClick={handleSave}
                    disabled={saving}
                    className="h-10 gap-2 rounded-xl shadow-lg text-white transition-all duration-300"
                    style={{ background: saved ? '#10b981' : 'linear-gradient(135deg, #6366f1, #a855f7)' }}
                >
                    {saving ? <Loader2 className="w-4 h-4 animate-spin" /> : saved ? <Check className="w-4 h-4" /> : <Save className="w-4 h-4" />}
                    {saving ? t("saving") : saved ? t("saved") : t("saveChanges")}
                </Button>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                {/* Language */}
                <div className="rounded-2xl p-6 bg-white dark:bg-slate-900 border shadow-lg relative overflow-hidden group hover:shadow-xl transition-all">
                    <div className="absolute top-0 left-0 right-0 h-1.5" style={{ background: cardColors[0] }} />
                    <div className="flex items-center gap-3 mb-5">
                        <div className="flex h-12 w-12 items-center justify-center rounded-xl shadow-lg transition-transform group-hover:scale-110" style={{ background: cardColors[0] }}>
                            <Globe className="h-5 w-5 text-white" />
                        </div>
                        <div>
                            <h3 className="text-sm font-semibold tracking-tight text-slate-900 dark:text-white">{t("language")}</h3>
                            <p className="text-xs text-slate-600 dark:text-slate-400">{t("selectLanguage")}</p>
                        </div>
                    </div>
                    <div className="grid grid-cols-2 sm:grid-cols-3 gap-2">
                        {languages.map((lang) => (
                            <button
                                key={lang.code}
                                onClick={() => changeLanguage(lang.code)}
                                className="flex items-center gap-2 px-4 py-3 rounded-xl text-sm border-2 transition-all duration-200"
                                style={{
                                    background: selectedLang === lang.code ? `${cardColors[0]}10` : 'transparent',
                                    borderColor: selectedLang === lang.code ? cardColors[0] : 'rgba(0,0,0,0.1)',
                                    color: selectedLang === lang.code ? cardColors[0] : undefined
                                }}
                            >
                                <span className="text-lg">{lang.flag}</span>
                                <span className="leading-relaxed font-medium text-slate-800 dark:text-slate-200">{lang.name}</span>
                                {selectedLang === lang.code && <Check className="w-4 h-4 ml-auto" style={{ color: cardColors[0] }} />}
                            </button>
                        ))}
                    </div>
                </div>

                {/* Company */}
                <div className="rounded-2xl p-6 bg-white dark:bg-slate-900 border shadow-lg relative overflow-hidden group hover:shadow-xl transition-all">
                    <div className="absolute top-0 left-0 right-0 h-1.5" style={{ background: cardColors[1] }} />
                    <div className="flex items-center gap-3 mb-5">
                        <div className="flex h-12 w-12 items-center justify-center rounded-xl shadow-lg transition-transform group-hover:scale-110" style={{ background: cardColors[1] }}>
                            <Building className="h-5 w-5 text-white" />
                        </div>
                        <div>
                            <h3 className="text-sm font-semibold tracking-tight text-slate-900 dark:text-white">{t("companyProfile")}</h3>
                            <p className="text-xs text-slate-600 dark:text-slate-400">{t("companyDesc")}</p>
                        </div>
                    </div>
                    <div className="space-y-4">
                        <div>
                            <label className="text-sm font-medium mb-1.5 block text-slate-700 dark:text-slate-300">{t("companyName")}</label>
                            <Input
                                value={settings.company_name || ""}
                                onChange={(e) => updateSetting("company_name", e.target.value)}
                                className="h-11 rounded-xl border-2"
                            />
                        </div>
                        <div>
                            <label className="text-sm font-medium mb-1.5 block text-slate-700 dark:text-slate-300">{t("headOffice")}</label>
                            <Input
                                value={settings.head_office_address || ""}
                                onChange={(e) => updateSetting("head_office_address", e.target.value)}
                                className="h-11 rounded-xl border-2"
                            />
                        </div>
                    </div>
                </div>

                {/* Office Location Map - NEW */}
                <div className="rounded-2xl p-6 bg-white dark:bg-slate-900 border shadow-lg relative overflow-hidden group hover:shadow-xl transition-all lg:col-span-2">
                    <div className="absolute top-0 left-0 right-0 h-1.5" style={{ background: cardColors[4] }} />
                    <div className="flex items-center justify-between mb-5">
                        <div className="flex items-center gap-3">
                            <div className="flex h-12 w-12 items-center justify-center rounded-xl shadow-lg transition-transform group-hover:scale-110" style={{ background: cardColors[4] }}>
                                <MapPin className="h-5 w-5 text-white" />
                            </div>
                            <div>
                                <h3 className="text-sm font-semibold tracking-tight text-slate-900 dark:text-white">{t("officeLocation")}</h3>
                                <p className="text-xs text-slate-600 dark:text-slate-400">{t("officeLocationDesc")}</p>
                            </div>
                        </div>
                        <div className="text-right">
                            <div className="text-xs text-slate-500 dark:text-slate-400">{t("coordinates")}</div>
                            <div className="font-mono text-sm text-slate-800 dark:text-slate-200">
                                {officeLat.toFixed(6)}, {officeLng.toFixed(6)}
                            </div>
                        </div>
                    </div>
                    <div className="grid grid-cols-1 lg:grid-cols-4 gap-4">
                        <div className="lg:col-span-3">
                            <AttendanceMap
                                lat={officeLat}
                                lng={officeLng}
                                editable={true}
                                onLocationChange={handleLocationChange}
                                popupText="Head Office"
                                className="h-[300px] rounded-xl border-0"
                            />
                            <p className="text-xs text-center text-slate-500 dark:text-slate-400 mt-2">{t("clickToSetLocation")}</p>
                        </div>
                        <div className="space-y-4">
                            <div>
                                <label className="text-sm font-medium mb-1.5 block text-slate-700 dark:text-slate-300">Latitude</label>
                                <Input
                                    value={settings.head_office_latitude || ""}
                                    onChange={(e) => updateSetting("head_office_latitude", e.target.value)}
                                    className="h-11 font-mono rounded-xl border-2"
                                />
                            </div>
                            <div>
                                <label className="text-sm font-medium mb-1.5 block text-slate-700 dark:text-slate-300">Longitude</label>
                                <Input
                                    value={settings.head_office_longitude || ""}
                                    onChange={(e) => updateSetting("head_office_longitude", e.target.value)}
                                    className="h-11 font-mono rounded-xl border-2"
                                />
                            </div>
                            <div>
                                <label className="text-sm font-medium mb-1.5 block text-slate-700 dark:text-slate-300">{t("geofenceRadius")}</label>
                                <Input
                                    type="number"
                                    value={settings.geofence_radius || "100"}
                                    onChange={(e) => updateSetting("geofence_radius", e.target.value)}
                                    className="h-11 font-mono rounded-xl border-2"
                                />
                            </div>
                        </div>
                    </div>
                </div>

                {/* ZKTeco */}
                <div className="rounded-2xl p-6 bg-white dark:bg-slate-900 border shadow-lg relative overflow-hidden group hover:shadow-xl transition-all">
                    <div className="absolute top-0 left-0 right-0 h-1.5" style={{ background: cardColors[2] }} />
                    <div className="flex items-center justify-between mb-5">
                        <div className="flex items-center gap-3">
                            <div className="flex h-12 w-12 items-center justify-center rounded-xl shadow-lg transition-transform group-hover:scale-110" style={{ background: cardColors[2] }}>
                                <Server className="h-5 w-5 text-white" />
                            </div>
                            <div>
                                <h3 className="text-sm font-semibold tracking-tight text-slate-900 dark:text-white">{t("deviceIntegration")}</h3>
                                <p className="text-xs text-slate-600 dark:text-slate-400">{t("deviceDesc")}</p>
                            </div>
                        </div>
                        <Switch
                            checked={settings.zkteco_enabled === "true"}
                            onCheckedChange={(v) => updateSetting("zkteco_enabled", v ? "true" : "false")}
                        />
                    </div>
                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className="text-sm font-medium mb-1.5 block text-slate-700 dark:text-slate-300">{t("ipAddress")}</label>
                            <Input
                                value={settings.zkteco_ip || ""}
                                onChange={(e) => updateSetting("zkteco_ip", e.target.value)}
                                className="h-11 font-mono rounded-xl border-2"
                            />
                        </div>
                        <div>
                            <label className="text-sm font-medium mb-1.5 block text-slate-700 dark:text-slate-300">{t("port")}</label>
                            <Input
                                type="number"
                                value={settings.zkteco_port || ""}
                                onChange={(e) => updateSetting("zkteco_port", e.target.value)}
                                className="h-11 font-mono rounded-xl border-2"
                            />
                        </div>
                    </div>
                    <div className="flex items-center gap-2 text-sm mt-4 p-3 rounded-xl bg-slate-100 dark:bg-slate-800">
                        <div
                            className="w-3 h-3 rounded-full animate-pulse shadow-lg"
                            style={{
                                background: settings.zkteco_enabled === "true" ? '#10b981' : '#94a3b8',
                                boxShadow: settings.zkteco_enabled === "true" ? '0 0 8px rgba(16,185,129,0.5)' : 'none'
                            }}
                        />
                        <span className="font-medium text-slate-700 dark:text-slate-300">
                            {settings.zkteco_enabled === "true" ? t("integrationActive") : t("integrationDisabled")}
                        </span>
                        {settings.zkteco_enabled === "true" && <Zap className="h-4 w-4 ml-auto" style={{ color: '#f59e0b' }} />}
                    </div>
                </div>

                {/* Payroll Config */}
                <div className="rounded-2xl p-6 bg-white dark:bg-slate-900 border shadow-lg relative overflow-hidden group hover:shadow-xl transition-all">
                    <div className="absolute top-0 left-0 right-0 h-1.5" style={{ background: cardColors[3] }} />
                    <div className="flex items-center gap-3 mb-5">
                        <div className="flex h-12 w-12 items-center justify-center rounded-xl shadow-lg transition-transform group-hover:scale-110" style={{ background: cardColors[3] }}>
                            <Coins className="h-5 w-5 text-white" />
                        </div>
                        <div>
                            <h3 className="text-sm font-semibold tracking-tight text-slate-900 dark:text-white">{t("payrollConfig")}</h3>
                            <p className="text-xs text-slate-600 dark:text-slate-400">{t("payrollConfigDesc")}</p>
                        </div>
                    </div>
                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className="text-sm font-medium mb-1.5 block text-slate-700 dark:text-slate-300">{t("usdToLak")}</label>
                            <Input
                                type="number"
                                value={settings.exchange_usd_lak || ""}
                                onChange={(e) => updateSetting("exchange_usd_lak", e.target.value)}
                                className="h-11 font-mono rounded-xl border-2"
                            />
                        </div>
                        <div>
                            <label className="text-sm font-medium mb-1.5 block text-slate-700 dark:text-slate-300">{t("thbToLak")}</label>
                            <Input
                                type="number"
                                value={settings.exchange_thb_lak || ""}
                                onChange={(e) => updateSetting("exchange_thb_lak", e.target.value)}
                                className="h-11 font-mono rounded-xl border-2"
                            />
                        </div>
                    </div>
                    <div className="pt-4 mt-4 border-t flex items-center justify-between">
                        <span className="text-sm font-medium text-slate-700 dark:text-slate-300">{t("nssfDeduction")}</span>
                        <Badge className="text-white border-0 shadow-sm" style={{ background: cardColors[3] }}>
                            {settings.nssf_rate || "5.5"}%
                        </Badge>
                    </div>
                </div>
            </div>
        </div>
    );
}
