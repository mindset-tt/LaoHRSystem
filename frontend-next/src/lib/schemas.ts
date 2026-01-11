import { z } from "zod";

export const employeeSchema = z.object({
    employeeCode: z.string().min(1, "Employee code is required").max(20, "Code too long"),
    laoName: z.string().min(1, "Lao name is required"),
    englishName: z.string().optional(),
    email: z.string().email("Invalid email address").optional().or(z.literal("")),
    phone: z.string().optional(),
    baseSalary: z.number().min(0, "Salary cannot be negative"),
    departmentId: z.number().min(1, "Department is required"),
    // Additional fields for production
    dateOfBirth: z.string().optional(),
    hireDate: z.string().min(1, "Hire date is required"),
    nssfId: z.string().optional(),
    taxId: z.string().optional(),
    dependentCount: z.number().min(0).max(10),
    salaryCurrency: z.enum(["LAK", "USD", "THB"])
});

export type EmployeeFormData = z.infer<typeof employeeSchema>;
