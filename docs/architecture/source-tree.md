# Source Tree

**Monorepo structure using Vertical Slice Architecture (VSA)**

```plaintext
Ledgerly/                                    # Monorepo root
├── .github/workflows/
│   ├── build.yml                            # CI: Build + Test
│   ├── release.yml                          # CI: Cross-platform Tauri builds
│   └── codeql.yml                           # Security scanning
├── docs/
│   ├── architecture/                        # Sharded architecture docs
│   │   ├── architecture.md                  # This file
│   │   ├── coding-standards.md
│   │   └── tech-stack.md
│   ├── prd/                                 # Product requirements
│   └── api/openapi.yaml                     # OpenAPI spec
├── scripts/
│   ├── bootstrap.sh                         # Setup dev environment
│   ├── build-all.sh
│   └── test-all.sh
├── src/
│   ├── Ledgerly.Api/                        # .NET 8 Backend
│   │   ├── Features/                        # VSA: Vertical slices
│   │   │   ├── ImportCsv/                   # Epic 2
│   │   │   │   ├── ImportCsvCommand.cs
│   │   │   │   ├── ImportCsvHandler.cs
│   │   │   │   ├── ImportCsvEndpoint.cs
│   │   │   │   ├── Adapters/                # Bank-specific CSV parsers
│   │   │   │   └── ImportCsv.Tests/         # Co-located tests
│   │   │   ├── GetDashboard/                # Epic 3
│   │   │   │   ├── GetDashboardQuery.cs
│   │   │   │   ├── GetCashFlowTimelineQuery.cs
│   │   │   │   └── GetDashboard.Tests/
│   │   │   ├── ManageTransactions/          # Epic 4
│   │   │   ├── DetectRecurring/             # Epic 5
│   │   │   ├── CategorizeTransaction/       # Epic 6
│   │   │   └── GenerateReports/             # Epic 7
│   │   ├── Common/                          # Shared kernel
│   │   │   ├── Hledger/                     # hledger integration
│   │   │   │   ├── HledgerBinaryManager.cs
│   │   │   │   ├── HledgerProcessRunner.cs
│   │   │   │   ├── HledgerFileWriter.cs
│   │   │   │   ├── HledgerFileParser.cs
│   │   │   │   └── CacheSynchronizer.cs
│   │   │   ├── Data/                        # EF Core
│   │   │   │   ├── LedgerlyDbContext.cs
│   │   │   │   ├── Entities/
│   │   │   │   └── Migrations/
│   │   │   ├── ValueObjects/
│   │   │   │   └── Money.cs                 # INTEGER cents → Decimal
│   │   │   └── Exceptions/
│   │   └── Resources/Binaries/              # Embedded hledger
│   │       ├── hledger-windows-x64.exe
│   │       ├── hledger-macos-arm64
│   │       └── hledger-linux-x64
│   ├── Ledgerly.Contracts/                  # Shared DTOs
│   ├── Ledgerly.Web/                        # Angular 17 Frontend
│   │   ├── src/app/
│   │   │   ├── core/services/               # Singleton services
│   │   │   ├── shared/components/           # Reusable UI
│   │   │   └── features/                    # Feature modules
│   │   │       ├── dashboard/
│   │   │       │   ├── components/
│   │   │       │   │   └── cash-flow-timeline/  # PRIMARY DIFFERENTIATOR
│   │   │       │   └── store/
│   │   │       │       └── dashboard.signals.ts
│   │   │       ├── transactions/
│   │   │       ├── import/
│   │   │       ├── reports/
│   │   │       └── predictions/
│   │   └── tests/
│   │       ├── unit/                        # Jest
│   │       └── e2e/                         # Playwright
│   └── Ledgerly.Desktop/                    # Tauri Wrapper
│       ├── src-tauri/                       # Rust code
│       │   ├── tauri.conf.json
│       │   ├── src/main.rs
│       │   └── icons/
│       └── src/index.html
└── tests/
    ├── Integration.Tests/
    └── E2E.Tests/                           # Playwright critical paths
```

## VSA Feature Slice Pattern

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

---
