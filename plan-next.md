# Next Implementation Plan

This document covers the next major work items after the RBAC/admin/seller rollout.

## Scope

Planned work:

1. Shipping tracking and provider integration
2. Complete Redis cache service
3. Improve README to match the actual system
4. Queue-based notifications
5. Real admin/seller/user seed and Keycloak setup docs

---

## Recommended Execution Order

1. Improve README to match the actual system
2. Real admin/seller/user seed and Keycloak setup docs
3. Complete Redis cache service
4. Queue-based notifications
5. Shipping tracking and provider integration

Reasoning:

- README and setup docs reduce confusion immediately
- seed/docs make the system easier to demo and test
- Redis improves the internal quality of existing features
- notifications and shipping are larger integrations and should come after the system is easier to operate

---

## 1. Shipping Tracking and Provider Integration

### Goal

Allow the platform to create shipments, store tracking information, and expose shipment status to admins, sellers, and customers.

### Scope Decision

Pick one of these as the first delivery target:

- Tracking only for manually-entered shipment codes
- Provider-backed shipment creation plus tracking
- Full label generation plus shipment tracking

Recommended first step:

- Provider-backed shipment creation plus tracking

### Backend Work

#### Domain / Data

Add or confirm shipment-related fields such as:

- `ShipmentProvider`
- `ProviderShipmentId`
- `TrackingCode`
- `TrackingUrl`
- `ShippingStatus`
- `LabelUrl`
- `ShippedAt`
- `DeliveredAt`
- `LastTrackingSyncAt`

### Application

Add abstraction:

- `IShippingProviderService`

Expected methods:

- create shipment
- get shipment tracking
- cancel shipment if supported
- generate tracking URL

### Infrastructure

Implement one provider adapter first.

Examples:

- GHN
- GHTK
- VNPost
- a mock/manual provider for local development

Recommended first delivery:

- manual/mock provider + one real provider adapter

### Web/API

Add endpoints for:

- create shipment for order
- get shipment tracking by order/shipment id
- refresh shipment tracking
- optionally register provider webhook callbacks

### Frontend

Add shipment tracking visibility to:

- admin order monitoring
- seller order management
- customer purchase/order detail page

### Acceptance Criteria

- seller/admin can attach or create shipment for an order
- tracking code and provider are stored
- customer can see tracking status for shipped orders
- provider refresh updates current shipment status
- failures are logged and do not corrupt order state

### Risks

- webhook verification complexity
- provider-specific field mismatches
- retry/idempotency issues on shipment creation

---

## 2. Complete Redis Cache Service

### Goal

Finish the Redis-backed cache layer and use it in the most valuable read-heavy paths.

### Current Gap

The Redis service exists, but parts of it are incomplete and at least one code path still throws `NotImplementedException`.

### Work Breakdown

#### Audit

Review:

- `Shopbe.Infrastructure/Services/RedisCacheService.cs`
- `ICacheService`
- all current cache consumers

Identify:

- unimplemented methods
- invalidation gaps
- missing connection multiplexer usage

#### Design

Define cache rules:

- key naming convention
- key versioning strategy
- TTL per entity type
- invalidation strategy by prefix or resource

#### Implementation

Complete:

- get/set/remove
- serialize/deserialize safety
- prefix invalidation if required
- pattern/key scanning approach
- optional `IConnectionMultiplexer` injection if needed

#### Apply Cache To High-Value Reads

Recommended first targets:

- product detail
- product list facets
- category list
- recommendation responses
- seller/admin overview summaries if needed

### Acceptance Criteria

- no `NotImplementedException` in Redis cache service
- cache keys are predictable and documented
- invalidation occurs after admin/seller product updates where needed
- cached paths remain correct after data changes

### Risks

- stale data after admin moderation changes
- expensive invalidation if key strategy is too broad
- over-caching user-specific content

---

## 3. Improve README To Match Actual System

### Goal

Make the documentation describe the real architecture and feature set rather than an aspirational future state.

### Main Corrections Needed

Update or remove claims about:

- API Gateway / microservice split
- ASP.NET Identity + refresh-token auth
- PayOS
- external ML recommendation service
- queue stack if not yet implemented

### Add Accurate Documentation For

- Keycloak + NextAuth authentication flow
- current Clean Architecture layout
- admin/seller/customer RBAC
- Stripe-based payment flow
- recommendation and chatbot current scope
- Docker/local startup instructions
- admin login and role setup flow

### Recommended README Structure

1. What is currently implemented
2. Architecture as it exists now
3. Feature matrix: done / partial / planned
4. Local development setup
5. Keycloak setup
6. Demo accounts and seeding
7. Known gaps / roadmap

