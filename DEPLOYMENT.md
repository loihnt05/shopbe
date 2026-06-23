# ShopBee Deployment Guide

This document describes the current production deployment for the ShopBee e-commerce project.

## Overview

ShopBee is deployed as a full-stack e-commerce application behind Nginx on a Google Cloud Platform VM.

Production traffic uses the following public endpoints:

- Frontend: `https://shopbee.page` and `https://www.shopbee.page`
- Backend API: `https://api.shopbee.page`
- Keycloak authentication server: `https://auth.shopbee.page`

High-level architecture:

```text
User Browser
  |
  | HTTPS
  v
Nginx on GCP VM: shopbe-server
  |-- shopbee.page / www.shopbee.page -> Next.js frontend
  |-- api.shopbee.page                -> ASP.NET Core backend on 127.0.0.1:5000
  |-- auth.shopbee.page               -> Keycloak container
  |
  | Docker network / localhost bindings
  v
Docker Compose services
  |-- backend    ASP.NET Core API
  |-- keycloak   Identity Provider
  |-- postgres   application database
  |-- redis      cache/session infrastructure
```

Nginx terminates HTTPS with Certbot/Let's Encrypt certificates and reverse proxies requests to the internal services.

## Domain Mapping

| Service | Public URL | Purpose |
| --- | --- | --- |
| Frontend | `https://shopbee.page` | Customer storefront and application UI |
| Frontend alias | `https://www.shopbee.page` | `www` entry point for the frontend |
| Backend API | `https://api.shopbee.page` | ASP.NET Core REST API |
| Auth server | `https://auth.shopbee.page` | Keycloak realm, login, token, and identity provider callbacks |

## DNS Records

All production hostnames currently point to the same GCP VM external IP address.

| Type | Name | Value |
| --- | --- | --- |
| `A` | `shopbee.page` | `34.21.198.135` |
| `A` | `www.shopbee.page` | `34.21.198.135` |
| `A` | `api.shopbee.page` | `34.21.198.135` |
| `A` | `auth.shopbee.page` | `34.21.198.135` |

After DNS changes, verify resolution:

```bash
dig shopbee.page
dig www.shopbee.page
dig api.shopbee.page
dig auth.shopbee.page
```

## VPS Information

| Field | Value |
| --- | --- |
| Provider | Google Cloud Platform |
| VM name | `shopbe-server` |
| Zone | `asia-southeast1-b` |
| External IP | `34.21.198.135` |
| Internal IP | `10.148.0.2` |

Required server components:

- Docker and Docker Compose
- Nginx
- Certbot with the Nginx plugin
- Git
- Node.js/pnpm if building or running the Next.js frontend directly on the VM

## Docker Services

The production Docker Compose stack contains these core services:

| Service | Container | Purpose | Public exposure |
| --- | --- | --- | --- |
| `backend` | `shopbe-backend` | ASP.NET Core API | Bound to `127.0.0.1:5000`, proxied by Nginx |
| `keycloak` | `keycloak` | Identity Provider and OAuth broker | Proxied by Nginx at `auth.shopbee.page` |
| `postgres` | `shopbend-postgres` | PostgreSQL database | Bound to localhost only |
| `redis` | `shopbend-redis` | Redis cache | Bound to localhost only |

Useful commands:

```bash
docker compose ps
docker compose up -d
docker compose up -d --build backend
docker compose logs -f backend
docker compose logs -f keycloak
docker compose logs -f postgres
docker compose logs -f redis
```

The backend container is expected to use:

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
```

Store production secrets in server-side environment files or secret storage. Do not commit real `.env` files.

## Nginx Reverse Proxy

Nginx is responsible for:

- Listening on ports `80` and `443`
- Redirecting HTTP to HTTPS
- Serving or proxying the Next.js frontend for `shopbee.page` and `www.shopbee.page`
- Proxying backend API traffic to `http://127.0.0.1:5000`
- Proxying Keycloak traffic to the internal Keycloak container port
- Passing the correct forwarded headers to backend services

Example backend API server block:

