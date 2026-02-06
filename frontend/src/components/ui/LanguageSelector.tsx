'use client';

import { useState, useRef, useEffect } from 'react';
import { useLanguage } from '@/components/providers/LanguageProvider';

const UKFlag = () => (
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 60 30" width="24" height="12">
        <clipPath id="s">
            <path d="M0,0 v30 h60 v-30 z" />
        </clipPath>
        <clipPath id="t">
            <path d="M30,15 h30 v15 z m0,0 v15 h-30 z m0,0 h-30 v-15 z m0,0 v-15 h30 z" />
        </clipPath>
        <g clipPath="url(#s)">
            <path d="M0,0 v30 h60 v-30 z" fill="#012169" />
            <path d="M0,0 L60,30 M60,0 L0,30" stroke="#fff" strokeWidth="6" />
            <path d="M0,0 L60,30 M60,0 L0,30" clipPath="url(#t)" stroke="#C8102E" strokeWidth="4" />
            <path d="M30,0 v30 M0,15 h60" stroke="#fff" strokeWidth="10" />
            <path d="M30,0 v30 M0,15 h60" stroke="#C8102E" strokeWidth="6" />
        </g>
    </svg>
);

const LaosFlag = () => (
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 30 20" width="24" height="16">
        <rect width="30" height="20" fill="#CE1126" />
        <rect y="5" width="30" height="10" fill="#002868" />
        <circle cx="15" cy="10" r="4" fill="#fff" />
    </svg>
);

export function LanguageSelector() {
    const { language, setLanguage } = useLanguage();
    const [isOpen, setIsOpen] = useState(false);
    const dropdownRef = useRef<HTMLDivElement>(null);

    const options = [
        { code: 'en', label: 'English', flag: <UKFlag /> },
        { code: 'lo', label: 'Lao', flag: <LaosFlag /> }
    ] as const;

    const currentOption = options.find(opt => opt.code === language) || options[0];

    // Close on click outside
    useEffect(() => {
        function handleClickOutside(event: MouseEvent) {
            if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
                setIsOpen(false);
            }
        }
        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, []);

    return (
        <div
            ref={dropdownRef}
            style={{ position: 'relative', zIndex: 50 }}
        >
            <button
                onClick={() => setIsOpen(!isOpen)}
                style={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: '8px',
                    padding: '8px 12px',
                    background: 'white',
                    border: '1px solid #e2e8f0',
                    borderRadius: '8px',
                    cursor: 'pointer',
                    fontSize: '14px',
                    fontWeight: 500,
                    color: '#1e293b',
                    boxShadow: '0 1px 2px rgba(0,0,0,0.05)',
                    transition: 'all 0.2s ease',
                    minWidth: '140px',
                    justifyContent: 'space-between'
                }}
            >
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                    <span style={{ display: 'flex', alignItems: 'center', width: '24px', overflow: 'hidden', borderRadius: '4px' }}>
                        {currentOption.flag}
                    </span>
                    <span>{currentOption.label}</span>
                </div>
                {/* Custom Chevron SVG */}
                <svg
                    width="16"
                    height="16"
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="2"
                    style={{
                        transform: isOpen ? 'rotate(180deg)' : 'rotate(0deg)',
                        transition: 'transform 0.2s'
                    }}
                >
                    <polyline points="6 9 12 15 18 9" />
                </svg>
            </button>

            {/* Dropdown Menu */}
            {isOpen && (
                <div style={{
                    position: 'absolute',
                    top: 'calc(100% + 4px)',
                    right: 0,
                    width: '100%',
                    background: 'white',
                    border: '1px solid #e2e8f0',
                    borderRadius: '8px',
                    boxShadow: '0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)',
                    padding: '4px',
                    overflow: 'hidden'
                }}>
                    {options.map((option) => (
                        <div
                            key={option.code}
                            onClick={() => {
                                setLanguage(option.code as 'en' | 'lo');
                                setIsOpen(false);
                            }}
                            style={{
                                display: 'flex',
                                alignItems: 'center',
                                gap: '8px',
                                padding: '8px 12px',
                                cursor: 'pointer',
                                borderRadius: '6px',
                                background: language === option.code ? '#f1f5f9' : 'transparent',
                                color: language === option.code ? '#0f172a' : '#64748b',
                                fontSize: '14px',
                                transition: 'background 0.1s'
                            }}
                            onMouseEnter={(e) => {
                                if (language !== option.code) {
                                    e.currentTarget.style.background = '#f8fafc';
                                    e.currentTarget.style.color = '#1e293b';
                                }
                            }}
                            onMouseLeave={(e) => {
                                if (language !== option.code) {
                                    e.currentTarget.style.background = 'transparent';
                                    e.currentTarget.style.color = '#64748b';
                                }
                            }}
                        >
                            <span style={{ display: 'flex', alignItems: 'center', width: '24px', overflow: 'hidden', borderRadius: '4px' }}>
                                {option.flag}
                            </span>
                            <span>{option.label}</span>
                            {language === option.code && (
                                <svg
                                    width="14"
                                    height="14"
                                    viewBox="0 0 24 24"
                                    fill="none"
                                    stroke="currentColor"
                                    strokeWidth="2"
                                    style={{ marginLeft: 'auto', color: '#4f46e5' }}
                                >
                                    <polyline points="20 6 9 17 4 12" />
                                </svg>
                            )}
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}
