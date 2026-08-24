'use client';

import { useState } from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { useAuth } from '@/components/providers/AuthProvider';
import styles from './Sidebar.module.css';

interface NavItem {
    label: string;
    href: string;
    icon: React.ReactNode;
    permission?: string;
    subItems?: { label: string; href: string }[];
}

export function Sidebar() {
    const [collapsed, setCollapsed] = useState(false);
    const [expandedMenu, setExpandedMenu] = useState<string | null>(null);
    const pathname = usePathname();
    const { user, logout, can } = useAuth();
    const { t, language } = useLanguage();

    const navItems: NavItem[] = [
        {
            label: t.sidebar.dashboard,
            href: '/',
            icon: <DashboardIcon />,
        },
        {
            label: t.sidebar.backOffice,
            href: '/backoffice',
            icon: <DashboardIcon />,
        },
        {
            label: t.sidebar.employees,
            href: '/employees',
            icon: <UsersIcon />,
        },
        {
            label: t.sidebar.recruitment,
            href: '/recruitment',
            icon: <UsersIcon />,
        },
        {
            label: t.sidebar.performance,
            href: '/performance',
            icon: <ChartIcon />,
        },
        {
            label: t.sidebar.attendance,
            href: '/attendance',
            icon: <ClockIcon />,
        },
        {
            label: t.sidebar.leave,
            href: '/leave',
            icon: <CalendarIcon />,
        },
        {
            label: t.sidebar.payroll,
            href: '/payroll',
            icon: <WalletIcon />,
            permission: 'payroll.view',
        },
        {
            label: t.sidebar.projects,
            href: '/projects',
            icon: <ProjectsIcon />,
        },
        {
            label: t.sidebar.portfolio,
            href: '/portfolio',
            icon: <ChartIcon />,
        },
        {
            label: t.sidebar.capacity,
            href: '/capacity',
            icon: <UsersIcon />,
        },
        {
            label: t.sidebar.myTasks,
            href: '/my-tasks',
            icon: <TasksIcon />,
        },
        {
            label: t.sidebar.myProfile,
            href: '/my-profile',
            icon: <UserIcon />,
        },
        {
            label: t.sidebar.myApprovals,
            href: '/my-approvals',
            icon: <CheckIcon />,
        },
        {
            label: t.sidebar.notifications,
            href: '/notifications',
            icon: <BellIcon />,
        },
        {
            label: t.sidebar.finance,
            href: '/finance/expenses',
            icon: <WalletIcon />,
            subItems: [
                { label: t.sidebar.financeDashboard, href: '/finance/dashboard' },
                { label: t.finance.expenses.title, href: '/finance/expenses' },
                { label: t.finance.loans.title, href: '/finance/loans' },
                { label: t.sidebar.supplierInvoices, href: '/finance/invoices' },
                { label: t.sidebar.payments, href: '/finance/payments' },
                { label: t.sidebar.chartOfAccounts, href: '/finance/accounts' },
                { label: t.sidebar.journals, href: '/finance/journals' },
                { label: t.sidebar.financeSettings, href: '/finance/settings' },
            ],
        },
        {
            label: t.sidebar.procurement,
            href: '/procurement',
            icon: <CartIcon />,
            permission: 'procurement.view',
            subItems: [
                { label: t.sidebar.purchaseRequests, href: '/procurement/requests' },
                { label: t.sidebar.purchaseOrders, href: '/procurement/orders' },
                { label: t.sidebar.goodsReceipts, href: '/procurement/receipts' },
                { label: t.sidebar.suppliers, href: '/procurement/suppliers' },
            ],
        },
        {
            label: t.sidebar.inventory,
            href: '/inventory',
            icon: <BoxIcon />,
            permission: 'inventory.view',
            subItems: [
                { label: t.sidebar.items, href: '/inventory/items' },
                { label: t.sidebar.warehouses, href: '/inventory/warehouses' },
                { label: t.sidebar.stock, href: '/inventory/stock' },
            ],
        },
        {
            label: t.sidebar.assets,
            href: '/assets',
            icon: <BoxIcon />,
            permission: 'assets.view',
        },
        {
            label: t.sidebar.contracts,
            href: '/contracts',
            icon: <FileIcon />,
            permission: 'contracts.view',
        },
        {
            label: t.sidebar.serviceRequests,
            href: '/service-requests',
            icon: <TicketIcon />,
        },
        {
            label: t.sidebar.budgets,
            href: '/budgets',
            icon: <WalletIcon />,
            permission: 'finance.view',
        },
        {
            label: t.sidebar.corporate,
            href: '/corporate',
            icon: <BuildingIcon />,
            permission: 'corporate.view',
            subItems: [
                { label: t.sidebar.corporateDashboard, href: '/corporate' },
                { label: t.sidebar.documents, href: '/corporate/documents' },
                { label: t.sidebar.contracts, href: '/corporate/contracts' },
                { label: t.sidebar.serviceRequests, href: '/corporate/service-desk' },
                { label: t.sidebar.facilities, href: '/corporate/facilities' },
                { label: t.sidebar.rooms, href: '/corporate/facilities/rooms' },
                { label: t.sidebar.maintenance, href: '/corporate/maintenance' },
                { label: t.sidebar.fleet, href: '/corporate/fleet' },
                { label: t.sidebar.travel, href: '/corporate/travel' },
                { label: t.sidebar.visitors, href: '/corporate/visitors' },
            ],
        },
        {
            label: t.sidebar.knowledge,
            href: '/knowledge',
            icon: <BooksIcon />,
            subItems: [
                { label: t.knowledge.articles.title, href: '/knowledge' },
                { label: t.knowledge.announcements.title, href: '/knowledge/announcements' },
            ],
        },
        {
            label: t.sidebar.reports,
            href: '/reports',
            icon: <ChartIcon />,
            permission: 'payroll.view',
        },
        {
            label: t.sidebar.analytics,
            href: '/analytics/executive',
            icon: <ChartIcon />,
            permission: 'payroll.view',
            subItems: [
                { label: t.sidebar.executive, href: '/analytics/executive' },
                { label: t.sidebar.hr, href: '/analytics/hr' },
                { label: t.sidebar.myTeam, href: '/analytics/my-team' },
            ],
        },
        {
            label: t.sidebar.settings,
            href: '/settings',
            icon: <SettingsIcon />,
            permission: 'settings.edit',
            subItems: [
                {
                    label: language === 'lo' ? 'ຂໍ້ມູນບໍລິສັດ' : 'Company Info',
                    href: '/settings/company'
                },
                {
                    label: language === 'lo' ? 'ຕາຕະລາງເຮັດວຽກ' : 'Work Schedule',
                    href: '/settings/work-schedule'
                },
                {
                    label: language === 'lo' ? 'ວັນພັກ' : 'Holidays',
                    href: '/settings/holidays'
                },
                {
                    label: language === 'lo' ? 'ນະໂຍບາຍລາພັກ' : 'Leave Policies',
                    href: '/settings/leave'
                },
                {
                    label: language === 'lo' ? 'ອັດຕາແລກປ່ຽນ' : 'Currency Rates',
                    href: '/settings/currency-rates'
                },
            ],
        },
    ];

    const filteredNavItems = navItems.filter((item) => {
        if (!item.permission) return true;
        return can(item.permission as Parameters<typeof can>[0]);
    });

    const toggleSubmenu = (href: string) => {
        setExpandedMenu(expandedMenu === href ? null : href);
    };

    const isSettingsActive = pathname.startsWith('/settings');

    return (
        <aside className={`${styles.sidebar} ${collapsed ? styles.collapsed : ''}`}>
            {/* Logo */}
            <div className={styles.logoSection}>
                <Link href="/" className={styles.logo}>
                    <div className={styles.logoIcon}>
                        <svg width="32" height="32" viewBox="0 0 48 48" fill="none">
                            <rect width="48" height="48" rx="12" fill="url(#sidebar-logo)" />
                            <path
                                d="M16 16h4v16h-4V16zm6 8h4v8h-4v-8zm6-4h4v12h-4V20z"
                                fill="white"
                                opacity="0.9"
                            />
                            <defs>
                                <linearGradient id="sidebar-logo" x1="0" y1="0" x2="48" y2="48">
                                    <stop stopColor="#6366f1" />
                                    <stop offset="1" stopColor="#4338ca" />
                                </linearGradient>
                            </defs>
                        </svg>
                    </div>
                    {!collapsed && <span className={styles.logoText}>LaoHR</span>}
                </Link>
                <button
                    className={styles.collapseButton}
                    onClick={() => setCollapsed(!collapsed)}
                    aria-label={collapsed ? t.sidebar.expand : t.sidebar.collapse}
                >
                    <ChevronIcon direction={collapsed ? 'right' : 'left'} />
                </button>
            </div>

            {/* Navigation */}
            <nav className={styles.nav}>
                <ul className={styles.navList}>
                    {filteredNavItems.map((item) => {
                        const isActive = pathname === item.href ||
                            (item.href !== '/' && pathname.startsWith(item.href));
                        const hasSubItems = item.subItems && item.subItems.length > 0;
                        const isExpanded = expandedMenu === item.href || (hasSubItems && isActive);

                        if (hasSubItems) {
                            return (
                                <li key={item.href} className={styles.navItemWithSub}>
                                    <button
                                        className={`${styles.navItem} ${isActive ? styles.active : ''}`}
                                        onClick={() => toggleSubmenu(item.href)}
                                        title={collapsed ? item.label : undefined}
                                    >
                                        <span className={styles.navIcon}>{item.icon}</span>
                                        {!collapsed && (
                                            <>
                                                <span className={styles.navLabel}>{item.label}</span>
                                                <span className={`${styles.submenuArrow} ${isExpanded ? styles.expanded : ''}`}>
                                                    <ChevronDownIcon />
                                                </span>
                                            </>
                                        )}
                                    </button>

                                    {!collapsed && isExpanded && (
                                        <ul className={styles.submenu}>
                                            {item.subItems?.map((subItem) => {
                                                const subActive = pathname === subItem.href;
                                                return (
                                                    <li key={subItem.href}>
                                                        <Link
                                                            href={subItem.href}
                                                            className={`${styles.submenuItem} ${subActive ? styles.active : ''}`}
                                                        >
                                                            {subItem.label}
                                                        </Link>
                                                    </li>
                                                );
                                            })}
                                        </ul>
                                    )}
                                </li>
                            );
                        }

                        return (
                            <li key={item.href}>
                                <Link
                                    href={item.href}
                                    className={`${styles.navItem} ${isActive ? styles.active : ''}`}
                                    title={collapsed ? item.label : undefined}
                                >
                                    <span className={styles.navIcon}>{item.icon}</span>
                                    {!collapsed && <span className={styles.navLabel}>{item.label}</span>}
                                </Link>
                            </li>
                        );
                    })}
                </ul>
            </nav>

            {/* User Section */}
            <div className={styles.userSection}>
                <div className={styles.userInfo}>
                    <div className={styles.userAvatar}>
                        {user?.displayName?.charAt(0) || 'U'}
                    </div>
                    {!collapsed && (
                        <div className={styles.userDetails}>
                            <span className={styles.userName}>{user?.displayName}</span>
                            <span className={styles.userRole}>{user?.role}</span>
                        </div>
                    )}
                </div>
                <button
                    className={styles.logoutButton}
                    onClick={logout}
                    title={t.common.logout}
                >
                    <LogoutIcon />
                </button>
            </div>
        </aside>
    );
}

