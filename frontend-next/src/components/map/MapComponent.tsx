"use client";

import { useEffect, useState, useRef } from "react";
import { MapContainer, TileLayer, Marker, Popup, useMapEvents } from "react-leaflet";
import type { LatLngExpression, Map as LeafletMap, Icon } from "leaflet";

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
    /** Geofence radius in meters (visual only) */
    geofenceRadius?: number;
}

// Click handler component
function MapClickHandler({ onLocationChange }: { onLocationChange?: (lat: number, lng: number) => void }) {
    useMapEvents({
        click: (e) => {
            if (onLocationChange) {
                onLocationChange(e.latlng.lat, e.latlng.lng);
            }
        },
    });
    return null;
}

export default function MapComponent({
    lat,
    lng,
    onLocationChange,
    editable = false,
    zoom = 16,
    popupText = "Location",
    geofenceRadius,
}: AttendanceMapProps) {
    const [leafletIcon, setLeafletIcon] = useState<Icon | null>(null);
    const mapRef = useRef<LeafletMap | null>(null);

    // Fix Leaflet icon issue - must be done client-side
    useEffect(() => {
        import("leaflet").then((L) => {
            // Import CSS
            import("leaflet/dist/leaflet.css");

            // Fix the default marker icon
            const icon = L.icon({
                iconUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png",
                iconRetinaUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png",
                shadowUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png",
                iconSize: [25, 41],
                iconAnchor: [12, 41],
                popupAnchor: [1, -34],
                shadowSize: [41, 41],
            });

            setLeafletIcon(icon);
        });
    }, []);

    // Update map center when lat/lng changes
    useEffect(() => {
        if (mapRef.current) {
            mapRef.current.setView([lat, lng], zoom);
        }
    }, [lat, lng, zoom]);

    const position: LatLngExpression = [lat, lng];

    if (!leafletIcon) {
        return null; // Will show skeleton in parent
    }

    return (
        <MapContainer
            center={position}
            zoom={zoom}
            style={{ height: "100%", width: "100%" }}
            scrollWheelZoom={true}
            ref={mapRef}
        >
            <TileLayer
                attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
                url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
            />

            <Marker position={position} icon={leafletIcon}>
                <Popup>
                    <div className="text-sm font-medium">{popupText}</div>
                    <div className="text-xs text-muted-foreground mt-1">
                        {lat.toFixed(6)}, {lng.toFixed(6)}
                    </div>
                </Popup>
            </Marker>

            {editable && <MapClickHandler onLocationChange={onLocationChange} />}
        </MapContainer>
    );
}
