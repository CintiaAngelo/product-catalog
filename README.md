# 🛒 Product Catalog API

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12.0-239120?style=for-the-badge&logo=c-sharp)
![Redis](https://img.shields.io/badge/Redis-Cache-DC382D?style=for-the-badge&logo=redis)
![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker)
![License](https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge)

**A production-ready RESTful API for product catalog management, built with .NET 8, Clean Architecture, SOLID principles, and distributed caching.**

[Features](#-features) · [Architecture](#-architecture) · [Getting Started](#-getting-started) · [API Docs](#-api-endpoints) · [Commit Guide](#-semantic-commits)

</div>

---

## ✨ Features

- ✅ **RESTful API** with versioned endpoints (`/api/v1/`)
- ✅ **Clean Architecture** — Domain, Application, Infrastructure, API layers
- ✅ **SOLID Principles** applied throughout the codebase
- ✅ **Distributed Cache** with Redis (falls back to in-memory for dev)
- ✅ **FluentValidation** for robust input validation
- ✅ **Serilog** for structured logging with file rotation
- ✅ **Swagger/OpenAPI** documentation out of the box
- ✅ **Global Exception Middleware** for consistent error responses
- ✅ **Soft Delete** pattern (products are deactivated, not removed)
- ✅ **Unit Tests** with xUnit, Moq and FluentAssertions
- ✅ **Docker & Docker Compose** ready
- ✅ **Health Check** endpoint at `/health`

---

## 🏛 Architecture

This project follows **Clean Architecture**, ensuring separation of concerns and testability.

```
ProductCatalog/
├── src/
│   ├── ProductCatalog.Domain/           # Enterprise business rules
│   │   ├── Entities/                    # Product, BaseEntity
│   │   ├── Enums/                       # ProductCategory
│   │   └── Interfaces/                  # IProductRepository
│   │
│   ├── ProductCatalog.Application/      # Application business rules
│   │   ├── DTOs/                        # Data Transfer Objects
│   │   ├── Interfaces/                  # IProductService, ICacheService
│   │   ├── Services/                    # ProductService
│   │   └── Validators/                  # FluentValidation validators
│   │
│   ├── ProductCatalog.Infrastructure/   # Frameworks & Drivers
│   │   ├── Cache/                       # Redis & InMemory cache
│   │   ├── Data/                        # AppDbContext (EF Core)
│   │   └── Repositories/               # ProductRepository
│   │
│   └── ProductCatalog.API/              # Interface Adapters
│       ├── Controllers/                 # ProductsController
│       ├── Extensions/                  # Swagger setup
│       └── Middleware/                  # Global exception handler
│
└── tests/
    └── ProductCatalog.UnitTests/        # xUnit + Moq + FluentAssertions
        ├── Services/                    # ProductServiceTests
        └── Validators/                  # ProductValidatorTests
```

### Dependency Flow

```
API → Application → Domain
         ↑
  Infrastructure (implements Domain interfaces)
```

---

## 🚀 Getting Started

### Prerequisites

| Tool | Version |
|------|---------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 8.0+ |
| [Docker](https://www.docker.com/) | Optional (for Redis) |
| [Git](https://git-scm.com/) | Any |

---

### ▶️ Option 1 — Run locally (no Redis, uses in-memory cache)

```bash
# 1. Clone the repository
git clone https://github.com/yourusername/product-catalog-api.git
cd product-catalog-api

# 2. Run the API
cd src/ProductCatalog.API
dotnet run

# 3. Open in browser
# http://localhost:5000  (Swagger UI)
# http://localhost:5000/health
```

---

### 🐳 Option 2 — Run with Docker Compose (API + Redis)

```bash
# 1. Clone the repository
git clone https://github.com/yourusername/product-catalog-api.git
cd product-catalog-api

# 2. Start all services
docker-compose up --build

# 3. Open Swagger UI
# http://localhost:5000
```

---

### 🧪 Running Tests

```bash
# Run all unit tests
dotnet test

# Run with coverage report
dotnet test --collect:"XPlat Code Coverage"
```

---

## 📡 API Endpoints

Base URL: `http://localhost:5000/api/v1`

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/products` | List all active products |
| `GET` | `/products/{id}` | Get product by ID |
| `GET` | `/products/category/{category}` | Filter by category |
| `POST` | `/products` | Create a new product |
| `PUT` | `/products/{id}` | Update an existing product |
| `DELETE` | `/products/{id}` | Soft-delete a product |

### Available Categories

`Electronics` · `Clothing` · `Food` · `Books` · `Sports` · `HomeAndGarden` · `Toys` · `Health`

---

### 📥 Request & Response Examples

**POST /api/v1/products**
```json
{
  "name": "Mechanical Keyboard",
  "description": "TKL layout, Cherry MX Red switches",
  "price": 399.90,
  "stockQuantity": 30,
  "category": 1
}
```

**Response 201 Created**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Mechanical Keyboard",
  "description": "TKL layout, Cherry MX Red switches",
  "price": 399.90,
  "stockQuantity": 30,
  "category": "Electronics",
  "isActive": true,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": null
}
```

---

## 🧠 Caching Strategy

The API uses a **cache-aside pattern**:

1. **On read** — check cache first; if miss, fetch from DB and populate cache
2. **On write** (create/update/delete) — invalidate all `products:*` cache keys
3. **TTL** — 10 minutes per entry

| Scenario | Cache Key |
|----------|-----------|
| All products | `products:all` |
| Single product | `products:{guid}` |
| By category | `products:category:{name}` |

If no Redis connection string is configured, the API automatically falls back to **in-memory cache** (perfect for development).

---

## 🔧 Configuration

In `appsettings.json` or environment variables:

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"   // Leave empty to use in-memory cache
  }
}
```

---

## 📝 Semantic Commits

This project follows [Conventional Commits](https://www.conventionalcommits.org/):

| Type | When to use |
|------|-------------|
| `feat` | New feature |
| `fix` | Bug fix |
| `docs` | Documentation only |
| `refactor` | Code change without new feature or bug fix |
| `test` | Adding or updating tests |
| `chore` | Build process, tooling |
| `perf` | Performance improvement |

**Examples used in this project:**

```bash
git commit -m "feat: add product catalog domain entities and repository interface"
git commit -m "feat: implement product service with cache-aside pattern"
git commit -m "feat: add redis and in-memory cache service implementations"
git commit -m "feat: implement products controller with full CRUD endpoints"
git commit -m "feat: add fluent validation for create and update product DTOs"
git commit -m "feat: add global exception middleware with structured error responses"
git commit -m "feat: configure swagger documentation with openapi metadata"
git commit -m "feat: add serilog structured logging with file sink"
git commit -m "test: add unit tests for product service with moq and fluent assertions"
git commit -m "test: add validator unit tests for product DTOs"
git commit -m "chore: add dockerfile and docker-compose with redis service"
git commit -m "docs: add comprehensive readme with architecture and usage guide"
```

---

## 🛡 SOLID Principles Applied

| Principle | Where |
|-----------|-------|
| **S** — Single Responsibility | Each class has one reason to change (Service, Repository, Middleware...) |
| **O** — Open/Closed | `ICacheService` allows new cache strategies without changing consumers |
| **L** — Liskov Substitution | `InMemoryCacheService` and `RedisCacheService` are interchangeable |
| **I** — Interface Segregation | `IProductRepository` and `ICacheService` are focused interfaces |
| **D** — Dependency Inversion | All dependencies injected via constructor; concrete types never referenced in high-level modules |

---

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.

---

<div align="center">
Made with ❤️ and C# · Give it a ⭐ if it helped you!
</div>
# product-catalog
