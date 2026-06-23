# ShopBee Online Shopping

ShopBee is a full-stack e-commerce application built as a single repository. It includes a Next.js storefront, seller portal, admin portal, ASP.NET Core API, PostgreSQL, Redis, Keycloak authentication, Stripe payments, recommendation endpoints, and deployment support with Docker Compose and Nginx.

Production deployment:

| Service | URL |
| --- | --- |
| Frontend | `https://shopbee.page` |
| Frontend alias | `https://www.shopbee.page` |
| Backend API | `https://api.shopbee.page` |
| Keycloak/Auth server | `https://auth.shopbee.page` |

Detailed production deployment notes are in [DEPLOYMENT.md](DEPLOYMENT.md).

## Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Repository Layout](#repository-layout)
- [Architecture](#architecture)
- [Authentication](#authentication)
- [Local Development](#local-development)
- [Environment Configuration](#environment-configuration)
- [Seed Data and Demo Accounts](#seed-data-and-demo-accounts)
- [Testing and Verification](#testing-and-verification)
- [CI/CD](#cicd)
- [Deployment](#deployment)
- [Security Notes](#security-notes)
- [Troubleshooting](#troubleshooting)
- [License](#license)

## Features

ShopBee currently includes:

- Customer storefront with home page, product listing, product detail, search, category browsing, cart, checkout, wishlist, reviews, recommendations, notifications, and purchase history.
- Seller portal with dashboard, shop profile, product management, product creation/editing, seller orders, and seller revenue analytics.
- Admin portal with dashboard, user management, seller management, product moderation, order monitoring, category management, notifications, and analytics.
- Authentication and authorization through Keycloak, NextAuth, JWT bearer tokens, realm roles, and route/API protection.
- Social login support through Keycloak Identity Providers for Google OAuth and GitHub OAuth.
- Stripe payment integration, PaymentIntent creation, webhook handling, and frontend Stripe Elements checkout.
- PostgreSQL persistence with Entity Framework Core migrations.
- Redis infrastructure for cache/session-ready workloads.
- Hangfire background jobs for cleanup and email recovery workflows.
- Product recommendations, user behavior tracking, and chatbot endpoints.
- Docker Compose services for local and server deployment.
- Backend unit/E2E tests and frontend lint/build/test scripts.

Partially implemented or still evolving:

- Redis is available but not applied to every high-value read path.
- Notifications use in-process/Hangfire background jobs.
- Shipping entities and calculation APIs exist, but provider-backed shipping integration is not finalized.
- Production upload storage should be reviewed if running multiple app instances.

## Tech Stack

### Frontend

- Next.js 16 App Router
- React 19
- TypeScript
- Tailwind CSS 4
- NextAuth.js 4
- Stripe React/Stripe.js
- Framer Motion
- Lucide React icons
- Vitest, Testing Library, ESLint
- pnpm 10

### Backend

- .NET 8
- ASP.NET Core Web API
- Clean Architecture style module boundaries
- Entity Framework Core with PostgreSQL
- MediatR/CQRS-style application handlers
- Keycloak JWT bearer authentication
- Stripe.net
- Hangfire with in-memory storage by default
- Redis integration
- Swagger/OpenAPI
- xUnit/E2E tests with Testcontainers

### Infrastructure

- Docker Compose
- PostgreSQL 16
- Redis 7 Alpine
- Keycloak 24
- Nginx reverse proxy
- Certbot/Let's Encrypt SSL
- Google Cloud Platform VM
- GitHub Actions CI

## Repository Layout

```text
.
├── .github/workflows/ci.yml
├── DEPLOYMENT.md
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
    ├── app/
    ├── lib/
    ├── public/
    └── package.json
```

## Architecture

ShopBee is organized as a modular monolith backend plus a separate frontend application.

```text
Browser
  |
  | Next.js pages and API auth routes
  v
shopfend
  |
  | HTTPS / Authorization: Bearer <access_token>
  v
Shopbe.Web ASP.NET Core API
  |
  | Application handlers and domain services
  v
Shopbe.Application / Shopbe.Domain / Shopbe.Infrastructure
  |
  | EF Core, Redis, Stripe, Keycloak, email/background jobs
  v
PostgreSQL / Redis / external providers
```

### Backend Projects

- `Shopbe.Domain`: entities, enums, and domain model.
- `Shopbe.Application`: DTOs, use cases, CQRS-style handlers, interfaces, and business rules.
- `Shopbe.Infrastructure`: EF Core persistence, repositories, seeders, caching, Stripe, email, recommendation services, and provider integrations.
- `Shopbe.Web`: ASP.NET Core API, controllers, middleware, Swagger, CORS, authentication, authorization, static uploads, and app startup.
- `Shopbe.Seeder`: optional data generation for larger local or staging datasets.
- `tests/Shopbe.Application.Tests`: focused application/unit tests.
- `tests/Shopbe.E2E.Tests`: API-level E2E tests backed by Testcontainers/PostgreSQL.

### Frontend App

- `app/`: Next.js App Router pages, layouts, route groups, and UI components.
- `app/api/auth/[...nextauth]/route.ts`: NextAuth configuration with Keycloak and optional direct OAuth providers.
- `lib/shopbeApi.ts`: typed API client and response normalization.
- `middleware.ts`: route protection for role-based frontend areas.
- `public/`: static frontend assets.

## Authentication

ShopBee uses Keycloak as the main Identity Provider.

Production realm:

```text
Realm: ShopBee
Issuer: https://auth.shopbee.page/realms/ShopBee
```

Expected roles:

- `Admin`
- `Seller`
- `Customer`

Authentication flow:

1. The frontend redirects users to Keycloak for login.
2. Keycloak authenticates with username/password or a configured Google/GitHub Identity Provider.
3. The frontend receives a Keycloak access token through NextAuth.
4. API calls include:

   ```http
   Authorization: Bearer <access_token>
   ```

5. The backend validates the JWT issuer against Keycloak and maps realm/resource roles.
6. `UserSyncMiddleware` links or creates the application-side user record.

The backend validates a single trusted issuer in production:

```text
https://auth.shopbee.page/realms/ShopBee
```

## Local Development

### Prerequisites

- Docker and Docker Compose
- .NET 8 SDK
- Node.js 22 for CI parity, or Node.js 20+ for local development
- pnpm 10+

### 1. Start Infrastructure

From the repository root:

```bash
docker compose up -d postgres redis keycloak
```

Local service URLs:

| Service | URL |
| --- | --- |
| Keycloak | `http://localhost:8080` |
| PostgreSQL | `localhost:5432` |
| Redis | `localhost:6379` |

The default local Keycloak admin credentials from `docker-compose.yml` are:

```text
username: admin
password: admin
```

### 2. Configure Keycloak

Import or use the `ShopBee` realm from:

```text
keycloak/realm-shopbee.json
```

See [keycloak/README.md](keycloak/README.md) for detailed local setup.

Expected clients:

- `shopfend`
- `shopbe-swagger`
- `shopbe-api`

### 3. Run Backend

```bash
dotnet restore shopbend/Shopbe.sln
dotnet run --project shopbend/Shopbe.Web
```

Default local backend URLs:

- API: `http://localhost:5072`
- Swagger: `http://localhost:5072/swagger`
- Hangfire dashboard, if enabled: `http://localhost:5072/hangfire`

Development startup behavior:

- EF Core migrations are applied automatically.
- Sample data is seeded automatically.
- Local uploads are served from `/uploads`.
- Stripe secret and webhook configuration are required in Development.

### 4. Run Frontend

```bash
cd shopfend
pnpm install
pnpm dev
```

Default frontend URL:

```text
http://localhost:3000
```

## Environment Configuration

### Frontend Local Environment

Create `shopfend/.env.local`:

```env
NEXTAUTH_URL=http://localhost:3000
NEXTAUTH_SECRET=replace-with-a-long-random-string

KEYCLOAK_URL=http://localhost:8080
KEYCLOAK_REALM=ShopBee
KEYCLOAK_CLIENT_ID=shopfend
KEYCLOAK_CLIENT_SECRET=replace-if-client-is-confidential

KEYCLOAK_ISSUER=http://localhost:8080/realms/ShopBee
NEXT_PUBLIC_KEYCLOAK_ISSUER=http://localhost:8080/realms/ShopBee
NEXT_PUBLIC_API_BASE_URL=http://localhost:5072

NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY=pk_test_replace_me
```

Optional direct NextAuth providers:

```env
GOOGLE_CLIENT_ID=
GOOGLE_CLIENT_SECRET=
GITHUB_CLIENT_ID=
GITHUB_CLIENT_SECRET=
```

For production, Google/GitHub login should normally be configured through Keycloak Identity Providers instead of direct frontend providers.

### Backend Local Environment

Development defaults live in `shopbend/Shopbe.Web/appsettings.Development.json`, but secrets should be provided through .NET user-secrets, server environment variables, or a non-committed local file.

Useful backend variables:

```env
ConnectionStrings__DefaultConnection=Server=localhost;Port=5432;Database=shopbend;Username=shop;Password=shop
ConnectionStrings__Redis=localhost:6379

Authentication__Keycloak__Authority=http://localhost:8080/realms/ShopBee
Authentication__Keycloak__AuthorityExternal=http://localhost:8080/realms/ShopBee
Authentication__Keycloak__RequireHttpsMetadata=false
Authentication__Keycloak__ValidateAudience=false

Stripe__SecretKey=sk_test_replace_me
Stripe__PublishableKey=pk_test_replace_me
Stripe__WebhookSecret=whsec_replace_me

Chatbot__ApiKey=replace-if-chatbot-is-enabled
```

### Production Environment

Frontend:

```env
NEXTAUTH_URL=https://shopbee.page
NEXTAUTH_SECRET=<long-random-secret>
KEYCLOAK_URL=https://auth.shopbee.page
KEYCLOAK_REALM=ShopBee
KEYCLOAK_ISSUER=https://auth.shopbee.page/realms/ShopBee
NEXT_PUBLIC_KEYCLOAK_ISSUER=https://auth.shopbee.page/realms/ShopBee
KEYCLOAK_CLIENT_ID=shopfend
KEYCLOAK_CLIENT_SECRET=<keycloak-client-secret-if-confidential>
NEXT_PUBLIC_API_BASE_URL=https://api.shopbee.page
NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY=<stripe-publishable-key>
```

Backend:

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__DefaultConnection=Server=postgres;Port=5432;Database=shopbend;Username=shop;Password=<database-password>
ConnectionStrings__Redis=redis:6379
Authentication__Keycloak__Authority=https://auth.shopbee.page/realms/ShopBee
Authentication__Keycloak__RequireHttpsMetadata=true
Authentication__Keycloak__ValidateAudience=false
Cors__FrontendOrigins__0=https://shopbee.page
Cors__FrontendOrigins__1=https://www.shopbee.page
Stripe__SecretKey=<stripe-secret-key>
Stripe__PublishableKey=<stripe-publishable-key>
Stripe__WebhookSecret=<stripe-webhook-secret>
Chatbot__ApiKey=<chatbot-api-key-if-enabled>
```

## Seed Data and Demo Accounts

When the backend starts in Development, `ShopbeDbSeeder` creates:

- Demo admin, seller, and customer application users.
- Seller profiles.
- Approved catalog products with images and variants.
- Demo orders for dashboard and purchase-flow verification.
- Coupons and shipping reference data.

Create matching users in Keycloak so the backend can link them to seeded application users by email.

| Username | Email | Role | Expected access |
| --- | --- | --- | --- |
| `admin` | `admin@shopbee.vn` | `Admin` | Admin portal and admin APIs |
| `seller1` | `seller1@shopbee.vn` | `Seller` | Seller dashboard, products, orders, analytics |
| `seller2` | `seller2@shopbee.vn` | `Seller` | Seller dashboard, products, orders, analytics |
| `customer1` | `customer1@shopbee.vn` | `Customer` | Storefront, cart, checkout, purchases |

For local demos, use a temporary password such as `Passw0rd!` and disable the required password reset action.

For larger local or staging datasets:

```bash
dotnet run --project shopbend/Shopbe.Seeder -- --migrate true --products 20000 --users 5000 --orders 20000 --use-dummy false
```

## Testing and Verification

### Backend

```bash
cd shopbend
dotnet restore Shopbe.sln
dotnet build Shopbe.sln --no-restore --configuration Release -m:1
dotnet test Shopbe.sln --no-build --configuration Release --verbosity normal
```

The `-m:1` flag avoids an MSBuild parallel output race between projects that reference shared backend assemblies.

### Frontend

```bash
cd shopfend
pnpm install --frozen-lockfile
pnpm lint
pnpm build
pnpm test
```

### Manual Smoke Test

1. Open `http://localhost:5072/swagger`.
2. Open `http://localhost:3000`.
3. Sign in as an admin and verify `/admin/overview`.
4. Sign in as a seller and verify `/seller/dashboard`.
5. Sign in as a customer, browse `/products`, add an item to cart, and start checkout.
6. Confirm product images render on the home page, product listing, and product detail pages.

Production endpoint checks:

```bash
curl -I https://shopbee.page
curl -I https://www.shopbee.page
curl https://api.shopbee.page/api/products
curl https://auth.shopbee.page/realms/ShopBee
curl https://auth.shopbee.page/realms/ShopBee/.well-known/openid-configuration
```

## CI/CD

GitHub Actions workflow:

- [.github/workflows/ci.yml](.github/workflows/ci.yml)

The workflow runs on pushes and pull requests targeting `main`.

Backend job:

1. Checkout.
2. Install .NET 8.
3. Restore `Shopbe.sln`.
4. Build in Release mode.
5. Detect test projects.
6. Run backend tests if test projects exist.

Frontend job:

1. Checkout.
2. Install Node.js 22.
3. Install pnpm 10.
4. Install dependencies with `pnpm install --frozen-lockfile`.
5. Run `pnpm lint`.
6. Run `pnpm build`.

The frontend Vitest suite is available with `pnpm test`; add it to CI if test enforcement is required for every pull request.

## Deployment

Current deployment target:

| Field | Value |
| --- | --- |
| Provider | Google Cloud Platform |
| VM name | `shopbe-server` |
| Zone | `asia-southeast1-b` |
| External IP | `34.21.198.135` |
| Internal IP | `10.148.0.2` |

DNS records:

| Type | Name | Value |
| --- | --- | --- |
| `A` | `shopbee.page` | `34.21.198.135` |
| `A` | `www.shopbee.page` | `34.21.198.135` |
| `A` | `api.shopbee.page` | `34.21.198.135` |
| `A` | `auth.shopbee.page` | `34.21.198.135` |

Deployment architecture:

- Frontend Next.js app is served at `shopbee.page` and `www.shopbee.page`.
- Backend ASP.NET Core API runs in Docker and is exposed internally through `127.0.0.1:5000`.
- Nginx reverse proxies `api.shopbee.page` to the backend.
- Keycloak runs in Docker and is reverse proxied at `auth.shopbee.page`.
- PostgreSQL and Redis run through Docker Compose.
- Nginx handles HTTPS with Certbot/Let's Encrypt certificates.

Useful server commands:

```bash
docker compose ps
docker compose up -d
docker compose up -d --build backend
docker compose logs -f backend
docker compose logs -f keycloak
sudo nginx -t
sudo systemctl reload nginx
sudo certbot certificates
```

See [DEPLOYMENT.md](DEPLOYMENT.md) for the complete production deployment guide, including Nginx examples, SSL commands, OAuth callback URLs, and server checks.

## Security Notes

Never commit real secrets to GitHub.

Do not commit:

- Keycloak admin passwords.
- Keycloak client secrets.
- Google OAuth client secrets.
- GitHub OAuth client secrets.
- PostgreSQL passwords.
- Redis passwords, if enabled.
- Stripe secret keys.
- Stripe webhook secrets.
- NextAuth secrets.
- Chatbot/model provider API keys.
- Real production `.env` files.

Recommended practices:

- Keep `.env`, `.env.local`, and production env files out of Git.
- Commit only `.env.example` files with placeholder values.
- Rotate any secret that was accidentally committed.
- Use strong random values for `NEXTAUTH_SECRET`, database passwords, and Keycloak admin credentials.
- Keep PostgreSQL, Redis, the backend internal port, and Keycloak internal HTTP ports private.
- Allow public ingress through Nginx on ports `80` and `443`.

## Troubleshooting

### Product Images Render On `/products` But Not Home

Check whether the home page is using the backend discovery endpoint or hardcoded/mock products. Product cards use normalized image fields:

- `primaryImageUrl`
- primary image from `images`
- first image from `images`

Backend public product endpoints should return active, approved, non-deleted products with image data included.

### Authenticated User Has No App Data

Confirm the Keycloak user email matches a seeded app user, or let `UserSyncMiddleware` create/link the user after login. Also confirm the Keycloak realm role matches the expected app role.

### Admin Or Seller Route Redirects Unexpectedly

Check both:

- The Keycloak token contains the expected realm role.
- The backend app user exists and has the matching app-side role/status.

### Backend Build Fails On `*.deps.json`

Use the CI build command:

```bash
dotnet build Shopbe.sln --no-restore --configuration Release -m:1
```

This avoids parallel writes to shared output files.

### Frontend Build Warns About Middleware

Next.js may warn that the `middleware` file convention is deprecated in favor of `proxy`. This is currently a warning, not a build failure.

## License

MIT
