"use client";

import { useTranslations } from "next-intl";
import { Users, Clock, AlertCircle, CreditCard, TrendingUp, TrendingDown, Activity, ArrowUpRight } from "lucide-react";
import { AttendanceChart } from "@/components/DashboardCharts";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";

// MOCK DATA
const mockActivity = [
  { user: "ສົມພອນ ຄຳສຸກ", action: "Clocked in", time: "08:00 AM", active: true },
  { user: "ດາວັນ ສີສະຫວັດ", action: "Approved leave", time: "08:15 AM" },
  { user: "ມະນີວັນ ສຸກສະຫວັນ", action: "Submitted payroll", time: "09:30 AM" },
  { user: "ພູວົງ ໄຊຍະວົງ", action: "Clocked in", time: "09:45 AM" },
  { user: "ບຸນມີ ວົງພະຈັນ", action: "Requested leave", time: "10:00 AM" },
];

const avatarColors = ["#6366f1", "#10b981", "#f59e0b", "#ef4444", "#8b5cf6"];

export default function Dashboard() {
  const t = useTranslations("dashboard");

  const metrics = [
    { key: "totalEmployees", value: "128", change: "+12", trend: "up", subtitle: t("fromLastMonth"), icon: Users, color: "#6366f1" },
    { key: "presentToday", value: "94.2%", change: "+2.1%", trend: "up", subtitle: t("attendanceRate"), icon: Clock, color: "#10b981" },
    { key: "estPayroll", value: "₭ 856M", change: "+5%", trend: "neutral", subtitle: t("thisMonth"), icon: CreditCard, color: "#f59e0b" },
    { key: "pendingLeave", value: "12", change: "3 urgent", trend: "down", subtitle: t("requiresAttention"), icon: AlertCircle, color: "#ef4444" },
  ];

  return (
    <div className="space-y-6">
      {/* Page Header with Gradient Banner */}
      <div className="relative overflow-hidden rounded-2xl p-8 text-white shadow-2xl" style={{ background: 'linear-gradient(135deg, #6366f1 0%, #8b5cf6 50%, #ec4899 100%)' }}>
        <div className="absolute inset-0 opacity-20" style={{ backgroundImage: 'url("data:image/svg+xml,%3Csvg width=\'60\' height=\'60\' viewBox=\'0 0 60 60\' xmlns=\'http://www.w3.org/2000/svg\'%3E%3Cg fill=\'none\' fill-rule=\'evenodd\'%3E%3Cg fill=\'%23ffffff\' fill-opacity=\'0.4\'%3E%3Cpath d=\'M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z\'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E")' }} />
        <div className="absolute top-0 right-0 w-64 h-64 bg-white/10 rounded-full blur-3xl -translate-y-1/2 translate-x-1/2" />
        <div className="relative">
          <h1 className="text-3xl font-bold tracking-tight">{t("title")}</h1>
          <p className="text-lg text-white/80 mt-2">{t("subtitle")}</p>
        </div>
      </div>

      {/* Metric Cards */}
      <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
        {metrics.map((m) => (
          <div key={m.key} className="rounded-2xl p-6 bg-white dark:bg-slate-900 border shadow-lg hover:shadow-xl transition-all duration-300 hover:-translate-y-1 relative overflow-hidden">
            <div className="absolute top-0 left-0 right-0 h-1.5" style={{ background: m.color }} />

            <div className="flex items-start justify-between">
              <div>
                <p className="text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">{t(m.key)}</p>
                <p className="mt-2 text-3xl font-bold tracking-tight text-slate-900 dark:text-white">{m.value}</p>
              </div>
              <div
                className="flex h-12 w-12 items-center justify-center rounded-xl shadow-lg"
                style={{ background: m.color }}
              >
                <m.icon className="h-6 w-6 text-white" />
              </div>
            </div>
            <div className="mt-4 flex items-baseline gap-2 text-xs">
              <span
                className="inline-flex items-center gap-1 font-semibold px-2 py-1 rounded-full text-white"
                style={{
                  background: m.trend === "up" ? '#10b981' : m.trend === "down" ? '#ef4444' : '#f59e0b'
                }}
              >
                {m.trend === "up" && <TrendingUp className="h-3 w-3" />}
                {m.trend === "down" && <TrendingDown className="h-3 w-3" />}
                {m.change}
              </span>
              <span className="text-slate-500 dark:text-slate-400">{m.subtitle}</span>
            </div>
          </div>
        ))}
      </div>

      {/* Charts Row */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-12">
        {/* Attendance Trend */}
        <div className="col-span-1 lg:col-span-8 rounded-2xl p-6 bg-white dark:bg-slate-900 border shadow-lg">
          <div className="mb-4 flex items-center justify-between">
            <div>
              <h3 className="text-base font-semibold tracking-tight text-slate-900 dark:text-white">{t("attendanceOverview")}</h3>
              <p className="text-xs text-slate-500 dark:text-slate-400 mt-0.5">{t("weeklyTrends")}</p>
            </div>
            <div
              className="flex items-center gap-1 text-xs font-medium px-3 py-1.5 rounded-full text-white"
              style={{ background: '#10b981' }}
            >
              <TrendingUp className="h-3 w-3" />
              +12% this week
            </div>
          </div>
          <div className="h-[280px] w-full">
            <AttendanceChart />
          </div>
        </div>

        {/* Recent Activity */}
        <div className="col-span-1 lg:col-span-4 rounded-2xl p-6 bg-white dark:bg-slate-900 border shadow-lg">
          <div className="mb-4 flex items-center justify-between">
            <h3 className="text-base font-semibold tracking-tight flex items-center gap-2 text-slate-900 dark:text-white">
              <Activity className="h-5 w-5" style={{ color: '#8b5cf6' }} />
              {t("recentActivity")}
            </h3>
            <a href="#" className="text-xs font-medium flex items-center gap-1" style={{ color: '#6366f1' }}>
              View All <ArrowUpRight className="h-3 w-3" />
            </a>
          </div>
          <div className="space-y-4">
            {mockActivity.map((item, i) => (
              <div key={i} className="flex items-start gap-3 group">
                <div className="relative">
                  <Avatar className="h-10 w-10 border-2" style={{ borderColor: avatarColors[i % avatarColors.length] }}>
                    <AvatarFallback className="text-[10px] font-medium text-white" style={{ background: avatarColors[i % avatarColors.length] }}>
                      {item.user.substring(0, 2).toUpperCase()}
                    </AvatarFallback>
                  </Avatar>
                  {item.active && (
                    <span className="absolute -bottom-0.5 -right-0.5 h-3 w-3 rounded-full border-2 border-white dark:border-slate-900 animate-pulse" style={{ background: '#10b981' }} />
                  )}
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm leading-relaxed">
                    <span className="font-medium text-slate-900 dark:text-white">{item.user}</span>
                    <span className="text-slate-500 dark:text-slate-400 ml-1">{item.action}</span>
                  </p>
                  <div className="flex items-baseline gap-1 text-[11px] text-slate-500 dark:text-slate-400">
                    <Clock className="h-3 w-3" />
                    <span>{item.time}</span>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
