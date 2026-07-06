# 02 · Technical Plan — Task Management API

> **Phase:** `plan` · Turns the [specification](01-spec.md) into concrete technical
> decisions. Every decision cites the requirement(s) it satisfies.

## 1. Stack & rationale

| Decision | Choice | Why |
|---|---|---|
| Language / framework | **C# · .NET 10 · ASP.NET Core Web API** | Consistency with the host solution; strong typing helps enforce invariants. |
| Persistence | **EF Core + SQLite** | Local, zero-setup; migrations for schema. |
| AuthN | **JWT bearer** (reuse host setup) | Satisfies **FR-8**; the token carries the user id. |
| Testing | **xUnit + Moq + FluentAssertions**, integration via `WebApplicationFactory` | Satisfies **NFR-5**. |

## 2. Architecture (Layered + Clean dependency rule)

Dependencies point inward; business rules never depend on EF Core or ASP.NET
(**NFR-1**).

```
Presentation (Controllers, JWT)  →  Services (use cases)  →  Domain (entity + rules)
Data Access (EF Core repo)       ─────────────────────────►  (implements Domain interfaces)
```

## 3. Domain model → satisfies FR-1..FR-5

`TaskItem` (named `TaskItem`, not `Task`, to avoid colliding with `System.Threading.Tasks.Task`):

| Field | Type | Rule (req) |
|---|---|---|
| `Id` | Guid (PK) | FR-1 |
| `Title` | string | required, 3–120 (FR-2) |
| `Description` | string? | ≤ 1000 (FR-2) |
| `Status` | `TaskState` enum `{ Todo, InProgress, Done }` | FR-3 (named `TaskState`, not `TaskStatus`, to avoid `System.Threading.Tasks.TaskStatus`) |
| `DueDate` | `DateOnly` | not in past at creation (FR-5) |
| `OwnerUserId` | Guid | FR-1, FR-7 |

- **Invariants live in the entity** (factory `Create` + `MoveTo(next)`), so an invalid task can't be constructed (NFR-1).
- **Status state machine** (FR-4): `Todo→InProgress`, `InProgress→Done`, `InProgress→Todo`; a lookup table rejects anything else.
- Due-date check takes "today" via `TimeProvider` so the rule is deterministically testable (NFR-5).

## 4. API contract → satisfies FR-6, FR-8

All endpoints `[Authorize]` (FR-8). Owner id is read from the **JWT claim**, never the body (FR-7).

| Verb | Route | Success | Errors | Req |
|---|---|---|---|---|
| GET | `/api/tasks` | 200 (caller's tasks) | 401 | US-2, FR-7 |
| GET | `/api/tasks/{id}` | 200 | 404, 401 | US-2, FR-7 |
| POST | `/api/tasks` | 201 | 400, 401 | US-1, FR-2, FR-5 |
| PUT | `/api/tasks/{id}` | 200 | 400, 404, 401 | US-3 |
| PATCH | `/api/tasks/{id}/status` | 200 | 400 (invalid transition), 404, 401 | US-4, FR-4 |
| DELETE | `/api/tasks/{id}` | 204 | 404, 401 | US-5 |

## 5. Validation & error strategy → satisfies NFR-2

- Field validation and transition rules throw domain exceptions; a single
  **exception-handling middleware** maps them to **RFC-7807 ProblemDetails**.
- Validation failures return **400** with an `errors` field→message map.

## 6. Authorization strategy → satisfies FR-7, NFR-3

- The service loads a task and compares `OwnerUserId` to the caller's id.
- If it doesn't exist **or** isn't owned by the caller, the service throws
  **NotFound** → the API returns **404, not 403**, so existence is never leaked (NFR-3, AC-6).

## 7. Security → satisfies NFR-4

- JWT signing key and connection string come from configuration/secrets, not source.
- (Guard learned in the host project: an HS256 signing key must be **> 256 bits**.)

## 8. Testing strategy → satisfies NFR-5

| Layer | Type | Covers |
|---|---|---|
| Domain/Service | Unit (Moq) | AC-1..AC-5 (validation, transitions, ownership) |
| API | Integration (`WebApplicationFactory`) | AC-6, AC-7 (ownership 404, auth) + happy-path CRUD |

Written **test-first**: each acceptance criterion becomes a failing test before code.
