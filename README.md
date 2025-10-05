# Ledgerly

**Local-First Personal Finance with Plain Text Accounting**

Ledgerly is a cross-platform desktop application that brings the power of Plain Text Accounting (PTA) to a modern, user-friendly interface. Built on hledger, it provides intelligent CSV import, cash flow forecasting, and beautiful visualizations while keeping your financial data in human-readable `.hledger` files.

## Architecture

Ledgerly uses a **Vertical Slice Architecture (VSA)** monorepo structure:

- **Backend**: .NET 8 Web API with Wolverine for CQRS and event-driven workflows
- **Frontend**: Angular 17 with Signals for reactive state management
- **Desktop**: Tauri wrapper for native cross-platform distribution
- **Database**: SQLite for caching only (`.hledger` files are the source of truth)

### Key Technologies

| Component | Technology | Version |
|-----------|-----------|---------|
| Backend Language | C# | 12 |
| Backend Runtime | .NET | 8.0.4 LTS |
| Messaging | Wolverine | 3.0.0 |
| Frontend Framework | Angular | 17.3.8 |
| Frontend Language | TypeScript | 5.3.3 |
| Desktop Wrapper | Tauri | 1.6.1 |
| Database (Cache) | SQLite | 3.45.1 |
| Double-Entry Engine | hledger | 1.32.3 |
| Logging | Serilog | 3.1.1 |

## Project Structure

```
Ledgerly/                                    # Monorepo root
├── docs/
│   ├── architecture/                        # Architecture documentation
│   ├── prd/                                 # Product requirements
│   └── api/                                 # OpenAPI specification
├── scripts/
│   ├── bootstrap.sh                         # Setup dev environment
│   ├── build-all.sh                         # Build all projects
│   └── export-openapi.sh                    # Export OpenAPI spec
├── src/
│   ├── Ledgerly.Api/                        # .NET 8 Backend
│   │   ├── Features/                        # VSA: Vertical slices
│   │   │   └── Health/                      # Health check endpoint
│   │   ├── Common/                          # Shared kernel
│   │   │   ├── Hledger/                     # hledger integration
│   │   │   ├── Data/                        # EF Core DbContext
│   │   │   ├── ValueObjects/                # Money value object
│   │   │   └── Exceptions/                  # Custom exceptions
│   │   └── Program.cs                       # Wolverine + Swagger config
│   ├── Ledgerly.Contracts/                  # Shared DTOs
│   ├── Ledgerly.Web/                        # Angular 17 Frontend
│   │   ├── src/app/
│   │   │   ├── core/services/               # Singleton services
│   │   │   ├── shared/components/           # Reusable UI
│   │   │   └── features/                    # Feature modules
│   │   │       ├── dashboard/
│   │   │       ├── transactions/
│   │   │       ├── import/
│   │   │       ├── reports/
│   │   │       └── predictions/
│   │   └── tests/
│   │       ├── unit/                        # Jest
│   │       └── e2e/                         # Playwright
│   └── Ledgerly.Desktop/                    # Tauri Wrapper
│       └── src-tauri/                       # Rust code
└── .editorconfig                            # Code style configuration
```

### VSA Feature Slice Pattern

Each feature follows this structure:

```
Features/FeatureName/
├── {Feature}Command.cs                      # Wolverine command
├── {Feature}Handler.cs                      # Business logic
├── {Feature}Endpoint.cs                     # API registration
├── {Feature}Validator.cs                    # FluentValidation
└── {Feature}.Tests/                         # Co-located tests
```

**Benefits:**
- ✅ Feature isolation (delete folder = remove feature)
- ✅ Parallel development (features don't collide)
- ✅ Clear boundaries (no shared state except Common/)
- ✅ Test discoverability (tests adjacent to implementation)

## Prerequisites

### Required
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (8.0.4 or higher)
- [Node.js](https://nodejs.org/) (20.11.0 LTS or higher)
- [hledger](https://hledger.org/install.html) (1.32.3 or higher)

### For Desktop App
- [Rust](https://www.rust-lang.org/tools/install) (latest stable)
- [Tauri Prerequisites](https://tauri.app/v1/guides/getting-started/prerequisites)

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/ledgerly.git
cd ledgerly
```

### 2. Install Dependencies

```bash
# Backend
dotnet restore src/Ledgerly.Api/Ledgerly.Api.csproj

# Frontend
npm install --prefix src/Ledgerly.Web
```

### 3. Run Backend API

```bash
dotnet run --project src/Ledgerly.Api
```

API will be available at `http://localhost:5000`
Swagger UI: `http://localhost:5000/swagger`

### 4. Run Frontend

```bash
cd src/Ledgerly.Web
npm start
```

Frontend will be available at `http://localhost:4200`

### 5. Build Desktop App (Optional)

Requires Rust to be installed.

```bash
cd src/Ledgerly.Desktop
cargo tauri dev
```

## Development

### Backend Development

```bash
# Build
dotnet build src/Ledgerly.Api/Ledgerly.Api.csproj

# Run tests
dotnet test src/Ledgerly.Api/

# Watch mode
dotnet watch --project src/Ledgerly.Api
```

### Frontend Development

```bash
cd src/Ledgerly.Web

# Development server
npm start

# Build
npm run build

# Lint
npx eslint .

# Format
npx prettier --write .
```

### Export OpenAPI Spec

```bash
# Start the API first
dotnet run --project src/Ledgerly.Api

# In another terminal
./scripts/export-openapi.sh
```

Output: `docs/api/openapi.yaml` (or `.json` if PyYAML not installed)

## Testing

### Backend Tests
```bash
dotnet test src/Ledgerly.Api/ --collect:"XPlat Code Coverage"
```

### Frontend Tests
```bash
cd src/Ledgerly.Web
npm test                    # Jest unit tests
npm run test:e2e            # Playwright E2E tests
```

## Key Design Principles

### 1. Plain Text Accounting Philosophy
- **`.hledger` files are the single source of truth** - SQLite is cache only
- User owns their data in human-readable format
- Compatible with existing PTA tools (hledger CLI, Ledger, Beancount with conversion)

### 2. Local-First Architecture
- No cloud dependency for MVP
- Fast offline operation
- Optional cloud sync in Phase 2

### 3. Coding Standards
- **Money Precision**: All monetary amounts use `Money` value object (integer cents storage)
- **No `console.log`**: Use Serilog (backend) or Angular logging service (frontend)
- **Async/Await**: All I/O operations are async
- **Nullable Reference Types**: Enabled and enforced
- **FluentValidation**: All command/query inputs have validators

## API Documentation

Swagger UI: `http://localhost:5000/swagger`

OpenAPI spec: `docs/api/openapi.yaml` (after running export script)

## Contributing

1. Follow Vertical Slice Architecture for new features
2. Run linters and tests before committing
3. Update OpenAPI spec when adding/modifying endpoints
4. Document SQLite usage as "cache only" in comments

## License

TBD

## Resources

- [Architecture Documentation](docs/architecture/)
- [Product Requirements](docs/prd/)
- [hledger Documentation](https://hledger.org/)
- [Wolverine Documentation](https://wolverine.netlify.app/)
- [Angular Documentation](https://angular.io/)
- [Tauri Documentation](https://tauri.app/)
