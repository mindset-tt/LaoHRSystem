/**
 * i18n parity audit — runs once at module load (dev only) and warns
 * about keys present in one language but missing in the other. Helps
 * keep the bilingual dictionary in sync as new sections are added.
 */
import { dictionary } from './i18n';

type Dict = Record<string, unknown>;

function flatten(prefix: string, node: unknown, out: Map<string, boolean>): void {
    if (node === null || node === undefined) return;
    if (typeof node === 'string' || typeof node === 'number' || typeof node === 'boolean') {
        out.set(prefix, true);
        return;
    }
    if (typeof node !== 'object') return;
    for (const [k, v] of Object.entries(node as Dict)) {
        const next = prefix ? `${prefix}.${k}` : k;
        flatten(next, v, out);
    }
}

function flattenKeys(root: unknown): Set<string> {
    const map = new Map<string, boolean>();
    flatten('', root, map);
    return new Set(map.keys());
}

let audited = false;

export function auditI18n(): void {
    if (audited) return;
    audited = true;
    if (typeof window === 'undefined' && typeof process !== 'undefined' && process.env?.NODE_ENV === 'production') {
        return;
    }
    const enKeys = flattenKeys(dictionary.en);
    const loKeys = flattenKeys(dictionary.lo);
    const missingInLo = [...enKeys].filter(k => !loKeys.has(k));
    const missingInEn = [...loKeys].filter(k => !enKeys.has(k));
    if (missingInLo.length === 0 && missingInEn.length === 0) return;
    if (typeof console !== 'undefined') {
        if (missingInLo.length > 0) {
            console.warn(`[i18n] ${missingInLo.length} key(s) missing in 'lo':`, missingInLo);
        }
        if (missingInEn.length > 0) {
            console.warn(`[i18n] ${missingInEn.length} key(s) missing in 'en':`, missingInEn);
        }
    }
}

/**
 * Key path list of every translation key in the 'en' dictionary.
 * Useful for: 1) editors that want to see all keys; 2) lint scripts;
 * 3) generating stub translations.
 */
export function getAllKeys(): string[] {
    const keys = flattenKeys(dictionary.en);
    return [...keys].sort();
}