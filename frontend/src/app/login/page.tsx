'use client';

import { useState, useEffect, type FormEvent } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/components/providers/AuthProvider';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card } from '@/components/ui/Card';
import { ApiClientError } from '@/lib/apiClient';
import styles from './page.module.css';

/**
 * Login Page
 * 
 * Premium, clean login experience with form validation
 * and error handling.
 */
export default function LoginPage() {
    const router = useRouter();
    const { login, isAuthenticated, loading: authLoading } = useAuth();
    const { t } = useLanguage();

    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);

    // Redirect if already authenticated
    useEffect(() => {
        if (!authLoading && isAuthenticated) {
            router.push('/');
        }
    }, [authLoading, isAuthenticated, router]);

    if (authLoading || isAuthenticated) {
        return (
            <div className={styles.container}>
                <div className={styles.loadingWrapper}>
                    <div className={styles.spinner} />
                </div>
            </div>
        );
    }

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();
        setError('');

        if (!username.trim() || !password.trim()) {
            setError(t.login.errors.required);
            return;
        }

        setLoading(true);

        try {
            await login({ username, password });
        } catch (err) {
            console.error('Login failed:', err);

            if (err instanceof ApiClientError && err.details) {
                // Format validation errors into a single string
                const detailMessages = Object.values(err.details).flat().join(', ');
                setError(detailMessages || err.message);
            } else {
                setError(
                    err instanceof Error
                        ? err.message
                        : t.login.errors.generic
                );
            }
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className={styles.container}>
            <div className={styles.content}>
                {/* Branding */}
                <div className={styles.branding}>
                    <div className={styles.logo}>
                        <svg
                            width="48"
                            height="48"
                            viewBox="0 0 48 48"
                            fill="none"
                            xmlns="http://www.w3.org/2000/svg"
                        >
                            <rect width="48" height="48" rx="12" fill="url(#logo-gradient)" />
                            <path
                                d="M16 16h4v16h-4V16zm6 8h4v8h-4v-8zm6-4h4v12h-4V20z"
                                fill="white"
                                opacity="0.9"
                            />
                            <defs>
                                <linearGradient
                                    id="logo-gradient"
                                    x1="0"
                                    y1="0"
                                    x2="48"
                                    y2="48"
                                    gradientUnits="userSpaceOnUse"
                                >
                                    <stop stopColor="#6366f1" />
                                    <stop offset="1" stopColor="#4338ca" />
                                </linearGradient>
                            </defs>
                        </svg>
                    </div>
                    <h1 className={styles.title}>{t.login.title}</h1>
                    <p className={styles.subtitle}>{t.login.subtitle}</p>
                </div>

                {/* Login Card */}
                <Card className={styles.loginCard}>
                    <form onSubmit={handleSubmit} className={styles.form}>
                        <div className={styles.formHeader}>
                            <h2 className={styles.formTitle}>{t.login.welcome}</h2>
                            <p className={styles.formDescription}>
                                {t.login.instruction}
                            </p>
                        </div>

                        {error && (
                            <div className={styles.errorAlert} role="alert">
                                <svg
                                    width="16"
                                    height="16"
                                    viewBox="0 0 24 24"
                                    fill="none"
                                    stroke="currentColor"
                                    strokeWidth="2"
                                >
                                    <circle cx="12" cy="12" r="10" />
                                    <line x1="12" y1="8" x2="12" y2="12" />
                                    <line x1="12" y1="16" x2="12.01" y2="16" />
                                </svg>
                                <span>{error}</span>
                            </div>
                        )}

                        <div className={styles.formFields}>
                            <Input
                                label={t.login.username}
                                type="text"
                                value={username}
                                onChange={(e) => setUsername(e.target.value)}
                                placeholder={t.login.usernamePlaceholder}
                                autoComplete="username"
                                disabled={loading}
                                size="lg"
                            />

                            <Input
                                label={t.login.password}
                                type="password"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                placeholder={t.login.passwordPlaceholder}
                                autoComplete="current-password"
                                disabled={loading}
                                size="lg"
                            />
                        </div>

                        <Button
                            type="submit"
                            size="lg"
                            fullWidth
                            loading={loading}
                        >
                            {loading ? t.login.loading : t.login.signIn}
                        </Button>
                    </form>
                </Card>

                {/* Footer */}
                <p className={styles.footer}>
                    {t.login.copyright.replace('{year}', new Date().getFullYear().toString())}
                </p>
            </div>

            {/* Background Decoration */}
            <div className={styles.backgroundDecoration} aria-hidden="true">
                <div className={styles.gradientOrb1} />
                <div className={styles.gradientOrb2} />
            </div>
        </div>
    );
}
