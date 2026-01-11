"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { employeeSchema, EmployeeFormData } from "@/lib/schemas";
import { useTranslations } from "next-intl";
import { X, Save, Camera, Upload } from "lucide-react";
import { Department } from "@/lib/api";
import { useState } from "react";

interface EmployeeFormProps {
    departments: Department[];
    onSubmit: (data: EmployeeFormData) => Promise<void>;
    onCancel: () => void;
    initialData?: Partial<EmployeeFormData>;
    isSubmitting?: boolean;
}

export default function EmployeeForm({ departments, onSubmit, onCancel, initialData, isSubmitting = false }: EmployeeFormProps) {
    const t = useTranslations("employees");
    const tCommon = useTranslations("common");

    const {
        register,
        handleSubmit,
        formState: { errors }
    } = useForm<EmployeeFormData>({
        resolver: zodResolver(employeeSchema),
        defaultValues: {
            employeeCode: "",
            laoName: "",
            englishName: "",
            departmentId: 0,
            baseSalary: 0,
            hireDate: new Date().toISOString().split('T')[0],
            dependentCount: initialData?.dependentCount ?? 0,
            salaryCurrency: initialData?.salaryCurrency ?? "LAK",
            ...initialData
        }
    });

    const [selectedFile, setSelectedFile] = useState<File | null>(null);
    const [previewUrl, setPreviewUrl] = useState<string | null>(null);

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (file) {
            setSelectedFile(file);
            setPreviewUrl(URL.createObjectURL(file));
        }
    };

    // Pass file back to parent on submit
    const handleFormSubmit = async (data: EmployeeFormData) => {
        // We append the file to data (casting effectively) so parent can handle it
        await onSubmit({ ...data, file: selectedFile } as any);
    };

    return (
        <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-6">
            {/* Photo Upload Section */}
            <div className="flex flex-col items-center justify-center p-4 bg-slate-800/50 rounded-lg border-2 border-dashed border-slate-700 hover:border-indigo-500/50 transition-colors">
                <div className="relative w-32 h-32 mb-4 group">
                    <div className={`w-full h-full rounded-full overflow-hidden bg-slate-700 flex items-center justify-center border-4 border-slate-800 shadow-xl ${!previewUrl ? 'text-slate-500' : ''}`}>
                        {previewUrl ? (
                            <img src={previewUrl} alt="Preview" className="w-full h-full object-cover" />
                        ) : (
                            <Camera className="w-12 h-12" />
                        )}
                    </div>
                    <label className="absolute bottom-0 right-0 p-2 bg-indigo-600 hover:bg-indigo-500 text-white rounded-full cursor-pointer shadow-lg transition-transform hover:scale-105">
                        <Upload className="w-4 h-4" />
                        <input
                            type="file"
                            accept="image/png, image/jpeg"
                            className="hidden"
                            onChange={handleFileChange}
                        />
                    </label>
                </div>
                <p className="text-sm text-slate-400">Allowed: JPG, PNG (Max 2MB)</p>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {/* Employee Code */}
                <div>
                    <label className="block text-sm font-medium text-slate-300 mb-1">
                        Employee ID <span className="text-red-400">*</span>
                    </label>
                    <input
                        {...register("employeeCode")}
                        className="w-full px-3 py-2 bg-slate-800 border border-slate-700 rounded-lg text-slate-100 focus:outline-none focus:border-indigo-500"
                        placeholder="EMP001"
                    />
                    {errors.employeeCode && (
                        <p className="mt-1 text-xs text-red-400">{errors.employeeCode.message}</p>
                    )}
                </div>

                {/* Hire Date */}
                <div>
                    <label className="block text-sm font-medium text-slate-300 mb-1">
                        Hire Date <span className="text-red-400">*</span>
                    </label>
                    <input
                        type="date"
                        {...register("hireDate")}
                        className="w-full px-3 py-2 bg-slate-800 border border-slate-700 rounded-lg text-slate-100 focus:outline-none focus:border-indigo-500"
                    />
                    {errors.hireDate && (
                        <p className="mt-1 text-xs text-red-400">{errors.hireDate.message}</p>
                    )}
                </div>

                {/* Lao Name */}
                <div>
                    <label className="block text-sm font-medium text-slate-300 mb-1">
                        Lao Name <span className="text-red-400">*</span>
                    </label>
                    <input
                        {...register("laoName")}
                        className="w-full px-3 py-2 bg-slate-800 border border-slate-700 rounded-lg text-slate-100 focus:outline-none focus:border-indigo-500 font-lao"
                        placeholder="ທ້າວ ສົມພອນ"
                    />
                    {errors.laoName && (
                        <p className="mt-1 text-xs text-red-400">{errors.laoName.message}</p>
                    )}
                </div>

                {/* English Name */}
                <div>
                    <label className="block text-sm font-medium text-slate-300 mb-1">
                        English Name
                    </label>
                    <input
                        {...register("englishName")}
                        className="w-full px-3 py-2 bg-slate-800 border border-slate-700 rounded-lg text-slate-100 focus:outline-none focus:border-indigo-500"
                        placeholder="Mr. Somphon"
                    />
                </div>

                {/* Department */}
                <div>
                    <label className="block text-sm font-medium text-slate-300 mb-1">
                        Department <span className="text-red-400">*</span>
                    </label>
                    <select
                        {...register("departmentId", { valueAsNumber: true })}
                        className="w-full px-3 py-2 bg-slate-800 border border-slate-700 rounded-lg text-slate-100 focus:outline-none focus:border-indigo-500"
                    >
                        <option value={0}>Select Department</option>
                        {departments.map((dept) => (
                            <option key={dept.id} value={dept.id}>
                                {dept.departmentNameEn} ({dept.departmentNameLo})
                            </option>
                        ))}
                    </select>
                    {errors.departmentId && (
                        <p className="mt-1 text-xs text-red-400">{errors.departmentId.message}</p>
                    )}
                </div>

                {/* Base Salary & Currency */}
                <div className="md:col-span-1">
                    <label className="block text-sm font-medium text-slate-300 mb-1">
                        Base Salary <span className="text-red-400">*</span>
                    </label>
                    <div className="flex gap-2">
                        <input
                            type="number"
                            {...register("baseSalary", { valueAsNumber: true })}
                            className="flex-1 px-3 py-2 bg-slate-800 border border-slate-700 rounded-lg text-slate-100 focus:outline-none focus:border-indigo-500"
                        />
                        <select
                            {...register("salaryCurrency")}
                            className="w-24 px-3 py-2 bg-slate-800 border border-slate-700 rounded-lg text-slate-100 focus:outline-none focus:border-indigo-500"
                        >
                            <option value="LAK">LAK</option>
                            <option value="USD">USD</option>
                            <option value="THB">THB</option>
                        </select>
                    </div>
                    {errors.baseSalary && (
                        <p className="mt-1 text-xs text-red-400">{errors.baseSalary.message}</p>
                    )}
                </div>

                {/* Dependent Count */}
                <div>
                    <label className="block text-sm font-medium text-slate-300 mb-1">
                        Number of Dependents
                    </label>
                    <input
                        type="number"
                        {...register("dependentCount", { valueAsNumber: true })}
                        className="w-full px-3 py-2 bg-slate-800 border border-slate-700 rounded-lg text-slate-100 focus:outline-none focus:border-indigo-500"
                        placeholder="0"
                        min="0"
                        max="10"
                    />
                    <p className="text-xs text-slate-500 mt-1">For Tax Deduction (Max 3 applied)</p>
                </div>

                {/* Email */}
                <div>
                    <label className="block text-sm font-medium text-slate-300 mb-1">Email</label>
                    <input
                        type="email"
                        {...register("email")}
                        className="w-full px-3 py-2 bg-slate-800 border border-slate-700 rounded-lg text-slate-100 focus:outline-none focus:border-indigo-500"
                    />
                    {errors.email && (
                        <p className="mt-1 text-xs text-red-400">{errors.email.message}</p>
                    )}
                </div>

                {/* Phone */}
                <div>
                    <label className="block text-sm font-medium text-slate-300 mb-1">Phone</label>
                    <input
                        {...register("phone")}
                        className="w-full px-3 py-2 bg-slate-800 border border-slate-700 rounded-lg text-slate-100 focus:outline-none focus:border-indigo-500"
                    />
                </div>
            </div>

            <div className="flex justify-end gap-3 pt-6 border-t border-slate-700">
                <button
                    type="button"
                    onClick={onCancel}
                    className="px-4 py-2 text-slate-300 hover:text-white hover:bg-slate-800 rounded-lg transition-colors"
                >
                    {tCommon("cancel")}
                </button>
                <button
                    type="submit"
                    disabled={isSubmitting}
                    className="flex items-center gap-2 px-6 py-2 bg-indigo-600 hover:bg-indigo-500 text-white rounded-lg transition-colors disabled:opacity-50"
                >
                    <Save className="w-4 h-4" />
                    {isSubmitting ? "Saving..." : tCommon("save")}
                </button>
            </div>
        </form>
    );
}
