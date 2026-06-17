# ShopBee Online Shopping

ShopBee is a full-stack e-commerce application in a single repository. It uses a .NET 8 backend, a Next.js frontend, PostgreSQL, Redis, Keycloak authentication, and Stripe payment integration.

The codebase is organized as a modular monolith backend plus a separate frontend app. It is not currently split into independently deployed microservices.

## Contents

- [Features](#features)
- [Repository Layout](#repository-layout)
- [Architecture](#architecture)
- [Local Development](#local-development)
- [Environment Configuration](#environment-configuration)
- [Demo Accounts](#demo-accounts)
- [Verification](#verification)
- [CI/CD](#cicd)
- [Deployment Notes](#deployment-notes)
- [Troubleshooting](#troubleshooting)

## Features

Implemented areas:

- Customer storefront: catalog, product details, cart, checkout, wishlist, purchases, and reviews
- Admin portal: overview, users, sellers, products, orders, analytics, categories, and notifications
- Seller portal: dashboard, products, orders, analytics, and profile
- Authentication and authorization with Keycloak, NextAuth, JWT bearer auth, and role checks
- Stripe payment endpoints and frontend payment flow
- Recommendation endpoints and UI surfaces
- Chatbot endpoint and chat page
- Local development infrastructure with Docker Compose
- Backend application and E2E tests
- Frontend lint, build, and Vitest tests

Partially implemented or still evolving:

- Redis caching is available but not applied to every high-value read path
- Notifications currently use in-process/Hangfire background jobs
- Shipping entities and calculation APIs exist, but provider-backed integration is not finalized
- Production deployment is environment-driven and should be reviewed per target platform

## Repository Layout

```text
.
├── .github/workflows/ci.yml
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

### Backend

The backend follows Clean Architecture boundaries:

- `Shopbe.Domain`: entities, enums, and domain model
- `Shopbe.Application`: DTOs, CQRS-style handlers, interfaces, and business rules
- `Shopbe.Infrastructure`: EF Core persistence, repositories, seeders, caching, email, integrations, and recommendation services
- `Shopbe.Web`: ASP.NET Core API, middleware, auth setup, Swagger, CORS, and HTTP controllers
- `tests/Shopbe.Application.Tests`: focused application/unit tests
- `tests/Shopbe.E2E.Tests`: API-level E2E tests backed by Testcontainers/PostgreSQL

### Frontend

The frontend is a Next.js 16 App Router app:

- `app/`: route segments and UI components
- `lib/shopbeApi.ts`: typed API client and response normalization
- `next-auth`: Keycloak-backed authentication
- `vitest`: frontend unit tests
- `eslint`: linting with Next.js config

### Authentication Flow

1. The frontend signs users in through NextAuth using the Keycloak client.
2. NextAuth stores the Keycloak access token and exposes user roles in the session.
3. Frontend middleware protects admin and seller routes.
4. The backend validates Keycloak JWTs and maps realm role claims.
5. `UserSyncMiddleware` links or creates the application-side user record.

Role claims control route/API authorization, while application-side users still control seller ownership, admin data queries, order ownership, and profile data.

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

This starts:

- Keycloak: `http://localhost:8080`
- PostgreSQL: `localhost:5432`
- Redis: `localhost:6379`

The Keycloak admin account from `docker-compose.yml` is:

```text
username: admin
password: admin
```

### 2. Configure Keycloak

Import/use the `ShopBee` realm from `keycloak/realm-shopbee.json`.

See [keycloak/README.md](keycloak/README.md) for detailed local setup.

Expected clients:

- `shopfend`
- `shopbe-swagger`
- `shopbe-api`

Expected realm roles:

- `Admin`
- `Seller`
- `Customer`

### 3. Run Backend

```bash
dotnet restore shopbend/Shopbe.sln
dotnet run --project shopbend/Shopbe.Web
```

Default local backend URLs:

- API: `http://localhost:5072`
- Swagger: `http://localhost:5072/swagger`

Development startup behavior:

- EF Core migrations are applied automatically
- sample data is seeded automatically
- local uploads are served from `/uploads`

### 4. Run Frontend

```bash
cd shopfend
pnpm install
pnpm dev
```

Default frontend URL:

- `http://localhost:3000`

## Environment Configuration

### Frontend

Create `shopfend/.env.local` for local development:

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

Optional OAuth providers can be configured with:

```env
GOOGLE_CLIENT_ID=
GOOGLE_CLIENT_SECRET=
GITHUB_CLIENT_ID=
GITHUB_CLIENT_SECRET=
```

### Backend

Development defaults live in `shopbend/Shopbe.Web/appsettings.Development.json`, but real secrets should be provided through user-secrets or environment variables.

Useful local environment variable names:

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

Do not commit real provider secrets, OAuth secrets, Stripe secrets, or model API keys.

## Demo Accounts

Create matching users in Keycloak so the backend can link them to seeded application users by email.

| Username | Email | Role | Expected Access |
| --- | --- | --- | --- |
| `admin` | `admin@shopbee.vn` | `Admin` | Admin portal and admin APIs |
| `seller1` | `seller1@shopbee.vn` | `Seller` | Seller dashboard, seller products, seller orders |
| `seller2` | `seller2@shopbee.vn` | `Seller` | Seller dashboard, seller products, seller orders |
| `customer1` | `customer1@shopbee.vn` | `Customer` | Storefront, cart, checkout, purchases |

For local demos, use a temporary password such as `Passw0rd!` and disable the required password reset action.

## Seeded Development Data

When the backend starts in Development, `ShopbeDbSeeder` creates:

- demo admin, seller, and customer application users
- seller profiles
- approved catalog products with images and variants
- demo orders for dashboard and purchase-flow verification
- coupons and shipping reference data

For larger local or staging datasets, use the seeder project:

```bash
dotnet run --project shopbend/Shopbe.Seeder -- --migrate true --products 2000 --users 500 --orders 1000
```

## Verification

### Backend

Run the same backend checks used by CI:

```bash
cd shopbend
dotnet restore Shopbe.sln
dotnet build Shopbe.sln --no-restore --configuration Release -m:1
dotnet test Shopbe.sln --no-build --configuration Release --verbosity normal
```

The `-m:1` flag avoids an MSBuild parallel output race between projects that reference shared backend assemblies.

### Frontend

Run the same frontend checks used by CI plus the optional test script:

```bash
cd shopfend
pnpm install --frozen-lockfile
pnpm lint
pnpm build
pnpm test
```

Current lint warnings are non-fatal under the existing ESLint configuration.

### Manual Smoke Test

1. Open `http://localhost:5072/swagger`.
2. Start the frontend and open `http://localhost:3000`.
3. Sign in as `admin` and verify `/admin/overview`.
4. Sign in as `seller1` and verify `/seller/dashboard`.
5. Sign in as `customer1`, browse `/products`, add an item to cart, and start checkout.
6. Confirm product cards show images on both catalog and discovery surfaces.

## CI/CD

GitHub Actions workflow:

- [.github/workflows/ci.yml](.github/workflows/ci.yml)

The workflow runs on pushes and pull requests targeting `main`.

Backend job:

1. checkout
2. install .NET 8
3. `dotnet restore Shopbe.sln`
4. `dotnet build Shopbe.sln --no-restore --configuration Release -m:1`
5. detect test projects
6. `dotnet test Shopbe.sln --no-build --configuration Release --verbosity normal`

Frontend job:

1. checkout
2. install Node 22
3. install pnpm 10
4. `pnpm install --frozen-lockfile`
5. `pnpm lint`
6. `pnpm build`

The frontend Vitest suite is available with `pnpm test`; add it to CI if test enforcement is desired for every pull request.

## Deployment Notes

### Backend

`docker-compose.yml` includes a backend service that builds `shopbend/Shopbe.Web/Dockerfile` and exposes the container on `127.0.0.1:5000`.

Production deployments should provide:

- PostgreSQL connection string
- Redis connection string
- Keycloak authority and audience settings
- frontend CORS origins
- Stripe keys and webhook secret
- chatbot/model API key if chatbot is enabled
- persistent storage strategy for `/uploads`

### Frontend

The frontend can be deployed to Vercel or any Node-capable host that supports Next.js.

Required production variables include:

- `NEXTAUTH_URL`
- `NEXTAUTH_SECRET`
- `KEYCLOAK_ISSUER`
- `KEYCLOAK_CLIENT_ID`
- `KEYCLOAK_CLIENT_SECRET` if using a confidential client
- `NEXT_PUBLIC_API_BASE_URL`
- `NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY`

Make sure the backend CORS config allows the deployed frontend origin.

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

- Keycloak token contains the expected realm role
- backend app user exists and has the matching app-side role/status

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
