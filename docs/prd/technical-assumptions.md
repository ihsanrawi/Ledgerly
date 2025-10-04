# Technical Assumptions

Based on the Project Brief's technical preferences and architecture requirements, here are the technical assumptions for Ledgerly:

## Repository Structure: Monorepo with Vertical Slice Architecture

- **Structure:** Single repository organized by feature slices (not layers)
- **Organization Pattern:**
  ```
  src/
    Ledgerly.Api/
      Features/              # Vertical slices
        ImportCsv/
          ImportCsvCommand.cs
          ImportCsvHandler.cs
          ImportCsvEndpoint.cs
          ImportCsvTests.cs
        GetDashboard/
        CategorizeTransaction/
      Common/                # Shared kernel
        Hledger/
          HledgerBinaryManager.cs
          HledgerProcessRunner.cs
    Ledgerly.Web/           # Angular frontend
    Ledgerly.Desktop/       # Tauri wrapper
    Ledgerly.Contracts/     # Shared DTOs
  ```
- **Rationale:**
  - Feature cohesion: All code for a feature together
  - Solo developer friendly: Clear boundaries, easy navigation
  - Parallel development: Features don't collide
  - CQRS natural fit: Commands/queries explicit in each slice
- **Trade-off:** Accept some code duplication vs. premature abstraction (Rule of Three applies)

## Service Architecture: Event-Driven Monolith with Embedded hledger

- **Decision:** Event-driven .NET backend using Wolverine for messaging, embedded hledger binary for calculations
- **Architecture Pattern:**
  - **Frontend:** Angular SPA with Signals for reactivity
  - **Backend:** ASP.NET Core + Wolverine (command/event handling)
  - **Double-Entry Engine:** Embedded hledger binary (battle-tested, 20+ years)
  - **Data Flow:**
    ```
    Angular UI → API → Wolverine Command
                          ↓
                    Write .hledger file
                          ↓
                    hledger binary (calc)
                          ↓
                    Parse output → Cache (SQLite) → UI
    ```
  - **Desktop Wrapper:** Tauri (Rust-based, lighter than Electron)
  - **Storage:** .hledger plain text files (source of truth) + SQLite (caching only)
- **Rationale:**
  - **PTA-authentic:** Real hledger engine, not custom implementation
  - **Event-driven:** Wolverine handles async workflows (import, categorization, analytics)
  - **Offline-first:** Everything local, no cloud dependency (NFR6, NFR7)
  - **Simplicity:** Simpler than full event sourcing; appropriate for desktop app
- **Phase 2 Consideration:** Wolverine can extend to distributed if cloud sync needed

## Testing Requirements: Unit + Integration + Critical Path E2E

- **MVP Testing Strategy:**
  - **Unit Tests:**
    - Frontend: Angular components, services (Jasmine/Karma or Jest)
    - Backend: .NET API controllers, business logic (xUnit or NUnit)
    - **Coverage Target:** 70%+ for core business logic (CSV import, categorization, Ledger generation)
  - **Integration Tests:**
    - Wolverine command/event handler tests with in-memory message bus
    - hledger binary integration (FR23: validate writes pass `hledger check`)
    - CSV import end-to-end (file → parsing → .hledger write → hledger calc → cache → UI)
    - FileSystemWatcher tests for external .hledger edits
  - **E2E Tests (Critical Paths - Week 10):**
    - **REQUIRED for MVP:** Financial app trust requires automated critical path validation
    - **Tooling:** Playwright (cross-browser, cross-platform support)
    - **Critical paths to test:**
      1. CSV import → .hledger write → Dashboard loads with hledger data → Drill-down to category
      2. Add manual transaction → .hledger append → Dashboard updates → hledger validates
      3. Edit transaction → .hledger rewrite → Changes persist → Re-launch app → Data retained
      4. External edit .hledger file → FileSystemWatcher detects → UI refreshes within 1s
      5. Large dataset (1,000 transactions) → hledger calc → Dashboard loads <2 seconds (NFR1)
    - **Cross-platform:** Run E2E suite on Windows, macOS, Linux in CI/CD
  - **Manual Testing:**
    - UX flows (drag-drop, drill-downs) validated by developer
    - Performance validation: 5,000 transaction database (NFR1-NFR3 targets)
    - Cross-platform smoke tests (installation, first launch, basic workflows)
