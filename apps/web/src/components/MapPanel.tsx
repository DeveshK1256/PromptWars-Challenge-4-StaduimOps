import { Eye } from "lucide-react";
import { useEffect, useRef, useState } from "react";
import type { NavigationRoute, PointOfInterest, Stadium } from "../services/api";
import { isGoogleMapsConfigured, loadGoogleMaps } from "../services/googleMaps";
import type { AsyncStatus } from "../types";
import { StaticStadiumMap } from "./StaticStadiumMap";

export function MapPanel({
  stadium,
  pois,
  route,
}: {
  stadium: Stadium | null;
  pois: PointOfInterest[];
  route: NavigationRoute | null;
}) {
  const mapRef = useRef<HTMLDivElement | null>(null);
  const [status, setStatus] = useState<AsyncStatus>("idle");
  const [message, setMessage] = useState("");
  const configured = isGoogleMapsConfigured();
  const showFallbackMap = !configured || status === "error";
  const fallbackReason = configured
    ? `Live Google Maps could not load: ${message || "unknown error"}. Showing the stadium operations map instead.`
    : "Live Google Maps key is not configured. Showing the built-in stadium operations map.";

  useEffect(() => {
    if (!stadium || !configured || !mapRef.current) {
      return;
    }

    setStatus("loading");
    loadGoogleMaps()
      .then(() => {
        if (!mapRef.current || !window.google?.maps?.Map) {
          throw new Error("Google Maps JavaScript API loaded without map constructor.");
        }

        const center = { lat: Number(stadium.latitude), lng: Number(stadium.longitude) };
        const map = new window.google.maps.Map(mapRef.current, {
          center,
          zoom: 15,
          mapTypeControl: false,
          streetViewControl: false,
          fullscreenControl: true,
        });
        if (window.google.maps.Marker) {
          new window.google.maps.Marker({ position: center, map, title: stadium.name });
        }
        setStatus("success");
        setMessage("Google Maps JavaScript API loaded.");
      })
      .catch((error: Error) => {
        setStatus("error");
        setMessage(error.message);
      });
  }, [configured, stadium]);

  if (!stadium) {
    return (
      <div className="map-panel empty">
        <Eye aria-hidden="true" />
        <p>Select a stadium to load map context.</p>
      </div>
    );
  }

  return (
    <div className="map-panel">
      <div className="map-header">
        <div>
          <strong>{stadium.name}</strong>
          <span>
            {stadium.city}, {stadium.country}
          </span>
        </div>
        <a
          className="text-button"
          href={`https://www.google.com/maps/search/?api=1&query=${stadium.latitude},${stadium.longitude}`}
          target="_blank"
          rel="noreferrer"
        >
          Open Maps
        </a>
      </div>
      {configured && (
        <>
          <div
            className={showFallbackMap ? "map-canvas is-hidden" : "map-canvas"}
            ref={mapRef}
            aria-label={`Google map for ${stadium.name}`}
          />
          <p className="notice-line" aria-live="polite">
            {status === "loading" ? "Loading Google Maps JavaScript API..." : message || "Map ready."}
          </p>
        </>
      )}
      {showFallbackMap && <StaticStadiumMap stadium={stadium} pois={pois} route={route} reason={fallbackReason} />}
    </div>
  );
}
