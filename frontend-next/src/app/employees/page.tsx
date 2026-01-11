"use client";

import { useState, useEffect } from "react";
import { useTranslations } from "next-intl";
import {
    Plus, Search, MoreHorizontal, Filter,
    Mail, Phone, Building2, Sparkles
} from "lucide-react";
import { employeesAPI, departmentsAPI, Employee, Department } from "@/lib/api";
import { useAuth } from "@/hooks/useAuth";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

// MOCK DATA FALLBACK
const mockEmployees: Employee[] = [
    { id: 1, laoName: "ສົມພອນ ຄຳສຸກ", englishName: "Somphon Khamsouk", jobTitle: "Software Engineer", departmentId: 1, employeeCode: "EMP001", hireDate: "2024-01-15", baseSalary: 8000000, status: "Active", email: "somphon@laohr.la", phone: "020 5555 1234" },
    { id: 2, laoName: "ດາວັນ ສີສະຫວັດ", englishName: "Davanh Sisavath", jobTitle: "HR Manager", departmentId: 2, employeeCode: "EMP002", hireDate: "2023-06-01", baseSalary: 12000000, status: "Active", email: "davanh@laohr.la", phone: "020 5555 5678" },
    { id: 3, laoName: "ມະນີວັນ ສຸກສະຫວັນ", englishName: "Manivanh Souksavan", jobTitle: "Accountant", departmentId: 3, employeeCode: "EMP003", hireDate: "2023-08-15", baseSalary: 9500000, status: "Active", email: "manivanh@laohr.la", phone: "020 5555 9012" },
    { id: 4, laoName: "ພູວົງ ໄຊຍະວົງ", englishName: "Phouvong Xaiyavong", jobTitle: "Sales Manager", departmentId: 4, employeeCode: "EMP004", hireDate: "2022-03-01", baseSalary: 11000000, status: "Active", email: "phouvong@laohr.la", phone: "020 5555 3456" },
    { id: 5, laoName: "ບຸນມີ ວົງພະຈັນ", englishName: "Bounmi Vongphachan", jobTitle: "Marketing Specialist", departmentId: 5, employeeCode: "EMP005", hireDate: "2024-02-01", baseSalary: 7500000, status: "Inactive", email: "bounmi@laohr.la", phone: "020 5555 7890" },
    { id: 6, laoName: "ນາງ ສຸພາພອນ", englishName: "Souphaphon Nang", jobTitle: "Office Admin", departmentId: 2, employeeCode: "EMP006", hireDate: "2023-11-15", baseSalary: 6500000, status: "Active", email: "souphaphon@laohr.la", phone: "020 5555 2468" },
    { id: 7, laoName: "ວິໄລພອນ ແກ້ວມະນີ", englishName: "Vilayphon Keomanee", jobTitle: "Senior Developer", departmentId: 1, employeeCode: "EMP007", hireDate: "2021-09-01", baseSalary: 15000000, status: "Active", email: "vilayphon@laohr.la", phone: "020 5555 1357" },
    { id: 8, laoName: "ຈັນທະລາ ພົມມະວົງ", englishName: "Chanthala Phommavong", jobTitle: "CFO", departmentId: 3, employeeCode: "EMP008", hireDate: "2020-01-01", baseSalary: 25000000, status: "Active", email: "chanthala@laohr.la", phone: "020 5555 8642" },
];

const mockDepartments: Department[] = [
    { id: 1, departmentNameLo: "ເຕັກໂນໂລຊີ", departmentNameEn: "IT" },
    { id: 2, departmentNameLo: "ບຸກຄະລາກອນ", departmentNameEn: "Human Resources" },
    { id: 3, departmentNameLo: "ການເງິນ", departmentNameEn: "Finance" },
    { id: 4, departmentNameLo: "ຂາຍ", departmentNameEn: "Sales" },
    { id: 5, departmentNameLo: "ການຕະຫຼາດ", departmentNameEn: "Marketing" },
];

const avatarColors = ["#6366f1", "#10b981", "#f59e0b", "#ef4444", "#8b5cf6", "#3b82f6", "#ec4899", "#14b8a6"];

