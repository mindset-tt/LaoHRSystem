import type { Metadata } from "next";
import { Inter, Noto_Sans_Lao, Noto_Sans_JP, Noto_Sans_KR, Noto_Sans_SC } from "next/font/google";
import { NextIntlClientProvider } from 'next-intl';
import { getLocale, getMessages } from 'next-intl/server';
import { ThemeProvider } from "@/components/theme-provider";
import DashboardShell from "@/components/DashboardShell";
import "./globals.css";
import { cn } from "@/lib/utils";

// Inter for Latin scripts (English, French, Vietnamese)
const inter = Inter({
  subsets: ["latin", "latin-ext", "vietnamese"],
  variable: "--font-inter",
  display: "swap",
});

// Noto Sans Lao for Lao script
const notoLao = Noto_Sans_Lao({
  subsets: ["lao"],
  variable: "--font-lao",
  weight: ["400", "500", "600", "700"],
  display: "swap",
});

// Noto Sans JP for Japanese
const notoJP = Noto_Sans_JP({
  subsets: ["latin"],
  variable: "--font-jp",
  weight: ["400", "500", "600", "700"],
  display: "swap",
});

// Noto Sans KR for Korean
const notoKR = Noto_Sans_KR({
  subsets: ["latin"],
  variable: "--font-kr",
  weight: ["400", "500", "600", "700"],
  display: "swap",
});

// Noto Sans SC for Simplified Chinese
const notoSC = Noto_Sans_SC({
  subsets: ["latin"],
  variable: "--font-sc",
  weight: ["400", "500", "600", "700"],
  display: "swap",
});

export const metadata: Metadata = {
  title: "Lao HR System",
  description: "Professional HR Management System - Multi-language support",
};

export default async function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const locale = await getLocale();
  const messages = await getMessages();

  // Select font class based on locale
  const getFontClass = () => {
    switch (locale) {
      case 'lo':
        return `${notoLao.variable} font-lao`;
      case 'ja':
        return `${notoJP.variable} font-jp`;
      case 'ko':
        return `${notoKR.variable} font-kr`;
      case 'zh':
        return `${notoSC.variable} font-sc`;
      default:
        return `${inter.variable} font-inter`;
    }
  };

  const fontVariables = `${inter.variable} ${notoLao.variable} ${notoJP.variable} ${notoKR.variable} ${notoSC.variable}`;

  return (
    <html lang={locale} suppressHydrationWarning>
      <body className={cn("min-h-screen bg-background font-sans antialiased", fontVariables, getFontClass())} suppressHydrationWarning>
        <NextIntlClientProvider messages={messages}>
          <ThemeProvider
            attribute="class"
            defaultTheme="system"
            enableSystem
            disableTransitionOnChange
          >
            <DashboardShell>
              {children}
            </DashboardShell>
          </ThemeProvider>
        </NextIntlClientProvider>
      </body>
    </html>
  );
}
