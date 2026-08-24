'use client';

import Link from 'next/link';

export default function ForbiddenPage() {

    return (
        <div style={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            minHeight: '60vh',
            textAlign: 'center',
            padding: '24px',
        }}>
            <h1 style={{ fontSize: '48px', margin: '0 0 8px', color: 'var(--text-primary)' }}>
                403
            </h1>
            <p style={{ fontSize: '18px', color: 'var(--text-secondary)', margin: '0 0 24px' }}>
                You do not have permission to access this page.
            </p>
            <Link
                href="/"
                style={{
                    padding: '10px 24px',
                    background: 'var(--color-primary-500)',
                    color: 'white',
                    borderRadius: '8px',
                    textDecoration: 'none',
                    fontSize: '14px',
                    fontWeight: 500,
                }}
            >
                Back to Dashboard
            </Link>
        </div>
    );
}