export default function EmployeesPage() {
    const t = useTranslations("employees");
    const { canManageEmployees } = useAuth();

    const [employees, setEmployees] = useState<Employee[]>(mockEmployees);
    const [departments, setDepartments] = useState<Department[]>(mockDepartments);
    const [loading, setLoading] = useState(true);
    const [search, setSearch] = useState("");

    useEffect(() => {
        loadData();
    }, []);

    const loadData = async () => {
        try {
            const [emps, depts] = await Promise.all([
                employeesAPI.getAll(),
                departmentsAPI.getAll()
            ]);
            setEmployees(emps.length > 0 ? emps : mockEmployees);
            setDepartments(depts.length > 0 ? depts : mockDepartments);
        } catch (e) {
            setEmployees(mockEmployees);
            setDepartments(mockDepartments);
        } finally {
            setLoading(false);
        }
    };

    const getDepartmentName = (deptId: number) => {
        return departments.find(d => d.id === deptId)?.departmentNameEn || "-";
    };

    const filteredEmployees = employees.filter(emp =>
        emp.laoName.toLowerCase().includes(search.toLowerCase()) ||
        emp.englishName?.toLowerCase().includes(search.toLowerCase())
    );

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
                <div>
                    <h1 className="text-2xl font-semibold tracking-tight">{t("title")}</h1>
                    <p className="text-sm text-muted-foreground mt-1 leading-relaxed">{t("subtitle")}</p>
                </div>
                <div className="flex items-center gap-2 w-full sm:w-auto">
                    <div className="relative flex-1 sm:w-64">
                        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                        <Input placeholder={t("searchPlaceholder")} className="pl-9 h-10 rounded-xl border-2" value={search} onChange={(e) => setSearch(e.target.value)} />
                    </div>
                    <Button variant="outline" size="sm" className="h-10 gap-2 rounded-xl border-2">
                        <Filter className="h-4 w-4" />
                    </Button>
                    {canManageEmployees && (
                        <Button
                            className="h-10 gap-2 rounded-xl shadow-lg text-white"
                            style={{ background: 'linear-gradient(135deg, #6366f1, #a855f7)' }}
                        >
                            <Plus className="h-4 w-4" />
                            {t("addEmployee")}
                        </Button>
                    )}
                </div>
            </div>

            {/* Stats Bar */}
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
                {[
                    { label: "Total", value: employees.length, color: "#3b82f6" },
                    { label: t("active"), value: employees.filter(e => e.status === 'Active').length, color: "#10b981" },
                    { label: t("inactive"), value: employees.filter(e => e.status !== 'Active').length, color: "#f59e0b" },
                    { label: "Departments", value: departments.length, color: "#8b5cf6" },
                ].map((stat, i) => (
                    <div key={i} className="rounded-xl p-4 bg-white dark:bg-slate-900 border shadow-lg flex items-center gap-3">
                        <div className="w-12 h-12 rounded-xl flex items-center justify-center shadow-md" style={{ background: stat.color }}>
                            <Sparkles className="h-5 w-5 text-white" />
                        </div>
                        <div>
                            <p className="text-2xl font-bold tracking-tight">{stat.value}</p>
                            <p className="text-xs text-muted-foreground">{stat.label}</p>
                        </div>
                    </div>
                ))}
            </div>

            {/* Table */}
            <div className="rounded-2xl overflow-hidden bg-white dark:bg-slate-900 border shadow-lg">
                <div className="overflow-x-auto">
                    <table className="w-full text-sm">
                        <thead>
                            <tr className="border-b bg-slate-50 dark:bg-slate-800/50">
                                <th className="px-5 py-4 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">{t("employee")}</th>
                                <th className="px-5 py-4 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">{t("roleAndDept")}</th>
                                <th className="px-5 py-4 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">{t("contact")}</th>
                                <th className="px-5 py-4 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">{t("status")}</th>
                                <th className="px-5 py-4 text-right text-xs font-medium text-muted-foreground uppercase tracking-wider">Actions</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y">
                            {filteredEmployees.map((emp, idx) => (
                                <tr key={emp.id} className="group hover:bg-slate-50 dark:hover:bg-slate-800/50 transition-colors">
                                    <td className="px-5 py-4">
                                        <div className="flex items-center gap-3">
                                            <Avatar className="h-11 w-11 border-2 transition-transform group-hover:scale-105" style={{ borderColor: avatarColors[idx % avatarColors.length] }}>
                                                <AvatarImage src={emp.profilePath ? `http://localhost:5000${emp.profilePath}` : undefined} />
                                                <AvatarFallback className="text-xs font-medium text-white" style={{ background: avatarColors[idx % avatarColors.length] }}>
                                                    {emp.englishName?.substring(0, 2).toUpperCase() || "EM"}
                                                </AvatarFallback>
                                            </Avatar>
                                            <div>
                                                <div className="font-medium leading-relaxed">{emp.laoName}</div>
                                                <div className="text-xs text-muted-foreground">{emp.englishName}</div>
                                            </div>
                                        </div>
                                    </td>
                                    <td className="px-5 py-4">
                                        <div className="flex flex-col gap-1">
                                            <Badge className="w-fit font-normal text-white border-0" style={{ background: avatarColors[idx % avatarColors.length] }}>
                                                {emp.jobTitle || t("noRole")}
                                            </Badge>
                                            <div className="flex items-baseline gap-1 text-xs text-muted-foreground">
                                                <Building2 className="h-3 w-3" />
                                                <span>{getDepartmentName(emp.departmentId)}</span>
                                            </div>
                                        </div>
                                    </td>
                                    <td className="px-5 py-4">
                                        <div className="flex flex-col text-xs text-muted-foreground gap-1">
                                            <div className="flex items-center gap-2"><Mail className="h-3 w-3" style={{ color: '#3b82f6' }} /><span>{emp.email || "No Email"}</span></div>
                                            <div className="flex items-center gap-2"><Phone className="h-3 w-3" style={{ color: '#10b981' }} /><span>{emp.phone || "-"}</span></div>
                                        </div>
                                    </td>
                                    <td className="px-5 py-4">
                                        <Badge className="font-medium border-0 text-white" style={{ background: emp.status === 'Active' ? '#10b981' : '#94a3b8' }}>
                                            <span className="mr-1.5 h-1.5 w-1.5 rounded-full bg-white" />
                                            {emp.status === 'Active' ? t("active") : t("inactive")}
                                        </Badge>
                                    </td>
                                    <td className="px-5 py-4 text-right">
                                        <DropdownMenu>
                                            <DropdownMenuTrigger asChild>
                                                <Button variant="ghost" className="h-8 w-8 p-0"><MoreHorizontal className="h-4 w-4" /></Button>
                                            </DropdownMenuTrigger>
                                            <DropdownMenuContent align="end" className="w-[160px]">
                                                <DropdownMenuLabel>Actions</DropdownMenuLabel>
                                                <DropdownMenuItem>{t("viewDetails")}</DropdownMenuItem>
                                                <DropdownMenuItem>{t("documents")}</DropdownMenuItem>
                                                <DropdownMenuSeparator />
                                                <DropdownMenuItem className="text-red-600">Delete</DropdownMenuItem>
                                            </DropdownMenuContent>
                                        </DropdownMenu>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    );
}
