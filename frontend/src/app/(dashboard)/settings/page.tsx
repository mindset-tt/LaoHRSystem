'use client';

import { useAuth } from '@/components/providers/AuthProvider';
import { useLanguage } from '@/components/providers/LanguageProvider';
import { Button } from '@/components/ui/Button';
import Link from 'next/link';
import { isHROrAdmin } from '@/lib/permissions';
import styles from './page.module.css';

export default function SettingsPage() {
    const { user, role, logout } = useAuth();
    const { language } = useLanguage();

    return (
        <div className={styles.container}>
            <div className={styles.header}>
                <h1 className={styles.title}>{language === 'lo' ? 'ການຕັ້ງຄ່າ' : 'Settings'}</h1>
                <p className={styles.subtitle}>
                    {language === 'lo' ? 'ຈັດການບັນຊີ ແລະ ການຕັ້ງຄ່າຂອງທ່ານ' : 'Manage your account and preferences'}
                </p>
            </div>

            {/* HR/Admin Settings */}
            {isHROrAdmin(role) && (
                <div className={styles.section}>
                    <div className={styles.sectionHeader}>
                        <h2 className={styles.sectionTitle}>
                            {language === 'lo' ? 'ການຕັ້ງຄ່າບໍລິສັດ' : 'Company Settings'}
                        </h2>
                        <p className={styles.sectionDescription}>
                            {language === 'lo' ? 'ຈັດການວັນເຮັດວຽກ ແລະ ວັນພັກ' : 'Manage work schedule and holidays'}
                        </p>
                    </div>

                    <div className={styles.settingsGrid}>
                        <Link href="/settings/work-schedule" className={styles.settingsCard}>
                            <div className={styles.settingsIcon}>📅</div>
                            <div className={styles.settingsInfo}>
                                <h3>{language === 'lo' ? 'ຕາຕະລາງເຮັດວຽກ' : 'Work Schedule'}</h3>
                                <p>{language === 'lo' ? 'ກຳນົດວັນເຮັດວຽກ ແລະ ເວລາ' : 'Configure working days and hours'}</p>
                            </div>
                        </Link>

                        <Link href="/settings/holidays" className={styles.settingsCard}>
                            <div className={styles.settingsIcon}>🎉</div>
                            <div className={styles.settingsInfo}>
                                <h3>{language === 'lo' ? 'ວັນພັກ' : 'Holidays'}</h3>
                                <p>{language === 'lo' ? 'ຈັດການວັນພັກຂອງບໍລິສັດ' : 'Manage company holidays'}</p>
                            </div>
                        </Link>

                        <Link href="/settings/leave" className={styles.settingsCard}>
                            <div className={styles.settingsIcon}>🌴</div>
                            <div className={styles.settingsInfo}>
                                <h3>{language === 'lo' ? 'ນະໂຍບາຍການລາ' : 'Leave Policies'}</h3>
                                <p>{language === 'lo' ? 'ຕັ້ງຄ່າໂຄຕ້າ ແລະ ກົດລະບຽບ' : 'Configure quotas and rules'}</p>
                            </div>
                        </Link>
                    </div>
                </div>
            )}

            {/* Profile Section */}
            <div className={styles.section}>
                <div className={styles.sectionHeader}>
                    <h2 className={styles.sectionTitle}>
                        {language === 'lo' ? 'ໂປຣໄຟລ໌' : 'Profile'}
                    </h2>
                    <p className={styles.sectionDescription}>
                        {language === 'lo' ? 'ຂໍ້ມູນບັນຊີຂອງທ່ານ' : 'Your account information'}
                    </p>
                </div>

                <div className={styles.field}>
                    <label className={styles.label}>
                        {language === 'lo' ? 'ຊື່ສະແດງ' : 'Display Name'}
                    </label>
                    <div className={styles.value}>{user?.displayName}</div>
                </div>

                <div className={styles.field}>
                    <label className={styles.label}>
                        {language === 'lo' ? 'ຊື່ຜູ້ໃຊ້' : 'Username'}
                    </label>
                    <div className={styles.value}>{user?.username}</div>
                </div>

                <div className={styles.field}>
                    <label className={styles.label}>
                        {language === 'lo' ? 'ບົດບາດ' : 'Role'}
                    </label>
                    <div className={styles.value}>{user?.role}</div>
                </div>

                <div className={styles.actions}>
                    <Button variant="secondary" onClick={logout}>
                        {language === 'lo' ? 'ອອກຈາກລະບົບ' : 'Sign Out'}
                    </Button>
                </div>
            </div>

            {/* Application Settings */}
            <div className={styles.section}>
                <div className={styles.sectionHeader}>
                    <h2 className={styles.sectionTitle}>
                        {language === 'lo' ? 'ແອັບພລິເຄຊັນ' : 'Application'}
                    </h2>
                    <p className={styles.sectionDescription}>
                        {language === 'lo' ? 'ການຕັ້ງຄ່າລະບົບ' : 'System preferences'}
                    </p>
                </div>

                <div className={styles.field}>
                    <label className={styles.label}>
                        {language === 'lo' ? 'ພາສາ' : 'Language'}
                    </label>
                    <select className={styles.input} disabled>
                        <option>English</option>
                        <option>Lao</option>
                    </select>
                </div>

                <div className={styles.field}>
                    <label className={styles.label}>
                        {language === 'lo' ? 'ຮູບແບບ' : 'Theme'}
                    </label>
                    <select className={styles.input} disabled>
                        <option>{language === 'lo' ? 'ຄ່າເລີ່ມຕົ້ນລະບົບ' : 'System Default'}</option>
                        <option>{language === 'lo' ? 'ສະຫວ່າງ' : 'Light'}</option>
                        <option>{language === 'lo' ? 'ມືດ' : 'Dark'}</option>
                    </select>
                </div>

                <div className={styles.actions}>
                    <Button disabled>{language === 'lo' ? 'ບັນທຶກ' : 'Save Changes'}</Button>
                </div>
            </div>
        </div>
    );
}