// Icons
function DashboardIcon() {
    return (
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <rect x="3" y="3" width="7" height="7" />
            <rect x="14" y="3" width="7" height="7" />
            <rect x="14" y="14" width="7" height="7" />
            <rect x="3" y="14" width="7" height="7" />
        </svg>
    );
}

function UsersIcon() {
    return (
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
            <circle cx="9" cy="7" r="4" />
            <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
            <path d="M16 3.13a4 4 0 0 1 0 7.75" />
        </svg>
    );
}

function UserIcon() {
    return (
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
            <circle cx="12" cy="7" r="4" />
        </svg>
    );
}

function CheckIcon() {
    return (
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <polyline points="20 6 9 17 4 12" />
        </svg>
    );
}

function BellIcon() {
    return (
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9" />
            <path d="M13.73 21a2 2 0 0 1-3.46 0" />
        </svg>
    );
}

function ClockIcon() {
    return (
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <circle cx="12" cy="12" r="10" />
            <polyline points="12 6 12 12 16 14" />
        </svg>
    );
}

function CalendarIcon() {
    return (
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
            <line x1="16" y1="2" x2="16" y2="6" />
            <line x1="8" y1="2" x2="8" y2="6" />
            <line x1="3" y1="10" x2="21" y2="10" />
        </svg>
    );
}

