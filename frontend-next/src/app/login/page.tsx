"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useTranslations } from "next-intl";
import { Loader2, Sparkles, Shield, Users, Clock, Building2, CreditCard, BarChart3 } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { cn } from "@/lib/utils";

const demoAccounts = [
    { label: "Administrator", username: "admin", password: "admin123", icon: Shield, color: "bg-indigo-600 hover:bg-indigo-700", desc: "Full system access" },
    { label: "HR Manager", username: "hr", password: "hr123", icon: Users, color: "bg-emerald-600 hover:bg-emerald-700", desc: "Employee management" },
    { label: "Employee", username: "employee", password: "emp123", icon: Clock, color: "bg-amber-600 hover:bg-amber-700", desc: "Self-service only" },
];

const features = [
    { icon: Building2, label: "Employee Management", color: "text-blue-400" },
    { icon: Clock, label: "Attendance Tracking", color: "text-emerald-400" },
    { icon: CreditCard, label: "Payroll & Tax", color: "text-amber-400" },
    { icon: BarChart3, label: "Reports & Analytics", color: "text-purple-400" },
];

export default function LoginPage() {
    const router = useRouter();
    const t = useTranslations("login");

    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);

    const handleLogin = async (e: React.FormEvent) => {
        e.preventDefault();
        setError("");
        setLoading(true);

        try {
            const res = await fetch("http://localhost:5000/api/auth/login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ username, password }),
            });

            if (!res.ok) {
                const data = await res.json().catch(() => ({}));
                throw new Error(data.message || t("invalidCredentials"));
            }

            const data = await res.json();
            localStorage.setItem("authToken", data.token);
            document.cookie = `token=${data.token}; path=/`;
            router.push("/");
        } catch (err) {
            setError(err instanceof Error ? err.message : t("invalidCredentials"));
        } finally {
            setLoading(false);
        }
    };

    const fillCredentials = (u: string, p: string) => {
        setUsername(u);
        setPassword(p);
    };

    return (
        <div className="min-h-screen grid lg:grid-cols-2">
            {/* Left: VIBRANT Gradient Branding */}
            <div className="hidden lg:flex flex-col relative overflow-hidden" style={{ background: 'linear-gradient(135deg, #6366f1 0%, #8b5cf6 25%, #a855f7 50%, #ec4899 75%, #f43f5e 100%)' }}>
                {/* Animated background shapes */}
                <div className="absolute inset-0">
                    <div className="absolute top-20 left-20 w-72 h-72 bg-white/10 rounded-full blur-3xl animate-pulse" />
                    <div className="absolute bottom-20 right-20 w-96 h-96 bg-pink-500/20 rounded-full blur-3xl animate-pulse" style={{ animationDelay: '1s' }} />
                    <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-64 h-64 bg-purple-400/20 rounded-full blur-3xl" />
                </div>

                <div className="relative z-10 flex flex-col h-full p-12 text-white">
                    {/* Logo */}
                    <div className="flex items-center gap-3">
                        <div className="h-14 w-14 rounded-2xl bg-white/20 backdrop-blur-sm flex items-center justify-center font-bold text-3xl shadow-2xl border border-white/20">
                            ລ
                        </div>
                        <div>
                            <span className="text-2xl font-bold tracking-tight">Lao HR System</span>
                            <div className="text-sm text-white/70 flex items-center gap-1">
                                <Sparkles className="h-4 w-4" /> Enterprise Edition
                            </div>
                        </div>
                    </div>

                    {/* Main content */}
                    <div className="flex-1 flex flex-col justify-center max-w-lg">
                        <h1 className="text-5xl font-bold leading-tight mb-6">
                            The complete HR solution for Laos
                        </h1>
                        <p className="text-xl text-white/80 leading-relaxed mb-10">
                            Payroll, Attendance, Leave Management, Tax Compliance, and NSSF reporting — all in one powerful platform.
                        </p>

                        {/* Feature pills */}
                        <div className="grid grid-cols-2 gap-3 mb-10">
                            {features.map((f, i) => (
                                <div key={i} className="flex items-center gap-2 bg-white/10 backdrop-blur-sm rounded-lg px-4 py-3 border border-white/10">
                                    <f.icon className={cn("h-5 w-5", f.color)} />
                                    <span className="text-sm font-medium">{f.label}</span>
                                </div>
                            ))}
                        </div>

                        {/* Stats with solid backgrounds */}
                        <div className="grid grid-cols-3 gap-4">
                            {[
                                { value: "500+", label: "Companies", bg: "bg-white/20" },
                                { value: "50K+", label: "Employees", bg: "bg-white/20" },
                                { value: "99.9%", label: "Uptime", bg: "bg-white/20" },
                            ].map((stat, i) => (
                                <div key={i} className={cn("text-center p-5 rounded-xl backdrop-blur-sm border border-white/20", stat.bg)}>
                                    <div className="text-3xl font-bold">{stat.value}</div>
                                    <div className="text-sm text-white/70 mt-1">{stat.label}</div>
                                </div>
                            ))}
                        </div>
                    </div>

                    <div className="text-sm text-white/50 uppercase tracking-widest text-center">
                        v2.0 Enterprise • Licensed for Laos
                    </div>
                </div>
            </div>

            {/* Right: Login Form with colored elements */}
            <div className="flex items-center justify-center p-8 bg-slate-50 dark:bg-slate-950">
                <div className="w-full max-w-md space-y-8">
                    {/* Mobile logo with visible gradient */}
                    <div className="lg:hidden text-center">
                        <div className="inline-flex h-16 w-16 rounded-2xl items-center justify-center text-white font-bold text-3xl shadow-xl mb-4" style={{ background: 'linear-gradient(135deg, #6366f1, #a855f7)' }}>
                            ລ
                        </div>
                        <h1 className="text-2xl font-bold tracking-tight">Lao HR System</h1>
                    </div>

                    <div className="space-y-2">
                        <h2 className="text-3xl font-bold tracking-tight">{t("title")}</h2>
                        <p className="text-sm text-muted-foreground">{t("subtitle")}</p>
                    </div>

                    {error && (
                        <div className="p-4 text-sm text-white bg-rose-500 rounded-xl flex items-center gap-2 shadow-lg">
                            <div className="h-2 w-2 rounded-full bg-white animate-pulse" />
                            {error}
                        </div>
                    )}

                    <form onSubmit={handleLogin} className="space-y-5">
                        <div className="space-y-2">
                            <Label htmlFor="username" className="text-sm font-medium">{t("username")}</Label>
                            <Input
                                id="username"
                                placeholder="Enter your username"
                                value={username}
                                onChange={(e) => setUsername(e.target.value)}
                                disabled={loading}
                                className="h-12 px-4 rounded-xl border-2 focus:border-indigo-500 focus:ring-indigo-500"
                            />
                        </div>
                        <div className="space-y-2">
                            <Label htmlFor="password" className="text-sm font-medium">{t("password")}</Label>
                            <Input
                                id="password"
                                type="password"
                                placeholder="Enter your password"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                disabled={loading}
                                className="h-12 px-4 rounded-xl border-2 focus:border-indigo-500 focus:ring-indigo-500"
                            />
                        </div>
                        <Button
                            className="w-full h-12 text-base font-semibold rounded-xl shadow-xl text-white"
                            style={{ background: 'linear-gradient(135deg, #6366f1, #a855f7)' }}
                            disabled={loading}
                        >
                            {loading && <Loader2 className="mr-2 h-5 w-5 animate-spin" />}
                            {t("signIn")}
                        </Button>
                    </form>

                    <div className="relative">
                        <div className="absolute inset-0 flex items-center">
                            <span className="w-full border-t border-slate-300 dark:border-slate-700" />
                        </div>
                        <div className="relative flex justify-center text-xs uppercase">
                            <span className="bg-slate-50 dark:bg-slate-950 px-4 text-muted-foreground tracking-widest">
                                {t("orContinueWith")}
                            </span>
                        </div>
                    </div>

                    {/* Demo accounts with VISIBLE colors */}
                    <div className="space-y-3">
                        {demoAccounts.map(acc => (
                            <button
                                key={acc.username}
                                onClick={() => fillCredentials(acc.username, acc.password)}
                                type="button"
                                className="w-full flex items-center gap-4 p-4 rounded-xl border-2 border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 hover:border-indigo-400 dark:hover:border-indigo-500 transition-all duration-200 group text-left shadow-sm hover:shadow-md"
                            >
                                <div className={cn("h-12 w-12 rounded-xl flex items-center justify-center shadow-lg transition-transform group-hover:scale-110", acc.color)}>
                                    <acc.icon className="h-6 w-6 text-white" />
                                </div>
                                <div className="flex-1">
                                    <div className="font-semibold text-base">{acc.label}</div>
                                    <div className="text-xs text-muted-foreground">{acc.desc}</div>
                                </div>
                                <code className="text-xs text-indigo-600 dark:text-indigo-400 font-mono bg-indigo-50 dark:bg-indigo-950 px-3 py-1.5 rounded-lg">{acc.username}</code>
                            </button>
                        ))}
                    </div>

                    <p className="text-center text-xs text-muted-foreground">
                        {t("secureConnection")}
                    </p>
                </div>
            </div>
        </div>
    );
}
