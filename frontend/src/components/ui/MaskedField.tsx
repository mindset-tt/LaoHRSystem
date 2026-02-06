'use client';

import { useState, useCallback } from 'react';
import styles from './MaskedField.module.css';

export interface MaskedFieldProps {
    /** The sensitive value to mask */
    value: string;
    /** Number of characters to show at the end */
    visibleChars?: number;
    /** Mask character */
    maskChar?: string;
    /** Label for the field */
    label?: string;
    /** Callback when value is revealed (for audit logging) */
    onReveal?: () => void;
    /** Time in seconds before auto-masking */
    autoMaskSeconds?: number;
}

/**
 * MaskedField Component
 * For displaying sensitive data (salary, bank account, tax ID) with masking
 * Reveal timer is UI-only; backend authorization is required on every request
 */
export function MaskedField({
    value,
    visibleChars = 4,
    maskChar = '•',
    label,
    onReveal,
    autoMaskSeconds = 30,
}: MaskedFieldProps) {
    const [isRevealed, setIsRevealed] = useState(false);

    const maskedValue = useCallback(() => {
        if (!value) return '';
        if (value.length <= visibleChars) {
            return maskChar.repeat(value.length);
        }
        const hiddenPart = maskChar.repeat(value.length - visibleChars);
        const visiblePart = value.slice(-visibleChars);
        return hiddenPart + visiblePart;
    }, [value, visibleChars, maskChar]);

    const handleReveal = () => {
        setIsRevealed(true);
        onReveal?.();

        // Auto-mask after specified seconds
        setTimeout(() => {
            setIsRevealed(false);
        }, autoMaskSeconds * 1000);
    };

    const handleHide = () => {
        setIsRevealed(false);
    };

    return (
        <div className={styles.wrapper}>
            {label && <span className={styles.label}>{label}</span>}
            <div className={styles.fieldWrapper}>
                <span className={styles.value}>
                    {isRevealed ? value : maskedValue()}
                </span>
                <button
                    type="button"
                    className={styles.toggleButton}
                    onClick={isRevealed ? handleHide : handleReveal}
                    aria-label={isRevealed ? 'Hide value' : 'Show value'}
                >
                    {isRevealed ? (
                        <EyeOffIcon />
                    ) : (
                        <EyeIcon />
                    )}
                </button>
            </div>
        </div>
    );
}

function EyeIcon() {
    return (
        <svg
            width="16"
            height="16"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
        >
            <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" />
            <circle cx="12" cy="12" r="3" />
        </svg>
    );
}

function EyeOffIcon() {
    return (
        <svg
            width="16"
            height="16"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
        >
            <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24" />
            <line x1="1" y1="1" x2="23" y2="23" />
        </svg>
    );
}