function WalletIcon() {
    return (
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <path d="M21 12V7H5a2 2 0 0 1 0-4h14v4" />
            <path d="M3 5v14a2 2 0 0 0 2 2h16v-5" />
            <path d="M18 12a2 2 0 0 0 0 4h4v-4h-4z" />
        </svg>
    );
}

function ChartIcon() {
    return (
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <line x1="18" y1="20" x2="18" y2="10" />
            <line x1="12" y1="20" x2="12" y2="4" />
            <line x1="6" y1="20" x2="6" y2="14" />
        </svg>
    );
}

function SettingsIcon() {
    return (
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <circle cx="12" cy="12" r="3" />
            <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1 0 2.83 2 2 0 0 1-2.83 0l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-2 2 2 2 0 0 1-2-2v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83 0 2 2 0 0 1 0-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1-2-2 2 2 0 0 1 2-2h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 0-2.83 2 2 0 0 1 2.83 0l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 2-2 2 2 0 0 1 2 2v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 0 2 2 0 0 1 0 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 2 2 2 2 0 0 1-2 2h-.09a1.65 1.65 0 0 0-1.51 1z" />
        </svg>
    );
}

function ChevronIcon({ direction }: { direction: 'left' | 'right' }) {
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
            style={{ transform: direction === 'right' ? 'rotate(180deg)' : undefined }}
        >
            <polyline points="15 18 9 12 15 6" />
        </svg>
    );
}

