'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { z } from 'zod';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { PageHeader } from '@/components/ui/PageHeader';
import { Form, FormField, FormSelectField, FormActions, FormGrid, FormSection } from '@/components/ui/Form';
import { useToast } from '@/components/ui/Toast';
import { expensesApi, employeesApi, type ExpenseCategoryItem } from '@/lib/endpoints';
import type { Employee } from '@/lib/types';
import styles from '../../page.module.css';

const CURRENCIES = ['LAK', 'USD', 'THB'] as const;

const expenseSchema = z.object({
    employeeId: z.number().int().positive(),
    categoryId: z.number().int().positive(),
    title: z.string().min(1, 'Title is required').max(200),
    description: z.string().max(2000).optional(),
    expenseDate: z.string().min(1),
    currency: z.enum(CURRENCIES),
    amount: z.number().positive('Amount must be positive'),
});

type ExpenseFormValues = z.infer<typeof expenseSchema>;

export default function NewExpensePage() {
    const router = useRouter();
    const { t } = useLanguage();
    const toast = useToast();
    const [submitting, setSubmitting] = useState(false);
    const [categories, setCategories] = useState<ExpenseCategoryItem[]>([]);
    const [employees, setEmployees] = useState<Array<{ employeeId: number; displayName: string }>>([]);

    useEffect(() => {
        expensesApi.categories().then(setCategories).catch(() => undefined);
        employeesApi.getAll({ pageSize: 500 })
            .then((p) => setEmployees(
                p.items.map(e => ({
                    employeeId: e.employeeId,
                    displayName: (e as Employee).englishName || (e as Employee).laoName || (e as Employee).employeeCode,
                }))
            ))
            .catch(() => undefined);
    }, []);

    async function onSubmit(values: ExpenseFormValues) {
        setSubmitting(true);
        try {
            await expensesApi.create({
                ...values,
                description: values.description || undefined,
            });
            toast.success(t.finance.expenses.messages.created);
            router.push('/finance/expenses');
        } catch (err) {
            console.error(err);
            toast.error(t.finance.expenses.messages.errorCreate);
        } finally {
            setSubmitting(false);
        }
    }

    const today = new Date().toISOString().slice(0, 10);

    return (
        <div className={styles.page}>
            <PageHeader
                title={t.finance.expenses.newExpense}
                breadcrumbs={[
                    { label: t.nav.dashboard, href: '/' },
                    { label: t.finance.expenses.title, href: '/finance/expenses' },
                    { label: t.finance.expenses.newExpense },
                ]}
            />

            <Card>
                <Form<ExpenseFormValues>
                    schema={expenseSchema}
                    onSubmit={onSubmit}
                    defaultValues={{
                        employeeId: 0,
                        categoryId: 0,
                        title: '',
                        description: '',
                        expenseDate: today,
                        currency: 'LAK',
                        amount: 0,
                    }}
                >
                    {({ formState: { errors } }) => (
                        <>
                            <FormSection title={t.finance.expenses.title} description={t.finance.expenses.subtitle}>
                                <FormGrid columns={2}>
                                    <FormSelectField
                                        name="employeeId"
                                        label={t.finance.expenses.fields.employee}
                                        options={[
                                            { value: '0', label: t.common.select },
                                            ...employees.map(e => ({ value: String(e.employeeId), label: e.displayName })),
                                        ]}
                                    />
                                    <FormSelectField
                                        name="categoryId"
                                        label={t.finance.expenses.fields.category}
                                        options={[
                                            { value: '0', label: t.common.select },
                                            ...categories.map(c => ({ value: String(c.expenseCategoryId), label: c.name })),
                                        ]}
                                    />
                                </FormGrid>

                                <FormGrid columns={1}>
                                    <FormField
                                        name="title"
                                        label={t.finance.expenses.fields.title}
                                        placeholder="Taxi to client meeting"
                                        error={errors.title?.message as string | undefined}
                                    />
                                </FormGrid>

                                <FormGrid columns={3}>
                                    <FormSelectField
                                        name="currency"
                                        label={t.finance.expenses.fields.currency}
                                        options={CURRENCIES.map(c => ({ value: c, label: c }))}
                                    />
                                    <FormField
                                        name="amount"
                                        label={t.finance.expenses.fields.amount}
                                        type="number"
                                        placeholder="0.00"
                                        error={errors.amount?.message as string | undefined}
                                    />
                                    <FormField
                                        name="expenseDate"
                                        label={t.finance.expenses.fields.date}
                                        type="date"
                                        error={errors.expenseDate?.message as string | undefined}
                                    />
                                </FormGrid>

                                <FormGrid columns={1}>
                                    <FormField
                                        name="description"
                                        label={t.finance.expenses.fields.description}
                                        placeholder="Notes for the approver"
                                        error={errors.description?.message as string | undefined}
                                    />
                                </FormGrid>
                            </FormSection>

                            <FormActions align="between">
                                <Button variant="ghost" onClick={() => router.push('/finance/expenses')}>
                                    {t.common.cancel}
                                </Button>
                                <Button type="submit" loading={submitting}>
                                    {t.finance.expenses.newExpense}
                                </Button>
                            </FormActions>
                        </>
                    )}
                </Form>
            </Card>
        </div>
    );
}
