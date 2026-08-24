'use client';

/**
 * Polymorphic comment thread component.
 *
 * Backs onto the `commentsApi` so a single component can be embedded on any
 * entity type (PROJECT, TASK, ISSUE, EXPENSE, LOAN, RISK, RESOURCE).
 *
 * One-level threading is rendered natively by the API (parent + replies);
 * deeper replies are flattened to a second level visually with a left border.
 *
 * Lightweight by design: no rich text editor — body is plain text.
 * Soft-deleted comments render as italicised "[deleted]" placeholders.
 */

import { useCallback, useEffect, useState } from 'react';
import { useAuth } from '@/components/providers/AuthProvider';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Button } from '@/components/ui/Button';
import { useToast } from '@/components/ui/Toast';
import { commentsApi, type CommentItem } from '@/lib/endpoints';
import styles from './CommentThread.module.css';

export interface CommentThreadProps {
    entityType: string;
    entityId: number;
    /** Optional title override; otherwise uses i18n `knowledge.comments.title`. */
    title?: string;
}

type DraftKind = 'top' | 'reply';

interface DraftState {
    kind: DraftKind;
    parentId: number | null;
    body: string;
}

const EMPTY_DRAFT: DraftState = { kind: 'top', parentId: null, body: '' };

export function CommentThread({ entityType, entityId, title }: CommentThreadProps) {
    const { user, role } = useAuth();
    const { t } = useLanguage();
    const toast = useToast();
    const isAdmin = role === 'Admin';

    const [comments, setComments] = useState<CommentItem[]>([]);
    const [loading, setLoading] = useState(true);
    const [draft, setDraft] = useState<DraftState>(EMPTY_DRAFT);
    const [editingId, setEditingId] = useState<number | null>(null);
    const [editingBody, setEditingBody] = useState('');
    const [submitting, setSubmitting] = useState(false);

    const c = t.knowledge.comments;

    const load = useCallback(async () => {
        setLoading(true);
        try {
            const data = await commentsApi.list(entityType, entityId);
            setComments(data ?? []);
        } catch (err) {
            console.error(err);
            toast.error(c.errorPost);
        } finally {
            setLoading(false);
        }
    }, [entityType, entityId, c.errorPost, toast]);

    useEffect(() => {
        if (!entityType || !entityId) return;
        load();
    }, [entityType, entityId, load]);

    const submitDraft = async () => {
        const trimmed = draft.body.trim();
        if (!trimmed) return;
        setSubmitting(true);
        try {
            await commentsApi.create({
                entityType,
                entityId,
                parentCommentId: draft.parentId,
                body: trimmed,
            });
            setDraft(EMPTY_DRAFT);
            await load();
        } catch (err) {
            console.error(err);
            toast.error(c.errorPost);
        } finally {
            setSubmitting(false);
        }
    };

    const startEdit = (cm: CommentItem) => {
        setEditingId(cm.entityCommentId);
        setEditingBody(cm.body);
    };

    const saveEdit = async (id: number) => {
        const trimmed = editingBody.trim();
        if (!trimmed) return;
        setSubmitting(true);
        try {
            await commentsApi.update(id, trimmed);
            setEditingId(null);
            setEditingBody('');
            await load();
        } catch (err) {
            console.error(err);
            toast.error(c.errorUpdate);
        } finally {
            setSubmitting(false);
        }
    };

    const remove = async (id: number) => {
        if (typeof window !== 'undefined' && !window.confirm(c.confirmDelete)) return;
        try {
            await commentsApi.delete(id);
            await load();
        } catch (err) {
            console.error(err);
            toast.error(c.errorDelete);
        }
    };

    const startReply = (parentId: number) => {
        setDraft({ kind: 'reply', parentId, body: '' });
    };

    const cancelReply = () => {
        setDraft(EMPTY_DRAFT);
    };

    // Compare by username when available; otherwise fall back to displayName.
    // (UserInfo exposes displayName; the API's CommentItem exposes authorName.)
    const currentAuthorKey =
        (user?.displayName ?? user?.username ?? '').trim().toLowerCase();

    return (
        <section className={styles.thread} aria-label={title ?? c.title}>
            <header className={styles.header}>
                <h3 className={styles.title}>{title ?? c.title}</h3>
                <span className={styles.count}>
                    {comments.reduce((sum, cm) => sum + 1 + (cm.replies?.length ?? 0), 0)}
                </span>
            </header>

            {loading ? (
                <p className={styles.muted}>{t.common.loading}</p>
            ) : comments.length === 0 ? (
                <p className={styles.muted}>{c.empty}</p>
            ) : (
                <ul className={styles.list}>
                    {comments.map(cm => (
                        <CommentNode
                            key={cm.entityCommentId}
                            comment={cm}
                            currentAuthorKey={currentAuthorKey}
                            isAdmin={isAdmin}
                            editingId={editingId}
                            editingBody={editingBody}
                            onStartEdit={startEdit}
                            onChangeEditBody={setEditingBody}
                            onSaveEdit={saveEdit}
                            onCancelEdit={() => { setEditingId(null); setEditingBody(''); }}
                            onDelete={remove}
                            onReply={startReply}
                            onCancelReply={cancelReply}
                            replyTargetId={draft.kind === 'reply' ? draft.parentId : null}
                            replyBody={draft.body}
                            onChangeReplyBody={(v) => setDraft({ kind: 'reply', parentId: cm.entityCommentId, body: v })}
                            onSubmitReply={submitDraft}
                            submitting={submitting}
                        />
                    ))}
                </ul>
            )}

            {/* Top-level draft */}
            <div className={styles.composer}>
                <textarea
                    className={styles.textarea}
                    placeholder={c.placeholder}
                    value={draft.body}
                    onChange={(e) => setDraft({ kind: 'top', parentId: null, body: e.target.value })}
                    disabled={submitting}
                    rows={3}
                />
                <div className={styles.composerActions}>
                    <Button
                        type="button"
                        onClick={submitDraft}
                        loading={submitting}
                        disabled={!draft.body.trim()}
                    >
                        {c.submit}
                    </Button>
                </div>
            </div>
        </section>
    );
}

