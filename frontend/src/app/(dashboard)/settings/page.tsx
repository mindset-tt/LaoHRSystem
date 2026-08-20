'use client';

import { useAuth } from '@/components/providers/AuthProvider';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { useTheme } from '@/components/providers/ThemeProvider';
import { Button } from '@/components/ui/Button';
import Link from 'next/link';
import { isHROrAdmin } from '@/lib/permissions';
import styles from './page.module.css';

export default function SettingsPage() {
    const { user, role, logout } = useAuth();
    const { language, setLanguage } = useLanguage();
    const { mode, setMode } = useTheme();

    const labels = {
        settings: language === 'lo' ? 'ການຕັ້ງຄ່າ' : 'Settings',
        manageAccount: language === 'lo'
            ? 'ຈັ�ການບັນຊີ �ລະ ການຕັ້�ຄ່າຂອງທ່�ນ'
            : 'Manage your account and preferences',
        company: language === 'lo' ? 'ການຕັ້ງຄ່າບໍລິສັດ' : 'Company Settings',
        companyDesc: language === 'lo' ? 'ຈ�ດການວັນເຮັດວຽກ ແລະ ວັນພັກ' : 'Manage work schedule and holidays',
        workSchedule: language === 'lo' ? 'ຕາຕະລາງເຮັດວຽກ' : 'Work Schedule',
        workScheduleDesc: language === 'lo' ? 'ກຳນ�ດວັນເຮັດວຽກ ແລະ ເວລາ' : 'Configure working days and hours',
        holidays: language === 'lo' ? 'ວັນພັກ' : 'Holidays',
        holidaysDesc: language === 'lo' ? 'ຈັດການວັນພັກຂອງບໍລິສັດ' : 'Manage company holidays',
        leavePolicies: language === 'lo' ? 'ນະໂຍບາຍການລາ' : 'Leave Policies',
        leavePoliciesDesc: language === 'lo' ? 'ຕັ�ງຄ່າໂຄຕ້າ ແລະ ກົດລະບຽບ' : 'Configure quotas and rules',
        profile: language === 'lo' ? 'ໂປຣໄຟລ໌' : 'Profile',
        profileDesc: language === 'lo' ? 'ຂໍ້ມູນບັນຊີຂອງທ່ານ' : 'Your account information',
        displayName: language === 'lo' ? 'ຊື່ສະແດງ' : 'Display Name',
        username: language === 'lo' ? '�ື່ຜູ້ໃຊ້' : 'Username',
        role: language === 'lo' ? 'ບົດບາດ' : 'Role',
        signOut: language === 'lo' ? 'ອອກຈາກລະບົບ' : 'Sign Out',
        application: language === 'lo' ? '�ອັບພລິເຄຊັນ' : 'Application',
        applicationDesc: language === 'lo' ? 'ກ�ນຕັ້ງຄ່າ�ະບົບ' : 'System preferences',
        language: language === 'lo' ? 'ພາສາ' : 'Language',
        theme: language === 'lo' ? 'ຮູບແບບ' : 'Theme',
        themeSystem: language === 'lo' ? 'ຄ່າເລີ່ມຕົ້ນລະບົບ' : 'System Default',
        themeLight: language === 'lo' ? 'ສະຫວ່າງ' : 'Light',
        themeDark: language === 'lo' ? 'ມືດ' : 'Dark',
    };

    return (
        <div className={styles.container}>
            <div className={styles.header}>
                <h1 className={styles.title}>{labels.settings}</h1>
                <p className={styles.subtitle}>{labels.manageAccount}</p>
            </div>

            {/* HR/Admin Settings */}
            {isHROrAdmin(role) && (
                <div className={styles.section}>
                    <div className={styles.sectionHeader}>
                        <h2 className={styles.sectionTitle}>{labels.company}</h2>
                        <p className={styles.sectionDescription}>{labels.companyDesc}</p>
                    </div>

                    <div className={styles.settingsGrid}>
                        <Link href="/settings/work-schedule" className={styles.settingsCard}>
                            <div className={styles.settingsIcon}>📅</div>
                            <div className={styles.settingsInfo}>
                                <h3>{labels.workSchedule}</h3>
                                <p>{labels.workScheduleDesc}</p>
                            </div>
                        </Link>

                        <Link href="/settings/holidays" className={styles.settingsCard}>
                            <div className={styles.settingsIcon}>🎉</div>
                            <div className={styles.settingsInfo}>
                                <h3>{labels.holidays}</h3>
                                <p>{labels.holidaysDesc}</p>
                            </div>
                        </Link>

                        <Link href="/settings/leave" className={styles.settingsCard}>
                            <div className={styles.settingsIcon}>🌴</div>
                            <div className={styles.settingsInfo}>
                                <h3>{labels.leavePolicies}</h3>
                                <p>{labels.leavePoliciesDesc}</p>
                            </div>
                        </Link>
                    </div>
                </div>
            )}

            {/* Profile Section */}
            <div className={styles.section}>
                <div className={styles.sectionHeader}>
                    <h2 className={styles.sectionTitle}>{labels.profile}</h2>
                    <p className={styles.sectionDescription}>{labels.profileDesc}</p>
                </div>

                <div className={styles.field}>
                    <label className={styles.label}>{labels.displayName}</label>
                    <div className={styles.value}>{user?.displayName}</div>
                </div>

                <div className={styles.field}>
                    <label className={styles.label}>{labels.username}</label>
                    <div className={styles.value}>{user?.username}</div>
                </div>

                <div className={styles.field}>
                    <label className={styles.label}>{labels.role}</label>
                    <div className={styles.value}>{user?.role}</div>
                </div>

                <div className={styles.actions}>
                    <Button variant="secondary" onClick={logout}>{labels.signOut}</Button>
                </div>
            </div>

            {/* Application Settings */}
            <div className={styles.section}>
                <div className={styles.sectionHeader}>
                    <h2 className={styles.sectionTitle}>{labels.application}</h2>
                    <p className={styles.sectionDescription}>{labels.applicationDesc}</p>
                </div>

                <div className={styles.field}>
                    <label className={styles.label}>{labels.language}</label>
                    <select
                        className={styles.input}
                        value={language}
                        onChange={(e) => setLanguage(e.target.value === 'lo' ? 'lo' : 'en')}
                    >
                        <option value="en">English</option>
                        <option value="lo">Lao</option>
                    </select>
                </div>

                <div className={styles.field}>
                    <label className={styles.label}>{labels.theme}</label>
                    <select
                        className={styles.input}
                        value={mode}
                        onChange={(e) => setMode((e.target.value as 'light' | 'dark' | 'system') ?? 'system')}
                    >
                        <option value="system">{labels.themeSystem}</option>
                        <option value="light">{labels.themeLight}</option>
                        <option value="dark">{labels.themeDark}</option>
                    </select>
                </div>
            </div>
        </div>
    );
}
