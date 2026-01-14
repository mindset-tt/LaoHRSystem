'use client';

import { forwardRef, type SelectHTMLAttributes, useId } from 'react';
import styles from './Select.module.css';

export interface SelectProps extends Omit<SelectHTMLAttributes<HTMLSelectElement>, 'size'> {
    label?: string;
    helperText?: string;
    error?: string;
    options: { value: string | number; label: string }[];
    size?: 'sm' | 'md' | 'lg';
    placeholder?: string;
}

/**
 * Premium Select Component
 */
export const Select = forwardRef<HTMLSelectElement, SelectProps>(
    (
        {
            label,
            helperText,
            error,
            options,
            size = 'md',
            disabled,
            className = '',
            placeholder = 'Select an option',
            id: providedId,
            ...props
        },
        ref
    ) => {
        const generatedId = useId();
        const id = providedId || generatedId;
        const hasError = Boolean(error);

        return (
            <div className={`${styles.wrapper} ${className}`}>
                {label && (
                    <label htmlFor={id} className={styles.label}>
                        {label}
                    </label>
                )}
                <div
                    className={`
            ${styles.selectWrapper}
            ${styles[size]}
            ${hasError ? styles.error : ''}
            ${disabled ? styles.disabled : ''}
          `.trim()}
                >
                    <select
                        ref={ref}
                        id={id}
                        className={styles.select}
                        disabled={disabled}
                        aria-invalid={hasError}
                        aria-describedby={
                            error ? `${id}-error` : helperText ? `${id}-helper` : undefined
                        }
                        {...props}
                    >
                        <option value="" disabled>
                            {placeholder}
                        </option>
                        {options.map((opt) => (
                            <option key={opt.value} value={opt.value}>
                                {opt.label}
                            </option>
                        ))}
                    </select>
                    <div className={styles.arrowIcon}>
                        <svg
                            xmlns="http://www.w3.org/2000/svg"
                            width="16"
                            height="16"
                            viewBox="0 0 24 24"
                            fill="none"
                            stroke="currentColor"
                            strokeWidth="2"
                            strokeLinecap="round"
                            strokeLinejoin="round"
                        >
                            <path d="m6 9 6 6 6-6" />
                        </svg>
                    </div>
                </div>
                {error && (
                    <p id={`${id}-error`} className={styles.errorText} role="alert">
                        {error}
                    </p>
                )}
                {!error && helperText && (
                    <p id={`${id}-helper`} className={styles.helperText}>
                        {helperText}
                    </p>
                )}
            </div>
        );
    }
);

Select.displayName = 'Select';
