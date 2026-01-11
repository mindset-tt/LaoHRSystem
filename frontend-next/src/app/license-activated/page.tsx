'use client';

import { useState } from 'react';
import { licenseAPI } from '@/lib/api';
import { useRouter } from 'next/navigation';
import { KeyRound, CheckCircle, AlertCircle, Loader2 } from 'lucide-react';

import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';

export default function LicensePage() {
    const [key, setKey] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');
    const router = useRouter();

    const handleActivate = async () => {
        setLoading(true);
        setError('');
        setSuccess('');

        try {
            const cleanKey = key.trim();
            if (!cleanKey) throw new Error('Please enter a license key');

            const res = await licenseAPI.activate(cleanKey);
            setSuccess(`Activated! Welcome, ${res.customer}`);

            setTimeout(() => { router.push('/'); }, 2000);
        } catch (err: any) {
            setError(err.message || 'Activation failed');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="min-h-screen flex items-center justify-center bg-slate-50/50 dark:bg-black p-4">
            <div className={cn(
                "w-full max-w-md rounded-xl p-8",
                "bg-white dark:bg-slate-900/50 border border-slate-200/60 dark:border-slate-800/60 shadow-xl"
            )}>
                <div className="text-center mb-6">
                    <div className="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-full bg-indigo-50 dark:bg-indigo-900/20 border border-indigo-100 dark:border-indigo-800">
                        <KeyRound className="h-7 w-7 text-indigo-600 dark:text-indigo-400" />
                    </div>
                    <h1 className="text-2xl font-bold tracking-tight text-slate-900 dark:text-white">Activate License</h1>
                    <p className="text-sm text-slate-500 mt-1 leading-relaxed">Enter your license key to unlock all features.</p>
                </div>

                <div className="space-y-4">
                    <div>
                        <label className="text-sm font-medium text-slate-700 dark:text-slate-300 mb-1 block">License Key</label>
                        <textarea
                            className={cn(
                                "w-full h-32 p-3 rounded-lg text-sm font-mono resize-none",
                                "border border-slate-200/60 dark:border-slate-700",
                                "bg-white dark:bg-slate-900",
                                "focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500/50",
                                "transition-all duration-200"
                            )}
                            placeholder="Paste your license key here..."
                            value={key}
                            onChange={(e) => setKey(e.target.value)}
                        />
                    </div>

                    {error && (
                        <div className="flex items-center gap-2 p-3 text-sm text-rose-600 bg-rose-50 dark:bg-rose-900/20 border border-rose-200 dark:border-rose-800 rounded-lg">
                            <AlertCircle className="h-4 w-4" />
                            {error}
                        </div>
                    )}

                    {success && (
                        <div className="flex items-center gap-2 p-3 text-sm text-emerald-600 bg-emerald-50 dark:bg-emerald-900/20 border border-emerald-200 dark:border-emerald-800 rounded-lg">
                            <CheckCircle className="h-4 w-4" />
                            {success}
                        </div>
                    )}

                    <Button onClick={handleActivate} disabled={loading || !key} className="w-full bg-indigo-600 hover:bg-indigo-700 active:scale-95 transition-all duration-200">
                        {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                        {loading ? 'Verifying...' : 'Activate License'}
                    </Button>

                    <p className="text-center text-xs text-slate-400">Contact support if you don't have a key. Hardware ID: <span className="font-mono">ANY (*)</span></p>
                </div>
            </div>
        </div>
    );
}
