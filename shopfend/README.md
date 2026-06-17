# ShopBee Frontend

This is the Next.js frontend for ShopBee. It contains the customer storefront, admin portal, seller portal, authentication UI, cart/checkout flow, wishlist, purchases, recommendations, notifications, and chat pages.

## Stack

- Next.js 16 App Router
- React 19
- TypeScript
- NextAuth with Keycloak
- Tailwind CSS
- Vitest and Testing Library
- ESLint with Next.js config

## Local Setup

Install dependencies:

```bash
pnpm install
```

Create `.env.local`:

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

Start the dev server:

```bash
pnpm dev
```

Open `http://localhost:3000`.

## Scripts

```bash
pnpm dev       # start local dev server
pnpm lint      # run ESLint
pnpm build     # production build
pnpm start     # run production server after build
pnpm test      # run Vitest tests
pnpm test:watch
```

## API Client

The frontend talks to the backend through `lib/shopbeApi.ts`.

Important behavior:

- `NEXT_PUBLIC_API_BASE_URL` controls the backend base URL.
- `resolveApiUrl` converts backend-relative upload paths into absolute URLs.
- `productResponseToListItem` normalizes product responses from multiple backend endpoints.

Product cards expect one of:

- `primaryImageUrl`
- a primary image in `images`
- the first image in `images`

## Auth

NextAuth uses Keycloak as the main identity provider. Protected routes are enforced by `middleware.ts`.

Expected role names:

- `Admin`
- `Seller`
- `Customer`

Admin routes live under `/admin/*`.
Seller routes live under `/seller/*`.
Customer-facing routes are public or session-aware depending on the feature.

## Quality Checks

Run before opening a PR:

```bash
pnpm install --frozen-lockfile
pnpm lint
pnpm build
pnpm test
```

The GitHub Actions workflow currently runs install, lint, and build. Vitest is available locally and can be added to CI when desired.

## Deployment

The app can be deployed to Vercel or another Next.js-capable host.

Required production variables:

```env
NEXTAUTH_URL=https://your-frontend-domain
NEXTAUTH_SECRET=replace-with-production-secret
KEYCLOAK_ISSUER=https://your-keycloak-domain/realms/ShopBee
KEYCLOAK_CLIENT_ID=shopfend
KEYCLOAK_CLIENT_SECRET=replace-if-client-is-confidential
NEXT_PUBLIC_API_BASE_URL=https://your-api-domain
NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY=pk_live_or_test_key
```

Also configure the backend CORS allowed origins to include the deployed frontend domain.

## Notes

- The production build currently warns that Next.js `middleware` is deprecated in favor of `proxy`; this is not a build failure.
- Do not commit real OAuth, Keycloak, Stripe, or model-provider secrets.
