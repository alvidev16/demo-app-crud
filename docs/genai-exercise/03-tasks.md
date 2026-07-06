# 03 · Tasks — Task Management API

> **Phase:** `tasks` · An ordered, traceable breakdown derived from the
> [plan](02-plan.md). Each task lists the requirement(s) it satisfies and its
> Definition of Done (DoD). Order follows TDD: a failing test precedes each behavior.

## Legend
- **Req** — requirement IDs from [01-spec.md](01-spec.md) this task satisfies.
- Check a box when its DoD is met.

---

### Foundation

- [ ] **T-01 · Project skeleton** — Create `Tasks.Domain`, `Tasks.Services`,
  `Tasks.DAL`, `Tasks.Api` projects + `Tasks.Services.Tests`, `Tasks.Api.Tests`;
  wire the inward dependency references.
  **Req:** NFR-1 · **DoD:** solution builds; dependency direction enforced.

### Domain (test-first)

- [ ] **T-02 · `TaskItem` creation tests** — Failing unit tests for valid creation,
  title bounds, description bound, and past-due-date rejection.
  **Req:** FR-1, FR-2, FR-5 / AC-1, AC-2, AC-3
- [ ] **T-03 · `TaskItem` entity** — Factory `Create(...)` enforcing FR-2/FR-5,
  defaulting status to **Todo**; make T-02 green.
  **Req:** FR-1, FR-2, FR-3, FR-5 · **DoD:** T-02 passes.
- [ ] **T-04 · Status state-machine tests** — Failing tests for each allowed and
  each forbidden transition.
  **Req:** FR-4 / AC-4, AC-5
- [ ] **T-05 · `MoveTo(next)` transition logic** — Lookup-table state machine; make
  T-04 green.
  **Req:** FR-4 · **DoD:** T-04 passes; invalid transition throws.

### Services (test-first)

- [ ] **T-06 · Repository & service interfaces** — `ITaskRepository`,
  `ITaskService` in the appropriate layers (Domain owns the repo contract).
  **Req:** NFR-1
- [ ] **T-07 · `TaskService` tests** — Create, list-own, get-own, update, change-status,
  delete, with a **mocked repository**; include ownership (non-owner → NotFound).
  **Req:** FR-6, FR-7 / AC-6
- [ ] **T-08 · `TaskService` implementation** — Orchestrates validation, ownership
  (`OwnedOrNotFound`), transitions; injects `TimeProvider` for the due-date rule.
  **Req:** FR-5, FR-6, FR-7 · **DoD:** T-07 passes.

### Data access

- [ ] **T-09 · EF Core `TaskRepository` + DbContext mapping** — Implements
  `ITaskRepository`; migration for the `Tasks` table.
  **Req:** FR-6 · **DoD:** repository round-trips against a real database in a test.

### Presentation (test-first)

- [ ] **T-10 · API integration tests** — `WebApplicationFactory`: CRUD happy paths;
  **AC-6** (another user's task → 404) and **AC-7** (no token → 401); invalid
  transition → 400; validation → 400.
  **Req:** FR-6, FR-7, FR-8 / AC-2, AC-3, AC-4, AC-6, AC-7
- [ ] **T-11 · `TasksController`** — Endpoints per the [API contract](02-plan.md#4-api-contract--satisfies-fr-6-fr-8);
  owner id from JWT claim; `[Authorize]`. Make T-10 green.
  **Req:** FR-6, FR-7, FR-8 · **DoD:** T-10 passes.
- [ ] **T-12 · Error middleware** — Domain exceptions → RFC-7807 ProblemDetails with
  field map.
  **Req:** NFR-2

### Cross-cutting

- [ ] **T-13 · Config & secrets** — JWT key + connection string from configuration
  (not source); validate HS256 key length.
  **Req:** NFR-4
- [ ] **T-14 · Full suite green + traceability check** — All acceptance criteria
  covered by a passing test; every FR/NFR maps to ≥ 1 task.
  **Req:** NFR-5

---

## Traceability matrix (requirement → tasks)

| Req | Tasks |
|---|---|
| FR-1 | T-02, T-03 |
| FR-2 | T-02, T-03 |
| FR-3 | T-03 |
| FR-4 | T-04, T-05 |
| FR-5 | T-02, T-03, T-08 |
| FR-6 | T-07, T-08, T-09, T-10, T-11 |
| FR-7 | T-07, T-08, T-10, T-11 |
| FR-8 | T-10, T-11 |
| NFR-1 | T-01, T-06 |
| NFR-2 | T-12 |
| NFR-3 | T-07, T-08, T-10 |
| NFR-4 | T-13 |
| NFR-5 | T-02..T-11, T-14 |
