import { getRequestConfig } from 'next-intl/server';
import { cookies } from 'next/headers';

export const locales = ['en', 'lo', 'vi', 'zh', 'ja', 'fr', 'ko'] as const;
export type Locale = (typeof locales)[number];

export const localeNames: Record<Locale, string> = {
    en: 'English',
    lo: 'ລາວ',
    vi: 'Tiếng Việt',
    zh: '中文',
    ja: '日本語',
    fr: 'Français',
    ko: '한국어'
};

export default getRequestConfig(async () => {
    const cookieStore = await cookies();
    const locale = (cookieStore.get('locale')?.value as Locale) || 'en';

    return {
        locale,
        messages: (await import(`../messages/${locale}.json`)).default
    };
});
