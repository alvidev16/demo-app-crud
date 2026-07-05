# Demo CRUD — Inventory Management (.NET 10 + React)

A full-stack sample built for a technical interview. It exposes a secured **Products CRUD API** and a separate **Auth API** (registration + JWT login), backed by a local **SQLite** database, and consumed by a responsive **React + TypeScript** frontend. Built with **Layered Architecture** and **Test-Driven Development**.

---

## User Story

> **As** an inventory administrator, **I want** to authenticate and manage (create, read, update and delete) the products in the catalog, **so that** I can keep stock and prices up to date securely, ensuring that only authenticated users can modify the data.

---

## Architecture — Layered (N-tier)

Dependencies flow downward. The business logic depends only on abstractions, never on EF Core or ASP.NET.

```
┌──────────────────────────────────────────────┐
│ Presentation  (demo-app-crud.Server)         │  Controllers, JWT auth, Swagger,
│                                              │  exception middleware, DI wiring
└───────────────┬──────────────────────────────┘
                │ depends on
┌───────────────▼───────────────────────────────┐
│ Services  (Demo.Services)                     │  ProductService, AuthService,
│                                               │  DTOs, service interfaces + ports
└───────────────┬───────────────────────────────┘
                │ depends on
┌───────────────▼───────────────────────────────┐
│ Domain  (Demo.Domain)                         │  Product & User entities (invariants),
│                                               │  domain exceptions, repository contracts
└───────────────────────────────────────────────┘
                ▲ implements contracts
┌───────────────┴───────────────────────────────┐
│ Data Access  (Demo.DAL)                       │  EF Core AppDbContext (SQLite),
│                                               │  repositories, BCrypt hasher, seed
└───────────────────────────────────────────────┘
```

| Project | Layer | Responsibility |
|---|---|---|
| `Demo.Domain` | Domain | Entities with business invariants, exceptions, repository interfaces. No external dependencies. |
| `Demo.Services` | Business Logic | Use cases, validation, uniqueness rules, DTOs. Depends only on Domain. |
| `Demo.DAL` | Data Access | EF Core + SQLite, repository implementations, password hasher, migrations, seeding. |
| `demo-app-crud.Server` | Presentation | Web API controllers, JWT issuing/validation, Swagger, error handling. |
| `demo-app-crud.client` | Frontend | React + TypeScript SPA. |

---

## Tech Stack

- **.NET 10**, ASP.NET Core Web API
- **Entity Framework Core 10** + **SQLite** (local file `demo.db`)
- **JWT** bearer authentication + **BCrypt** password hashing
- **React 19** + **TypeScript** + **Vite**
- **xUnit** + **Moq** + **FluentAssertions** (unit) and **WebApplicationFactory** (integration)

---

## Data Model

**Products**

| Field | Type | Rules |
|---|---|---|
| `Id` | Guid (PK) | generated |
| `Name` | string | required, 3–100 chars |
| `Sku` | string | required, **unique** |
| `Price` | decimal | > 0 |
| `Stock` | int | ≥ 0 |
| `Category` | string | required |
| `CreatedAt` / `UpdatedAt` | DateTime | automatic |

**Users**

| Field | Type | Rules |
|---|---|---|
| `Id` | Guid (PK) | generated |
| `Email` | string | required, unique, valid format |
| `PasswordHash` | string | BCrypt (never plain text) |
| `Role` | string | `Admin` / `User` |
| `CreatedAt` | DateTime | automatic |

---

## API Endpoints

### Products API — `/api/products` (all require a valid JWT)

| Verb | Route | Success | Errors |
|---|---|---|---|
| GET | `/api/products` | 200 | 401 |
| GET | `/api/products/{id}` | 200 | 404, 401 |
| POST | `/api/products` | 201 | 400, 409, 401 |
| PUT | `/api/products/{id}` | 200 | 400, 404, 409, 401 |
| DELETE | `/api/products/{id}` | 204 | 404, 401 |

### Auth API — `/api/auth`

| Verb | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/register` | anonymous | Create a user (201) |
| POST | `/api/auth/login` | anonymous | Returns a JWT (200) or 401 |
| GET | `/api/auth/me` | **authorized** | Current user (demonstrates `[Authorize]`) |
| GET | `/api/auth/public` | **anonymous** | Open endpoint (demonstrates `[AllowAnonymous]`) |

Errors are returned as RFC-7807 `ProblemDetails`; validation failures include an `errors` map.

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)

### 1. Backend

```bash
cd demo-app-crud.Server
dotnet run
```

On first run the app **applies the EF Core migration** (creating `demo.db`) and **seeds** the demo data automatically. Swagger UI is available at `https://localhost:7152/swagger` (Development).

### 2. Frontend

```bash
cd demo-app-crud.client
npm install
npm run dev
```

> In Visual Studio you can simply run the solution: the `demo-app-crud.Server` project launches the API and starts the Vite dev server automatically (SPA proxy). The frontend proxies `/api/*` calls to the backend.

### Demo Credentials

| Email | Password | Role |
|---|---|---|
| `admin@demo.com` | `Admin123!` | Admin |

The login screen is pre-filled with these credentials. Five sample products are seeded on first run.

---

## Running the Tests

```bash
# from the repository root
dotnet test
```

| Test project | Type | Covers |
|---|---|---|
| `Demo.Services.Tests` | Unit | `ProductService` & `AuthService` business rules (repositories mocked with Moq) — 25 tests |
| `Demo.Api.Tests` | Integration | Full HTTP pipeline via `WebApplicationFactory` with an in-memory database — 14 tests |

**39 tests total.** The suite drove development TDD-style: tests were written before each service and controller.

---

## Project Structure

```
demo-app-crud/
├── src/
│   ├── Demo.Domain/         # entities, exceptions, repository interfaces
│   ├── Demo.Services/       # business logic, DTOs, service interfaces + ports
│   └── Demo.DAL/            # EF Core, repositories, hasher, migrations, seeder
├── demo-app-crud.Server/    # ASP.NET Core Web API (presentation)
├── demo-app-crud.client/    # React + TypeScript frontend
│   └── src/
│       ├── api/             # typed fetch client + endpoints
│       ├── auth/            # auth context (token in localStorage)
│       └── components/      # Login, Products list, ProductForm
├── tests/
│   ├── Demo.Services.Tests/
│   └── Demo.Api.Tests/
└── demo-app-crud.slnx
```

---

## Design Notes

- **Domain invariants** live in the entities (`Product.Create/Update`, `User.Create`) so an invalid object can never be constructed.
- **The business layer is persistence-agnostic**: it talks to `IProductRepository` / `IUserRepository` (declared in Domain, implemented in the DAL), which keeps it fully unit-testable without a database.
- **Cross-cutting concerns** (password hashing, token issuing) are abstracted behind `IPasswordHasher` / `ITokenService` ports so they can be swapped or mocked.
- **Errors → HTTP** mapping is centralized in a single exception-handling middleware, keeping controllers thin.
