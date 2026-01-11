"use client";

import { useState, useEffect } from "react";
import { useTranslations } from "next-intl";
import { Plus, Calendar as CalendarIcon, MoreHorizontal, Trash2, Edit, Sparkles, Star } from "lucide-react";
import { holidaysAPI, Holiday } from "@/lib/api";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { DropdownMenu, DropdownMenuTrigger, DropdownMenuContent, DropdownMenuItem } from "@/components/ui/dropdown-menu";

// MOCK DATA FALLBACK
const mockHolidays = [
    { holidayId: 1, date: "2026-01-01", name: "ວັນປີໃໝ່ສາກົນ", nameEn: "New Year's Day", isRecurring: true, year: 2026 },
    { holidayId: 2, date: "2026-03-08", name: "ວັນແມ່ຍິງສາກົນ", nameEn: "International Women's Day", isRecurring: true, year: 2026 },
    { holidayId: 3, date: "2026-04-14", name: "ວັນປີໃໝ່ລາວ", nameEn: "Lao New Year (Pi Mai)", isRecurring: true, year: 2026 },
    { holidayId: 4, date: "2026-04-15", name: "ວັນປີໃໝ່ລາວ", nameEn: "Lao New Year (Day 2)", isRecurring: true, year: 2026 },
    { holidayId: 5, date: "2026-04-16", name: "ວັນປີໃໝ່ລາວ", nameEn: "Lao New Year (Day 3)", isRecurring: true, year: 2026 },
    { holidayId: 6, date: "2026-05-01", name: "ວັນກຳມະກອນສາກົນ", nameEn: "Labor Day", isRecurring: true, year: 2026 },
    { holidayId: 7, date: "2026-12-02", name: "ວັນຊາດ", nameEn: "National Day", isRecurring: true, year: 2026 },
    { holidayId: 8, date: "2026-07-20", name: "ວັນບຸນເຂົ້າພັນສາ", nameEn: "Khao Phansa", isRecurring: false, year: 2026 },
];

const monthColors = [
    "#3b82f6", "#8b5cf6", "#ec4899", "#f59e0b", "#10b981", "#14b8a6",
    "#6366f1", "#a855f7", "#f472b6", "#f97316", "#06b6d4", "#ef4444"
];

