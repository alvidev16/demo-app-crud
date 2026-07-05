# Architecture — Layered structure that honors Clean Architecture principles

This project is organized as **Layered (N-tier)** architecture, but its dependency
structure deliberately follows the **Clean Architecture dependency rule**. This
document maps one to the other so the design intent is explicit.

---

## The core idea

> **Clean Architecture's single most important rule:** source-code dependencies
> point *inward*, toward the business rules. Inner layers know nothing about outer
> layers — nothing about databases, the web, or frameworks.

Our projects satisfy exactly this. `Demo.Domain` is at the center and depends on
nothing; every other project depends inward toward it.

```
        outer (volatile: frameworks, I/O)                inner (stable: business rules)
   ┌─────────────────────────────────────────────────────────────────────────────┐
   │                                                                             │
   │   demo-app-crud.Server ─┐                                                   │
   │   (Web / Controllers)   │                                                   │
   │                         ├──► Demo.Services ──► Demo.Domain   ◄── center     │
   │   Demo.DAL ─────────────┘        (use cases)     (entities +                │
   │   (EF Core / SQLite) ───────────────────────────► repository interfaces)    │
   │                                                                             │
   └─────────────────────────────────────────────────────────────────────────────┘
                         all arrows point inward ─────────────►
```

The dependency inversion happens at the Domain boundary: `Demo.Domain` **declares**
`IProductRepository` / `IUserRepository`; `Demo.DAL` (an outer layer) **implements**
them. So the database depends on the domain — never the reverse.

---

## Layer ↔ Clean Architecture ring mapping

| Clean Architecture ring | Our project | What lives here | Depends on |
|---|---|---|---|
| **Entities** (Enterprise business rules) | `Demo.Domain` | `Product`, `User` with invariants; domain exceptions; repository interfaces | *nothing* |
| **Use Cases** (Application business rules) | `Demo.Services` | `ProductService`, `AuthService`; DTOs; `IPasswordHasher` / `ITokenService` ports | Domain |
| **Interface Adapters** | `demo-app-crud.Server` controllers + `Demo.DAL` repositories | Controllers, repository implementations, JWT token service, EF mappings | Services, Domain |
| **Frameworks & Drivers** | EF Core, ASP.NET Core, SQLite, React | The actual frameworks and the DB | (outermost) |

---

## How each Clean Architecture principle is met

### 1. Separation of concerns
Each project has one reason to change:
- **Domain** — business invariants (e.g. *price must be > 0*, *SKU required*).
- **Services** — use-case orchestration and cross-entity rules (e.g. *SKU must be unique*, *login must verify a hash*).
- **DAL** — persistence details (EF Core, SQL, migrations).
- **Server** — HTTP concerns (routing, status codes, auth, serialization).

### 2. Component independence (the dependency rule)
- `Demo.Domain` has **zero** package or project references — it cannot even *see* EF Core or ASP.NET.
- `Demo.Services` references **only** `Demo.Domain`. It has no dependency on EF Core, so the business logic is fully unit-testable with mocked repositories (see `Demo.Services.Tests`, 25 tests using Moq).
- The persistence and web frameworks sit at the edges and are swappable: SQLite could become SQL Server by changing only `Demo.DAL`; the React SPA could be replaced without touching any C#.

### 3. Dependency inversion at the boundaries
- Repository contracts (`IProductRepository`, `IUserRepository`) are owned by the **Domain**, implemented by the **DAL**.
- Infrastructure concerns (password hashing, token issuing) are owned by the **Services** as ports (`IPasswordHasher`, `ITokenService`) and implemented in outer layers (`BCryptPasswordHasher` in the DAL, `JwtTokenService` in the Server).
- Result: business code depends on abstractions; concrete frameworks depend on those abstractions.

### 4. Testability as a consequence
Because the inner layers are framework-free, they are tested in isolation:

| Layer | Test project | Strategy |
|---|---|---|
| Use cases (Services) | `Demo.Services.Tests` | Pure unit tests, repositories mocked (Moq) |
| Persistence (DAL) | `Demo.DAL.Tests` | Real SQLite in-memory — verifies SQL translation & unique indexes |
| End-to-end (Server) | `Demo.Api.Tests` | `WebApplicationFactory`, full HTTP pipeline |

---

## Why "Layered" naming instead of the literal Clean Architecture labels?

Layered (N-tier) is the more familiar vocabulary for a CRUD application of this size,
and it keeps the project count small (Domain / Services / DAL / Presentation). The
**dependency direction is identical** to Clean Architecture, so we get the benefits
— independence, testability, swappable infrastructure — without extra ceremony.
Renaming `Demo.Services` → `Demo.Application` and `Demo.DAL` → `Demo.Infrastructure`
would make it read as textbook Clean Architecture without changing a single dependency.