```nginx
server {
    server_name api.shopbee.page;

    location / {
        proxy_pass http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Example Keycloak server block:

```nginx
server {
    server_name auth.shopbee.page;

    location / {
        proxy_pass http://127.0.0.1:8080;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header X-Forwarded-Host $host;
    }
}
```

Validate Nginx before reload:

```bash
sudo nginx -t
sudo systemctl reload nginx
sudo systemctl status nginx
```

## HTTPS and SSL

TLS certificates are issued by Certbot/Let's Encrypt.

Issue or renew certificates:

```bash
sudo certbot --nginx -d shopbee.page -d www.shopbee.page
sudo certbot --nginx -d api.shopbee.page
sudo certbot --nginx -d auth.shopbee.page
```

Check renewal:

```bash
sudo certbot renew --dry-run
sudo certbot certificates
```

All production OAuth, Keycloak, NextAuth, Stripe webhook, and API URLs should use HTTPS.

## Keycloak Authentication

ShopBee uses Keycloak as the primary Identity Provider.

Production realm:

```text
Realm: ShopBee
Issuer: https://auth.shopbee.page/realms/ShopBee
```

Authentication flow:

1. The frontend redirects the user to Keycloak.
2. Keycloak authenticates the user with username/password or a configured third-party identity provider.
3. After successful login, the frontend receives the Keycloak tokens.
4. The frontend sends API requests with:

   ```http
   Authorization: Bearer <access_token>
   ```

5. The backend validates JWT tokens using the Keycloak issuer:

   ```text
   https://auth.shopbee.page/realms/ShopBee
   ```

Expected Keycloak roles:

- `Admin`
- `Seller`
- `Customer`

Production Keycloak settings should include:

- Correct frontend redirect URIs for `https://shopbee.page`
- Correct web origins for `https://shopbee.page` and `https://www.shopbee.page`
- HTTPS issuer URL: `https://auth.shopbee.page/realms/ShopBee`
- Strong admin password
- Persistent Keycloak data volume

## Google and GitHub Login Setup

Google and GitHub social login should be configured as Keycloak Identity Providers for the `ShopBee` realm.

### Google OAuth

Configure a Google OAuth client in Google Cloud Console.

Recommended values:

```text
Authorized JavaScript origins:
https://auth.shopbee.page

Authorized redirect URI:
https://auth.shopbee.page/realms/ShopBee/broker/google/endpoint
```

Then configure the Google Identity Provider in Keycloak with the generated Google client ID and client secret.

### GitHub OAuth

Configure a GitHub OAuth App.

Recommended values:

```text
Homepage URL:
https://shopbee.page

Authorization callback URL:
https://auth.shopbee.page/realms/ShopBee/broker/github/endpoint
```

Then configure the GitHub Identity Provider in Keycloak with the generated GitHub client ID and client secret.

In production, prefer routing Google/GitHub login through Keycloak so the backend continues to validate a single issuer: `https://auth.shopbee.page/realms/ShopBee`.

## Environment Variables

### Frontend

Production frontend variables:

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

Only set direct `GOOGLE_CLIENT_ID`, `GOOGLE_CLIENT_SECRET`, `GITHUB_CLIENT_ID`, and `GITHUB_CLIENT_SECRET` in the frontend if intentionally enabling direct NextAuth providers. The preferred production setup is Google/GitHub through Keycloak Identity Providers.

### Backend

Production backend variables:

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

### Keycloak

Production Keycloak variables should include strong credentials and the public hostname:

```env
KEYCLOAK_ADMIN=<admin-username>
KEYCLOAK_ADMIN_PASSWORD=<strong-admin-password>
KC_HTTP_ENABLED=true
KC_HOSTNAME=auth.shopbee.page
KC_HOSTNAME_STRICT=false
KC_HOSTNAME_STRICT_HTTPS=false
KC_PROXY=edge
```

## Deployment Checks

DNS:

```bash
dig shopbee.page
dig api.shopbee.page
dig auth.shopbee.page
```

Containers:

```bash
docker compose ps
docker compose logs --tail=100 backend
docker compose logs --tail=100 keycloak
```

Nginx:

```bash
sudo nginx -t
sudo systemctl status nginx
```

Public endpoints:

```bash
curl -I https://shopbee.page
curl -I https://www.shopbee.page
curl https://api.shopbee.page/api/products
curl https://auth.shopbee.page/realms/ShopBee
curl https://auth.shopbee.page/realms/ShopBee/.well-known/openid-configuration
```

Backend local port on the VPS:

```bash
curl http://127.0.0.1:5000/api/products
```

TLS:

```bash
sudo certbot certificates
```

## Security Notes

Never commit production secrets to GitHub.

Do not commit:

- Keycloak admin passwords
- Keycloak client secrets
- Google OAuth client secrets
- GitHub OAuth client secrets
- PostgreSQL passwords
- Redis passwords, if enabled
- Stripe secret keys
- Stripe webhook secrets
- NextAuth secrets
- Chatbot/model provider API keys
- Real production `.env` files

Recommended practices:

- Keep `.env` files out of Git.
- Commit only `.env.example` files with placeholder values.
- Rotate any secret that was accidentally committed.
- Use strong random values for `NEXTAUTH_SECRET`, database passwords, and Keycloak admin credentials.
- Restrict database and Redis ports to localhost or the Docker network.
- Keep ports `5432`, `6379`, `5000`, and Keycloak's internal HTTP port closed from the public internet.
- Allow public ingress only through Nginx on ports `80` and `443`.
