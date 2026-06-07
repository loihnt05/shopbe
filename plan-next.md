# Next Plan: Phase 4

This phase improves notification delivery resilience using the persistence that already exists in the email queue path.

## Goal

Make persisted email messages recoverable if the immediate Hangfire enqueue/processing path is interrupted.

## Why This Is Next

- Phase 3 improved cache correctness and high-value read performance.
- The app already writes `EmailMessage` records before queueing background work.
- The README still calls notifications partial, and the smallest useful improvement is to recover stuck pending/failed email messages automatically.

## Phase 4 Scope

1. Add a recovery job for `EmailMessage` rows that are pending or retryable.
2. Schedule that recovery job as recurring background work.
3. Keep the change bounded to email reliability, not a full event-driven notification redesign.

## Deliverables

### 1. Email Recovery Job

Add a background job that scans persisted email messages and re-enqueues retryable work.

Required behavior:

- picks up `Pending` and `Failed` messages
- ignores `Sent` and `DeadLetter` messages
- respects `MaxAttempts`
- avoids hot-loop re-enqueueing by using a retry delay window

### 2. Startup Scheduling

Wire the recovery job into app startup similarly to the existing recurring cleanup job.

Required behavior:

- configurable enable/disable flag
- configurable cron expression
- configurable batch size and retry delay window

### 3. Verification Path

Run backend tests/build to ensure the new recurring job wiring does not break existing flows.

Recommended verification:

- `dotnet test tests/Shopbe.Application.Tests/Shopbe.Application.Tests.csproj`
- `dotnet test tests/Shopbe.E2E.Tests/Shopbe.E2E.Tests.csproj`
- `dotnet build Shopbe.sln`

## Execution Order

1. Add the email recovery job.
2. Register it in DI.
3. Schedule it from `Program.cs` with configuration defaults.
4. Re-run tests/build and confirm no startup regressions.

## Acceptance Criteria

- Retryable persisted email messages can be re-enqueued automatically.
- Recovery scheduling is configuration-driven.
- Existing tests still pass after the new background job is introduced.

## Out Of Scope For Phase 4

- replacing Hangfire with an external broker
- general notification center redesign
- SMS/push channels
- full outbox/event bus architecture

## Exit Condition

Phase 4 is done when persisted email messages have an automatic recovery path instead of depending only on the initial enqueue attempt.
