"use client";

import { useTranslations } from "next-intl";
import { FileText, Users, Clock, CreditCard, Building, Landmark, Download, FileSpreadsheet, FileBarChart, CheckCircle } from "lucide-react";
import { useState } from "react";

import { Button } from "@/components/ui/button";

const reportColors = {
    payroll: "#6366f1",
    attendance: "#10b981",
    employees: "#3b82f6",
    nssf: "#f59e0b",
    tax: "#ef4444",
    bank: "#8b5cf6",
    leave: "#14b8a6",
    performance: "#94a3b8",
};

export default function ReportsPage() {
    const t = useTranslations("reports");
    const [downloading, setDownloading] = useState<string | null>(null);
    const [downloaded, setDownloaded] = useState<string[]>([]);

    const reports = [
        { key: 'payroll', icon: CreditCard, title: t("payrollSummary"), description: t("payrollDesc"), generated: `6 ${t("reportsAvailable")}` },
        { key: 'attendance', icon: Clock, title: t("attendanceLog"), description: t("attendanceDesc"), generated: `12 ${t("reportsAvailable")}` },
        { key: 'employees', icon: Users, title: t("staffDirectory"), description: t("staffDesc"), generated: t("updatedToday") },
        { key: 'nssf', icon: Building, title: t("nssfReport"), description: t("nssfDesc"), generated: `Q4 2025 ${t("ready")}` },
        { key: 'tax', icon: FileText, title: t("taxSummary"), description: t("taxDesc"), generated: `6 ${t("reportsAvailable")}` },
        { key: 'bank', icon: Landmark, title: t("bankTransfers"), description: t("bankDesc"), generated: `Jan 2026 ${t("ready")}` },
        { key: 'leave', icon: FileSpreadsheet, title: t("leaveReport"), description: t("leaveDesc"), generated: t("updatedToday") },
        { key: 'performance', icon: FileBarChart, title: t("performance"), description: t("performanceDesc"), generated: t("comingSoon"), disabled: true },
    ];

    const handleDownload = async (key: string) => {
        setDownloading(key);
        await new Promise(resolve => setTimeout(resolve, 1500));
        setDownloading(null);
        setDownloaded([...downloaded, key]);
        setTimeout(() => setDownloaded(downloaded.filter(k => k !== key)), 3000);
    };

    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-2xl font-semibold tracking-tight">{t("title")}</h1>
                <p className="text-sm text-muted-foreground mt-1 leading-relaxed">{t("subtitle")}</p>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-5">
                {reports.map((report) => {
                    const color = reportColors[report.key as keyof typeof reportColors];
                    return (
                        <div
                            key={report.key}
                            className="group relative rounded-2xl p-5 bg-white dark:bg-slate-900 border shadow-lg overflow-hidden transition-all duration-300 hover:shadow-xl hover:-translate-y-1"
                            style={{ opacity: report.disabled ? 0.6 : 1 }}
                        >
                            {/* Color accent bar */}
                            <div className="absolute top-0 left-0 right-0 h-1.5" style={{ background: color }} />

                            <div className="flex items-start gap-4 mb-4">
                                <div
                                    className="flex h-14 w-14 items-center justify-center rounded-xl shadow-lg transition-transform duration-300 group-hover:scale-110"
                                    style={{ background: color }}
                                >
                                    <report.icon className="h-6 w-6 text-white" />
                                </div>
                                <div className="flex-1">
                                    <h3 className="text-sm font-semibold tracking-tight">{report.title}</h3>
                                    <p className="text-[10px] font-medium mt-0.5" style={{ color }}>{report.generated}</p>
                                </div>
                            </div>

                            <p className="text-xs text-muted-foreground mb-5 leading-relaxed">{report.description}</p>

                            <Button
                                className="w-full h-11 text-sm font-medium transition-all duration-300 text-white rounded-xl"
                                style={{
                                    background: downloaded.includes(report.key) ? '#10b981' :
                                        report.disabled ? '#94a3b8' : color
                                }}
                                onClick={() => !report.disabled && handleDownload(report.key)}
                                disabled={downloading === report.key || report.disabled}
                            >
                                {downloaded.includes(report.key) ? (
                                    <>
                                        <CheckCircle className="mr-2 h-4 w-4" />
                                        Downloaded!
                                    </>
                                ) : downloading === report.key ? (
                                    <>
                                        <span className="mr-2 h-4 w-4 animate-spin rounded-full border-2 border-current border-t-transparent" />
                                        Downloading...
                                    </>
                                ) : report.disabled ? (
                                    t("comingSoon")
                                ) : (
                                    <>
                                        <Download className="mr-2 h-4 w-4" />
                                        Download
                                    </>
                                )}
                            </Button>
                        </div>
                    );
                })}
            </div>
        </div>
    );
}
