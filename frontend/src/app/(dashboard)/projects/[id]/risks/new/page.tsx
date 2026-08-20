'use client';

import { useRouter } from 'next/navigation';
import { useParams } from 'next/navigation';
import { useState } from 'react';
import { z } from 'zod';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { PageHeader } from '@/components/ui/PageHeader';
import { Form, FormField, FormSelectField, FormActions, FormGrid, FormSection } from '@/components/ui/Form';
import { useToast } from '@/components/ui/Toast';
import { risksApi } from '@/lib/endpoints';
import styles from '../../page.module.css';

const PRIORITIES = ['LOW', 'MEDIUM', 'HIGH', 'CRITICAL'] as const;
const STATUSES = ['OPEN', 'MITIGATING', 'CLOSED', 'ACCEPTED'] as const;
const LIKELIHOODS = [1, 2, 3, 4, 5] as const;

const riskSchema = z.object({
    title: z.string().min(1, 'Title is required').max(200),
    description: z.string().max(2000).optional(),
    priority: z.enum(PRIORITIES),
    likelihood: z.number().int().min(1).max(5),
    impact: z.number().int().min(1).max(5),
    status: z.enum(STATUSES),
    mitigation: z.string().max(2000).optional(),
    dueDate: z.string().optional(),
});

type RiskFormValues = z.infer<typeof riskSchema>;

export default function NewRiskPage() {
    const params = useParams<{ id: string }>();
    const projectId = Number(params.id);
    const router = useRouter();
    const { t } = useLanguage();
    const toast = useToast();
    const [submitting, setSubmitting] = useState(false);

    async function onSubmit(values: RiskFormValues) {
        setSubmitting(true);
        try {
            await risksApi.create(projectId, {
                title: values.title,
                description: values.description,
                priority: values.priority,
                likelihood: values.likelihood,
                impact: values.impact,
                status: values.status,
                mitigation: values.mitigation,
                dueDate: values.dueDate || undefined,
            });
            toast.success(t.risks.messages.created);
            router.push(`/projects/${projectId}/risks`);
        } catch (err) {
            console.error(err);
            toast.error(t.risks.messages.errorCreate);
        } finally {
            setSubmitting(false);
        }
    }

    return (
        <div className={styles.page}>
            <PageHeader
                title={t.risks.newRisk}
                breadcrumbs={[
                    { label: t.nav.dashboard, href: '/' },
                    { label: t.projects.title, href: '/projects' },
                    { label: `Project #${projectId}`, href: `/projects/${projectId}` },
                    { label: t.risks.title, href: `/projects/${projectId}/risks` },
                    { label: t.risks.newRisk },
                ]}
            />

            <Card>
                <Form<RiskFormValues>
                    schema={riskSchema}
                    onSubmit={onSubmit}
                    defaultValues={{
                        title: '',
                        description: '',
                        priority: 'MEDIUM',
                        likelihood: 3,
                        impact: 3,
                        status: 'OPEN',
                        mitigation: '',
                        dueDate: '',
                    }}
                >
                    {({ formState: { errors } }) => (
                        <>
                            <FormSection title={t.risks.title} description={t.risks.subtitle}>
                                <FormGrid columns={2}>
                                    <FormField
                                        name="title"
                                        label={t.risks.fields.title}
                                        placeholder="Database migration delay"
                                        error={errors.title?.message as string | undefined}
                                    />
                                    <FormSelectField
                                        name="priority"
                                        label={t.risks.fields.priority}
                                        options={PRIORITIES.map(p => ({
                                            value: p,
                                            label: (t.tasks.priority as Record<string, string>)[p.toLowerCase()] ?? p,
                                        }))}
                                    />
                                </FormGrid>

                                <FormGrid columns={3}>
                                    <FormSelectField
                                        name="likelihood"
                                        label={t.risks.fields.likelihood}
                                        options={LIKELIHOODS.map(l => ({ value: String(l), label: String(l) }))}
                                    />
                                    <FormSelectField
                                        name="impact"
                                        label={t.risks.fields.impact}
                                        options={LIKELIHOODS.map(l => ({ value: String(l), label: String(l) }))}
                                    />
                                    <FormSelectField
                                        name="status"
                                        label={t.risks.fields.status}
                                        options={STATUSES.map(s => ({
                                            value: s,
                                            label: (t.risks.status as Record<string, string>)[s.toLowerCase()] ?? s,
                                        }))}
                                    />
                                </FormGrid>

                                <FormGrid columns={1}>
                                    <FormField
                                        name="description"
                                        label={t.risks.fields.description}
                                        placeholder="What is the risk?"
                                        error={errors.description?.message as string | undefined}
                                    />
                                    <FormField
                                        name="mitigation"
                                        label={t.risks.fields.mitigation}
                                        placeholder="How will we mitigate?"
                                        error={errors.mitigation?.message as string | undefined}
                                    />
                                    <FormField
                                        name="dueDate"
                                        label={t.risks.fields.dueDate}
                                        type="date"
                                        error={errors.dueDate?.message as string | undefined}
                                    />
                                </FormGrid>
                            </FormSection>

                            <FormActions align="between">
                                <Button variant="ghost" onClick={() => router.push(`/projects/${projectId}/risks`)}>
                                    {t.common.cancel}
                                </Button>
                                <Button type="submit" loading={submitting}>
                                    {t.risks.newRisk}
                                </Button>
                            </FormActions>
                        </>
                    )}
                </Form>
            </Card>
        </div>
    );
}