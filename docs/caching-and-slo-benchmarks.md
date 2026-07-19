# Caching Policies & Latency SLO Benchmarks

This document details the performance baselines, caching configurations, and Service Level Objectives (SLOs) for the StadiumOps enterprise architecture.

---

## 1. Latency Service Level Objectives (SLOs)

We target high-availability, high-performance response times across all services:

| Metric / Endpoint Group | p50 Latency | p95 Latency | p99 Latency |
|-------------------------|-------------|-------------|-------------|
| **Core Auth Endpoints** | < 120ms     | < 250ms     | < 400ms     |
| **Cached Stadium API**  | < 20ms      | < 50ms      | < 100ms     |
| **Operational Updates** | < 100ms     | < 200ms     | < 350ms     |
| **Generative AI Chat**  | < 1.5s      | < 2.5s      | < 4.0s      |

*Note: All measurements are logged via OpenTelemetry telemetry instrumentation (e.g. Azure Application Insights / Cloud Trace) using custom metrics tracking.*

---

## 2. Caching Strategy & TTLs

To prevent query overhead on backend databases under match-day load:

### Caching Layers
1. **L1 Memory Cache**: High-speed, in-memory cache directly on host worker instances. Used for highly transient data.
2. **L2 Distributed Cache (Redis / Memorystore)**: Shared cache for horizontal scale groups, preventing cache stamps when new server nodes bootstrap.

### Time-To-Live (TTL) Configuration

* **Today's Match Details (`match:today`)**:
  * **TTL**: 5 Minutes.
  * **Rationale**: Match status changes (Kickoff, Half-time, Goals) happen frequently; 5 minutes provides balance between data freshness and load shielding.
* **Stadium Venues (`stadiums:*`)**:
  * **TTL**: 10 Minutes.
  * **Rationale**: Stadium listings and capacities are static.
* **Points of Interest (`stadiums:id:pois`)**:
  * **TTL**: 10 Minutes.
  * **Rationale**: Static coordinates, concessions, and accessibility landmarks do not shift on short timelines.

### Cache Invalidation Strategy

* **Eviction on State Modification**:
  * When an operator updates matching details or creates/modifies a stadium's points of interest, the corresponding cache key is explicitly evicted:
    * `TodayMatchUpdate` → Evicts key `match:today`.
    * `POIUpdate` → Evicts key `stadiums:{stadiumId}:pois`.
* **Outbox Telemetry**:
  * Distributed caching invalidations are pushed via the outbox publisher mechanism using a Pub/Sub fan-out topology to clear cache instances across multiple load-balancer nodes.

---

## 3. Frontend Bundle Optimization

* **Tree-Shaking**: Enforced in production Vite configurations to strip dead code pathways from final output files.
* **Code-Splitting**: Route-based code splitting using React `lazy()` and `Suspense` ensures users only download the layout chunks necessary for their active page (e.g. login bundle vs operational control dashboard).
* **Static Assets Caching**:
  * Production responses return `Cache-Control: public, max-age=31536000, immutable` for static resources (JS chunks, CSS files, icons) to guarantee browser-level caching.
