import { ReactNode } from 'react';
import { render } from '@testing-library/react';
import { LanguageProvider } from '@/components/providers/LanguageProvider';
import { ToastProvider } from '@/components/ui/Toast';

/**
 * Render a component wrapped in the providers it depends on.
 */
export function renderWithProviders(ui: ReactNode) {
    return render(
        <LanguageProvider>
            <ToastProvider>{ui}</ToastProvider>
        </LanguageProvider>,
    );
}
