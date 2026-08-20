import type { Metadata } from "next";
import { AuthProvider } from "@/components/providers/AuthProvider";
import { LanguageProvider } from "@/components/providers/LanguageProvider";
import { ThemeProvider } from "@/components/providers/ThemeProvider";
import { ToastProvider } from "@/components/ui/Toast";
import "./globals.css";

export const metadata: Metadata = {
  title: {
    default: "LaoHR - Human Resource Management System",
    template: "%s | LaoHR",
  },
  description: "Enterprise-grade HR management platform for Lao businesses. Manage employees, attendance, payroll, and leave with ease.",
  keywords: ["HR", "Human Resources", "Payroll", "Attendance", "Leave Management", "Laos", "ລາວ"],
  authors: [{ name: "LaoHR Team" }],
  robots: {
    index: false,
    follow: false,
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body>
        <ThemeProvider>
          <LanguageProvider>
            <ToastProvider>
              <AuthProvider>
                {children}
              </AuthProvider>
            </ToastProvider>
          </LanguageProvider>
        </ThemeProvider>
      </body>
    </html>
  );
}