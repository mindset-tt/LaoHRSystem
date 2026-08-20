'use client';

import { forwardRef, type ReactNode } from 'react';
import {
    FormProvider,
    Controller,
    useFormContext,
    useForm,
    type UseFormProps,
    type UseFormReturn,
    type FieldValues,
    type Path,
    type ControllerRenderProps,
    type FieldError,
} from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import type { ZodTypeAny } from 'zod';
import { Input, type InputProps } from './Input';
import { Select, type SelectProps } from './Select';
import styles from './Form.module.css';

export interface FormProps<T extends FieldValues>
    extends Omit<UseFormProps<T>, 'resolver'> {
    schema: ZodTypeAny;
    onSubmit: (values: T) => void | Promise<void>;
    children: (methods: UseFormReturn<T>) => ReactNode;
    className?: string;
}

/**
 * Form primitive — wires react-hook-form + Zod schema validation together.
 *
 * Usage:
 *   <Form schema={mySchema} onSubmit={handleSubmit} defaultValues={...}>
 *     {({ register, formState: { errors } }) => (
 *       <>
 *         <FormField name="title" label="Title" error={errors.title?.message} />
 *         <Button type="submit">Save</Button>
 *       </>
 *     )}
 *   </Form>
 */
export function Form<T extends FieldValues>({
    schema,
    onSubmit,
    children,
    className,
    ...formProps
}: FormProps<T>) {
    const methods = useForm<T>({
        ...formProps,
        resolver: zodResolver(schema as never),
        mode: 'onBlur',
    });

    return (
        <FormProvider {...methods}>
            <form
                className={`${styles.form} ${className ?? ''}`}
                onSubmit={methods.handleSubmit(async (values) => {
                    await onSubmit(values);
                })}
                noValidate
            >
                {children(methods as UseFormReturn<FieldValues> as UseFormReturn<T>)}
            </form>
        </FormProvider>
    );
}

interface FormFieldProps extends Omit<InputProps, 'name' | 'error'> {
    name: string;
    label?: string;
    helperText?: string;
    error?: string;
}

/**
 * Bridges react-hook-form's `register` to the `Input` primitive. Must be
 * used inside a `<Form>` provider.
 */
export const FormField = forwardRef<HTMLInputElement, FormFieldProps>(
    ({ name, label, helperText, error, ...inputProps }, ref) => {
        const { register } = useFormContext();
        const reg = register(name as Path<FieldValues>);
        return (
            <Input
                ref={(node) => {
                    reg.ref(node);
                    if (typeof ref === 'function') ref(node);
                    else if (ref) ref.current = node;
                }}
                label={label}
                helperText={helperText}
                error={error}
                name={reg.name}
                onChange={reg.onChange}
                onBlur={reg.onBlur}
                {...inputProps}
            />
        );
    }
);
FormField.displayName = 'FormField';

interface FormSelectFieldProps extends Omit<SelectProps, 'name' | 'value' | 'onChange' | 'error'> {
    name: string;
}

/**
 * Select variant of <FormField>. Renders via `Controller` from react-hook-form.
 * Label and helper text are passed straight through to the Select primitive.
 */
export function FormSelectField({ name, options, ...selectProps }: FormSelectFieldProps) {
    const { control } = useFormContext();
    return (
        <Controller
            control={control}
            name={name as Path<FieldValues>}
            render={({ field, fieldState }: { field: ControllerRenderProps<FieldValues, Path<FieldValues>>; fieldState: { error?: FieldError } }) => (
                <Select
                    {...selectProps}
                    value={String(field.value ?? '')}
                    onChange={(e) => field.onChange(e.target.value)}
                    onBlur={field.onBlur}
                    options={options}
                    error={fieldState.error?.message}
                />
            )}
        />
    );
}

export interface FormActionsProps {
    children: ReactNode;
    align?: 'left' | 'right' | 'between';
}

/**
 * Footer area for form action buttons.
 */
export function FormActions({ children, align = 'right' }: FormActionsProps) {
    return (
        <div className={`${styles.actions} ${styles[`align_${align}`]}`}>
            {children}
        </div>
    );
}

export interface FormSectionProps {
    title?: string;
    description?: string;
    children: ReactNode;
}

/**
 * Visual section within a form. Optional title + description.
 */
export function FormSection({ title, description, children }: FormSectionProps) {
    return (
        <section className={styles.section}>
            {(title || description) && (
                <header className={styles.sectionHeader}>
                    {title && <h3 className={styles.sectionTitle}>{title}</h3>}
                    {description && <p className={styles.sectionDescription}>{description}</p>}
                </header>
            )}
            <div className={styles.sectionBody}>{children}</div>
        </section>
    );
}

export interface FormGridProps {
    children: ReactNode;
    columns?: 1 | 2 | 3 | 4;
}

/**
 * Responsive grid for form fields. Collapses to a single column under 720px.
 */
export function FormGrid({ children, columns = 2 }: FormGridProps) {
    return (
        <div className={`${styles.grid} ${styles[`grid_${columns}`]}`}>
            {children}
        </div>
    );
}