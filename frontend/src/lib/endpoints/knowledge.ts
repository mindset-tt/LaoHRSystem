// Phase 5 — Knowledge & Collaboration API clients.
// Mirrors the conventions used by projects.ts / finance.ts.

import { apiClient } from '../apiClient';
import type { PaginatedResponse } from '../types/pagination';

export const ANNOUNCEMENT_SEVERITIES = ['INFO', 'WARNING', 'URGENT'] as const;
export const ARTICLE_STATUSES = ['DRAFT', 'PUBLISHED', 'ARCHIVED'] as const;
export const COMMENT_ENTITY_TYPES = [
    'PROJECT','TASK','ISSUE','EXPENSE','LOAN','RISK','RESOURCE',
] as const;

// ---------------- Announcements ----------------

export interface AnnouncementListItem {
    announcementId: number;
    title: string;
    titleLao?: string | null;
    severity: string;
    audience: string;
    isPinned: boolean;
    publishFrom?: string | null;
    publishUntil?: string | null;
    authorId: number;
    authorName?: string | null;
    createdAt: string;
    isRead: boolean;
}

export interface AnnouncementDetail extends AnnouncementListItem {
    body: string;
    bodyLao?: string | null;
    audienceDepartmentId?: number | null;
    updatedAt: string;
}

export interface CreateAnnouncementInput {
    title: string;
    titleLao?: string | null;
    body: string;
    bodyLao?: string | null;
    severity?: string;
    audience?: string;
    audienceDepartmentId?: number | null;
    isPinned?: boolean;
    publishFrom?: string | null;
    publishUntil?: string | null;
}

export interface UpdateAnnouncementInput {
    title?: string;
    titleLao?: string | null;
    body?: string;
    bodyLao?: string | null;
    severity?: string;
    audience?: string;
    audienceDepartmentId?: number | null;
    isPinned?: boolean;
    publishFrom?: string | null;
    publishUntil?: string | null;
}

function toQuery(params: Record<string, unknown>): string {
    const sp = new URLSearchParams();
    for (const [k, v] of Object.entries(params)) {
        if (v === undefined || v === null || v === '') continue;
        sp.set(k, String(v));
    }
    const q = sp.toString();
    return q ? `?${q}` : '';
}

export const announcementsApi = {
    list: (params: Record<string, string | number | boolean | undefined> = {}) =>
        apiClient.get<PaginatedResponse<AnnouncementListItem>>(`/api/announcements${toQuery(params)}`),
    get: (id: number) => apiClient.get<AnnouncementDetail>(`/api/announcements/${id}`),
    create: (input: CreateAnnouncementInput) =>
        apiClient.post<AnnouncementDetail>('/api/announcements', input),
    update: (id: number, input: UpdateAnnouncementInput) =>
        apiClient.put<void>(`/api/announcements/${id}`, input),
    delete: (id: number) => apiClient.delete<void>(`/api/announcements/${id}`),
    markRead: (id: number) => apiClient.post<void>(`/api/announcements/${id}/read`),
};

// ---------------- Knowledge Articles ----------------

export interface KnowledgeCategoryItem {
    knowledgeCategoryId: number;
    code: string;
    name: string;
    nameLao?: string | null;
    description?: string | null;
    sortOrder: number;
    isActive: boolean;
    articleCount: number;
}

export interface KnowledgeArticleListItem {
    knowledgeArticleId: number;
    title: string;
    titleLao?: string | null;
    summary: string;
    categoryId: number;
    categoryName?: string | null;
    status: string;
    viewCount: number;
    authorId: number;
    authorName?: string | null;
    publishedAt?: string | null;
    updatedAt: string;
}

export interface KnowledgeArticleDetail extends KnowledgeArticleListItem {
    body: string;
    bodyLao?: string | null;
    createdAt: string;
}

export interface CreateArticleInput {
    title: string;
    titleLao?: string | null;
    summary: string;
    body: string;
    bodyLao?: string | null;
    categoryId: number;
    status?: string;
}

export interface UpdateArticleInput {
    title?: string;
    titleLao?: string | null;
    summary?: string;
    body?: string;
    bodyLao?: string | null;
    categoryId?: number;
    status?: string;
}

export const knowledgeApi = {
    categories: () => apiClient.get<KnowledgeCategoryItem[]>('/api/knowledgeArticles/categories'),
    list: (params: Record<string, string | number | boolean | undefined> = {}) =>
        apiClient.get<PaginatedResponse<KnowledgeArticleListItem>>(`/api/knowledgeArticles${toQuery(params)}`),
    get: (id: number) => apiClient.get<KnowledgeArticleDetail>(`/api/knowledgeArticles/${id}`),
    create: (input: CreateArticleInput) => apiClient.post<KnowledgeArticleDetail>('/api/knowledgeArticles', input),
    update: (id: number, input: UpdateArticleInput) => apiClient.put<void>(`/api/knowledgeArticles/${id}`, input),
    delete: (id: number) => apiClient.delete<void>(`/api/knowledgeArticles/${id}`),
};

// ---------------- Comments (polymorphic) ----------------

export interface CommentItem {
    entityCommentId: number;
    entityType: string;
    entityId: number;
    parentCommentId?: number | null;
    authorId: number;
    authorName?: string | null;
    body: string;
    createdAt: string;
    updatedAt?: string | null;
    deletedAt?: string | null;
    replies: CommentItem[];
}

export interface CreateCommentInput {
    entityType: string;
    entityId: number;
    parentCommentId?: number | null;
    body: string;
}

export interface CommentSummary {
    entityType: string;
    entityId: number;
    totalComments: number;
}

export const commentsApi = {
    list: (entityType: string, entityId: number) =>
        apiClient.get<CommentItem[]>(`/api/comments/${entityType}/${entityId}`),
    summary: (entityType: string, entityId: number) =>
        apiClient.get<CommentSummary>(`/api/comments/${entityType}/${entityId}/summary`),
    create: (input: CreateCommentInput) => apiClient.post<CommentItem>('/api/comments', input),
    update: (id: number, body: string) => apiClient.put<void>(`/api/comments/${id}`, { body }),
    delete: (id: number) => apiClient.delete<void>(`/api/comments/${id}`),
};
