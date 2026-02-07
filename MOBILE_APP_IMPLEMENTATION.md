# IkerFinance Mobile App Implementation Documentation

## Table of Contents
1. [Overview](#overview)
2. [Architecture Decision](#architecture-decision)
3. [Project Structure](#project-structure)
4. [Implementation Plan](#implementation-plan)
5. [Technical Stack](#technical-stack)
6. [Database Strategy](#database-strategy)
7. [Code Reusability](#code-reusability)
8. [Implementation Steps](#implementation-steps)
9. [File Changes Required](#file-changes-required)
10. [Testing Strategy](#testing-strategy)
11. [Future Enhancements](#future-enhancements)

---

## Overview

### Goal
Create an offline-first mobile application for IkerFinance that:
- Works completely offline without internet connection
- Uses local SQLite database for data storage
- Reuses existing business logic from Domain and Application layers
- Requires no database server or configuration from users
- Supports iOS, Android, Windows, and macOS platforms

### Key Requirements
- ✅ **Offline-first**: Full functionality without internet
- ✅ **Local database**: SQLite embedded in the app
- ✅ **Code reuse**: Leverage existing Domain and Application layers (80-90% reuse)
- ✅ **Clean Architecture**: Maintain separation of concerns
- ✅ **Extensibility**: Easy to add new features and platforms
- ✅ **Industry standards**: Follow Microsoft and enterprise best practices

---

## Architecture Decision

### Pattern: Separate Infrastructure Projects

Based on industry research and Microsoft's official guidance, we will use **separate infrastructure projects** for different consumers (API vs Mobile).

#### Why This Pattern?

**Microsoft's Official Guidance:**
> "Store each database provider's migration in an independent project"
> — [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/providers)

**Industry Examples:**
- **Microsoft eShop**: Each microservice has its own infrastructure
- **ABP Framework**: Separate infrastructure per bounded context
- **Clean Architecture Templates**: Infrastructure per provider when needed

**Benefits:**
1. **Independent Evolution**: Mobile and API can evolve separately
2. **Platform-Specific Optimizations**: SQLite optimizations vs PostgreSQL optimizations
3. **Clean Separation**: No cross-contamination between server and mobile concerns
4. **Team Independence**: Different teams can work on mobile vs backend
5. **Testability**: Easier to test each infrastructure independently
6. **Extensibility**: Easy to add Desktop, Web, or other platforms later

---

## Project Structure

### Current Structure (Before Changes)
```
iker-finance-backend/
├── src/
│   ├── IkerFinance.Domain/           (Shared)
│   ├── IkerFinance.Application/      (Shared)
│   ├── IkerFinance.Infrastructure/   (PostgreSQL)
│   ├── IkerFinance.API/              (Backend API)
│   └── IkerFinance.Shared/
├── tests/
└── IkerFinance.sln
```

### Target Structure (After Implementation)
```
iker-finance-backend/
├── src/
│   ├── IkerFinance.Domain/                    (Shared - Business entities)
│   ├── IkerFinance.Application/               (Shared - Use cases, CQRS)
│   ├── IkerFinance.Infrastructure.API/        (PostgreSQL - Server)
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs        (PostgreSQL DbContext)
│   │   │   └── DbInitializer.cs
│   │   ├── Repositories/                      (Server-optimized)
│   │   │   ├── TransactionRepository.cs
│   │   │   ├── BudgetRepository.cs
│   │   │   ├── ExchangeRateRepository.cs
│   │   │   └── FeedbackRepository.cs
│   │   ├── Services/
│   │   │   ├── CurrencyConversionService.cs
│   │   │   └── TokenService.cs
│   │   ├── Identity/
│   │   │   └── ApplicationUser.cs
│   │   ├── Migrations/                        (PostgreSQL migrations)
│   │   └── DependencyInjection.cs
│   ├── IkerFinance.Infrastructure.Mobile/     (SQLite - Mobile)
│   │   ├── Data/
│   │   │   ├── MobileDbContext.cs            (SQLite DbContext)
│   │   │   └── DbInitializer.cs              (Pre-seed data)
│   │   ├── Repositories/                     (Mobile-optimized)
│   │   │   ├── MobileTransactionRepository.cs
│   │   │   ├── MobileBudgetRepository.cs
│   │   │   ├── MobileExchangeRateRepository.cs
│   │   │   └── MobileFeedbackRepository.cs
│   │   ├── Services/
│   │   │   ├── MobileCurrencyConversionService.cs (Offline-first)
│   │   │   ├── SecureStorageService.cs       (Authentication)
│   │   │   ├── LocalCacheService.cs          (Caching)
│   │   │   └── SyncService.cs                (Optional: Future sync)
│   │   ├── Migrations/                       (SQLite migrations)
│   │   └── DependencyInjection.cs
│   ├── IkerFinance.API/                       (Backend API)
│   ├── IkerFinance.Mobile/                    (MAUI App)
│   │   ├── Platforms/
│   │   │   ├── Android/
│   │   │   ├── iOS/
│   │   │   ├── Windows/
│   │   │   └── MacCatalyst/
│   │   ├── Views/                            (XAML pages)
│   │   │   ├── Dashboard/
│   │   │   ├── Transactions/
│   │   │   ├── Budgets/
│   │   │   └── Settings/
│   │   ├── ViewModels/                       (MVVM pattern)
│   │   │   ├── DashboardViewModel.cs
│   │   │   ├── TransactionsViewModel.cs
│   │   │   └── BudgetsViewModel.cs
│   │   ├── Services/
│   │   │   └── NavigationService.cs
│   │   ├── MauiProgram.cs                    (DI configuration)
│   │   └── App.xaml
│   └── IkerFinance.Shared/
├── tests/
│   ├── IkerFinance.Domain.Tests/
│   ├── IkerFinance.Application.Tests/
│   ├── IkerFinance.Infrastructure.API.Tests/
│   └── IkerFinance.Infrastructure.Mobile.Tests/ (NEW)
└── IkerFinance.sln
```

### Project Dependencies Graph
```
┌─────────────────────────────────────────────────────────────┐
│                      IkerFinance.Domain                      │
│                  (Pure Business Logic)                       │
└─────────────────────────────────────────────────────────────┘
                            ▲
                            │
                            │ depends on
                            │
┌─────────────────────────────────────────────────────────────┐
│                   IkerFinance.Application                    │
│              (CQRS, Use Cases, Interfaces)                   │
└─────────────────────────────────────────────────────────────┘
                            ▲
                            │
                ┌───────────┴───────────┐
                │                       │
                │                       │
┌───────────────────────────┐ ┌───────────────────────────────┐
│ IkerFinance.Infrastructure│ │IkerFinance.Infrastructure     │
│         .API              │ │        .Mobile                │
│    (PostgreSQL)           │ │      (SQLite)                 │
└───────────────────────────┘ └───────────────────────────────┘
                │                       │
                │                       │
                ▼                       ▼
┌───────────────────────────┐ ┌───────────────────────────────┐
│    IkerFinance.API        │ │   IkerFinance.Mobile          │
│   (REST API Server)       │ │    (MAUI App)                 │
└───────────────────────────┘ └───────────────────────────────┘
```

---

## Implementation Plan

### Phase 1: Project Restructuring
**Goal:** Rename and organize projects for clarity

**Tasks:**
1. Rename `IkerFinance.Infrastructure` → `IkerFinance.Infrastructure.API`
2. Create new `IkerFinance.Infrastructure.Mobile` project
3. Create new `IkerFinance.Mobile` MAUI project
4. Update solution file with new projects
5. Update all project references
6. Update namespaces across affected files

**Files to Update:**
- Solution file: `IkerFinance.sln`
- API project: `IkerFinance.API/IkerFinance.API.csproj`
- All files in Infrastructure.API: namespace changes
- Test projects: update references

---

### Phase 2: Infrastructure.Mobile Implementation
**Goal:** Create SQLite-based infrastructure for mobile

#### 2.1 Create MobileDbContext

**File:** `IkerFinance.Infrastructure.Mobile/Data/MobileDbContext.cs`

**Purpose:** SQLite-based DbContext implementing `IApplicationDbContext`

**Key Features:**
- Uses SQLite provider (`Microsoft.EntityFrameworkCore.Sqlite`)
- Same entity configurations as API (reused from Infrastructure.API or shared)
- Simplified Identity (no ASP.NET Core Identity, custom user model)
- File-based database path (stored in app data directory)

**Configuration Differences from API:**
- Remove ASP.NET Core Identity dependencies
- Adjust filtered indexes for SQLite syntax
- Use SQLite data types (INTEGER for booleans, TEXT for dates)
- Configure file path for database location

#### 2.2 Implement Mobile Repositories

**Files:**
- `MobileTransactionRepository.cs`
- `MobileBudgetRepository.cs`
- `MobileExchangeRateRepository.cs`
- `MobileFeedbackRepository.cs`

**Strategy:**
- Implement same interfaces from Application layer
- Reuse LINQ queries from API repositories (they're provider-agnostic!)
- Add mobile-specific optimizations (indexing, caching)
- Consider lazy loading strategies for performance

**Analysis Result:** Current API repositories use pure EF Core LINQ with zero provider-specific code, so they can be copied and adapted easily.

#### 2.3 Implement Mobile Services

**MobileCurrencyConversionService** (`Services/MobileCurrencyConversionService.cs`)
- Implements `ICurrencyConversionService` from Application layer
- Offline-first: Uses pre-seeded exchange rates
- Fallback strategy: Use last known rates
- Optional: Sync rates when online (future enhancement)

**SecureStorageService** (`Services/SecureStorageService.cs`)
- Platform-specific secure storage (iOS Keychain, Android KeyStore)
- Store user credentials, biometric auth tokens
- Replace JWT authentication with local secure storage

**LocalCacheService** (`Services/LocalCacheService.cs`)
- In-memory caching for frequently accessed data
- Reduce database queries
- Improve app performance

**SyncService** (`Services/SyncService.cs`) - Optional for future
- Sync local data with backend API
- Conflict resolution strategies
- Background sync when online

#### 2.4 Data Seeding

**File:** `Data/DbInitializer.cs`

**Pre-seed Data:**
1. **Categories** (System categories from API)
   - Income categories: Salary, Freelance, Investments, etc.
   - Expense categories: Food, Transport, Entertainment, etc.

2. **Currencies** (Major world currencies)
   - USD, EUR, GBP, JPY, CNY, INR, etc.
   - With proper symbols and decimal places

3. **Exchange Rates** (Initial rates)
   - Common currency pairs
   - Base date for rates
   - Can be updated later

4. **Demo User** (Optional for testing)
   - Pre-configured user with sample data
   - Sample transactions and budgets

#### 2.5 SQLite Migrations

**Location:** `IkerFinance.Infrastructure.Mobile/Migrations/`

**Process:**
1. Configure MobileDbContext
2. Run: `dotnet ef migrations add InitialCreate --project Infrastructure.Mobile`
3. Migrations will be SQLite-specific
4. Apply on app first launch

---

### Phase 3: MAUI Application Setup
**Goal:** Create mobile UI that uses existing business logic

#### 3.1 Project Configuration

**File:** `IkerFinance.Mobile/MauiProgram.cs`

**Configure:**
- Dependency injection (register services, ViewModels, Views)
- Register MobileDbContext with SQLite
- Register MediatR (same as API)
- Register FluentValidation (same as API)
- Register Infrastructure.Mobile services
- Configure navigation

#### 3.2 ViewModels (MVVM Pattern)

**Base ViewModel:** `ViewModels/BaseViewModel.cs`
- INotifyPropertyChanged implementation
- Busy indicator
- Error handling
- Navigation helpers

**Feature ViewModels:**

**DashboardViewModel** (`ViewModels/DashboardViewModel.cs`)
- Uses `GetBudgetSummaryQuery` (from Application layer)
- Uses `GetTransactionSummaryQuery` (from Application layer)
- Displays overview of finances

**TransactionsViewModel** (`ViewModels/TransactionsViewModel.cs`)
- Uses `GetTransactionsQuery` (from Application layer)
- Uses `CreateTransactionCommand` (from Application layer)
- Uses `UpdateTransactionCommand` (from Application layer)
- Uses `DeleteTransactionCommand` (from Application layer)
- List, create, edit, delete transactions

**BudgetsViewModel** (`ViewModels/BudgetsViewModel.cs`)
- Uses `GetBudgetsQuery` (from Application layer)
- Uses `CreateBudgetCommand` (from Application layer)
- Uses `GetBudgetSummaryQuery` (from Application layer)
- Manage budgets, view spending

**Key Point:** ViewModels use MediatR to send commands/queries - **zero business logic in ViewModels!** All logic is in Application layer.

#### 3.3 Views (XAML Pages)

**Shell Navigation:** `AppShell.xaml`
- Tab-based navigation
- Dashboard, Transactions, Budgets, Settings tabs

**Dashboard Page:** `Views/Dashboard/DashboardPage.xaml`
- Financial overview
- Budget progress bars
- Recent transactions

**Transactions Page:** `Views/Transactions/TransactionsPage.xaml`
- Transaction list with filters
- Add new transaction button
- Edit/delete actions

**Transaction Detail Page:** `Views/Transactions/TransactionDetailPage.xaml`
- Create/edit transaction form
- Category picker
- Currency selector
- Date picker

**Budgets Page:** `Views/Budgets/BudgetsPage.xaml`
- Budget list
- Progress indicators
- Alerts for over-budget

**Budget Detail Page:** `Views/Budgets/BudgetDetailPage.xaml`
- Create/edit budget form
- Period selector
- Alert threshold configuration

---

### Phase 4: Testing & Validation
**Goal:** Ensure everything works offline

#### 4.1 Unit Tests

**Infrastructure.Mobile Tests:**
- Test MobileDbContext with in-memory SQLite
- Test repositories with test data
- Test service implementations

#### 4.2 Integration Tests

- Test CQRS handlers with SQLite
- Test data seeding
- Test migrations

#### 4.3 Mobile App Testing

- Test on iOS simulator
- Test on Android emulator
- Test offline functionality
- Test data persistence
- Test performance

---

## Technical Stack

### Shared Layers (Reused from Backend)
- **.NET 8.0** (Domain, Application)
- **MediatR 12.4.1** - CQRS pattern
- **FluentValidation 11.10.0** - Input validation
- **AutoMapper 13.0.1** - Object mapping

### Mobile-Specific Technology
- **.NET 9.0** - Latest MAUI requires .NET 9
- **.NET MAUI** - Cross-platform UI framework
- **SQLite** - Local database
  - `Microsoft.EntityFrameworkCore.Sqlite 9.0.0`
  - `sqlite-net-pcl 1.9.172` (Optional: For direct SQLite access)

### Platforms Supported
- iOS 15.0+
- Android 7.0+ (API 24+)
- Windows 10.0.17763.0+
- macOS 10.15+

### UI Framework
- **XAML** - UI markup language
- **MVVM Pattern** - Model-View-ViewModel
- **CommunityToolkit.Mvvm** - MVVM helpers

---

## Database Strategy

### API (Server) Database
- **Database:** PostgreSQL 16
- **Provider:** Npgsql.EntityFrameworkCore.PostgreSQL 8.0.11
- **Context:** `ApplicationDbContext` (in Infrastructure.API)
- **Migrations:** PostgreSQL-specific migrations
- **Connection:** Connection string to PostgreSQL server

### Mobile (Local) Database
- **Database:** SQLite
- **Provider:** Microsoft.EntityFrameworkCore.Sqlite 9.0.0
- **Context:** `MobileDbContext` (in Infrastructure.Mobile)
- **Migrations:** SQLite-specific migrations
- **Location:** App data directory (platform-specific)
  - iOS: `~/Library/Application Support/`
  - Android: `/data/data/{package}/files/`
  - Windows: `%LOCALAPPDATA%`
  - macOS: `~/Library/Application Support/`

### Database File Location Strategy

```csharp
// In MobileDbContext or service
public static string GetDatabasePath()
{
    var appDataPath = FileSystem.AppDataDirectory;
    return Path.Combine(appDataPath, "ikerfinance.db");
}
```

### Data Synchronization (Future Enhancement)
- Currently: Fully offline, no sync
- Future: Optional sync with backend API
  - Conflict resolution: Last-write-wins or custom strategy
  - Sync trigger: Manual or automatic when online
  - Delta sync: Only changed records

---

## Code Reusability

### 100% Reusable (No Changes)
✅ **Domain Layer** - `IkerFinance.Domain`
- All entities: Transaction, Budget, Category, Currency, ExchangeRate, Feedback
- Domain services: TransactionService, BudgetService, BudgetCalculator
- Enums: TransactionType, BudgetPeriod, FeedbackType, etc.
- **No changes needed!**

✅ **Application Layer - Use Cases** - `IkerFinance.Application`
- All CQRS commands and queries
- MediatR handlers
- FluentValidation validators
- DTOs
- **Minimal changes (5-10%):**
  - Remove API-specific concerns (HttpContext if any)
  - Add mobile-specific queries if needed

### 90% Reusable (Minor Adaptations)
⚠️ **Application Interfaces**
- `IApplicationDbContext` - Used as-is
- Repository interfaces - Used as-is
- Service interfaces - Used as-is
- **Changes:** None to interfaces, only implementations

### 60% Reusable (Adapted Implementations)
⚠️ **Repository Implementations**
- Logic and LINQ queries can be reused
- **Analysis shows:** All repositories use provider-agnostic EF Core LINQ
- **Changes:**
  - Copy implementations to Infrastructure.Mobile
  - Rename classes (e.g., `TransactionRepository` → `MobileTransactionRepository`)
  - Adjust for SQLite performance if needed
  - Add mobile-specific optimizations

### 50% Reusable (Significant Changes)
⚠️ **Service Implementations**
- `CurrencyConversionService` → `MobileCurrencyConversionService`
  - Adapt for offline-first (pre-seeded rates)
- Authentication → `SecureStorageService`
  - Replace JWT with secure local storage

### 0% Reusable (New Code)
❌ **Mobile UI Layer** - Completely new
- MAUI Views (XAML)
- ViewModels
- Navigation
- Platform-specific code

### Reusability Summary
| Layer | Reusability | Lines of Code | Effort |
|-------|-------------|---------------|--------|
| Domain | 100% | ~1,500 | None |
| Application | 90% | ~3,000 | Minimal |
| Repositories | 60% | ~800 | Copy + adapt |
| Services | 50% | ~400 | Adapt logic |
| UI | 0% | ~2,000 | New code |
| **Total** | **~80%** | **~7,700** | **Medium** |

**Estimated new code:** ~2,500-3,000 lines (UI + mobile infrastructure)

---

## Implementation Steps

### Step 1: Rename Infrastructure Project
**Estimated Time:** 30 minutes

1. Rename folder: `IkerFinance.Infrastructure` → `IkerFinance.Infrastructure.API`
2. Rename project file: `IkerFinance.Infrastructure.csproj` → `IkerFinance.Infrastructure.API.csproj`
3. Update solution file: Replace project references
4. Update `IkerFinance.API.csproj`: Update `<ProjectReference>`
5. Update all namespaces in Infrastructure.API files:
   - Change `namespace IkerFinance.Infrastructure` → `namespace IkerFinance.Infrastructure.API`
6. Update using statements across the solution
7. Test build: `dotnet build`

### Step 2: Create Infrastructure.Mobile Project
**Estimated Time:** 1 hour

1. Create class library: `dotnet new classlib -n IkerFinance.Infrastructure.Mobile -f net9.0`
2. Add to solution: `dotnet sln add src/IkerFinance.Infrastructure.Mobile`
3. Add project references:
   ```bash
   dotnet add Infrastructure.Mobile reference Domain
   dotnet add Infrastructure.Mobile reference Application
   ```
4. Add NuGet packages:
   ```bash
   dotnet add Infrastructure.Mobile package Microsoft.EntityFrameworkCore.Sqlite
   dotnet add Infrastructure.Mobile package Microsoft.EntityFrameworkCore.Design
   ```
5. Create folder structure:
   - Data/
   - Repositories/
   - Services/
   - Migrations/

### Step 3: Implement MobileDbContext
**Estimated Time:** 2 hours

1. Create `Data/MobileDbContext.cs`
2. Implement `IApplicationDbContext` interface
3. Add all DbSets (same as API)
4. Copy entity configurations from Infrastructure.API
5. Adjust for SQLite:
   - Fix filtered indexes (PostgreSQL → SQLite syntax)
   - Configure database path
6. Create `Data/DbInitializer.cs` for seeding
7. Test with unit tests

### Step 4: Implement Mobile Repositories
**Estimated Time:** 2 hours

1. Copy repository classes from Infrastructure.API
2. Rename to Mobile* prefix
3. Update namespace to `IkerFinance.Infrastructure.Mobile.Repositories`
4. Adjust constructors to use `MobileDbContext`
5. Keep LINQ queries as-is (they're provider-agnostic)
6. Add any mobile-specific optimizations
7. Test with unit tests

### Step 5: Implement Mobile Services
**Estimated Time:** 3 hours

1. **MobileCurrencyConversionService:**
   - Implement `ICurrencyConversionService`
   - Use local exchange rate data
   - Add fallback logic

2. **SecureStorageService:**
   - Platform-specific secure storage wrapper
   - Store user credentials
   - Biometric authentication support

3. **LocalCacheService:**
   - In-memory caching
   - Cache expiration strategies

4. Create `DependencyInjection.cs` to register services

### Step 6: Create SQLite Migrations
**Estimated Time:** 1 hour

1. Configure EF Core tools
2. Run: `dotnet ef migrations add InitialCreate --project Infrastructure.Mobile --startup-project Mobile`
3. Verify migration files
4. Test migration application

### Step 7: Create MAUI Project
**Estimated Time:** 1 hour

1. Create MAUI project: `dotnet new maui -n IkerFinance.Mobile -f net9.0`
2. Add to solution
3. Add project references:
   - Domain
   - Application
   - Infrastructure.Mobile
4. Add NuGet packages:
   - CommunityToolkit.Mvvm
   - CommunityToolkit.Maui
   - MediatR
5. Configure `MauiProgram.cs` with DI

### Step 8: Implement ViewModels
**Estimated Time:** 4 hours

1. Create `ViewModels/BaseViewModel.cs`
2. Create feature ViewModels:
   - DashboardViewModel
   - TransactionsViewModel
   - BudgetsViewModel
3. Inject MediatR
4. Send commands/queries to Application layer
5. Implement INotifyPropertyChanged
6. Add validation

### Step 9: Implement Views
**Estimated Time:** 6 hours

1. Design `AppShell.xaml` (navigation)
2. Create Dashboard page
3. Create Transactions list and detail pages
4. Create Budgets list and detail pages
5. Create Settings page
6. Style with XAML
7. Bind to ViewModels

### Step 10: Data Seeding & First Launch
**Estimated Time:** 2 hours

1. Implement first-launch detection
2. Apply migrations on first run
3. Seed initial data:
   - System categories
   - Currencies
   - Exchange rates
4. Create demo user (optional)

### Step 11: Testing
**Estimated Time:** 4 hours

1. Unit tests for Infrastructure.Mobile
2. Integration tests for CQRS with SQLite
3. Manual testing on simulators/emulators
4. Test offline functionality
5. Test data persistence
6. Performance testing

### Step 12: Documentation & Cleanup
**Estimated Time:** 2 hours

1. Update README.md
2. Add mobile-specific documentation
3. Create user guide
4. Add code comments
5. Clean up unused code

**Total Estimated Time:** 28-30 hours

---

## File Changes Required

### New Files (To Be Created)

#### Infrastructure.Mobile (19 files)
```
IkerFinance.Infrastructure.Mobile/
├── IkerFinance.Infrastructure.Mobile.csproj
├── Data/
│   ├── MobileDbContext.cs
│   └── DbInitializer.cs
├── Repositories/
│   ├── MobileTransactionRepository.cs
│   ├── MobileBudgetRepository.cs
│   ├── MobileExchangeRateRepository.cs
│   └── MobileFeedbackRepository.cs
├── Services/
│   ├── MobileCurrencyConversionService.cs
│   ├── SecureStorageService.cs
│   ├── LocalCacheService.cs
│   └── SyncService.cs (future)
├── DependencyInjection.cs
└── Migrations/
    └── (Generated migration files)
```

#### Mobile MAUI App (25+ files)
```
IkerFinance.Mobile/
├── IkerFinance.Mobile.csproj
├── MauiProgram.cs
├── App.xaml
├── App.xaml.cs
├── AppShell.xaml
├── AppShell.xaml.cs
├── ViewModels/
│   ├── BaseViewModel.cs
│   ├── DashboardViewModel.cs
│   ├── TransactionsViewModel.cs
│   ├── TransactionDetailViewModel.cs
│   ├── BudgetsViewModel.cs
│   ├── BudgetDetailViewModel.cs
│   └── SettingsViewModel.cs
├── Views/
│   ├── Dashboard/
│   │   ├── DashboardPage.xaml
│   │   └── DashboardPage.xaml.cs
│   ├── Transactions/
│   │   ├── TransactionsPage.xaml
│   │   ├── TransactionsPage.xaml.cs
│   │   ├── TransactionDetailPage.xaml
│   │   └── TransactionDetailPage.xaml.cs
│   ├── Budgets/
│   │   ├── BudgetsPage.xaml
│   │   ├── BudgetsPage.xaml.cs
│   │   ├── BudgetDetailPage.xaml
│   │   └── BudgetDetailPage.xaml.cs
│   └── Settings/
│       ├── SettingsPage.xaml
│       └── SettingsPage.xaml.cs
├── Services/
│   └── NavigationService.cs
└── Resources/
    └── (Images, fonts, styles)
```

### Files to Modify

#### Rename Operations
1. **Folder rename:**
   - `src/IkerFinance.Infrastructure/` → `src/IkerFinance.Infrastructure.API/`

2. **Project file rename:**
   - `IkerFinance.Infrastructure.csproj` → `IkerFinance.Infrastructure.API.csproj`

#### Namespace Changes (All files in Infrastructure.API)
**Estimated:** ~40 files

Files that need namespace update from `IkerFinance.Infrastructure` to `IkerFinance.Infrastructure.API`:
- All `.cs` files in Data/ (3 files)
- All `.cs` files in Repositories/ (4 files)
- All `.cs` files in Services/ (2 files)
- All `.cs` files in Identity/ (1 file)
- All `.cs` files in Migrations/ (~30 files)
- DependencyInjection.cs (1 file)

#### Project Reference Updates
1. **IkerFinance.API.csproj:**
   ```xml
   <!-- OLD -->
   <ProjectReference Include="..\IkerFinance.Infrastructure\IkerFinance.Infrastructure.csproj" />

   <!-- NEW -->
   <ProjectReference Include="..\IkerFinance.Infrastructure.API\IkerFinance.Infrastructure.API.csproj" />
   ```

2. **IkerFinance.sln:**
   - Update project path for Infrastructure.API
   - Add Infrastructure.Mobile project
   - Add Mobile project

3. **Test projects:**
   - Update references to Infrastructure.API

#### Using Statement Updates
Files that import Infrastructure namespace:
- `IkerFinance.API/Program.cs`
- Test files in `tests/IkerFinance.Application.Tests/`

### Files That Stay Unchanged
✅ All files in `IkerFinance.Domain/` (0 changes)
✅ Most files in `IkerFinance.Application/` (minimal changes)
✅ Test files for Domain and most Application tests

---

## Testing Strategy

### Unit Tests

#### Domain Tests (Existing - No Changes)
- Entity validation tests
- Domain service tests
- Business rule tests

#### Application Tests (Existing - Minimal Changes)
- Command handler tests
- Query handler tests
- Validation tests
- May need to add tests for mobile-specific queries

#### Infrastructure.Mobile Tests (New)
**Location:** `tests/IkerFinance.Infrastructure.Mobile.Tests/`

**Test Categories:**
1. **DbContext Tests:**
   - Test MobileDbContext creation
   - Test entity configurations
   - Test migrations application

2. **Repository Tests:**
   - Test CRUD operations
   - Test filtering and pagination
   - Test with in-memory SQLite

3. **Service Tests:**
   - Test MobileCurrencyConversionService
   - Test SecureStorageService
   - Test data seeding

### Integration Tests

**Test Scenarios:**
1. CQRS handlers with SQLite database
2. End-to-end transaction creation flow
3. Budget calculation with real data
4. Data seeding and initialization

### Mobile App Tests

#### Manual Testing Checklist
- [ ] App launches successfully on iOS
- [ ] App launches successfully on Android
- [ ] Database is created on first launch
- [ ] Initial data is seeded correctly
- [ ] Can create transactions offline
- [ ] Can create budgets offline
- [ ] Can view dashboard
- [ ] Data persists after app restart
- [ ] App works without internet connection
- [ ] Currency conversion works offline
- [ ] Performance is acceptable

#### Automated UI Tests (Optional)
- Use Appium or MAUI UI Testing
- Test critical user flows
- Screenshot testing

---

## Future Enhancements

### Phase 1 (Current Scope)
✅ Offline-first mobile app
✅ Local SQLite database
✅ All core features (transactions, budgets)
✅ iOS, Android, Windows, macOS support

### Phase 2 (Future)
🔲 **Cloud Sync**
   - Sync local data with backend API
   - Conflict resolution
   - Background sync

🔲 **Biometric Authentication**
   - Face ID, Touch ID (iOS)
   - Fingerprint, Face Unlock (Android)

🔲 **Data Export**
   - Export to CSV, PDF
   - Share reports

🔲 **Charts & Visualizations**
   - Spending trends
   - Budget vs actual charts
   - Category breakdowns

### Phase 3 (Advanced Features)
🔲 **Recurring Transactions**
   - Automatic transaction creation
   - Subscription tracking

🔲 **Multi-Device Sync**
   - Use backend as sync server
   - Real-time updates

🔲 **Dark Mode**
   - Theme switching
   - System theme detection

🔲 **Localization**
   - Multi-language support
   - Right-to-left (RTL) languages

🔲 **Notifications**
   - Budget alerts
   - Bill reminders
   - Push notifications

### Phase 4 (Enterprise Features)
🔲 **Shared Budgets**
   - Family/team budgets
   - Collaborative features

🔲 **Receipt Scanning**
   - OCR for receipts
   - Auto-fill transaction details

🔲 **Bank Integration**
   - Connect to bank accounts
   - Auto-import transactions

🔲 **Investment Tracking**
   - Portfolio management
   - Stock/crypto tracking

---

## Architecture Principles

### Clean Architecture Compliance
✅ **Dependency Rule:** Dependencies point inward (UI → Infrastructure → Application → Domain)
✅ **Interface Segregation:** Small, focused interfaces
✅ **Dependency Inversion:** Depend on abstractions, not concretions
✅ **Single Responsibility:** Each class has one reason to change

### SOLID Principles
✅ **Single Responsibility:** Each class does one thing
✅ **Open/Closed:** Open for extension, closed for modification
✅ **Liskov Substitution:** Implementations are substitutable
✅ **Interface Segregation:** Focused interfaces
✅ **Dependency Inversion:** Depend on abstractions

### Design Patterns Used
- **CQRS:** Command Query Responsibility Segregation
- **Repository:** Data access abstraction
- **Unit of Work:** DbContext as UoW
- **Mediator:** MediatR for CQRS
- **MVVM:** Model-View-ViewModel for UI
- **Dependency Injection:** Service registration
- **Strategy:** Multiple database providers

---

## Technology Decisions

### Why .NET MAUI?
✅ Cross-platform (iOS, Android, Windows, macOS)
✅ Native performance
✅ C# and XAML (familiar to .NET developers)
✅ Reuse business logic
✅ Single codebase for all platforms
✅ Official Microsoft support
✅ Good community and ecosystem

### Why SQLite?
✅ Embedded database (no server needed)
✅ Zero configuration
✅ File-based (single .db file)
✅ Fast for mobile
✅ Built into iOS and Android
✅ EF Core support
✅ Industry standard for mobile

### Why Separate Infrastructure Projects?
✅ Microsoft's official recommendation
✅ Independent evolution
✅ Platform-specific optimizations
✅ Clean separation of concerns
✅ Easier testing
✅ Better extensibility

### Why MVVM Pattern?
✅ Standard pattern for XAML-based apps
✅ Separation of UI and logic
✅ Testable ViewModels
✅ Data binding support
✅ Community standard

---

## Success Criteria

### Functional Requirements
✅ App works completely offline
✅ All CRUD operations for transactions work
✅ All CRUD operations for budgets work
✅ Budget calculations are accurate
✅ Currency conversion works offline
✅ Data persists between app sessions
✅ Multi-platform support (iOS, Android minimum)

### Non-Functional Requirements
✅ App launch time < 3 seconds
✅ Transaction list loads < 1 second
✅ Database size < 50MB for typical usage
✅ Memory usage < 100MB
✅ Battery efficient (no constant background work)
✅ Smooth UI (60 FPS)

### Code Quality Requirements
✅ 80%+ code reuse from backend
✅ Unit test coverage > 70%
✅ Zero critical bugs
✅ Clean code (follows SOLID principles)
✅ Proper documentation
✅ No hardcoded strings (use resources)

---

## Risk Assessment

### Technical Risks

**Risk 1: SQLite Performance on Large Datasets**
- **Likelihood:** Medium
- **Impact:** Medium
- **Mitigation:**
  - Implement pagination
  - Add proper indexes
  - Use lazy loading
  - Archive old transactions

**Risk 2: .NET MAUI Maturity Issues**
- **Likelihood:** Medium (MAUI is relatively new)
- **Impact:** High
- **Mitigation:**
  - Use stable NuGet packages
  - Test thoroughly on target platforms
  - Have fallback to Xamarin if needed

**Risk 3: Migration Compatibility**
- **Likelihood:** Low
- **Impact:** High
- **Mitigation:**
  - Test migrations extensively
  - Backup database before migrations
  - Version migration files

**Risk 4: Platform-Specific Issues**
- **Likelihood:** Medium
- **Impact:** Medium
- **Mitigation:**
  - Test on real devices
  - Use platform-specific code when needed
  - Follow platform guidelines

### Project Risks

**Risk 5: Scope Creep**
- **Likelihood:** High
- **Impact:** Medium
- **Mitigation:**
  - Stick to MVP features
  - Document future enhancements separately
  - Use phased approach

**Risk 6: Underestimated Complexity**
- **Likelihood:** Medium
- **Impact:** Medium
- **Mitigation:**
  - Break work into small tasks
  - Regular progress checks
  - Buffer time in estimates

---

## Conclusion

This implementation plan provides a comprehensive roadmap for creating an offline-first mobile application that reuses 80-90% of existing business logic from the IkerFinance backend. By following Clean Architecture principles and industry best practices, we ensure:

1. **Maximum code reuse** - Domain and Application layers work unchanged
2. **Clean separation** - Infrastructure.API and Infrastructure.Mobile are independent
3. **Extensibility** - Easy to add new platforms or features
4. **Industry standards** - Following Microsoft and enterprise patterns
5. **Testability** - Proper abstractions enable comprehensive testing

The phased approach allows for incremental delivery and validation, reducing risk and ensuring quality at each step.

**Next Steps:**
1. Get approval for this plan
2. Start with Phase 1 (Project Restructuring)
3. Build incrementally
4. Test continuously
5. Document as we go

---

## References

- [Microsoft .NET MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
- [Entity Framework Core SQLite Provider](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/)
- [Clean Architecture by Jason Taylor](https://github.com/jasontaylordev/CleanArchitecture)
- [Microsoft eShop Reference Application](https://github.com/dotnet/eShop)
- [MVVM Community Toolkit](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)

---

**Document Version:** 1.0
**Date:** November 3, 2025
**Author:** Development Team
**Status:** Ready for Review
