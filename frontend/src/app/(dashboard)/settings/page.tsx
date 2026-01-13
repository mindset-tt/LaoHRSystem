'use client';

import { useAuth } from '@/components/providers/AuthProvider';
import { Button } from '@/components/ui/Button';
import styles from './page.module.css';

export default function SettingsPage() {
    const { user, logout } = useAuth();

    return (
        <div className={styles.container}>
            <div className={styles.header}>
                <h1 className={styles.title}>Settings</h1>
                <p className={styles.subtitle}>Manage your account and preferences</p>
            </div>

            {/* Profile Section */}
            <div className={styles.section}>
                <div className={styles.sectionHeader}>
                    <h2 className={styles.sectionTitle}>Profile</h2>
                    <p className={styles.sectionDescription}>
                        Your account information
                    </p>
                </div>

                <div className={styles.field}>
                    <label className={styles.label}>Display Name</label>
                    <div className={styles.value}>{user?.displayName}</div>
                </div>

                <div className={styles.field}>
                    <label className={styles.label}>Username</label>
                    <div className={styles.value}>{user?.username}</div>
                </div>

                <div className={styles.field}>
                    <label className={styles.label}>Role</label>
                    <div className={styles.value}>{user?.role}</div>
                </div>

                <div className={styles.actions}>
                    <Button variant="secondary" onClick={logout}>
                        Sign Out
                    </Button>
                </div>
            </div>

            {/* Application Settings (Placeholder) */}
            <div className={styles.section}>
                <div className={styles.sectionHeader}>
                    <h2 className={styles.sectionTitle}>Application</h2>
                    <p className={styles.sectionDescription}>
                        System preferences
                    </p>
                </div>

                <div className={styles.field}>
                    <label className={styles.label}>Language</label>
                    <select className={styles.input} disabled>
                        <option>English</option>
                        <option>Lao</option>
                    </select>
                </div>

                <div className={styles.field}>
                    <label className={styles.label}>Theme</label>
                    <select className={styles.input} disabled>
                        <option>System Default</option>
                        <option>Light</option>
                        <option>Dark</option>
                    </select>
                </div>

                <div className={styles.actions}>
                    <Button disabled>Save Changes</Button>
                </div>
            </div>
        </div>
    );
}