### Acceptance Criteria

- README no longer claims unsupported features
- a new developer can run the project from docs alone
- auth/admin setup is documented clearly

---

## 4. Queue-Based Notifications

### Goal

Move notification delivery toward an event-driven flow using a proper queue and background consumer.

### Scope Decision

Pick one queue technology first.

Recommended:

- RabbitMQ

Do not implement RabbitMQ and Kafka at the same time.

### Work Breakdown

#### Event Model

Define notification events such as:

- order created
- payment succeeded
- order shipped
- product approved/rejected
- seller status changed

#### Producer Side

Publish queue messages when business events occur.

Recommended pattern:

- domain event or application event
- optional outbox pattern if stronger reliability is required

#### Consumer Side

Create a worker/consumer that:

- receives notification jobs
- sends email
- writes notification logs
- updates delivery status

#### Persistence / Audit

Track:

- message id
- delivery status
- retry count
- failure reason
- sent timestamp

### Integration Targets

First events to implement:

- order confirmation email
- payment success email
- admin moderation email

### Acceptance Criteria

- notifications are published asynchronously through queue
- consumer processes and logs delivery attempts
- retries are supported for transient failures
- duplicate delivery is prevented or minimized via idempotency key

### Risks

- eventual consistency surprises
- duplicated events without idempotency
- partial migration if some notifications still use direct send

---

## 5. Real Admin/Seller/User Seed and Keycloak Setup Docs

### Goal

Provide a stable demo/test setup with real app-side users, seller profiles, and matching Keycloak role instructions.

### Demo Accounts To Support

Recommended accounts:

- `admin`
- `seller1`
- `seller2`
- `customer1`

### Seeder Work

Seed or ensure existence of:

- one admin user in app DB
- at least two sellers in app DB
- seller profiles for seller users
- one or more customers
- demo products owned by sellers
- demo orders and payments if helpful

### Keycloak Docs Work

Document:

- how to access Keycloak admin console
- how to import/use realm `ShopBee`
- how to create users
- how to assign roles: `Admin`, `Seller`, `Customer`
- which client is used by frontend
- how token roles map to app authorization

### Important Note To Document

Admin/seller access depends on:

- Keycloak role claims in token
- matching app-side user data where required by handlers

### Files To Update

- `README.md`
- `keycloak/README.md`
- optional new file: `docs/demo-users.md`

### Acceptance Criteria

- a developer can create or import demo users in Keycloak quickly
- app login works for admin, seller, and customer examples
- admin dashboard access steps are explicitly documented

---

## Suggested Deliverables By Milestone

### Milestone A: Documentation and Setup

- README corrected
- Keycloak README written
- demo users documented
- seed accounts aligned with docs

### Milestone B: Platform Reliability and Performance

- Redis cache service completed
- cache integrated into key read paths

### Milestone C: Event-Driven Notifications

- queue integration added
- notification consumer implemented
- first email events migrated

### Milestone D: Shipping Integration

- shipment provider abstraction added
- first provider implemented
- tracking exposed to admin/seller/customer

---

## Recommended File/Area Targets

### Shipping

- `shopbend/Shopbe.Domain/Entities/...`
- `shopbend/Shopbe.Application/Shipping/...`
- `shopbend/Shopbe.Infrastructure/...Shipping...`
- `shopbend/Shopbe.Web/Controllers/ShipmentsController.cs`
- frontend order pages in `shopfend/app/admin`, `shopfend/app/seller`, `shopfend/app/purchases`

### Redis

- `shopbend/Shopbe.Infrastructure/Services/RedisCacheService.cs`
- `shopbend/Shopbe.Application/Common/Interfaces/...`
- read-heavy handlers using `ICacheService`

### README / Docs

- `README.md`
- `keycloak/README.md`
- optional `docs/`

### Notifications

- `shopbend/Shopbe.Application/Common/Interfaces/Notifications/...`
- `shopbend/Shopbe.Infrastructure/...Email...`
- queue integration area in infrastructure
- notification logs / consumer worker

### Seed / Keycloak

- `shopbend/Shopbe.Seeder/...`
- `shopbend/Shopbe.Infrastructure/Persistence/ShopbeDbSeeder.cs`
- `keycloak/realm-shopbee.json`
- docs files

---

## Final Recommendation

Start with:

1. README alignment
2. real seed + Keycloak docs

Then move to:

3. Redis completion
4. queue-based notifications
5. shipping provider integration

This order gives the best balance of clarity, usability, and implementation risk.
