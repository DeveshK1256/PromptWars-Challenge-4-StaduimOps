import type { PointOfInterest } from "../services/api";
import type { NavigationRoute, Stadium } from "../services/api";

export function getPoiTone(poi: PointOfInterest) {
  const category = poi.category.toLowerCase();
  if (category.includes("entrance") || category.includes("gate")) {
    return "gate";
  }
  if (category.includes("food")) {
    return "food";
  }
  if (category.includes("medical") || category.includes("first aid")) {
    return "medical";
  }
  return poi.isAccessible ? "accessible" : "default";
}

export function getPoiInitial(poi: PointOfInterest) {
  const category = poi.category.toLowerCase();
  if (category.includes("entrance") || category.includes("gate")) {
    return "G";
  }
  if (category.includes("food")) {
    return "F";
  }
  if (category.includes("medical") || category.includes("first aid")) {
    return "M";
  }
  return "P";
}

const mapMarkerLayout = [
  { left: "18%", top: "22%" },
  { left: "62%", top: "58%" },
  { left: "28%", top: "70%" },
  { left: "64%", top: "72%" },
];

export function StaticStadiumMap({
  stadium,
  pois,
  route,
  reason,
}: {
  stadium: Stadium;
  pois: PointOfInterest[];
  route: NavigationRoute | null;
  reason: string;
}) {
  const markers = pois.slice(0, mapMarkerLayout.length).map((poi, index) => ({
    poi,
    position: mapMarkerLayout[index],
    tone: getPoiTone(poi),
  }));

  return (
    <div
      className="static-map"
      role="img"
      aria-label={`${stadium.name} operations map with concourse, seating bowl, route, and points of interest.`}
    >
      <div className="static-map-board" aria-hidden="true">
        <div className="map-compass">N</div>
        <div className="map-ring map-ring-outer">Concourse</div>
        <div className="map-ring map-ring-inner">Seating bowl</div>
        <div className="map-field">
          <span>Pitch</span>
        </div>
        <div className="map-route map-route-main" />
        <div className="map-route map-route-turn" />
        <div className="map-route-dot map-route-start">Start</div>
        <div className="map-route-dot map-route-end">Gate C</div>
        {markers.map(({ poi, position, tone }) => (
          <div
            className={`map-marker ${tone}`}
            key={poi.id}
            style={{ left: position.left, top: position.top }}
            title={`${poi.name}, ${poi.category}, wait ${poi.estimatedWaitMinutes} minutes`}
          >
            <span>{getPoiInitial(poi)}</span>
            <strong>{poi.name}</strong>
          </div>
        ))}
      </div>
      <div className="map-legend">
        <span>
          <i className="legend-swatch gate" /> Entrances
        </span>
        <span>
          <i className="legend-swatch food" /> Food
        </span>
        <span>
          <i className="legend-swatch medical" /> Medical
        </span>
        <span>
          <i className="legend-line" /> Recommended route
        </span>
      </div>
      {route && (
        <div className="map-route-summary">
          <strong>
            {route.fromLocation} to {route.toLocation}
          </strong>
          <span>
            {route.distanceMeters} m, {route.estimatedMinutes} min, {route.crowdLoadPercent}% crowd load
          </span>
        </div>
      )}
      <p className="notice-line">{reason}</p>
    </div>
  );
}
