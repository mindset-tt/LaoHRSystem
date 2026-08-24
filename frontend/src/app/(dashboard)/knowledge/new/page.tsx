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
import { knowledgeApi, ARTICLE_STATUSES, type KnowledgeCategoryItem } from '@/lib/endpoints';
import styles from '../page.module.css';

const STATUSES = ARTICLE_STATUSES;

const articleSchema = z.object({
    title: z.string().min(1, 'Title is required').max(200),
    summary: z.string().min(1, 'Summary is required').max(500),
    body: z.string().min(1, 'Body is required'),
    bodyLao: z.string().optional(),
    categoryId: z.number().int().positive(),
    status: z.enum(STATUSES),
});

type ArticleFormValues = z.infer<typeof articleSchema>;

export default function NewArticlePage() {
    const router = useRouter();
    const { t } = useLanguage();
    const toast = useToast();
    const [submitting, setSubmitting] = useState(false);
    const [categories, setCategories] = useState<KnowledgeCategoryItem[]>([]);

    useEffect(() => {
        knowledgeApi.categories().then(setCategories).catch(() => undefined);
    }, []);

    async function onSubmit(values: ArticleFormValues) {
        setSubmitting(true);
        try {
            await knowledgeApi.create({
                title: values.title,
                summary: values.summary,
                body: values.body,
                bodyLao: values.bodyLao || undefined,
                categoryId: values.categoryId,
                status: values.status,
            });
            toast.success(t.knowledge.articles.messages.created);
            router.push('/knowledge');
        } catch (err) {
            console.error(err);
            toast.error(t.knowledge.articles.messages.errorCreate);
        } finally {
            setSubmitting(false);
        }
    }

    return (
        <div className={styles.page}>
            <PageHeader
                title={t.knowledge.articles.newArticle}
                breadcrumbs={[
                    { label: t.nav.dashboard, href: '/' },
                    { label: t.knowledge.articles.title, href: '/knowledge' },
                    { label: t.knowledge.articles.newArticle },
                ]}
            />

            <Card>
                <Form<ArticleFormValues>
                    schema={articleSchema}
                    onSubmit={onSubmit}
                    defaultValues={{
                        title: '',
                        summary: '',
                        body: '',
                        bodyLao: '',
                        categoryId: 0,
                        status: 'DRAFT',
                    }}
                >
                    {({ formState: { errors } }) => (
                        <>
                            <FormSection title={t.knowledge.articles.title} description={t.knowledge.articles.subtitle}>
                                <FormGrid columns={2}>
                                    <FormSelectField
                                        name="categoryId"
                                        label={t.knowledge.articles.fields.category}
                                        options={[
                                            { value: '0', label: t.common.select },
                                            ...categories.map(c => ({ value: String(c.knowledgeCategoryId), label: c.name })),
                                        ]}
                                    />
                                    <FormSelectField
                                        name="status"
                                        label={t.knowledge.articles.fields.status}
                                        options={STATUSES.map(s => ({
                                            value: s,
                                            label: (t.knowledge.articles.status as Record<string, string>)[s.toLowerCase()] ?? s,
                                        }))}
                                    />
                                </FormGrid>

                                <FormGrid columns={1}>
                                    <FormField name="title" label={t.knowledge.articles.fields.title} error={errors.title?.message as string | undefined} />
                                    <FormField name="summary" label={t.knowledge.articles.fields.summary} error={errors.summary?.message as string | undefined} />
                                </FormGrid>

                                <FormGrid columns={1}>
                                    <FormField name="body" label={t.knowledge.articles.fields.body} error={errors.body?.message as string | undefined} />
                                    <FormField name="bodyLao" label="Body (Lao)" />
                                </FormGrid>
                            </FormSection>

                            <FormActions align="between">
                                <Button variant="ghost" onClick={() => router.push('/knowledge')}>
                                    {t.common.cancel}
                                </Button>
                                <Button type="submit" loading={submitting}>
                                    {t.knowledge.articles.newArticle}
                                </Button>
                            </FormActions>
                        </>
                    )}
                </Form>
            </Card>
        </div>
    );
}
