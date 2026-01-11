"use client";

import { useEffect, useState } from "react";
import { useTranslations } from "next-intl";
import { CheckCircle, XCircle, Navigation, Clock, Sparkles } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { AttendanceMap } from "@/components/map";

// MOCK DATA
const mockAttendance = [
    { name: "ສົມພອນ ຄຳສຸກ", nameEn: "Somphon K.", type: "Clock In", time: "08:00:00", status: "On Time" },
    { name: "ດາວັນ ສີສະຫວັດ", nameEn: "Davanh S.", type: "Clock In", time: "08:15:00", status: "On Time" },
    { name: "ມະນີວັນ ສຸກສະຫວັນ", nameEn: "Manivanh S.", type: "Clock In", time: "09:30:00", status: "Late" },
    { name: "ພູວົງ ໄຊຍະວົງ", nameEn: "Phouvong X.", type: "Clock Out", time: "17:00:00", status: "Regular" },
    { name: "ບຸນມີ ວົງພະຈັນ", nameEn: "Bounmi V.", type: "Clock Out", time: "17:30:00", status: "Overtime" },
];

export default function AttendancePage() {
    const t = useTranslations("attendance");

    const [currentTime, setCurrentTime] = useState<Date | null>(null);
    const [isClockedIn, setIsClockedIn] = useState(false);
    const [location, setLocation] = useState({ lat: 17.9757, lng: 102.6331 });
    const [locationName] = useState<string>("Vientiane, Laos");
    const [attendanceLog] = useState(mockAttendance);

    useEffect(() => {
        setCurrentTime(new Date());
        const timer = setInterval(() => setCurrentTime(new Date()), 1000);
        return () => clearInterval(timer);
    }, []);

    const formatTime = (date: Date) => date.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: false });
    const formatDate = (date: Date) => date.toLocaleDateString('en-US', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });

    const avatarColors = ["#6366f1", "#10b981", "#f59e0b", "#ef4444", "#8b5cf6"];

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
                <div>
                    <h1 className="text-2xl font-semibold tracking-tight text-slate-900 dark:text-white">{t("title")}</h1>
                    <p className="text-sm text-slate-600 dark:text-slate-400 mt-1 leading-relaxed">{t("subtitle")}</p>
                </div>
                <div className="inline-flex items-center px-5 py-3 rounded-xl text-sm font-medium border bg-white dark:bg-slate-900 shadow-lg">
                    <Clock className="w-5 h-5 mr-3" style={{ color: '#6366f1' }} />
                    <span className="font-mono text-xl font-bold" style={{ color: '#6366f1' }}>{currentTime ? formatTime(currentTime) : "--:--:--"}</span>
                    <span className="mx-4 h-6 w-px bg-slate-200 dark:bg-slate-700" />
                    <span className="text-slate-600 dark:text-slate-400">{currentTime ? formatDate(currentTime) : "Loading..."}</span>
                </div>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                {/* Clock Card */}
                <div className="rounded-2xl p-8 flex flex-col items-center justify-center border bg-white dark:bg-slate-900 shadow-xl relative overflow-hidden">
                    <div className="absolute top-0 left-0 right-0 h-2" style={{ background: 'linear-gradient(90deg, #6366f1, #a855f7, #ec4899)' }} />

                    <div className="w-52 h-52 rounded-full flex items-center justify-center mb-8 relative" style={{ background: 'linear-gradient(135deg, rgba(99,102,241,0.1), rgba(168,85,247,0.1))' }}>
                        <div className="absolute inset-2 rounded-full border-4 border-dashed border-indigo-300 dark:border-indigo-600" />
                        <div className="text-5xl font-mono font-bold" style={{ color: '#6366f1' }}>
                            {currentTime ? formatTime(currentTime) : "--:--"}
                        </div>
                    </div>

                    <div className="flex gap-4 w-full max-w-sm">
                        <Button
                            onClick={() => setIsClockedIn(true)}
                            disabled={isClockedIn}
                            className="flex-1 h-14 text-lg font-semibold rounded-xl shadow-xl text-white"
                            style={{ background: isClockedIn ? '#94a3b8' : '#10b981' }}
                        >
                            <CheckCircle className="w-6 h-6 mr-2" />
                            {t("clockIn")}
                        </Button>
                        <Button
                            onClick={() => setIsClockedIn(false)}
                            disabled={!isClockedIn}
                            className="flex-1 h-14 text-lg font-semibold rounded-xl shadow-xl text-white"
                            style={{ background: !isClockedIn ? '#94a3b8' : '#ef4444' }}
                        >
                            <XCircle className="w-6 h-6 mr-2" />
                            {t("clockOut")}
                        </Button>
                    </div>
                </div>

                {/* Location Card with AttendanceMap Component */}
                <div className="rounded-2xl overflow-hidden border bg-white dark:bg-slate-900 shadow-xl">
                    <div className="p-4 border-b flex items-center justify-between">
                        <div>
                            <h3 className="text-sm font-semibold flex items-center gap-2 tracking-tight text-slate-900 dark:text-white">
                                <Navigation className="w-4 h-4" style={{ color: '#ef4444' }} /> {t("currentLocation")}
                            </h3>
                            <p className="text-xs text-slate-600 dark:text-slate-400 mt-1 flex items-center gap-1 truncate">
                                {locationName} ({location.lat.toFixed(4)}, {location.lng.toFixed(4)})
                            </p>
                        </div>
                        <Badge className="text-white border-0 shadow-md" style={{ background: '#10b981' }}>
                            Within Office Zone
                        </Badge>
                    </div>
                    {/* Using the new AttendanceMap component */}
                    <div className="h-[280px]">
                        <AttendanceMap
                            lat={location.lat}
                            lng={location.lng}
                            popupText="Your Location"
                            className="h-full rounded-none border-0"
                        />
                    </div>
                </div>
            </div>

            {/* History Table */}
            <div className="rounded-2xl overflow-hidden border bg-white dark:bg-slate-900 shadow-xl">
                <div className="p-4 border-b flex items-center gap-2" style={{ background: 'linear-gradient(90deg, rgba(99,102,241,0.1), rgba(168,85,247,0.05))' }}>
                    <Sparkles className="h-5 w-5" style={{ color: '#f59e0b' }} />
                    <h3 className="text-sm font-semibold tracking-tight text-slate-900 dark:text-white">{t("todaysLog")}</h3>
                    <Badge variant="secondary" className="ml-auto">{attendanceLog.length} records</Badge>
                </div>
                <table className="w-full text-sm">
                    <thead>
                        <tr className="border-b bg-slate-50 dark:bg-slate-800/50">
                            <th className="px-5 py-3 text-left text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">Employee</th>
                            <th className="px-5 py-3 text-left text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">{t("type")}</th>
                            <th className="px-5 py-3 text-left text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">{t("time")}</th>
                            <th className="px-5 py-3 text-left text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">Status</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-200 dark:divide-slate-700">
                        {attendanceLog.map((row, i) => (
                            <tr key={i} className="hover:bg-slate-50 dark:hover:bg-slate-800/50 transition-colors">
                                <td className="px-5 py-4">
                                    <div className="flex items-center gap-3">
                                        <Avatar className="h-10 w-10 border-2" style={{ borderColor: avatarColors[i % avatarColors.length] }}>
                                            <AvatarFallback className="text-xs font-medium text-white" style={{ background: avatarColors[i % avatarColors.length] }}>
                                                {row.nameEn.substring(0, 2)}
                                            </AvatarFallback>
                                        </Avatar>
                                        <div>
                                            <span className="font-medium text-slate-900 dark:text-white">{row.name}</span>
                                            <div className="text-xs text-slate-500 dark:text-slate-400">{row.nameEn}</div>
                                        </div>
                                    </div>
                                </td>
                                <td className="px-5 py-4">
                                    <Badge className="text-white border-0" style={{ background: row.type === "Clock In" ? '#10b981' : '#6366f1' }}>
                                        {row.type === "Clock In" ? t("clockIn") : t("clockOut")}
                                    </Badge>
                                </td>
                                <td className="px-5 py-4 font-mono text-slate-600 dark:text-slate-400">{row.time}</td>
                                <td className="px-5 py-4">
                                    <Badge className="border-0 text-white" style={{
                                        background: row.status === "On Time" ? '#10b981' :
                                            row.status === "Late" ? '#ef4444' :
                                                row.status === "Overtime" ? '#f59e0b' : '#64748b'
                                    }}>
                                        {row.status === "On Time" ? t("onTime") : row.status === "Late" ? t("late") : row.status === "Overtime" ? t("overtime") : t("regular")}
                                    </Badge>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
}
