# Task Management API

The GenAI exercise, **implemented** via Spec-Driven Development. The code was generated
from the artifacts in [`../docs/genai-exercise/`](../docs/genai-exercise/) (spec → plan →
tasks), test-first, one task at a time.

- **Spec:** [01-spec.md](../docs/genai-exercise/01-spec.md) · **Plan:** [02-plan.md](../docs/genai-exercise/02-plan.md) · **Tasks:** [03-tasks.md](../docs/genai-exercise/03-tasks.md)

## Stack & architecture

.NET 10 · ASP.NET Core · EF Core (SQLite) · JWT. Layered with the Clean dependency rule:

```
Tasks.Api (Presentation)  →  Tasks.Services  →  Tasks.Domain  ←  Tasks.DAL
```

- **Tasks.Domain** — `TaskItem` entity with invariants and the status state machine; `ITaskRepository`.
- **Tasks.Services** — `TaskService` (validation, transitions, ownership); DTOs.
- **Tasks.DAL** — EF Core `TasksDbContext` + `TaskRepository` + migration.
- **Tasks.Api** — controllers, JWT auth, error middleware.

## Endpoints (all `[Authorize]`)

| Verb | Route | Notes |
|---|---|---|
| GET | `/api/tasks` | the caller's tasks |
| GET | `/api/tasks/{id}` | 404 if missing **or not owned** (no existence leak) |
| POST | `/api/tasks` | 201; 400 on invalid title/description/past due date |
| PUT | `/api/tasks/{id}` | update details |
| PATCH | `/api/tasks/{id}/status` | 400 on an invalid state transition |
| DELETE | `/api/tasks/{id}` | 204 |
| POST | `/api/auth/token` | **demo stand-in** for the host's auth — mints a JWT for a user id |

Status is exchanged as a **name** (`"Todo"`/`"InProgress"`/`"Done"`). Valid transitions:
`Todo→InProgress`, `InProgress→Done`, `InProgress→Todo`.

## Run

```bash
cd src/Tasks.Api
dotnet run          # applies the migration, creates tasks.db
```

```bash
# get a token, then call the API
TOKEN=$(curl -s -X POST http://localhost:5300/api/auth/token -H "Content-Type: application/json" \
  -d '{"userId":"11111111-1111-1111-1111-111111111111"}' | jq -r .token)

curl -X POST http://localhost:5300/api/tasks -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title":"Write the report","dueDate":"2026-12-31"}'
```

## Tests

```bash
dotnet test        # 31 tests, all green
```

| Project | Type | Covers |
|---|---|---|
| `Tasks.Services.Tests` | Unit (Moq) | entity invariants, state machine, service rules & ownership — AC-1..AC-6 |
| `Tasks.Api.Tests` | Integration + real-SQLite repo | auth (AC-7), ownership isolation (AC-6), CRUD, transitions, string-enum contract |

## Notes on the build (critical-thinking, not blind generation)

- **`Microsoft.OpenApi` CVE** — the Web API template pulled a vulnerable transitive package;
  removed the unused `Microsoft.AspNetCore.OpenApi` reference → 0 warnings.
- **Enum-as-integer bug** — unit/integration tests passed with the typed DTO, but a real HTTP
  client sending `"status":"InProgress"` got a 400. **End-to-end runtime verification** caught it;
  added `JsonStringEnumConverter` and a regression test.
- Applied host-project lessons up front: patched `SQLitePCLRaw`, JWT key length ≥ 256 bits.
