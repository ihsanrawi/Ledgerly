# Tech Stack

## Cloud Infrastructure

**Provider:** None (Local-First Architecture)
**Key Services:** N/A for MVP - Phase 2 may introduce cloud sync (AWS S3/Azure Blob for backup, Cloudflare for distribution)
**Deployment Regions:** N/A - Desktop application with local execution only

## Technology Stack Table

| Category | Technology | Version | Purpose | Rationale |
|----------|-----------|---------|---------|-----------|
| **Language** | C# | 12 | Backend primary language | Nullable reference types, records for DTOs, pattern matching, top-level statements for minimal APIs |
| **Language** | TypeScript | 5.3.3 | Frontend primary language | Strong typing, excellent Angular tooling, team expertise per PRD |
| **Runtime** | .NET | 8.0.4 LTS | Backend runtime | Long-term support (Nov 2026), performance improvements, native AOT ready for future optimization |
| **Runtime** | Node.js | 20.11.0 LTS | Frontend build/dev tooling | LTS version (Apr 2026), stable performance, Angular CLI requirement |
| **Backend Framework** | ASP.NET Core | 8.0.4 | Web API foundation | Minimal APIs for endpoints, high performance, cross-platform |
| **Messaging Framework** | Wolverine | 3.0.0 | Command/event handling | Local message bus, async workflows, CQRS support, .NET native |
| **Frontend Framework** | Angular | 17.3.8 | SPA framework | Signals (reactive state), standalone components, tree-shaking, mature ecosystem |
| **State Management** | Angular Signals | 17.3.8 (built-in) | Reactive UI state | Built-in reactivity, simpler than NgRx for solo dev, excellent computed values |
| **Async Orchestration** | RxJS | 7.8.1 | Complex async workflows | HTTP streams, CSV import progress, WebSocket (future), proven patterns |
| **Desktop Wrapper** | Tauri | 1.6.1 | Cross-platform desktop | 10-15MB bundle, Rust security, native file I/O, process spawning (Week 1 validation required) |
| **Desktop Wrapper (Fallback)** | Electron | 29.1.0 | Backup if Tauri fails | Battle-tested, larger bundle acceptable if Tauri validation fails Week 1 |
| **UI Component Library** | Angular Material | 17.3.8 | Prebuilt UI components | Accessibility (WCAG AA), consistent design system, rapid prototyping |
| **Charting** | Chart.js | 4.4.2 | Data visualizations | Lightweight (61KB), interactive, sufficient for dashboard/reports, good documentation |
| **PDF Generation** | jsPDF | 2.5.1 | Report PDF export | Client-side PDF generation, Chart.js integration, no server dependency (FR16) |
| **Date/Time** | Luxon | 3.4.4 | Transaction date parsing | Timezone-safe, immutable, ISO 8601 support, successor to Moment.js |
| **ORM** | Entity Framework Core | 8.0.4 | SQLite data access | Code-first migrations, LINQ queries, change tracking for cache invalidation |
| **Database** | SQLite | 3.45.1 (via better-sqlite3 9.x) | Local caching only | Serverless, cross-platform, perfect for desktop apps, NOT for financial data (only cache). No encryption in MVP—aligns with plaintext .hledger PTA philosophy. Phase 2 may add full-disk encryption. |
| **CSV Parsing** | CsvHelper | 30.0.1 | Bank CSV import | 50M+ downloads, handles encoding/delimiters/edge cases, excellent error handling |
| **Double-Entry Engine** | hledger | 1.32.3 (embedded binary) | Accounting calculations | Battle-tested (20+ years Ledger lineage), PTA community standard, JSON output support. Distributed as separate subprocess (GPL-compliant, no linking). |
| **File Watching** | FileSystemWatcher | Built-in .NET | Detect external .hledger edits | Native .NET, cross-platform, triggers cache invalidation |
| **Validation** | FluentValidation | 11.9.1 | Input validation | Expressive syntax, testable, separates validation from domain logic |
| **Testing - Backend Unit** | xUnit | 2.7.0 | .NET unit tests | Modern, async-friendly, parameterized tests, popular in .NET community |
| **Testing - Backend Mocking** | NSubstitute | 5.1.0 | Test doubles | Clean syntax, easy mocking for HledgerProcessRunner and file I/O |
| **Testing - Frontend Unit** | Jest | 29.7.0 | Angular unit tests | Faster than Jasmine/Karma, better DX, snapshot testing, parallel execution |
| **Testing - E2E** | Playwright | 1.42.1 | Critical path validation | Cross-browser, cross-platform, auto-wait, traces/videos for debugging |
| **Testing - Test Data** | Bogus | 35.5.0 | Fake data generation | Generate realistic test transactions, deterministic seeds for repeatability |
| **Logging** | Serilog | 3.1.1 | Structured logging | JSON output, sinks for file/console, correlation IDs, easy debugging |
| **CI/CD** | GitHub Actions | N/A (SaaS) | Build/test automation | Free for public repos, matrix builds (Windows/macOS/Linux), GitHub integration |
| **Package Manager - Backend** | NuGet | Built-in .NET | .NET dependencies | Standard .NET package manager |
| **Package Manager - Frontend** | npm | 10.5.0 | Node.js dependencies | Standard Node.js package manager, lockfile for reproducibility |
| **Code Quality - Backend** | Roslyn Analyzers | Built-in .NET | Static code analysis | StyleCop rules, nullable analysis, enforce conventions |
| **Code Quality - Frontend** | ESLint | 8.57.0 | TypeScript linting | Angular recommended rules, enforce code style |
| **Code Formatting** | Prettier | 3.2.5 | Frontend code formatting | Consistent formatting, integrates with ESLint |
| **API Documentation** | Swashbuckle.AspNetCore | 6.5.0+ | OpenAPI spec generation | Auto-generate API documentation with XML comments, export to `docs/api/openapi.yaml` |
| **API Validation** | Spectral | Latest | OpenAPI linting | Validate OpenAPI spec for errors and best practices |

---
