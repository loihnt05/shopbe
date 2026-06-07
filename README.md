# ShopBee Online Shopping

ShopBee is a full-stack e-commerce application built as a single repository with a .NET 8 backend, a Next.js frontend, and Keycloak-based authentication.

## Current State

What is implemented now:

- ASP.NET Core 8 backend using Clean Architecture in `shopbend/`
- Next.js 16 frontend in `shopfend/`
- Keycloak authentication with NextAuth on the frontend
- JWT bearer authentication and role checks on the backend
- customer flows: catalog, product detail, cart, checkout, wishlist, purchases, reviews
- admin flows: overview, users, sellers, products, orders, analytics, categories
- seller flows: dashboard, products, orders, analytics, profile
- Stripe payment endpoints
- recommendation and chat endpoints/pages
- local development infrastructure via Docker Compose: Keycloak, PostgreSQL, Redis

What is only partial or planned:

- Redis is configured but not yet fully rolled out across the highest-value read paths
- notifications currently rely on in-process background jobs, not a queue-based pipeline
- shipping entities and APIs exist, but provider-backed shipping integration is not the current local-dev focus
- this repo is not running as separate deployed microservices today

## Repository Layout

```text
.
├── docker-compose.yml
├── keycloak/
│   ├── README.md
│   └── realm-shopbee.json
├── shopbend/
│   ├── Shopbe.Domain/
│   ├── Shopbe.Application/
│   ├── Shopbe.Infrastructure/
│   ├── Shopbe.Web/
│   ├── Shopbe.Seeder/
│   └── tests/
└── shopfend/
```

## Architecture

The backend is a modular monolith, not an API gateway plus independently deployed services.

- `Shopbe.Domain`: entities and enums
- `Shopbe.Application`: CQRS handlers, DTOs, business logic
- `Shopbe.Infrastructure`: EF Core, persistence, integrations, seeders, services
- `Shopbe.Web`: HTTP API, auth wiring, middleware, Swagger
- `shopfend`: customer/admin/seller UI built with App Router

## Auth And Roles

The auth flow is:

1. The frontend signs users in through NextAuth using the Keycloak client `shopfend`.
2. NextAuth stores the Keycloak access token and exposes realm roles in `session.user.roles`.
3. Frontend middleware protects `/admin/*` and `/seller/*` based on those roles.
4. The backend validates Keycloak JWTs and maps role claims from the token.
5. `UserSyncMiddleware` ensures the authenticated Keycloak user exists in the app database.

Important detail:

- Keycloak role claims control frontend access and backend `[Authorize(Roles = ...)]` checks.
- App-side user records still matter for seller/admin data queries.
- If a seeded app user and a Keycloak user share the same email, the backend links the existing app user to the Keycloak `sub` on first authenticated request.

## Feature Status

| Area | Status | Notes |
| --- | --- | --- |
| Keycloak login | Done | NextAuth + backend JWT validation |
| Admin RBAC | Done | Admin pages and admin API controllers exist |
| Seller RBAC | Done | Seller pages and seller API controllers exist |
| Customer storefront | Done | Catalog, cart, checkout, purchases, wishlist |
| Payments | Done | Stripe flow is wired in the backend |
| Reviews | Done | Review endpoints and UI exist |
| Recommendations | Partial | Available in current app, still evolving |
| Chatbot | Partial | Available in current app, still evolving |
| Redis caching | Partial | Present in infrastructure, not fully applied |
| Notifications | Partial | Background job based, not queue based |
| Shipping integration | Partial | Core pieces exist, provider integration is not finalized |

## Local Setup

### Prerequisites

- Docker and Docker Compose
- .NET 8 SDK
- Node.js 20
- `pnpm`

### 1. Start infrastructure

From the repo root:

```bash
docker compose up -d
```

This starts:

- Keycloak on `http://localhost:8080`
- PostgreSQL on `localhost:5432`
- Redis on `localhost:6379`

### 2. Configure Keycloak users and roles

Use the guide in `keycloak/README.md`.

The imported realm already includes the main clients:

- `shopfend`
- `shopbe-swagger`
- `shopbe-api`

You still need to create the demo realm roles and demo users unless they already exist in your Keycloak data volume.

### 3. Run the backend

```bash
dotnet restore shopbend/Shopbe.sln
dotnet run --project shopbend/Shopbe.Web
```

Notes:

- the backend development profile listens on `http://localhost:5072`
- EF Core migrations are applied automatically at startup in development
- the development seeder runs automatically and creates demo app-side users plus sample catalog data
- Swagger is available at `http://localhost:5072/swagger`

### 4. Run the frontend

Create `shopfend/.env.local` with:

```env
NEXTAUTH_URL=http://localhost:3000
NEXTAUTH_SECRET=replace-with-a-long-random-string
KEYCLOAK_URL=http://localhost:8080
KEYCLOAK_REALM=ShopBee
KEYCLOAK_CLIENT_ID=shopfend
NEXT_PUBLIC_API_BASE_URL=http://localhost:5072
```

Then start the frontend:

```bash
cd shopfend
pnpm install
pnpm dev
```

The frontend runs at `http://localhost:3000`.

## Demo Accounts

Use these emails in Keycloak so they match the seeded backend users:

| Username | Email | Role | Expected access |
| --- | --- | --- | --- |
| `admin` | `admin@shopbee.vn` | `Admin` | `/admin/overview` and admin APIs |
| `seller1` | `seller1@shopbee.vn` | `Seller` | `/seller/dashboard` with seller-owned sample products |
| `seller2` | `seller2@shopbee.vn` | `Seller` | `/seller/dashboard` with seller-owned sample products |
| `customer1` | `customer1@shopbee.vn` | `Customer` | storefront, cart, checkout, purchases |

Recommended local-dev password convention:

- set the same temporary password for all demo users, for example `Passw0rd!`
- disable the temporary-password reset requirement if you want a smoother demo flow

## Seeded Development Data

When `shopbend/Shopbe.Web` starts in development, the built-in seeder ensures:

- demo users for admin, two sellers, and one customer
- seller profiles for the seller accounts
- sample catalog data
- a few stable seller-owned products for dashboard and moderation testing
- two stable demo orders for `customer1`: one delivered seller order and one pending seller order
- coupons and shipping reference data

The large-data generator in `shopbend/Shopbe.Seeder` is separate and intended for bigger local or staging datasets.

Example:

```bash
dotnet run --project shopbend/Shopbe.Seeder -- --migrate true --products 2000 --users 500 --orders 1000
```

## Verification Checklist

After setup, verify these flows:

1. Sign in as `admin` and open `/admin/overview`.
2. Confirm the admin overview shows at least one delivered order and one pending order.
3. Sign in as `seller1` and open `/seller/dashboard` to confirm delivered-order revenue appears.
4. Sign in as `seller2` and open `/seller/orders` to confirm a pending order appears.
5. Sign in as `customer1` and browse `/products`.
6. Open `http://localhost:5072/swagger` and confirm authenticated admin/seller endpoints are visible.
7. Call `GET /api/auth/me` through Swagger or an authenticated client to inspect token claims if role mapping looks wrong.

## Known Gaps

- queue-based notification delivery is not implemented yet
- shipping provider integration is not finalized
- Redis usage needs a targeted rollout and invalidation review
- some roadmap items in older docs were aspirational and are being corrected to match the current codebase

## License

MIT
