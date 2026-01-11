"use client";

import { useTransition } from "react";
import { Globe, Check } from "lucide-react";
import { useLocale } from "next-intl";
import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { cn } from "@/lib/utils";

const languages = [
    { code: 'en', name: 'English' },
    { code: 'lo', name: 'ລາວ' },
    { code: 'vi', name: 'Tiếng Việt' },
    { code: 'zh', name: '中文' },
    { code: 'ja', name: '日本語' },
    { code: 'fr', name: 'Français' },
    { code: 'ko', name: '한국어' },
];

export default function LanguageSwitcher() {
    const [isPending, startTransition] = useTransition();
    const currentLocale = useLocale();

    const changeLanguage = (locale: string) => {
        startTransition(() => {
            document.cookie = `locale=${locale};path=/;max-age=31536000`;
            window.location.reload();
        });
    };

    const currentLang = languages.find(l => l.code === currentLocale) || languages[0];

    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                <Button variant="ghost" size="sm" className="w-9 px-0 border border-slate-200 dark:border-slate-800">
                    <Globe className="h-4 w-4" />
                    <span className="sr-only">Toggle language</span>
                </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
                {languages.map((lang) => (
                    <DropdownMenuItem
                        key={lang.code}
                        onClick={() => changeLanguage(lang.code)}
                        className="cursor-pointer"
                    >
                        <span className="flex-1">{lang.name}</span>
                        {currentLocale === lang.code && (
                            <Check className="ml-2 h-4 w-4" />
                        )}
                    </DropdownMenuItem>
                ))}
            </DropdownMenuContent>
        </DropdownMenu>
    );
}
