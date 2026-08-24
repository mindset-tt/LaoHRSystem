'use client';

import { useEffect, useState } from 'react';
import { useParams } from 'next/navigation';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { PageHeader } from '@/components/ui/PageHeader';
import { ErrorState } from '@/components/ui/EmptyState';
import { knowledgeApi, type KnowledgeArticleDetail } from '@/lib/endpoints';
import styles from '../page.module.css';

export default function ArticleDetailPage() {
    const params = useParams<{ id: string }>();
    const articleId = Number(params.id);
    const { t } = useLanguage();
    const [article, setArticle] = useState<KnowledgeArticleDetail | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        if (!articleId) return;
        let cancelled = false;
        setLoading(true);
        knowledgeApi.get(articleId)
            .then((d) => { if (!cancelled) setArticle(d); })
            .catch(() => undefined)
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [articleId]);

    if (loading) {
        return (
            <PageHeader
                title="…"
                subtitle="…"
                breadcrumbs={[
                    { label: t.nav.dashboard, href: '/' },
                    { label: t.knowledge.articles.title, href: '/knowledge' },
                    { label: '…' },
                ]}
            />
        );
    }

    if (!article) {
        return (
            <Card>
                <ErrorState
                    title="Article not found"
                    description="The article you tried to view does not exist or has been archived."
                />
            </Card>
        );
    }

    return (
        <div className={styles.page}>
            <PageHeader
                title={article.title}
                subtitle={article.summary}
                breadcrumbs={[
                    { label: t.nav.dashboard, href: '/' },
                    { label: t.knowledge.articles.title, href: '/knowledge' },
                    { label: article.title },
                ]}
            />

            <Card>
                <div className={styles.articleMeta} style={{ marginBottom: 'var(--space-3)' }}>
                    <span>{article.categoryName ?? '—'}</span>
                    <span>·</span>
                    <span>{article.authorName ?? '—'}</span>
                    {article.publishedAt && (
                        <>
                            <span>·</span>
                            <span>{new Date(article.publishedAt).toLocaleDateString()}</span>
                        </>
                    )}
                    <span>·</span>
                    <span>{article.viewCount} {t.knowledge.articles.fields.views}</span>
                </div>
                <div className={styles.articleBody}>{article.body}</div>

                {article.titleLao && article.bodyLao && (
                    <details style={{ marginTop: 'var(--space-4)' }}>
                        <summary style={{ cursor: 'pointer', color: 'var(--text-muted)' }}>ສະບັບພາສາລາວ</summary>
                        <h3 style={{ marginTop: 'var(--space-2)' }}>{article.titleLao}</h3>
                        <div className={styles.articleBody}>{article.bodyLao}</div>
                    </details>
                )}
            </Card>
        </div>
    );
}
