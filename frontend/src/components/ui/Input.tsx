'use client';

import { forwardRef, type InputHTMLAttributes, type ReactNode, useId } from 'react';
import styles from './Input.module.css';

export interface InputProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'size'> {
    label?: string;
    helperText?: string;
    error?: string;
    leftIcon?: ReactNode;
    rightIcon?: ReactNode;
    size?: 'sm' | 'md' | 'lg';
}

/**
 * Premium Input Component
 * With label, helper text, error states, and icon support
 */
export const Input = forwardRef<HTMLInputElement, InputProps>(
    (
        {
            label,
            helperText,
            error,
            leftIcon,
            rightIcon,
            size = 'md',
            disabled,
            className = '',
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
            ${styles.inputWrapper}
            ${styles[size]}
            ${hasError ? styles.error : ''}
            ${disabled ? styles.disabled : ''}
          `.trim()}
                >
                    {leftIcon && <span className={styles.iconLeft}>{leftIcon}</span>}
                    <input
                        ref={ref}
                        id={id}
                        className={styles.input}
                        disabled={disabled}
                        aria-invalid={hasError}
                        aria-describedby={
                            error ? `${id}-error` : helperText ? `${id}-helper` : undefined
                        }
                        {...props}
                    />
                    {rightIcon && <span className={styles.iconRight}>{rightIcon}</span>}
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

Input.displayName = 'Input';
