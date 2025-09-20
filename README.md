# IKER Finance - Backend API

A multi-currency personal finance management system built with .NET 8, Clean Architecture, and CQRS patterns.

## Prerequisites

### All Platforms
- .NET 8 SDK
- Git
- Code editor (Visual Studio Code recommended)

### Database
- PostgreSQL 15

## Setup Instructions

### 1. Clone and Setup Project

```bash
# Clone the repository
git clone https://github.com/IKER-Finance/iker-finance-backend.git
cd iker-finance-backend

# Verify .NET installation
dotnet --version

# Restore dependencies
dotnet restore

# Build the solution
dotnet build
```

### 2. PostgreSQL Database Setup

#### macOS

**Option A: Using Postgres.app (Recommended)**
1. Download and install [Postgres.app](https://postgresapp.com/)
2. Start the application
3. Click "Initialize" to create default database server
4. Database runs on default port 5432

**Option B: Using Homebrew**
```bash
brew install postgresql@15
brew services start postgresql@15
```

**Create Database:**
```bash
# Connect to PostgreSQL
psql postgres

# Create database
CREATE DATABASE "IkerFinanceDB";

# Create user (optional)
CREATE USER iker_user WITH PASSWORD 'your_password';
GRANT ALL PRIVILEGES ON DATABASE "IkerFinanceDB" TO iker_user;

# Exit
\q
```

#### Windows

**Option A: PostgreSQL Installer (Recommended)**
1. Download PostgreSQL installer from [postgresql.org](https://www.postgresql.org/download/windows/)
2. Run installer and follow setup wizard
3. Remember the password you set for postgres user
4. Default port is 5432

**Option B: Using Chocolatey**
```bash
choco install postgresql
```

**Create Database:**
```cmd
# Open Command Prompt or PowerShell
# Connect to PostgreSQL
psql -U postgres

# Create database
CREATE DATABASE "IkerFinanceDB";

# Exit
\q
```

### 3. Configure Application

Create `appsettings.Development.json` in `src/IkerFinance.API/`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=IkerFinanceDB;Username=postgres;Password=your_password;Port=5432"
  },
  "Jwt": {
    "SecretKey": "YourVeryLongAndSecureSecretKeyThatIsAtLeast32CharactersLong",
    "Issuer": "IkerFinance",
    "Audience": "IkerFinance.Users",
    "ExpiryInHours": 24
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### 4. Run Database Migrations

```bash
# Navigate to API project
cd src/IkerFinance.API

# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Create and apply initial migration
dotnet ef migrations add InitialCreate --project ../IkerFinance.Infrastructure
dotnet ef database update --project ../IkerFinance.Infrastructure
```

### 5. Start the Application

```bash
# From the API project directory
dotnet run

# Or from root directory
dotnet run --project src/IkerFinance.API
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `http://localhost:5000/swagger`

## Project Structure

```
src/
├── IkerFinance.API/              # Web API controllers and configuration
├── IkerFinance.Application/      # Business logic and CQRS handlers
├── IkerFinance.Domain/           # Domain entities and business rules
├── IkerFinance.Infrastructure/   # Data access and external services
└── IkerFinance.Shared/           # Shared DTOs and utilities

tests/
├── IkerFinance.UnitTests/        # Unit tests
├── IkerFinance.IntegrationTests/ # Integration tests
└── IkerFinance.ArchitectureTests/ # Architecture validation tests
```

## Development Commands

```bash
# Build solution
dotnet build

# Run tests
dotnet test

# Run specific project
dotnet run --project src/IkerFinance.API

# Create new migration
dotnet ef migrations add MigrationName --project src/IkerFinance.Infrastructure

# Update database
dotnet ef database update --project src/IkerFinance.Infrastructure

# Remove last migration (if not applied to database)
dotnet ef migrations remove --project src/IkerFinance.Infrastructure
```

## Architecture

- **Clean Architecture** with dependency inversion
- **CQRS** pattern with MediatR
- **Role-based authentication** (Admin/User roles)  
- **Multi-currency** transaction support
- **PostgreSQL** database with Entity Framework Core
- **Comprehensive testing** strategy