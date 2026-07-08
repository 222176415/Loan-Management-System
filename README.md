# Loan Management System (LMS) API

A production-grade, high-concurrency, multi-tenant financial engine built with **.NET 9**. This backend controls the lifecycle of short-term loans, automated interest/VAT snapshotting, multi-tier audit records, and live system logging.

---

## 🏗️ Architecture Design (Clean Architecture)

The system is decoupled into four explicit layout layers to protect domain integrity and enforce the separation of concerns:
- **`Domain`**: Independent layer holding system core objects (POCOs), Domain Enums (`LoanStatus`), and structural entity models.
- **`Application`**: Contains structural Data Transfer Objects (DTOs), processing contracts (`IAuditService`, `IExcelService`), and security engines.
- **`Infrastructure`**: Holds concrete framework implementations (`ApplicationDbContext`), multi-tenant token extractors (`CurrentTenantService`), real-time pipelines, background tasks (`ChronosWorker`), and reporting dependencies.
- **`Api`**: Web interface, request controllers, middleware processing pipelines, and explicit OpenApi/Scalar mappings.

---

## 🛠️ Core Technology Stack

- **Framework**: .NET 9.0 (Web API)
- **Database Engine**: Microsoft SQL Server
- **ORM**: Entity Framework Core 9.0
- **Real-Time Transmission**: ASP.NET Core SignalR (WebSockets)
- **Cryptographic Hashing**: BCrypt.Net-Next
- **Reporting Engine**: ClosedXML (OpenXML wrapper for Excel generation)
- **Interactive API Docs**: Microsoft.AspNetCore.OpenApi + Scalar API
- **Containerization Engine**: Docker & Docker Compose

---

## ⚡ Key System Rules & Principles

1. **Multi-Tenancy (Data Isolation):** Automated tenant filtering via EF Core `HasQueryFilter` tied to the current user's authenticated `OrganizationId` claim. Cross-tenant data leaks are structurally blocked at the database execution layer.
2. **Financial Snapshotting:** Interest and VAT values are permanently copied from the `Organization` row settings into the `Loan` row at the exact moment of contract creation to insulate historical ledger records against future configuration adjustments.
3. **Soft Delete Integration:** Data records are never physically stripped from the persistent storage. Rows are updated with an `IsDeleted = true` flag, stamped with the admin tracking ID (`DeletedById`), and automatically ignored by active query streams.
4. **Descriptive Auditing:** The `IAuditService` records explicit "Before" and "After" state data variations into the database rather than system codes, ensuring human-readable, compliant ledger reviews.

---

## 🏁 Getting Started

### 📋 Prerequisites
- [.NET 9.0 SDK](https://microsoft.com)
- [Docker Desktop](https://docker.com) or local Microsoft SQL Server instance
- Modern Terminal (PowerShell / Bash)

### 💾 Local Configuration
Update the configuration variables in `LoanManagementSystem.Api/appsettings.json` to link to your runtime database instance:

```json
{
  "ConnectionStrings": {
    "DbConnection": "Server=YOUR_SERVER;Database=LMSDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "PCuLYf7bWtAOvXejfHO5IuIhmWCMyzr7Bz3RKHuYf1d0VChhRIAbSvksszyuJx6H+VHxwp2PosDOgVF2uv3YXg==",
    "Issuer": "LoanManagementSystem",
    "Audience": "LoanManagementSystemUsers",
    "DurationInMinutes": 60
  }
}
```

### ⌨️ CLI Commands Execution

From the **root folder** containing your `.sln` file:

**1. Restore dependencies and verify compilation:**
```bash
dotnet restore
dotnet build
```

**2. Generate a new database schema migration (if updates are introduced):**
```bash
dotnet ef migrations add NameOfYourMigration --project LoanManagementSystem.Infrastructure --startup-project LoanManagementSystem.Api
```

**3. Directly apply database updates:**
```bash
dotnet ef database update --project LoanManagementSystem.Infrastructure --startup-project LoanManagementSystem.Api
```

---

## 🐳 Docker Deployment

The platform is configured with an automated multi-stage build script that orchestrates both the .NET API application and a fresh SQL Server container workspace instantly.

To spin up the entire backend container environment, run the following command inside the root folder:

```bash
docker-compose up --build
```

- **Scalar API Interactive Panel**: Navigate to `http://localhost:5000/scalar/v1` inside your browser once compilation completes.
- **Raw Document Endpoint**: Extract your schema mapping data straight from `http://localhost:5000/openapi/v1.json`.

---

## 📡 API Endpoint Index
##NOTE!!!:Latest API full documentation is on docs/v1.json

### Auth Routing
- `POST /api/auth/login` - Authenticates administrative or officer profiles. Returns signature JWT Bearer data token.

### Branch Tenant Routing
- `GET /api/organizations` - Yields total master list of branches (*Strict Access: System Root Branch Only*).
- `POST /api/organizations` - Onboards a new operational tenant organization into the cloud ecosystem.
- `PUT /api/organizations/{id}` - Updates live baseline settings (VAT/Default Rates) for a branch footprint.

### Workforce Directory Routing
- `GET /api/users` - Extracts active personnel rosters attached to the current company context.
- `POST /api/users` - Introduces a new employee context (*Passwords run automatic underlying BCrypt salt hashes*).
- `PUT /api/users/{id}/reset-password` - Overwrites employee access hashes during account lockouts.
- `GET /api/users/export` - Compiles a highly formatted, striped Excel list of total company staff profiles.

### Capital Allocation & Ledger Routing
- `GET /api/loans` - Extracts tenant-isolated, open financial loans.
- `POST /api/loans/auto-issue` - Evaluates customer emails, auto-creates profiles if unrecognized, and secures the active loan under one operational transaction.
- `PUT /api/loans/{id}/default` - Manually shifts overdue lending assets into a frozen default state for collections tracking.
- `GET /api/loans/export` - Streams a polished, board-ready financial report excel spreadsheet down to the local machine.
- `POST /api/payments` - Records customer repayments. Automatically moves the master loan state into `Paid` if the ledger math clears the current `TotalAmountDue`.

### WebSockets Hub Access Paths
- `/hubs/notifications` - Broadcasts generalized operational shifts to active staff nodes.
- `/hubs/audit` - Feeds a highly responsive security dashboard monitor with immediate `ActivityLog` and `UserLoginLog` data rows right after they commit to the core tables.
  AND MORE.



  PLEASE  NOTE: if youre having issues with running the project unzip the LMS folder thats the back-up.