- **NOT in MVP:**
  - Extensive E2E coverage (80%+ paths) – focus on critical paths only
  - Performance testing automation (manual validation sufficient for MVP)
  - Load testing (single-user local app; not applicable)
- **Rationale:** Financial app requires high confidence in data integrity; E2E tests for critical paths are non-negotiable. Balance comprehensive testing with MVP timeline by focusing on highest-risk workflows.

## Additional Technical Assumptions and Requests

### Frontend Technology Stack

- **Framework:** Angular 16+ (latest stable)
  - **Signals:** Reactive state management for simple component state and derived values (built-in, no external library)
  - **RxJS (Hybrid Approach):** For complex async workflows (HTTP calls, CSV import progress, WebSocket updates if added)
  - **Rationale:** Use Signals where they shine (reactivity, computed values); use RxJS for proven async patterns (don't force Signals everywhere)
  - **Standalone Components:** Tree-shaking benefits, simplified architecture
  - **Angular Material:** UI component library (buttons, forms, dialogs) for rapid prototyping
- **Charting Library:** Chart.js (lightweight, interactive) or D3.js (if complex visualizations needed)
- **HTTP Client:** Angular HttpClient for API calls to localhost .NET backend
- **Routing:** Angular Router for navigation (dashboard, reports, settings views)

### Backend Technology Stack

- **Framework:** .NET 8+ (ASP.NET Core Web API)
  - **Language:** C# 12 (nullable reference types, records for DTOs)
  - **Messaging:** Wolverine (command/event handling, local queues)
  - **API Style:** Minimal APIs + Wolverine HTTP endpoints
- **Architecture Pattern:** Vertical Slice Architecture (features self-contained)
- **hledger Integration:**
  - **Binary Management:** Embedded hledger binaries for Windows/macOS/Linux
  - **Process Execution:** ProcessStartInfo for CLI invocation
  - **Output Parsing:** JSON output (`hledger bal -O json`) + text parsing fallback
  - **Validation:** `hledger check` after all .hledger writes
- **CSV Parsing:** CsvHelper library (robust, well-maintained, handles edge cases)
- **Caching:** SQLite with Entity Framework Core (app state only, NOT financial data)

### Data Storage Architecture

- **Financial Data (Source of Truth):**
  - **Format:** Plain text .hledger files (standard hledger format)
  - **Location:** User-selected directory (default: `~/Documents/Ledgerly/ledger.hledger`)
  - **Structure:**
    ```
    ; Account declarations
    account Assets:Checking
    account Expenses:Groceries

    2025-01-15 Whole Foods
        Expenses:Groceries    $45.23
        Assets:Checking
    ```
  - **File Operations:**
    - Atomic writes (temp file → rename) to prevent corruption
    - Automatic .hledger.bak backup before each write
    - FileSystemWatcher for external editor detection
- **Cache Layer (SQLite):**
  - **Purpose:** Query performance, app state, NOT financial data
  - **Tables:** DashboardCache, CategoryCache, PredictionCache, ImportRules, AppSettings
  - **Invalidation:** Clear cache on .hledger file change (FileSystemWatcher trigger)
- **Backup Strategy:**
  - Automatic .hledger.bak on each write
  - User-triggered "Backup to..." (copy .hledger to any location)
  - Git integration encouraged (plain text = version controllable)

### Desktop Wrapper

- **Technology:** Tauri 1.5+ (Rust-based, lightweight alternative to Electron)
  - **Bundle Size:** ~10-15MB (vs. Electron's 50-100MB)
  - **Security:** Rust memory safety, sandboxed WebView
  - **Trade-off:** Less mature ecosystem vs. Electron, but proven for production apps (e.g., Warp terminal, Logseq)
  - **CRITICAL: Week 1 Validation Required**
    - Build proof-of-concept Tauri app testing:
      1. .hledger file read/write operations (atomic writes, FileSystemWatcher)
      2. hledger binary execution (ProcessStartInfo, output capture)
      3. Cross-platform builds (Windows .exe, macOS .dmg, Linux .AppImage)
    - **Decision Point (End of Week 1):** If Tauri blockers found (e.g., process spawning issues, file permissions), pivot to Electron
- **Fallback Plan:** Electron (widely proven, larger bundle acceptable)
  - Add 1 week to timeline for Electron migration if Tauri validation fails
  - Bundle size trade-off: 100MB download acceptable if stability prioritized

### Dependency Management

- **Frontend:** npm or pnpm (faster lockfile resolution)
- **Backend:** NuGet for .NET packages
  - **Key Dependencies:** WolverineFx, WolverineFx.Http, CsvHelper, EF Core
- **hledger Binaries:** Bundled in app resources (Windows/macOS/Linux versions)

### CI/CD Pipeline

- **Platform:** GitHub Actions (free for public repos, integrated with GitHub)
- **Workflow:**
  - **On Push:** Lint (ESLint, C# analyzers), Unit Tests, Build verification
  - **On PR:** Full test suite (unit + integration), build all platforms
  - **On Release Tag:** Build production binaries (Windows .exe, macOS .dmg, Linux .AppImage), publish to GitHub Releases
- **Cross-Platform Builds:** GitHub Actions matrix (ubuntu-latest, macos-latest, windows-latest)

### Security Considerations

- **Data at Rest:**
  - SQLite database encryption via sqlcipher (Phase 2, pre-launch if feasible)
  - User-provided password for database unlock
- **Data in Transit:**
  - Local-only in MVP (localhost HTTP calls, no external network)
  - Phase 2 (cloud sync): HTTPS with end-to-end encryption
- **No Telemetry in MVP:** Privacy-first positioning requires no analytics without explicit opt-in
- **Dependency Scanning:** Dependabot or Snyk for vulnerability alerts

### Performance Assumptions

- **hledger Performance:** Typical queries <1 second for 5,000-10,000 transactions
  - **Optimization:** Leverage hledger's built-in performance (20+ years optimized)
  - **Week 6 Validation:** Test .hledger file with 50,000 transactions; validate NFR1-NFR3 targets
  - **Mitigation:** If slow, use hledger's JSON output + aggressive SQLite caching
- **CSV Import Speed:** CsvHelper parsing + .hledger append <5 seconds for 1,000 transactions (NFR2)
  - **Async Processing:** Wolverine handles async commands; progress updates via SignalR
- **Dashboard Load Time:** hledger calc + Chart.js rendering <2 seconds (NFR1)
  - **Caching Strategy:** SQLite caches hledger output; invalidate on .hledger changes
  - **Lazy Loading:** Dashboard widgets load on-demand

### hledger File Format Support

- **Primary Format:** hledger (embedded binary, battle-tested)
- **Compatibility:** hledger files work with Ledger/beancount via conversion tools
- **File Structure:**
  - Account declarations at top
  - Transactions in chronological order
  - 2-space indentation, aligned amounts
  - ISO dates (YYYY-MM-DD)
- **Validation Strategy:**
  - All writes validated via `hledger check` (NFR14)
  - Collect 50+ community .hledger files for edge case testing (Week 2)

### Third-Party Integrations (MVP Scope)

- **NOT included in MVP:**
  - Bank APIs (Plaid, open banking) – CSV-only for MVP
  - Cloud sync/backup services – local-only
  - Receipt OCR or attachment storage – defer to Phase 3
  - External reporting tools (tax software exports) – defer to Phase 2
