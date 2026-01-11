"use client";

import { useEffect, useState } from "react";
import { useTranslations } from "next-intl";
import { Calculator, DollarSign, Users, CreditCard, Building2, FileText, Sparkles, Download, CheckCircle } from "lucide-react";
import { useAuth } from "@/hooks/useAuth";

import { Button } from "@/components/ui/button";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";

// MOCK DATA
const mockPayroll = [
    { id: 1, laoName: "ສົມພອນ ຄຳສຸກ", englishName: "Somphon K.", department: "IT", baseSalary: 8000000, netSalary: 7150000, status: "Paid" },
    { id: 2, laoName: "ດາວັນ ສີສະຫວັດ", englishName: "Davanh S.", department: "HR", baseSalary: 12000000, netSalary: 10500000, status: "Paid" },
    { id: 3, laoName: "ມະນີວັນ ສຸກສະຫວັນ", englishName: "Manivanh S.", department: "Finance", baseSalary: 9500000, netSalary: 8400000, status: "Pending" },
    { id: 4, laoName: "ພູວົງ ໄຊຍະວົງ", englishName: "Phouvong X.", department: "Sales", baseSalary: 11000000, netSalary: 9700000, status: "Paid" },
    { id: 5, laoName: "ບຸນມີ ວົງພະຈັນ", englishName: "Bounmi V.", department: "Marketing", baseSalary: 7500000, netSalary: 6800000, status: "Pending" },
    { id: 6, laoName: "ຈັນທະລາ ພົມມະວົງ", englishName: "Chanthala P.", department: "Finance", baseSalary: 25000000, netSalary: 21200000, status: "Paid" },
];

const avatarColors = ["#6366f1", "#10b981", "#f59e0b", "#ef4444", "#8b5cf6", "#3b82f6"];

