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
import { loansApi, employeesApi } from '@/lib/endpoints';
import type { Employee } from '@/lib/types';
import styles from '../../page.module.css';

const CURRENCIES = ['LAK', 'USD', 'THB'] as const;
const LOAN_TYPES = ['LOAN', 'ADVANCE'] as const;

const loanSchema = z.object({
    employeeId: z.number().int().positive(),
    loanType: z.enum(LOAN_TYPES),
    purpose: z.string().max(500).optional(),
    currency: z.enum(CURRENCIES),
    principalAmount: z.number().positive('Principal must be positive'),
    interestRate: z.number().min(0).max(100),
    installments: z.number().int().min(1).max(120),
    startDate: z.string().min(1),
});

type LoanFormValues = z.infer<typeof loanSchema>;

export default function NewLoanPage() {
    const router = useRouter();
    const { t } = useLanguage();
    const toast = useToast();
    const [submitting, setSubmitting] = useState(false);
    const [employees, setEmployees] = useState<Array<{ employeeId: number; displayName: string }>>([]);

    useEffect(() => {
        employeesApi.getAll({ pageSize: 500 })
            .then((p) => setEmployees(
                p.items.map(e => ({
                    employeeId: e.employeeId,
                    displayName: (e as Employee).englishName || (e as Employee).laoName || (e as Employee).employeeCode,
                }))
            ))
            .catch(() => undefined);
    }, []);

    async function onSubmit(values: LoanFormValues) {
        setSubmitting(true);
        try {
            await loansApi.create({
                ...values,
                purpose: values.purpose || undefined,
            });
            toast.success(t.finance.loans.messages.created);
            router.push('/finance/loans');
        } catch (err) {
            console.error(err);
            toast.error(t.finance.loans.messages.errorCreate);
        } finally {
            setSubmitting(false);
        }
    }

    const today = new Date().toISOString().slice(0, 10);

    return (
        <div className={styles.page}>
            <PageHeader
                title={t.finance.loans.newLoan}
                breadcrumbs={[
                    { label: t.nav.dashboard, href: '/' },
                    { label: t.finance.loans.title, href: '/finance/loans' },
                    { label: t.finance.loans.newLoan },
                ]}
            />

            <Card>
                <Form<LoanFormValues>
                    schema={loanSchema}
                    onSubmit={onSubmit}
                    defaultValues={{
                        employeeId: 0,
                        loanType: 'LOAN',
                        purpose: '',
                        currency: 'LAK',
                        principalAmount: 0,
                        interestRate: 0,
                        installments: 6,
                        startDate: today,
                    }}
                >
                    {({ formState: { errors } }) => (
                        <>
                            <FormSection title={t.finance.loans.title} description={t.finance.loans.subtitle}>
                                <FormGrid columns={2}>
                                    <FormSelectField
                                        name="employeeId"
                                        label={t.finance.loans.fields.employee}
                                        options={[
                                            { value: '0', label: t.common.select },
                                            ...employees.map(e => ({ value: String(e.employeeId), label: e.displayName })),
                                        ]}
                                    />
                                    <FormSelectField
                                        name="loanType"
                                        label={t.finance.loans.fields.type}
                                        options={LOAN_TYPES.map(lt => ({
                                            value: lt,
                                            label: lt === 'ADVANCE' ? t.finance.loans.type.advance : t.finance.loans.type.loan,
                                        }))}
                                    />
                                </FormGrid>

                                <FormGrid columns={1}>
                                    <FormField
                                        name="purpose"
                                        label={t.finance.loans.fields.purpose}
                                        placeholder="Medical emergency"
                                        error={errors.purpose?.message as string | undefined}
                                    />
                                </FormGrid>

                                <FormGrid columns={4}>
                                    <FormSelectField
                                        name="currency"
                                        label={t.finance.loans.fields.principal}
                                        options={CURRENCIES.map(c => ({ value: c, label: c }))}
                                    />
                                    <FormField
                                        name="principalAmount"
                                        label={t.finance.loans.fields.principal}
                                        type="number"
                                        placeholder="0.00"
                                        error={errors.principalAmount?.message as string | undefined}
                                    />
                                    <FormField
                                        name="interestRate"
                                        label={t.finance.loans.fields.interestRate}
                                        type="number"
                                        placeholder="0"
                                        error={errors.interestRate?.message as string | undefined}
                                    />
                                    <FormField
                                        name="installments"
                                        label={t.finance.loans.fields.installments}
                                        type="number"
                                        placeholder="6"
                                        error={errors.installments?.message as string | undefined}
                                    />
                                </FormGrid>

                                <FormGrid columns={1}>
                                    <FormField
                                        name="startDate"
                                        label={t.finance.loans.fields.startDate}
                                        type="date"
                                        error={errors.startDate?.message as string | undefined}
                                    />
                                </FormGrid>
                            </FormSection>

                            <FormActions align="between">
                                <Button variant="ghost" onClick={() => router.push('/finance/loans')}>
                                    {t.common.cancel}
                                </Button>
                                <Button type="submit" loading={submitting}>
                                    {t.finance.loans.newLoan}
                                </Button>
                            </FormActions>
                        </>
                    )}
                </Form>
            </Card>
        </div>
    );
}
