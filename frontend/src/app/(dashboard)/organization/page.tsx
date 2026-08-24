'use client';

import { useEffect, useState } from 'react';
import { Card } from '@/components/ui/Card';
import { Skeleton } from '@/components/ui/Skeleton';
import { organizationApi } from '@/lib/endpoints';
import type { DepartmentNode } from '@/lib/endpoints';
import styles from './page.module.css';

/**
 * Organization Page — department hierarchy tree.
 * Phase 3C1 foundation. Read-only tree for now; admin editing comes in a later phase.
 */
export default function OrganizationPage() {
    const [loading, setLoading] = useState(true);
    const [tree, setTree] = useState<DepartmentNode[]>([]);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const load = async () => {
            try {
                const data = await organizationApi.getTree();
                setTree(data);
            } catch (err) {
                console.error('Failed to load organization tree:', err);
                setError('Failed to load organization structure');
            } finally {
                setLoading(false);
            }
        };
        load();
    }, []);

    if (loading) {
        return (
            <div className={styles.page}>
                <Skeleton width={200} height={32} />
                <Card>
                    <Skeleton width="100%" height={200} />
                </Card>
            </div>
        );
    }

    if (error) {
        return (
            <div className={styles.page}>
                <h1 className={styles.title}>Organization</h1>
                <Card>
                    <p className={styles.error}>{error}</p>
                </Card>
            </div>
        );
    }

    if (tree.length === 0) {
        return (
            <div className={styles.page}>
                <h1 className={styles.title}>Organization</h1>
                <Card>
                    <p className={styles.empty}>No departments yet.</p>
                </Card>
            </div>
        );
    }

    return (
        <div className={styles.page}>
            <h1 className={styles.title}>Organization</h1>
            <Card>
                <div className={styles.tree}>
                    {tree.map((node) => (
                        <DepartmentNodeView key={node.departmentId} node={node} depth={0} />
                    ))}
                </div>
            </Card>
        </div>
    );
}

function DepartmentNodeView({ node, depth }: { node: DepartmentNode; depth: number }) {
    return (
        <div className={styles.node} style={{ marginLeft: depth * 24 }}>
            <div className={styles.nodeHeader}>
                <span className={styles.nodeName}>
                    {node.departmentName}
                    {node.departmentNameEn && node.departmentNameEn !== node.departmentName
                        ? ` (${node.departmentNameEn})`
                        : ''}
                </span>
                {node.managerName && <span className={styles.nodeManager}>👤 {node.managerName}</span>}
                <span className={styles.nodeHeadcount}>{node.directHeadcount} staff</span>
            </div>
            {node.children.length > 0 && (
                <div className={styles.children}>
                    {node.children.map((child) => (
                        <DepartmentNodeView key={child.departmentId} node={child} depth={depth + 1} />
                    ))}
                </div>
            )}
        </div>
    );
}