export default function PayrollPage() {
    const t = useTranslations("payroll");
    const { canManagePayroll, canExportBankFiles } = useAuth();

    const [payrollData, setPayrollData] = useState(mockPayroll);
    const [month, setMonth] = useState("01");
    const [year, setYear] = useState("2026");
    const [loading, setLoading] = useState(false);

    const totalGross = payrollData.reduce((s, e) => s + e.baseSalary, 0);
    const totalNet = payrollData.reduce((s, e) => s + e.netSalary, 0);
    const paidCount = payrollData.filter(e => e.status === "Paid").length;

    const formatCurrency = (n: number) => `₭ ${n.toLocaleString()}`;

    const summaryCards = [
        { label: t("totalGross"), value: formatCurrency(totalGross), icon: DollarSign, color: "#6366f1" },
        { label: t("totalNet"), value: formatCurrency(totalNet), icon: CreditCard, color: "#10b981" },
        { label: t("employees"), value: `${payrollData.length} staff`, icon: Users, color: "#f59e0b" },
        { label: t("paidStatus"), value: `${paidCount}/${payrollData.length}`, icon: CheckCircle, color: "#8b5cf6" },
    ];

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
                <div>
                    <h1 className="text-2xl font-semibold tracking-tight text-slate-900 dark:text-white">{t("title")}</h1>
                    <p className="text-sm text-slate-600 dark:text-slate-400 mt-1 leading-relaxed">{t("subtitle")}</p>
                </div>
                <div className="flex items-center gap-3">
                    <Select value={month} onValueChange={setMonth}>
                        <SelectTrigger className="w-[120px] h-10 rounded-xl border-2">
                            <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                            {["01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12"].map(m => (
                                <SelectItem key={m} value={m}>{new Date(2026, parseInt(m) - 1, 1).toLocaleString('en-US', { month: 'long' })}</SelectItem>
                            ))}
                        </SelectContent>
                    </Select>
                    <Select value={year} onValueChange={setYear}>
                        <SelectTrigger className="w-[100px] h-10 rounded-xl border-2">
                            <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value="2025">2025</SelectItem>
                            <SelectItem value="2026">2026</SelectItem>
                        </SelectContent>
                    </Select>
                    {canManagePayroll && (
                        <Button
                            className="h-10 gap-2 rounded-xl shadow-lg text-white"
                            style={{ background: 'linear-gradient(135deg, #6366f1, #a855f7)' }}
                        >
                            <Calculator className="w-4 h-4" />
                            {t("runPayroll")}
                        </Button>
                    )}
                </div>
            </div>

            {/* Summary Cards */}
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                {summaryCards.map((card) => (
                    <div key={card.label} className="rounded-2xl p-5 bg-white dark:bg-slate-900 border shadow-lg relative overflow-hidden group hover:shadow-xl hover:-translate-y-1 transition-all duration-300">
                        <div className="absolute top-0 left-0 right-0 h-1.5" style={{ background: card.color }} />
                        <div className="flex items-center justify-between">
                            <div>
                                <p className="text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">{card.label}</p>
                                <p className="text-2xl font-bold tracking-tight mt-2 text-slate-900 dark:text-white">{card.value}</p>
                            </div>
                            <div className="flex h-12 w-12 items-center justify-center rounded-xl shadow-lg transition-transform group-hover:scale-110" style={{ background: card.color }}>
                                <card.icon className="h-6 w-6 text-white" />
                            </div>
                        </div>
                    </div>
                ))}
            </div>

            {/* Quick Actions */}
            {canExportBankFiles && (
                <div className="flex flex-wrap gap-3">
                    {[
                        { label: t("exportBank"), icon: Building2, color: "#3b82f6" },
                        { label: t("nssfReport"), icon: FileText, color: "#10b981" },
                        { label: t("payslips"), icon: Download, color: "#8b5cf6" },
                    ].map((action) => (
                        <Button key={action.label} variant="outline" className="h-10 gap-2 rounded-xl border-2">
                            <action.icon className="h-4 w-4" style={{ color: action.color }} />
                            {action.label}
                        </Button>
                    ))}
                </div>
            )}

            {/* Payroll Table */}
            <div className="rounded-2xl overflow-hidden bg-white dark:bg-slate-900 border shadow-lg">
                <div className="p-4 border-b flex items-center gap-2" style={{ background: 'linear-gradient(90deg, rgba(99,102,241,0.1), rgba(168,85,247,0.05))' }}>
                    <Sparkles className="h-5 w-5" style={{ color: '#f59e0b' }} />
                    <h3 className="text-sm font-semibold tracking-tight text-slate-900 dark:text-white">January 2026 Payroll</h3>
                    <Badge className="ml-auto text-white border-0" style={{ background: '#6366f1' }}>{payrollData.length} Employees</Badge>
                </div>
                <div className="overflow-x-auto">
                    <table className="w-full text-sm">
                        <thead>
                            <tr className="border-b bg-slate-50 dark:bg-slate-800/50">
                                <th className="px-5 py-4 text-left text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">{t("employee")}</th>
                                <th className="px-5 py-4 text-left text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">{t("department")}</th>
                                <th className="px-5 py-4 text-right text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">{t("grossSalary")}</th>
                                <th className="px-5 py-4 text-right text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">{t("netSalary")}</th>
                                <th className="px-5 py-4 text-center text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">Status</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-slate-200 dark:divide-slate-700">
                            {payrollData.map((emp, idx) => (
                                <tr key={emp.id} className="hover:bg-slate-50 dark:hover:bg-slate-800/50 transition-colors group">
                                    <td className="px-5 py-4">
                                        <div className="flex items-center gap-3">
                                            <Avatar className="h-10 w-10 border-2 transition-transform group-hover:scale-105" style={{ borderColor: avatarColors[idx % avatarColors.length] }}>
                                                <AvatarFallback className="text-xs font-medium text-white" style={{ background: avatarColors[idx % avatarColors.length] }}>
                                                    {emp.englishName.substring(0, 2).toUpperCase()}
                                                </AvatarFallback>
                                            </Avatar>
                                            <div>
                                                <div className="font-medium text-slate-900 dark:text-white">{emp.laoName}</div>
                                                <div className="text-xs text-slate-500 dark:text-slate-400">{emp.englishName}</div>
                                            </div>
                                        </div>
                                    </td>
                                    <td className="px-5 py-4">
                                        <div className="flex items-center gap-2 text-slate-600 dark:text-slate-400">
                                            <Building2 className="h-4 w-4" style={{ color: avatarColors[idx % avatarColors.length] }} />
                                            {emp.department}
                                        </div>
                                    </td>
                                    <td className="px-5 py-4 text-right font-mono text-slate-900 dark:text-white">{formatCurrency(emp.baseSalary)}</td>
                                    <td className="px-5 py-4 text-right font-mono font-semibold" style={{ color: '#10b981' }}>{formatCurrency(emp.netSalary)}</td>
                                    <td className="px-5 py-4 text-center">
                                        <Badge
                                            className="font-medium border-0 text-white"
                                            style={{ background: emp.status === 'Paid' ? '#10b981' : '#f59e0b' }}
                                        >
                                            {emp.status === 'Paid' ? t("paid") : t("pending")}
                                        </Badge>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                        <tfoot>
                            <tr className="border-t-2 bg-slate-50 dark:bg-slate-800/50 font-semibold">
                                <td className="px-5 py-4 text-slate-900 dark:text-white" colSpan={2}>{t("total")}</td>
                                <td className="px-5 py-4 text-right font-mono text-slate-900 dark:text-white">{formatCurrency(totalGross)}</td>
                                <td className="px-5 py-4 text-right font-mono" style={{ color: '#10b981' }}>{formatCurrency(totalNet)}</td>
                                <td className="px-5 py-4"></td>
                            </tr>
                        </tfoot>
                    </table>
                </div>
            </div>
        </div>
    );
}