interface CommentNodeProps {
    comment: CommentItem;
    currentAuthorKey: string;
    isAdmin: boolean;
    editingId: number | null;
    editingBody: string;
    onStartEdit: (c: CommentItem) => void;
    onChangeEditBody: (s: string) => void;
    onSaveEdit: (id: number) => void;
    onCancelEdit: () => void;
    onDelete: (id: number) => void;
    onReply: (parentId: number) => void;
    onCancelReply: () => void;
    replyTargetId: number | null;
    replyBody: string;
    onChangeReplyBody: (s: string) => void;
    onSubmitReply: () => void;
    submitting: boolean;
}

function CommentNode(props: CommentNodeProps) {
    const { comment, currentAuthorKey, isAdmin, editingId, editingBody,
        onStartEdit, onChangeEditBody, onSaveEdit, onCancelEdit,
        onDelete, onReply, onCancelReply, replyTargetId, replyBody,
        onChangeReplyBody, onSubmitReply, submitting } = props;
    const { t } = useLanguage();
    const c = t.knowledge.comments;

    const isDeleted = !!comment.deletedAt;
    const isEditing = editingId === comment.entityCommentId;
    const isAuthor = !!currentAuthorKey &&
        !!comment.authorName &&
        comment.authorName.trim().toLowerCase() === currentAuthorKey;
    const canModify = !isDeleted && (isAuthor || isAdmin);
    const isReplying = replyTargetId === comment.entityCommentId;

    return (
        <li className={styles.item}>
            <article className={`${styles.comment} ${isDeleted ? styles.deleted : ''}`}>
                <header className={styles.commentHeader}>
                    <span className={styles.author}>{comment.authorName ?? 'Unknown'}</span>
                    <time className={styles.time} dateTime={comment.createdAt}>
                        {new Date(comment.createdAt).toLocaleString()}
                    </time>
                </header>

                {isEditing ? (
                    <div className={styles.editArea}>
                        <textarea
                            className={styles.textarea}
                            value={editingBody}
                            onChange={(e) => onChangeEditBody(e.target.value)}
                            rows={3}
                            disabled={submitting}
                        />
                        <div className={styles.inlineActions}>
                            <Button size="sm" onClick={() => onSaveEdit(comment.entityCommentId)} loading={submitting}>
                                {c.save}
                            </Button>
                            <Button size="sm" variant="ghost" onClick={onCancelEdit}>
                                {c.cancel}
                            </Button>
                        </div>
                    </div>
                ) : (
                    <p className={styles.body}>{comment.body}</p>
                )}

                {!isDeleted && !isEditing && (
                    <div className={styles.actions}>
                        <button
                            type="button"
                            className={styles.actionBtn}
                            onClick={() => onReply(comment.entityCommentId)}
                        >
                            {c.reply}
                        </button>
                        {canModify && (
                            <>
                                <button
                                    type="button"
                                    className={styles.actionBtn}
                                    onClick={() => onStartEdit(comment)}
                                >
                                    {c.edit}
                                </button>
                                <button
                                    type="button"
                                    className={styles.actionBtn}
                                    onClick={() => onDelete(comment.entityCommentId)}
                                >
                                    {c.delete}
                                </button>
                            </>
                        )}
                    </div>
                )}

                {isReplying && (
                    <div className={styles.replyComposer}>
                        <textarea
                            className={styles.textarea}
                            placeholder={c.placeholder}
                            value={replyBody}
                            onChange={(e) => onChangeReplyBody(e.target.value)}
                            rows={2}
                            disabled={submitting}
                        />
                        <div className={styles.inlineActions}>
                            <Button size="sm" onClick={onSubmitReply} loading={submitting} disabled={!replyBody.trim()}>
                                {c.reply}
                            </Button>
                            <Button size="sm" variant="ghost" onClick={onCancelReply}>
                                {c.cancel}
                            </Button>
                        </div>
                    </div>
                )}

                {comment.replies && comment.replies.length > 0 && (
                    <ul className={styles.replies}>
                        {comment.replies.map(r => (
                            <CommentNode
                                key={r.entityCommentId}
                                comment={r}
                                currentAuthorKey={currentAuthorKey}
                                isAdmin={isAdmin}
                                editingId={editingId}
                                editingBody={editingBody}
                                onStartEdit={onStartEdit}
                                onChangeEditBody={onChangeEditBody}
                                onSaveEdit={onSaveEdit}
                                onCancelEdit={onCancelEdit}
                                onDelete={onDelete}
                                onReply={onReply}
                                onCancelReply={onCancelReply}
                                replyTargetId={replyTargetId}
                                replyBody={replyBody}
                                onChangeReplyBody={onChangeReplyBody}
                                onSubmitReply={onSubmitReply}
                                submitting={submitting}
                            />
                        ))}
                    </ul>
                )}
            </article>
        </li>
    );
}
