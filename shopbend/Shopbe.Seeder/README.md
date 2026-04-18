# Shopbe.Seeder

Console app to seed **large amounts of realistic-ish data** into the Shopbe PostgreSQL database.

It generates:

- Categories
- Brands
- Products + product images + variants
- Users + addresses + shopping carts
- Orders + order items (with snapshot fields)

The default mode is **idempotent “top-up”**: it checks existing counts and only creates the missing rows up to the requested target.

## Run locally

From repository root:

```bash
cd shopbend

dotnet run --project Shopbe.Seeder -- \
  --connection "Host=localhost;Port=5432;Database=shopbe;Username=shopbe;Password=shopbe" \
  --mode ensure \
  --migrate true \
  --categories 50 \
  --brands 200 \
  --products 20000 \
  --variants-min 1 \
  --variants-max 5 \
  --users 5000 \
  --orders 20000 \
  --max-items 5 \
  --batch 2000
```

### Smaller “quick smoke test” seed

```bash
cd shopbend

dotnet run --project Shopbe.Seeder -- \
  --connection "Host=localhost;Port=5432;Database=shopbe;Username=shopbe;Password=shopbe" \
  --categories 10 --brands 20 --products 200 --users 50 --orders 200 --batch 200
```

## Wipe & reseed

`wipe` will **TRUNCATE** a curated list of tables and restart identities.

```bash
cd shopbend

dotnet run --project Shopbe.Seeder -- \
  --connection "Host=localhost;Port=5432;Database=shopbe;Username=shopbe;Password=shopbe" \
  --mode wipe \
  --categories 50 --brands 200 --products 20000 --users 5000 --orders 20000
```

## Notes / performance

- For *very large* datasets (hundreds of thousands to millions of rows), EF Core will be slower.
  If you need that, we can extend this with bulk insert (e.g. EFCore.BulkExtensions) or PostgreSQL COPY.
- Seeding orders is currently simplified (no shipments/payments/coupons), but the relationships are valid.

