'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { z } from 'zod';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { PageHeader } from '@/components/ui/PageHeader';
import { Form, FormField, FormSelectField, FormActions, FormGrid, FormSection } from '@/components/ui/Form';
import { useToast } from '@/components/ui/Toast';
import { announcementsApi, ANNOUNCEMENT_SEVERITIES } from '@/lib/endpoints';
import styles from '../../page.module.css';

const SEVERITIES = ANNOUNCEMENT_SEVERITIES;
const AUDIENCES = ['ALL', 'ROLE', 'DEPARTMENT'] as const;

const announcementSchema = z.object({
    title: z.string().min(1, 'Title is required').max(200),
    titleLao: z.string().max(200).optional(),
    body: z.string().min(1, 'Body is required'),
    bodyLao: z.string().optional(),
    severity: z.enum(SEVERITIES),
    audience: z.enum(AUDIENCES),
    isPinned: z.boolean(),
    publishFrom: z.string().optional(),
    publishUntil: z.string().optional(),
});

type AnnouncementFormValues = z.infer<typeof announcementSchema>;

export default function NewAnnouncementPage() {
    const router = useRouter();
    const { t } = useLanguage();
    const toast = useToast();
    const [submitting, setSubmitting] = useState(false);

    async function onSubmit(values: AnnouncementFormValues) {
        setSubmitting(true);
        try {
            await announcementsApi.create({
                title: values.title,
                titleLao: values.titleLao || undefined,
                body: values.body,
                bodyLao: values.bodyLao || undefined,
                severity: values.severity,
                audience: values.audience,
                isPinned: values.isPinned,
                publishFrom: values.publishFrom ? new Date(values.publishFrom).toISOString() : null,
                publishUntil: values.publishUntil ? new Date(values.publishUntil).toISOString() : null,
            });
            toast.success(t.knowledge.announcements.messages.created);
            router.push('/knowledge/announcements');
        } catch (err) {
            console.error(err);
            toast.error(t.knowledge.announcements.messages.errorCreate);
        } finally {
            setSubmitting(false);
        }
    }

    return (
        <div className={styles.page}>
            <PageHeader
                title={t.knowledge.announcements.newAnnouncement}
                breadcrumbs={[
                    { label: t.nav.dashboard, href: '/' },
                    { label: t.knowledge.announcements.title, href: '/knowledge/announcements' },
                    { label: t.knowledge.announcements.newAnnouncement },
                ]}
            />

            <Card>
                <Form<AnnouncementFormValues>
                    schema={announcementSchema}
                    onSubmit={onSubmit}
                    defaultValues={{
                        title: '',
                        titleLao: '',
                        body: '',
                        bodyLao: '',
                        severity: 'INFO',
                        audience: 'ALL',
                        isPinned: false,
                        publishFrom: '',
                        publishUntil: '',
                    }}
                >
                    {({ formState: { errors } }) => (
                        <>
                            <FormSection title={t.knowledge.announcements.title} description={t.knowledge.announcements.subtitle}>
                                <FormGrid columns={1}>
                                    <FormField name="title" label={t.knowledge.announcements.fields.title} error={errors.title?.message as string | undefined} />
                                    <FormField name="titleLao" label={t.knowledge.announcements.fields.titleLao} error={errors.titleLao?.message as string | undefined} />
                                </FormGrid>

                                <FormGrid columns={2}>
                                    <FormSelectField
                                        name="severity"
                                        label={t.knowledge.announcements.fields.severity}
                                        options={SEVERITIES.map(s => ({
                                            value: s,
                                            label: (t.knowledge.announcements.severity as Record<string, string>)[s.toLowerCase()] ?? s,
                                        }))}
                                    />
                                    <FormSelectField
                                        name="audience"
                                        label={t.knowledge.announcements.fields.audience}
                                        options={AUDIENCES.map(a => ({
                                            value: a,
                                            label: (t.knowledge.announcements.audience as Record<string, string>)[a.toLowerCase()] ?? a,
                                        }))}
                                    />
                                </FormGrid>

                                <FormGrid columns={2}>
                                    <FormField name="publishFrom" label={t.knowledge.announcements.fields.publishFrom} type="datetime-local" />
                                    <FormField name="publishUntil" label={t.knowledge.announcements.fields.publishUntil} type="datetime-local" />
                                </FormGrid>

                                <FormGrid columns={1}>
                                    <FormField name="body" label={t.knowledge.announcements.fields.body} error={errors.body?.message as string | undefined} />
                                    <FormField name="bodyLao" label={t.knowledge.announcements.fields.bodyLao} />
                                </FormGrid>
                            </FormSection>

                            <FormActions align="between">
                                <Button variant="ghost" onClick={() => router.push('/knowledge/announcements')}>
                                    {t.common.cancel}
                                </Button>
                                <Button type="submit" loading={submitting}>
                                    {t.knowledge.announcements.newAnnouncement}
                                </Button>
                            </FormActions>
                        </>
                    )}
                </Form>
            </Card>
        </div>
    );
}