export default function HolidaysPage() {
    const t = useTranslations("holidays");
    const [holidays, setHolidays] = useState(mockHolidays);
    const [loading, setLoading] = useState(true);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [formData, setFormData] = useState({ date: new Date().toISOString().split('T')[0], name: "", nameEn: "", isRecurring: true });
    const [editingId, setEditingId] = useState<number | null>(null);

    useEffect(() => {
        loadData();
    }, []);

    const loadData = async () => {
        try {
            const data = await holidaysAPI.getAll();
            if (data?.length > 0) {
                setHolidays(data);
            }
        } catch (e) {
            // Use mock data
        } finally {
            setLoading(false);
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        const payload = { ...formData, year: parseInt(formData.date.split('-')[0]) };
        try {
            if (editingId) {
                await holidaysAPI.update(editingId, payload);
            } else {
                await holidaysAPI.create(payload);
            }
            loadData();
        } catch (err) {
            if (editingId) {
                setHolidays(holidays.map(h => h.holidayId === editingId ? { ...h, ...formData, year: parseInt(formData.date.split('-')[0]) } : h));
            } else {
                setHolidays([...holidays, { holidayId: Date.now(), ...formData, year: parseInt(formData.date.split('-')[0]) }]);
            }
        }
        setIsModalOpen(false);
        setEditingId(null);
        setFormData({ date: new Date().toISOString().split('T')[0], name: "", nameEn: "", isRecurring: true });
    };

    const handleEdit = (holiday: Holiday) => {
        setEditingId(holiday.holidayId);
        setFormData({ date: holiday.date.split('T')[0], name: holiday.name, nameEn: holiday.nameEn, isRecurring: holiday.isRecurring });
        setIsModalOpen(true);
    };

    const handleDelete = async (id: number) => {
        try {
            await holidaysAPI.delete(id);
            loadData();
        } catch (e) {
            setHolidays(holidays.filter(h => h.holidayId !== id));
        }
    };

    return (
        <div className="space-y-6">
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
                <div>
                    <h1 className="text-2xl font-semibold tracking-tight">{t("title")}</h1>
                    <p className="text-sm text-muted-foreground mt-1 leading-relaxed">{t("subtitle")}</p>
                </div>
                <Button
                    onClick={() => { setEditingId(null); setFormData({ date: new Date().toISOString().split('T')[0], name: "", nameEn: "", isRecurring: true }); setIsModalOpen(true); }}
                    className="h-10 gap-2 rounded-xl shadow-lg text-white"
                    style={{ background: 'linear-gradient(135deg, #6366f1, #a855f7)' }}
                >
                    <Plus className="w-4 h-4" />
                    {t("addHoliday")}
                </Button>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                <div className="lg:col-span-2 rounded-2xl overflow-hidden bg-white dark:bg-slate-900 border shadow-lg">
                    <div className="p-4 border-b flex items-center gap-2" style={{ background: 'linear-gradient(90deg, rgba(99,102,241,0.1), rgba(168,85,247,0.05))' }}>
                        <Sparkles className="h-5 w-5" style={{ color: '#f59e0b' }} />
                        <h3 className="text-sm font-semibold tracking-tight">{t("publicHolidays")} 2026</h3>
                    </div>
                    <div className="divide-y">
                        {holidays.map((holiday) => {
                            const month = new Date(holiday.date).getMonth();
                            const color = monthColors[month];
                            return (
                                <div key={holiday.holidayId} className="flex items-center justify-between p-4 hover:bg-slate-50 dark:hover:bg-slate-800/50 transition-colors group">
                                    <div className="flex items-center gap-4">
                                        <div className="flex flex-col items-center justify-center h-16 w-16 rounded-xl text-white shadow-lg" style={{ background: color }}>
                                            <span className="text-[10px] uppercase font-bold opacity-80">{new Date(holiday.date).toLocaleString('en-US', { month: 'short' })}</span>
                                            <span className="text-2xl font-bold leading-none">{new Date(holiday.date).getDate()}</span>
                                        </div>
                                        <div>
                                            <div className="font-medium leading-relaxed flex items-center gap-2">
                                                {holiday.name}
                                                {holiday.name.includes("ປີໃໝ່") && <Star className="h-4 w-4" style={{ color: '#f59e0b', fill: '#f59e0b' }} />}
                                            </div>
                                            <div className="text-xs text-muted-foreground">{holiday.nameEn}</div>
                                        </div>
                                    </div>
                                    <div className="flex items-center gap-3">
                                        {holiday.isRecurring && <Badge className="text-xs font-normal text-white border-0" style={{ background: '#10b981' }}>{t("recurring")}</Badge>}
                                        <DropdownMenu>
                                            <DropdownMenuTrigger asChild>
                                                <Button variant="ghost" size="icon" className="h-8 w-8 opacity-0 group-hover:opacity-100 transition-opacity">
                                                    <MoreHorizontal className="h-4 w-4" />
                                                </Button>
                                            </DropdownMenuTrigger>
                                            <DropdownMenuContent align="end">
                                                <DropdownMenuItem onClick={() => handleEdit(holiday)}><Edit className="w-4 h-4 mr-2" /> Edit</DropdownMenuItem>
                                                <DropdownMenuItem onClick={() => handleDelete(holiday.holidayId)} className="text-destructive"><Trash2 className="w-4 h-4 mr-2" /> Delete</DropdownMenuItem>
                                            </DropdownMenuContent>
                                        </DropdownMenu>
                                    </div>
                                </div>
                            );
                        })}
                    </div>
                </div>

                {/* Summary Card */}
                <div className="rounded-2xl p-8 text-white shadow-2xl relative overflow-hidden" style={{ background: 'linear-gradient(135deg, #6366f1 0%, #8b5cf6 50%, #ec4899 100%)' }}>
                    <div className="absolute top-4 right-4">
                        <CalendarIcon className="h-24 w-24 text-white/10" />
                    </div>
                    <p className="text-xs font-medium text-white/80 uppercase tracking-wider mb-2">{t("totalHolidays")}</p>
                    <p className="text-6xl font-bold tracking-tight">{holidays.length}</p>
                    <p className="text-sm text-white/70 mt-6">
                        <span className="font-semibold text-white">{holidays.filter(h => h.isRecurring).length}</span> {t("recurringAnnually")}
                    </p>
                    <div className="mt-6 pt-6 border-t border-white/20">
                        <p className="text-sm text-white/80">Next: <span className="font-semibold text-white">Lao New Year - Apr 14</span></p>
                    </div>
                </div>
            </div>

            <Dialog open={isModalOpen} onOpenChange={setIsModalOpen}>
                <DialogContent className="rounded-2xl">
                    <DialogHeader><DialogTitle>{editingId ? t("editHoliday") : t("addHoliday")}</DialogTitle></DialogHeader>
                    <form onSubmit={handleSubmit} className="space-y-4 py-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium">{t("date")}</label>
                            <Input type="date" required value={formData.date} onChange={(e) => setFormData({ ...formData, date: e.target.value })} className="h-11 rounded-xl" />
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm font-medium">{t("nameLao")}</label>
                            <Input required value={formData.name} onChange={(e) => setFormData({ ...formData, name: e.target.value })} placeholder="e.g. ວັນປີໃໝ່ລາວ" className="h-11 rounded-xl" />
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm font-medium">{t("nameEn")}</label>
                            <Input value={formData.nameEn} onChange={(e) => setFormData({ ...formData, nameEn: e.target.value })} placeholder="e.g. Lao New Year" className="h-11 rounded-xl" />
                        </div>
                        <div className="flex items-center gap-2">
                            <input type="checkbox" id="recurring" checked={formData.isRecurring} onChange={(e) => setFormData({ ...formData, isRecurring: e.target.checked })} className="h-4 w-4 rounded" style={{ accentColor: '#6366f1' }} />
                            <label htmlFor="recurring" className="text-sm">{t("everyYear")}</label>
                        </div>
                        <DialogFooter>
                            <Button type="button" variant="outline" onClick={() => setIsModalOpen(false)} className="rounded-xl">Cancel</Button>
                            <Button type="submit" className="rounded-xl text-white" style={{ background: 'linear-gradient(135deg, #6366f1, #a855f7)' }}>Save</Button>
                        </DialogFooter>
                    </form>
                </DialogContent>
            </Dialog>
        </div>
    );
}
