"use client";

import { useState } from "react";
import Sidebar from "@/components/Sidebar";
import LanguageSwitcher from "@/components/LanguageSwitcher";
import { ThemeToggle } from "@/components/ThemeToggle";
import { UserNav } from "@/components/UserNav";
import { Input } from "@/components/ui/input";
import { Search, Menu, Command } from "lucide-react";
import { Button } from "@/components/ui/button";
import { usePathname } from "next/navigation";

interface DashboardShellProps {
    children: React.ReactNode;
}

export default function DashboardShell({ children }: DashboardShellProps) {
    const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
    const pathname = usePathname();

    const publicPaths = ['/login', '/register', '/license-activated', '/license-expired'];
    const isPublic = publicPaths.some(path => pathname?.startsWith(path));

    if (isPublic) {
        return <>{children}</>;
    }

    return (
        <div className="min-h-screen bg-background">
            {/* Desktop Fixed Sidebar */}
            <div className="hidden md:block">
                <Sidebar />
            </div>

            {/* Main Layout Area */}
            <div className="flex flex-col min-h-screen md:pl-64">
                {/* Header */}
                <header className="sticky top-0 z-40 flex h-14 items-center justify-between gap-4 px-4 md:px-6 border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
                    <div className="flex items-center gap-4">
                        <Button
                            variant="ghost"
                            size="icon"
                            className="md:hidden -ml-2"
                            onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
                        >
                            <Menu className="h-5 w-5" />
                        </Button>

                        {/* Global Search */}
                        <div className="relative hidden sm:block">
                            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                            <Input
                                type="search"
                                placeholder="Search..."
                                className="h-9 w-64 pl-9 pr-12"
                            />
                            <kbd className="absolute right-3 top-1/2 -translate-y-1/2 pointer-events-none hidden sm:inline-flex h-5 select-none items-center gap-1 rounded border bg-muted px-1.5 font-mono text-[10px] font-medium text-muted-foreground">
                                <Command className="h-3 w-3" />K
                            </kbd>
                        </div>
                    </div>

                    <div className="flex items-center gap-2">
                        <LanguageSwitcher />
                        <ThemeToggle />
                        <div className="hidden sm:block h-4 w-px bg-border mx-1" />
                        <UserNav />
                    </div>
                </header>

                {/* Page Content */}
                <main className="flex-1 p-4 md:p-6 lg:p-8">
                    {children}
                </main>
            </div>

            {/* Mobile Sidebar Overlay */}
            {isMobileMenuOpen && (
                <div className="fixed inset-0 z-50 md:hidden">
                    <div
                        className="fixed inset-0 bg-background/80 backdrop-blur-sm"
                        onClick={() => setIsMobileMenuOpen(false)}
                    />
                    <div className="fixed inset-y-0 left-0 w-64 animate-in slide-in-from-left duration-300">
                        <Sidebar />
                    </div>
                </div>
            )}
        </div>
    );
}
