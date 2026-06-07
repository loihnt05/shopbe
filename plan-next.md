# Next Plan: Phase 3

This phase turns the existing cache scaffolding into a correct and useful runtime optimization for public read paths.

## Goal

Improve cache correctness and reduce repeated database work on high-value product and recommendation reads.

## Why This Is Next

- Phase 1 made the app easier to start and demo.
- Phase 2 made the demo flow verified and stable.
- The repo already advertises Redis as partial infrastructure, but cache invalidation is incomplete and some expensive public reads are still uncached.

## Phase 3 Scope

1. Fix product cache invalidation so updates and deletes clear the real cache keys.
2. Implement prefix-based cache invalidation for grouped product/recommendation cache entries.
3. Add cache coverage to the most valuable public recommendation endpoints.
4. Keep all cache changes bounded to runtime behavior, not new product features.

## Deliverables

### 1. Cache Invalidation Correctness

Fix mismatches between read cache keys and mutation invalidation logic.

Required coverage:

- product detail cache key invalidates correctly after update/delete
- search/list caches can be invalidated by prefix
- recommendation caches can be invalidated by prefix when product data changes

### 2. Prefix Cache Removal

Replace the current `NotImplementedException` in `RedisCacheService.RemoveByPrefixAsync` with working Redis-backed prefix deletion.

Required behavior:

- uses the configured Redis connection
- respects the configured instance-name prefix
- safely removes matching keys without changing non-matching keys

### 3. Recommendation Read Caching

Add cache-aside behavior for public recommendation endpoints that are likely to be hit often.

Recommended targets:

- top selling products
- similar products by product ID
- frequently bought together

### 4. Verification Path

Run targeted backend tests after the cache changes and ensure existing E2E coverage still passes.

Recommended verification:

- `dotnet test tests/Shopbe.E2E.Tests/Shopbe.E2E.Tests.csproj`
- `dotnet build Shopbe.sln`

## Execution Order

1. Fix the current invalidation key mismatch.
2. Implement working prefix invalidation in Redis cache service.
3. Add recommendation caching with clear cache keys.
4. Invalidate search/recommendation caches from product mutation handlers.
5. Re-run tests/build and fix any fallout.

## Acceptance Criteria

- Product updates and deletes no longer leave stale detail cache entries behind.
- Product search and recommendation caches can be invalidated by prefix.
- Public recommendation endpoints use cache-aside reads.
- Existing tests still pass after cache behavior changes.

## Out Of Scope For Phase 3

- cache metrics dashboards
- full cache invalidation for every domain object
- write-through caching
- queue-based cache invalidation events

## Exit Condition

Phase 3 is done when the public product/recommendation read paths use cache more effectively and product mutations invalidate the relevant cache entries correctly.
