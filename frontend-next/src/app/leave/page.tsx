"use client";

import { useState, useEffect } from "react";
import { useTranslations } from "next-intl";
import { Plus, Check, X, CalendarDays, Sparkles } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";

// MOCK DATA FALLBACK
const mockLeaveRequests = [
    { id: 1, employee: "ສົມພອນ ຄຳສຸກ", employeeEn: "Somphon K.", type: "Annual Leave", start: "2026-01-20", end: "2026-01-22", status: "Pending", days: 3 },
    { id: 2, employee: "ດາວັນ ສີສະຫວັດ", employeeEn: "Davanh S.", type: "Sick Leave", start: "2026-01-15", end: "2026-01-15", status: "Approved", days: 1 },
    { id: 3, employee: "ມະນີວັນ ສຸກສະຫວັນ", employeeEn: "Manivanh S.", type: "Personal", start: "2026-01-25", end: "2026-01-25", status: "Pending", days: 1 },
    { id: 4, employee: "ພູວົງ ໄຊຍະວົງ", employeeEn: "Phouvong X.", type: "Annual Leave", start: "2026-02-01", end: "2026-02-05", status: "Rejected", days: 5 },
    { id: 5, employee: "ບຸນມີ ວົງພະຈັນ", employeeEn: "Bounmi V.", type: "Sick Leave", start: "2026-01-10", end: "2026-01-11", status: "Approved", days: 2 },
];

const balanceColors = ["#6366f1", "#10b981", "#8b5cf6", "#ec4899"];
const avatarColors = ["#6366f1", "#10b981", "#f59e0b", "#ef4444", "#8b5cf6"];

export default function LeavePage() {
    const t = useTranslations("leave");
    const [requests, setRequests] = useState(mockLeaveRequests);
    const [loading, setLoading] = useState(true);

    const leaveBalances = [
        { label: t("annualLeave"), total: 15, used: 3 },
        { label: t("sickLeave"), total: 30, used: 5 },
        { label: t("personal"), total: 5, used: 1 },
        { label: t("maternity"), total: 90, used: 0 },
    ];

    useEffect(() => {
        // Use mock data directly - no API call to avoid errors
        setLoading(false);
    }, []);

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
                <div>
                    <h1 className="text-2xl font-semibold tracking-tight text-slate-900 dark:text-white">{t("title")}</h1>
                    <p className="text-sm text-slate-600 dark:text-slate-400 mt-1 leading-relaxed">{t("subtitle")}</p>
                </div>
                <Button
                    className="h-10 gap-2 rounded-xl shadow-lg text-white"
                    style={{ background: 'linear-gradient(135deg, #6366f1, #a855f7)' }}
                >
                    <Plus className="w-4 h-4" />
                    {t("newRequest")}
                </Button>
            </div>

            {/* Leave Balance Cards */}
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                {leaveBalances.map((type, idx) => {
                    const color = balanceColors[idx];
                    return (
                        <div key={type.label} className="group relative rounded-2xl p-5 bg-white dark:bg-slate-900 border shadow-lg hover:shadow-xl hover:-translate-y-1 transition-all duration-300 overflow-hidden">
                            <div className="absolute top-0 left-0 right-0 h-1.5" style={{ background: color }} />
                            <div className="flex items-center justify-between mb-3">
                                <p className="text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">{type.label}</p>
                                <div className="h-10 w-10 rounded-xl flex items-center justify-center shadow-lg transition-transform group-hover:scale-110" style={{ background: color }}>
                                    <CalendarDays className="h-5 w-5 text-white" />
                                </div>
                            </div>
                            <div className="text-3xl font-bold tracking-tight text-slate-900 dark:text-white">
                                {type.total - type.used} <span className="text-sm font-normal text-slate-500 dark:text-slate-400">{t("daysLeft")}</span>
                            </div>
                            <div className="mt-3 h-2.5 w-full bg-slate-200 dark:bg-slate-700 rounded-full overflow-hidden">
                                <div className="h-full transition-all duration-500" style={{ width: `${(type.used / type.total) * 100}%`, background: color }} />
                            </div>
                            <p className="text-xs text-slate-500 dark:text-slate-400 mt-2">{type.used} {t("usedOf")} {type.total}</p>
                        </div>
                    );
                })}
            </div>

            {/* Requests Table */}
            <div className="rounded-2xl overflow-hidden bg-white dark:bg-slate-900 border shadow-lg">
                <div className="p-4 border-b flex items-center gap-2" style={{ background: 'linear-gradient(90deg, rgba(99,102,241,0.1), rgba(168,85,247,0.05))' }}>
                    <Sparkles className="h-5 w-5" style={{ color: '#8b5cf6' }} />
                    <h3 className="text-sm font-semibold tracking-tight text-slate-900 dark:text-white">{t("leaveRequests")}</h3>
                </div>
                <table className="w-full text-sm">
                    <thead>
                        <tr className="border-b bg-slate-50 dark:bg-slate-800/50">
                            <th className="px-5 py-3 text-left text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">Employee</th>
                            <th className="px-5 py-3 text-left text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">Type</th>
                            <th className="px-5 py-3 text-left text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">{t("dates")}</th>
                            <th className="px-5 py-3 text-left text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">{t("duration")}</th>
                            <th className="px-5 py-3 text-left text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">Status</th>
                            <th className="px-5 py-3 text-right text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">Actions</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-200 dark:divide-slate-700">
                        {requests.map((req, idx) => (
                            <tr key={req.id} className="hover:bg-slate-50 dark:hover:bg-slate-800/50 transition-colors group">
                                <td className="px-5 py-4">
                                    <div className="flex items-center gap-3">
                                        <Avatar className="h-10 w-10 border-2" style={{ borderColor: avatarColors[idx % avatarColors.length] }}>
                                            <AvatarFallback className="text-xs text-white" style={{ background: avatarColors[idx % avatarColors.length] }}>
                                                {req.employeeEn.substring(0, 2)}
                                            </AvatarFallback>
                                        </Avatar>
                                        <div>
                                            <span className="font-medium text-slate-900 dark:text-white">{req.employee}</span>
                                            <div className="text-xs text-slate-500 dark:text-slate-400">{req.employeeEn}</div>
                                        </div>
                                    </div>
                                </td>
                                <td className="px-5 py-4">
                                    <Badge variant="outline" className="font-normal">{req.type}</Badge>
                                </td>
                                <td className="px-5 py-4 text-slate-600 dark:text-slate-400 font-mono text-xs">{req.start} → {req.end}</td>
                                <td className="px-5 py-4 text-slate-600 dark:text-slate-400">{req.days} {t("days")}</td>
                                <td className="px-5 py-4">
                                    <Badge
                                        className="font-medium border-0 text-white"
                                        style={{
                                            background: req.status === 'Approved' ? '#10b981' :
                                                req.status === 'Rejected' ? '#ef4444' : '#f59e0b'
                                        }}
                                    >
                                        {req.status === 'Approved' ? t("approved") : req.status === 'Rejected' ? t("rejected") : t("pending")}
                                    </Badge>
                                </td>
                                <td className="px-5 py-4 text-right">
                                    {req.status === 'Pending' && (
                                        <div className="flex justify-end gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                                            <Button size="icon" variant="ghost" className="h-8 w-8 text-white hover:text-white" style={{ background: '#10b981' }}>
                                                <Check className="h-4 w-4" />
                                            </Button>
                                            <Button size="icon" variant="ghost" className="h-8 w-8 text-white hover:text-white" style={{ background: '#ef4444' }}>
                                                <X className="h-4 w-4" />
                                            </Button>
                                        </div>
                                    )}
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
}
