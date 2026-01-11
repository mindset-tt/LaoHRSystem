"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useTranslations } from "next-intl";
import { cn } from "@/lib/utils";
import { useAuth } from "@/hooks/useAuth";
import {
    LayoutDashboard, Users, Clock, CreditCard, CalendarDays,
    Calendar, FileText, Settings, LogOut, ChevronRight, Sparkles
} from "lucide-react";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";

const navColors = {
    dashboard: "#6366f1",
    employees: "#3b82f6",
    attendance: "#10b981",
    payroll: "#f59e0b",
    leave: "#8b5cf6",
    holidays: "#ec4899",
    reports: "#06b6d4",
    settings: "#64748b",
};

export default function Sidebar() {
    const pathname = usePathname();
    const t = useTranslations("sidebar");
    const { user, logout } = useAuth();

    const navItems = [
        { key: "dashboard", href: "/", icon: LayoutDashboard },
        { key: "employees", href: "/employees", icon: Users },
        { key: "attendance", href: "/attendance", icon: Clock },
        { key: "payroll", href: "/payroll", icon: CreditCard },
        { key: "leave", href: "/leave", icon: CalendarDays },
        { key: "holidays", href: "/holidays", icon: Calendar },
    ];

    const managementItems = [
        { key: "reports", href: "/reports", icon: FileText },
        { key: "settings", href: "/settings", icon: Settings },
    ];

    const isActive = (href: string) => {
        if (href === "/") return pathname === "/";
        return pathname?.startsWith(href);
    };

    return (
        <aside className="fixed inset-y-0 left-0 z-50 flex h-full w-64 flex-col border-r bg-white dark:bg-slate-950">
            {/* Logo Header with gradient */}
            <div className="flex h-16 items-center gap-3 border-b px-5">
                <div className="flex h-11 w-11 items-center justify-center rounded-xl text-white font-bold text-xl shadow-lg" style={{ background: 'linear-gradient(135deg, #6366f1, #a855f7)' }}>
                    ລ
                </div>
                <div className="flex flex-col">
                    <span className="text-sm font-bold tracking-tight" style={{ color: '#6366f1' }}>Lao HR</span>
                    <span className="text-[10px] text-muted-foreground uppercase tracking-widest flex items-center gap-1">
                        <Sparkles className="h-3 w-3" style={{ color: '#f59e0b' }} /> Enterprise
                    </span>
                </div>
            </div>

            {/* Navigation */}
            <nav className="flex-1 overflow-y-auto px-3 py-4">
                {/* Main Menu */}
                <div className="mb-6">
                    <p className="mb-3 px-3 text-[11px] font-medium uppercase tracking-wider text-muted-foreground">
                        {t("mainMenu")}
                    </p>
                    <ul className="space-y-1">
                        {navItems.map((item) => {
                            const Icon = item.icon;
                            const active = isActive(item.href);
                            const color = navColors[item.key as keyof typeof navColors];
                            return (
                                <li key={item.key}>
                                    <Link
                                        href={item.href}
                                        className={cn(
                                            "group relative flex items-center gap-3 rounded-xl px-3 py-2.5 text-[13px] font-medium transition-all duration-200",
                                            active
                                                ? "bg-slate-100 dark:bg-slate-800"
                                                : "hover:bg-slate-50 dark:hover:bg-slate-900"
                                        )}
                                    >
                                        {active && (
                                            <span className="absolute left-0 top-1/2 -translate-y-1/2 h-6 w-1 rounded-r-full" style={{ background: color }} />
                                        )}
                                        <div
                                            className="flex h-9 w-9 items-center justify-center rounded-lg shadow-md transition-transform group-hover:scale-110"
                                            style={{ background: active ? color : `${color}20` }}
                                        >
                                            <Icon className="h-4 w-4" style={{ color: active ? 'white' : color }} />
                                        </div>
                                        <span style={{ color: active ? color : undefined }}>{t(item.key)}</span>
                                        {active && <ChevronRight className="ml-auto h-4 w-4" style={{ color }} />}
                                    </Link>
                                </li>
                            );
                        })}
                    </ul>
                </div>

                {/* Management */}
                <div>
                    <p className="mb-3 px-3 text-[11px] font-medium uppercase tracking-wider text-muted-foreground">
                        {t("management")}
                    </p>
                    <ul className="space-y-1">
                        {managementItems.map((item) => {
                            const Icon = item.icon;
                            const active = isActive(item.href);
                            const color = navColors[item.key as keyof typeof navColors];
                            return (
                                <li key={item.key}>
                                    <Link
                                        href={item.href}
                                        className={cn(
                                            "group relative flex items-center gap-3 rounded-xl px-3 py-2.5 text-[13px] font-medium transition-all duration-200",
                                            active
                                                ? "bg-slate-100 dark:bg-slate-800"
                                                : "hover:bg-slate-50 dark:hover:bg-slate-900"
                                        )}
                                    >
                                        {active && (
                                            <span className="absolute left-0 top-1/2 -translate-y-1/2 h-6 w-1 rounded-r-full" style={{ background: color }} />
                                        )}
                                        <div
                                            className="flex h-9 w-9 items-center justify-center rounded-lg shadow-md transition-transform group-hover:scale-110"
                                            style={{ background: active ? color : `${color}20` }}
                                        >
                                            <Icon className="h-4 w-4" style={{ color: active ? 'white' : color }} />
                                        </div>
                                        <span style={{ color: active ? color : undefined }}>{t(item.key)}</span>
                                    </Link>
                                </li>
                            );
                        })}
                    </ul>
                </div>
            </nav>

            {/* User Profile Footer */}
            <div className="border-t p-3">
                <div className="flex items-center gap-3 rounded-xl p-3 bg-slate-50 dark:bg-slate-900">
                    <Avatar className="h-10 w-10 border-2" style={{ borderColor: '#6366f1' }}>
                        <AvatarImage src="/avatars/user.png" />
                        <AvatarFallback className="text-xs font-medium text-white" style={{ background: 'linear-gradient(135deg, #6366f1, #a855f7)' }}>
                            {user?.displayName?.substring(0, 2).toUpperCase() || "U"}
                        </AvatarFallback>
                    </Avatar>
                    <div className="flex-1 min-w-0">
                        <p className="text-[13px] font-medium truncate tracking-tight">
                            {user?.displayName || "User"}
                        </p>
                        <p className="text-[11px] text-muted-foreground truncate">
                            {user?.role || "Employee"}
                        </p>
                    </div>
                    <button
                        onClick={logout}
                        className="flex h-9 w-9 items-center justify-center rounded-lg transition-colors hover:bg-rose-50 dark:hover:bg-rose-950"
                        style={{ color: '#ef4444' }}
                        title={t("logout")}
                    >
                        <LogOut className="h-4 w-4" />
                    </button>
                </div>
            </div>
        </aside>
    );
}
