# Keycloak Local Setup

This folder contains the local Keycloak realm import for ShopBee.

## What The Import Gives You

`realm-shopbee.json` imports the `ShopBee` realm and the main clients used by the app:

- `shopfend`: NextAuth frontend client
- `shopbe-swagger`: Swagger UI login client
- `shopbe-api`: bearer-only API client

The file does not currently provide ready-made demo users and realm roles in a way you should rely on, so create them in the admin console as part of local setup.

## Start Keycloak

From the repo root:

```bash
docker compose up -d keycloak
```

Keycloak will be available at `http://localhost:8080`.

Default admin credentials from `docker-compose.yml`:

- username: `admin`
- password: `admin`

## Import Behavior

The container starts with `start-dev --import-realm` and mounts this folder into Keycloak's import directory.

Important:

- realm import happens when Keycloak initializes its data store
- if you already have an existing `shopbend_keycloak_data` Docker volume, old state may remain
- if a realm change does not appear locally, reset the Keycloak volume and start again

Example clean reset:

```bash
docker compose down
docker volume rm shopbend_keycloak_data
docker compose up -d keycloak
```

## Required Realm Roles

After opening the Keycloak admin console:

1. Open the `ShopBee` realm.
2. Go to `Realm roles`.
3. Create these roles exactly:

- `Admin`
- `Seller`
- `Customer`

These realm roles are what the frontend and backend expect in the token.

## Demo Users To Create

Create these users in the `ShopBee` realm.

Recommended password for local development:

- `Passw0rd!`

When creating credentials, turn off `Temporary` unless you want Keycloak to force a password reset on first login.

| Username | Email | Realm role |
| --- | --- | --- |
| `admin` | `admin@shopbee.vn` | `Admin` |
| `seller1` | `seller1@shopbee.vn` | `Seller` |
| `seller2` | `seller2@shopbee.vn` | `Seller` |
| `customer1` | `customer1@shopbee.vn` | `Customer` |

## How To Create A User

For each demo account:

1. Go to `Users`.
2. Click `Add user`.
3. Enter the username and email from the table above.
4. Save.
5. Open the `Credentials` tab.
6. Set a password.
7. Open the `Role mapping` tab.
8. Assign the matching realm role.

## Why The Emails Matter

The backend development seeder creates app-side users for these same emails:

- `admin@shopbee.vn`
- `seller1@shopbee.vn`
- `seller2@shopbee.vn`
- `customer1@shopbee.vn`

On first authenticated backend request, `UserSyncMiddleware` links the Keycloak user to the existing app user by email if the Keycloak `sub` is not already known in the database.

This is why the Keycloak email must match the seeded backend email for the smoothest demo flow.

## Frontend Configuration

The frontend expects these values in `shopfend/.env.local`:

```env
NEXTAUTH_URL=http://localhost:3000
NEXTAUTH_SECRET=replace-with-a-long-random-string
KEYCLOAK_URL=http://localhost:8080
KEYCLOAK_REALM=ShopBee
KEYCLOAK_CLIENT_ID=shopfend
NEXT_PUBLIC_API_BASE_URL=http://localhost:5072
```

`shopfend` is a public client using the standard authorization code flow with PKCE.

## Backend Configuration

In development, the backend is already configured to validate tokens against:

- `Authentication:Keycloak:Authority = http://localhost:8080/realms/ShopBee`

The backend extracts role claims from Keycloak token role data and uses them for `[Authorize(Roles = ...)]` checks.

## Swagger Login

The imported realm also includes the `shopbe-swagger` client.

Use it at:

- `http://localhost:5072/swagger`

If backend and Keycloak are both running, the Swagger `Authorize` flow should redirect through Keycloak.

## Troubleshooting

### I can log in, but `/admin` redirects to `/`

Check:

- the user has the `Admin` realm role in Keycloak
- you signed out and back in after changing roles
- the frontend is using the expected Keycloak realm and client

### I can reach the frontend, but backend seller/admin APIs return forbidden

Check:

- the access token contains the expected realm role
- the role name matches exactly: `Admin`, `Seller`, or `Customer`
- you are sending the Keycloak access token to the backend

### I can log in, but seller data looks empty or wrong

Check:

- the Keycloak email matches the seeded backend email
- the backend has started in `Development` so the dev seeder ran
- the seller account has a seeded app-side `SellerProfile`

### I changed `realm-shopbee.json`, but nothing changed in Keycloak

You are probably reusing the existing Docker volume. Reset `shopbend_keycloak_data` and start Keycloak again.
