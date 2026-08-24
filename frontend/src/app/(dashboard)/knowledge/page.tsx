'use client';

import { useEffect, useMemo, useState } from 'react';
import Link from 'next/link';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { PageHeader } from '@/components/ui/PageHeader';
import { Pagination } from '@/components/ui/Pagination';
import { useToast } from '@/components/ui/Toast';
import {
    knowledgeApi,
    type KnowledgeArticleListItem,
    type KnowledgeCategoryItem,
} from '@/lib/endpoints';
import type { PaginatedResponse } from '@/lib/types/pagination';
import styles from './page.module.css';

export default function KnowledgePage() {
    const { t } = useLanguage();
    const toast = useToast();
    const [pageData, setPageData] = useState<PaginatedResponse<KnowledgeArticleListItem>>({
        items: [], page: 1, pageSize: 25, totalItems: 0, totalPages: 0, hasNext: false, hasPrevious: false,
    });
    const [categories, setCategories] = useState<KnowledgeCategoryItem[]>([]);
    const [activeCategory, setActiveCategory] = useState<number | null>(null);
    const [loading, setLoading] = useState(true);
    const [search, setSearch] = useState('');
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(25);

    useEffect(() => {
        knowledgeApi.categories().then(setCategories).catch(() => undefined);
    }, []);

    useEffect(() => {
        let cancelled = false;
        setLoading(true);
        knowledgeApi.list({
            categoryId: activeCategory ?? undefined,
            search: search || undefined,
            page,
            pageSize,
        })
            .then((p) => { if (!cancelled) setPageData(p); })
            .catch((err) => {
                console.error(err);
                toast.error(t.knowledge.articles.messages.errorUpdate);
            })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [activeCategory, search, page, pageSize, t.knowledge.articles.messages.errorUpdate, toast]);

    const headerActions = useMemo(() => (
        <Link href="/knowledge/new">
            <Button>{t.knowledge.articles.newArticle}</Button>
        </Link>
    ), [t]);

    return (
        <div className={styles.page}>
            <PageHeader
                title={t.knowledge.articles.title}
                subtitle={t.knowledge.articles.subtitle}
                breadcrumbs={[
                    { label: t.nav.dashboard, href: '/' },
                    { label: t.knowledge.articles.title },
                ]}
                actions={headerActions}
            />

            <Card className={styles.filtersCard}>
                <div className={styles.filters}>
                    <input
                        type="search"
                        placeholder={t.projects.searchPlaceholder}
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                        className={styles.searchInput}
                    />
                </div>
            </Card>

            <div className={styles.knowledgeLayout}>
                <Card>
                    <h3 style={{ marginTop: 0 }}>{t.knowledge.articles.categories.title}</h3>
                    <div className={styles.categoryList}>
                        <button
                            type="button"
                            className={`${styles.categoryItem} ${activeCategory === null ? styles.active : ''}`}
                            onClick={() => { setActiveCategory(null); setPage(1); }}
                        >
                            <span>All</span>
                            <span className={styles.categoryCount}>
                                {categories.reduce((s, c) => s + c.articleCount, 0)}
                            </span>
                        </button>
                        {categories.map(c => (
                            <button
                                type="button"
                                key={c.knowledgeCategoryId}
                                className={`${styles.categoryItem} ${activeCategory === c.knowledgeCategoryId ? styles.active : ''}`}
                                onClick={() => { setActiveCategory(c.knowledgeCategoryId); setPage(1); }}
                            >
                                <span>{c.name}</span>
                                <span className={styles.categoryCount}>{c.articleCount}</span>
                            </button>
                        ))}
                    </div>
                </Card>

                <div>
                    {loading ? (
                        <Card>…</Card>
                    ) : pageData.items.length === 0 ? (
                        <Card>
                            <div style={{ padding: 'var(--space-4)', textAlign: 'center' }}>
                                <h3>{t.knowledge.articles.empty.title}</h3>
                                <p style={{ color: 'var(--text-muted)' }}>{t.knowledge.articles.empty.description}</p>
                            </div>
                        </Card>
                    ) : (
                        <>
                            <div className={styles.articleGrid}>
                                {pageData.items.map(a => (
                                    <Link
                                        key={a.knowledgeArticleId}
                                        href={`/knowledge/${a.knowledgeArticleId}`}
                                        className={styles.articleCard}
                                    >
                                        <h4 className={styles.articleTitle}>{a.title}</h4>
                                        <p className={styles.articleSummary}>{a.summary}</p>
                                        <div className={styles.articleMeta}>
                                            <span>{a.categoryName ?? '—'}</span>
                                            <span>·</span>
                                            <span>{new Date(a.updatedAt).toLocaleDateString()}</span>
                                            {a.viewCount > 0 && (
                                                <>
                                                    <span>·</span>
                                                    <span>{a.viewCount} {t.knowledge.articles.fields.views}</span>
                                                </>
                                            )}
                                        </div>
                                    </Link>
                                ))}
                            </div>
                            <div style={{ marginTop: 'var(--space-3)' }}>
                                <Pagination
                                    page={pageData.page}
                                    pageSize={pageData.pageSize}
                                    totalItems={pageData.totalItems}
                                    totalPages={pageData.totalPages}
                                    hasNext={pageData.hasNext}
                                    hasPrevious={pageData.hasPrevious}
                                    onPageChange={setPage}
                                    onPageSizeChange={setPageSize}
                                />
                            </div>
                        </>
                    )}
                </div>
            </div>
        </div>
    );
}
