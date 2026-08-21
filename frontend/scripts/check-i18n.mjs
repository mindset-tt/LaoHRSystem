#!/usr/bin/env node
// Audit en vs lo i18n parity by reading the source TypeScript.
// Run: node scripts/check-i18n.mjs
import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { dirname, resolve } from 'node:path';

const __dirname = dirname(fileURLToPath(import.meta.url));
const src = readFileSync(resolve(__dirname, '../src/lib/i18n.ts'), 'utf8');

/**
 * Walks the dictionary literal and returns a flat list of dot-separated keys.
 * @param {string} src full source
 * @param {string} langKey 'en' or 'lo'
 */
function extractKeys(src, langKey) {
    const startMarker = `${langKey}: {`;
    const start = src.indexOf(startMarker);
    if (start < 0) throw new Error(`Language '${langKey}' not found`);
    let i = start + startMarker.length - 1;
    let depth = 0;
    let end = -1;
    for (; i < src.length; i++) {
        if (src[i] === '{') depth++;
        else if (src[i] === '}') { depth--; if (depth === 0) { end = i; break; } }
    }
    if (end < 0) throw new Error(`Unterminated '${langKey}' block`);

    const keys = [];

    function walk(s, prefix) {
        const re = /(['"])?([a-zA-Z][a-zA-Z0-9_]*)\1\s*:/g;
        let m;
        while ((m = re.exec(s))) {
            const k = m[2];
            let i = m.index + m[0].length;
            while (i < s.length && /\s/.test(s[i])) i++;
            if (s[i] === '{') {
                let depth = 0;
                let j = i;
                for (; j < s.length; j++) {
                    if (s[j] === '{') depth++;
                    else if (s[j] === '}') { depth--; if (depth === 0) break; }
                }
                walk(s.substring(i + 1, j), prefix ? `${prefix}.${k}` : k);
                re.lastIndex = j + 1;
            } else {
                keys.push(prefix ? `${prefix}.${k}` : k);
            }
        }
    }

    walk(src.substring(start + startMarker.length, end + 1), '');
    return keys;
}

const enKeys = new Set(extractKeys(src, 'en'));
const loKeys = new Set(extractKeys(src, 'lo'));

const missingInLo = [...enKeys].filter(k => !loKeys.has(k));
const missingInEn = [...loKeys].filter(k => !enKeys.has(k));

console.log(`en: ${enKeys.size} keys, lo: ${loKeys.size} keys`);
if (missingInLo.length === 0 && missingInEn.length === 0) {
    console.log('✓ i18n parity OK');
    process.exit(0);
}
if (missingInLo.length > 0) {
    console.log(`\n✗ ${missingInLo.length} keys missing in 'lo':`);
    for (const k of missingInLo) console.log('  - ' + k);
}
if (missingInEn.length > 0) {
    console.log(`\n✗ ${missingInEn.length} keys missing in 'en':`);
    for (const k of missingInEn) console.log('  - ' + k);
}
process.exit(1);