function ChevronDownIcon() {
    return (
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <polyline points="6 9 12 15 18 9" />
        </svg>
    );
}

function ProjectsIcon() {
    return (
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <rect x="3" y="3" width="7" height="7" rx="1" />
            <rect x="14" y="3" width="7" height="7" rx="1" />
            <rect x="3" y="14" width="7" height="7" rx="1" />
            <rect x="14" y="14" width="7" height="7" rx="1" />
        </svg>
    );
}

function TasksIcon() {
    return (
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <polyline points="9 11 12 14 22 4" />
            <path d="M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11" />
        </svg>
    );
}

function BooksIcon() {
    return (
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20" />
            <path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z" />
        </svg>
    );
}

function CartIcon() {
    return (
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <circle cx="9" cy="21" r="1" />
            <circle cx="20" cy="21" r="1" />
            <path d="M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6" />
        </svg>
    );
}

function BoxIcon() {
    return (
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z" />
            <polyline points="3.27 6.96 12 12.01 20.73 6.96" />
            <line x1="12" y1="22.08" x2="12" y2="12" />
        </svg>
    );
}

function FileIcon() {
    return (
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
            <polyline points="14 2 14 8 20 8" />
        </svg>
    );
}

function TicketIcon() {
    return (
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <path d="M2 9a3 3 0 0 1 0 6v2a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2v-2a3 3 0 0 1 0-6V7a2 2 0 0 0-2-2H4a2 2 0 0 0-2 2z" />
            <path d="M13 5v2" />
            <path d="M13 17v2" />
            <path d="M13 11v2" />
        </svg>
    );
}

function BuildingIcon() {
    return (
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <rect x="4" y="2" width="16" height="20" rx="2" />
            <path d="M9 22v-4h6v4" />
            <path d="M8 6h.01M16 6h.01M12 6h.01M8 10h.01M16 10h.01M12 10h.01M8 14h.01M16 14h.01M12 14h.01" />
        </svg>
    );
}

function LogoutIcon() {
    return (
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" />
            <polyline points="16 17 21 12 16 7" />
            <line x1="21" y1="12" x2="9" y2="12" />
        </svg>
    );
}
