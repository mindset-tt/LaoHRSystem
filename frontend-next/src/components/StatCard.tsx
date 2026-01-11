import { LucideIcon } from "lucide-react";

interface StatCardProps {
    title: string;
    value: string | number;
    subtitle?: string;
    icon: LucideIcon;
    trend?: {
        value: string;
        positive?: boolean;
    };
    variant?: "primary" | "success" | "warning" | "error";
}

const variantStyles = {
    primary: "bg-indigo-500/10 text-indigo-400",
    success: "bg-emerald-500/10 text-emerald-400",
    warning: "bg-amber-500/10 text-amber-400",
    error: "bg-red-500/10 text-red-400",
};

export default function StatCard({
    title,
    value,
    subtitle,
    icon: Icon,
    trend,
    variant = "primary",
}: StatCardProps) {
    return (
        <div className="bg-slate-900 border border-slate-800 rounded-xl p-5">
            <div className={`w-10 h-10 rounded-lg flex items-center justify-center mb-4 ${variantStyles[variant]}`}>
                <Icon className="w-5 h-5" />
            </div>
            <div className="text-2xl font-bold text-slate-100 mb-1">{value}</div>
            <div className="text-sm text-slate-500">{title}</div>
            {trend && (
                <div className={`text-xs mt-2 ${trend.positive ? "text-emerald-400" : "text-slate-500"}`}>
                    {trend.value}
                </div>
            )}
            {subtitle && !trend && (
                <div className="text-xs text-slate-600 mt-2">{subtitle}</div>
            )}
        </div>
    );
}
