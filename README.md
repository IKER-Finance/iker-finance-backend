# IKER Finance - Backend API

Multi-currency personal finance management system built with Clean Architecture, CQRS, and MediatR patterns.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17-336791)](https://www.postgresql.org/)
[![EF Core](https://img.shields.io/badge/EF_Core-8.0-512BD4)](https://docs.microsoft.com/ef/)
[![MediatR](https://img.shields.io/badge/MediatR-12.4-orange)](https://github.com/jbogard/MediatR)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean-blue)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
[![CQRS](https://img.shields.io/badge/Pattern-CQRS-brightgreen)](https://martinfowler.com/bliki/CQRS.html)
[![JWT](https://img.shields.io/badge/Auth-JWT-black)](https://jwt.io/)
[![xUnit](https://img.shields.io/badge/Tests-xUnit-red)](https://xunit.net/)
[![Swagger](https://img.shields.io/badge/Docs-Swagger-85EA2D)](https://swagger.io/)

## Live Demo

**API Documentation**: [https://iker-finance.onrender.com/swagger](https://iker-finance.onrender.com/swagger)

```
Test Credentials
Email: test@ikerfinance.com
Password: Test@123456
```

> Deployed on Render free tier - initial requests may take 30-60 seconds (cold start).

## Overview

IKER Finance provides a RESTful API for managing multi-currency personal finances, targeting immigrants, foreign students, expatriates, and international travelers.

**Core Features**

- **Multi-Currency Support** - Track transactions in any active currency with automatic conversion to your home currency
- **Smart Budget Management** - Period-based budgets (daily/weekly/monthly/quarterly/yearly) with category-level tracking
- **Customizable Categories** - Use default categories or create custom ones tailored to your spending patterns
- **Historical Accuracy** - Exchange rates captured at transaction time, preserving accurate historical values
- **Real-Time Dashboard** - Spending summaries and budget status calculated on-demand
- **Data Export** - Export transaction history as CSV, PDF, or Excel files
- **Secure Authentication** - JWT-based authentication with role-based access control

## Architecture

```
API Layer
  ├─ Controllers (HTTP endpoints, JWT extraction)
  └─ Middleware (Global exception handling)
         │
Application Layer
  ├─ Command/Query Handlers (Use case orchestration)
  ├─ FluentValidation (Input validation)
  └─ MediatR Pipeline (CQRS orchestration)
         │
Domain Layer
  ├─ Entities (Rich domain models)
  ├─ Domain Services (Complex business logic)
  └─ Business Rules & Invariants
         │
Infrastructure Layer
  ├─ EF Core + PostgreSQL (Data persistence)
  └─ Services (External integrations)
```

**Design Principles**

- **Vertical Slice Architecture** - Features organized by business capability
- **Clean Architecture** - Dependency inversion with separation of concerns
- **CQRS Pattern** - Separate read and write operations
- **Domain-Driven Design** - Rich domain models with encapsulated business logic
- **Test-Driven Development** - Comprehensive test coverage with unit and integration tests

## Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 17](https://www.postgresql.org/download/)

### Installation

1. Clone and restore dependencies

```bash
git clone https://github.com/IKER-Finance/iker-finance-backend.git
cd iker-finance-backend
dotnet restore
```

2. Create PostgreSQL database

```sql
CREATE DATABASE "IkerFinanceDB";
```

3. Configure connection string

Update `src/IkerFinance.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=IkerFinanceDB;Username=postgres;Password=your_password;Port=5432"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-minimum-64-characters-required",
    "Issuer": "IkerFinance",
    "Audience": "IkerFinance-Users"
  }
}
```

4. Apply database migrations

```bash
cd src/IkerFinance.API
dotnet ef database update --project ../IkerFinance.Infrastructure --startup-project .
```

5. Run the application

```bash
dotnet run
```

API available at: `http://localhost:5008` | Swagger UI: `http://localhost:5008/swagger`

## Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/IkerFinance.UnitTests
dotnet test tests/IkerFinance.IntegrationTests
dotnet test tests/IkerFinance.ArchitectureTests
```

## Project Structure

```
src/
├── IkerFinance.API/                       # Presentation layer
│   ├── Controllers/                       # REST endpoints
│   └── Middleware/                        # Global exception handling
│
├── IkerFinance.Application/               # Application layer (CQRS)
│   ├── Common/
│   │   ├── Behaviors/                     # MediatR pipeline
│   │   ├── Exceptions/                    # Custom exceptions
│   │   └── Interfaces/                    # Service contracts
│   │
│   └── Features/                          # Vertical slices
│       └── {FeatureName}/
│           ├── Commands/                  # Write operations
│           │   └── {CommandName}/
│           │       ├── {Command}.cs
│           │       ├── {CommandHandler}.cs
│           │       └── {CommandValidator}.cs
│           └── Queries/                   # Read operations
│               └── {QueryName}/
│                   ├── {Query}.cs
│                   └── {QueryHandler}.cs
│
├── IkerFinance.Domain/                    # Domain layer
│   ├── Common/                            # Base entities
│   ├── Entities/                          # Domain models
│   ├── Enums/                             # Domain types
│   └── Services/                          # Domain services (business logic)
│
├── IkerFinance.Infrastructure/            # Infrastructure layer
│   ├── Data/                              # EF Core DbContext
│   └── Services/                          # Service implementations
│
└── IkerFinance.Shared/                    # Shared contracts
    └── DTOs/                              # Data transfer objects

tests/
├── IkerFinance.UnitTests/                 # Unit tests
├── IkerFinance.IntegrationTests/          # Integration tests
└── IkerFinance.ArchitectureTests/         # Architecture validation
```

## API Documentation

Complete API documentation available via **Swagger UI** at `/swagger` or visit the [live demo](https://iker-finance.onrender.com/swagger).

### Authentication

Obtain JWT token:

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!"
}
```

Use token in subsequent requests:

```http
GET /api/transactions
Authorization: Bearer {your-jwt-token}
```

### Available Resources

- **Authentication** - User registration and login
- **Transactions** - Multi-currency transaction management
- **Budgets** - Budget creation and tracking
- **Categories** - Expense/income categorization
- **Currencies** - Currency and exchange rate information

Refer to Swagger documentation for detailed endpoint specifications and request/response schemas.

## Technology Stack

**Framework & Language**
- .NET 8.0 - Modern cross-platform framework
- C# 12 - Latest language features

**Data & Persistence**
- PostgreSQL 17 - Primary database
- Entity Framework Core 8.0 - ORM with migrations
- ASP.NET Core Identity - User management

**Patterns & Architecture**
- MediatR 12.4.0 - CQRS implementation
- FluentValidation 11.9.0 - Input validation
- Clean Architecture - Layered separation of concerns

**Security & Authentication**
- JWT Bearer Tokens - Stateless authentication
- BCrypt - Password hashing

**Testing**
- xUnit 2.5.3 - Test framework
- FluentAssertions 6.12.0 - Fluent test assertions
- Moq 4.20.70 - Mocking framework
- EF Core InMemory - Test database provider

## Contributing

### Git Workflow

1. Create feature branch from `develop`

```bash
git checkout develop
git pull origin develop
git checkout -b feature/your-feature-name
```

2. Make changes and commit

```bash
git add .
git commit -m "feat: add transaction filtering by date range"
```

3. Push and create Pull Request to `develop`

```bash
git push origin feature/your-feature-name
```

### Branch Naming

- `feature/description` - New features
- `bugfix/description` - Bug fixes
- `refactor/description` - Code refactoring
- `docs/description` - Documentation updates

### Commit Messages

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
feat(transactions): add CSV export functionality
fix(budgets): correct period overlap validation
docs(readme): update installation instructions
```

### Development Guidelines

1. Follow Clean Architecture principles
2. Use CQRS pattern for new features
3. Validate all inputs with FluentValidation
4. Handle exceptions properly (use custom exception types)
5. Return DTOs from handlers, not domain entities
6. Filter by UserId for all user-scoped resources
7. Write comprehensive tests after implementing features

## Related Repositories

**Frontend**: [iker-finance-frontend](https://github.com/IKER-Finance/iker-finance-frontend)

---

**Built with Clean Architecture | Powered by .NET 8 | Secured with JWT**