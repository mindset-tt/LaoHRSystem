"use client";

import dynamic from "next/dynamic";
import { Skeleton } from "@/components/ui/skeleton";
import { MapPin } from "lucide-react";

// Dynamic import with SSR disabled - CRITICAL for Next.js + Leaflet
const MapComponent = dynamic(() => import("./MapComponent"), {
    ssr: false,
    loading: () => <MapSkeleton />,
});

// Loading skeleton with Linear/Vercel style
function MapSkeleton() {
    return (
        <div className="h-full w-full flex items-center justify-center bg-slate-100 dark:bg-slate-800 animate-pulse">
            <div className="flex flex-col items-center gap-3 text-slate-400 dark:text-slate-500">
                <MapPin className="h-8 w-8 animate-bounce" />
                <span className="text-sm font-medium">Loading map...</span>
            </div>
        </div>
    );
}

interface AttendanceMapProps {
    /** Current latitude */
    lat: number;
    /** Current longitude */
    lng: number;
    /** Callback when user clicks map to update location */
    onLocationChange?: (lat: number, lng: number) => void;
    /** Whether user can click to change location (Admin mode) */
    editable?: boolean;
    /** Zoom level */
    zoom?: number;
    /** Popup text for the marker */
    popupText?: string;
    /** Optional className for container */
    className?: string;
}

/**
 * AttendanceMap - A reusable map component for HR System
 * 
 * Features:
 * - SSR-safe (uses dynamic import)
 * - Fixed Leaflet marker icons
 * - Click-to-set location (Admin mode)
 * - Linear/Vercel style design
 * 
 * @example
 * // Employee view (read-only)
 * <AttendanceMap lat={17.9757} lng={102.6331} popupText="Your location" />
 * 
 * // Admin view (editable geofence)
 * <AttendanceMap 
 *   lat={officeLat} 
 *   lng={officeLng} 
 *   editable 
 *   onLocationChange={(lat, lng) => setOfficeLocation({ lat, lng })}
 *   popupText="Office Location"
 * />
 */
export default function AttendanceMap({
    lat,
    lng,
    onLocationChange,
    editable = false,
    zoom = 16,
    popupText = "Location",
    className = "",
}: AttendanceMapProps) {
    return (
        <div className={`h-[400px] w-full rounded-xl border border-slate-200 dark:border-slate-700 overflow-hidden shadow-lg ${className}`}>
            <MapComponent
                lat={lat}
                lng={lng}
                onLocationChange={onLocationChange}
                editable={editable}
                zoom={zoom}
                popupText={popupText}
            />
        </div>
    );
}
