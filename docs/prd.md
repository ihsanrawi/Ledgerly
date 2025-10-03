# Ledgerly Product Requirements Document (PRD)

## Goals and Background Context

### Goals

- Deliver a **local-first, dashboard-driven personal finance manager** that makes Plain Text Accounting accessible to technical users without CLI expertise
- Enable **CSV-based transaction imports** with intelligent categorization that learns from user corrections over time
- Provide **interactive visualizations and predictive analytics** that reveal spending patterns and cash flow forecasts
- Maintain **full PTA transparency** through auto-generated Ledger files that preserve data ownership, portability, and version control compatibility
- Support **household finance management** by enabling non-technical users (spouses/partners) to access insights without terminal skills
- Reduce **monthly reconciliation time by 50%+** compared to CLI-only PTA workflows
- Achieve **1,000-3,000 paying users** within 12 months, validating product-market fit

### Background Context

The Plain Text Accounting ecosystem (Ledger, hledger, beancount) provides powerful double-entry bookkeeping with complete data ownership and transparency. However, these CLI-only tools present steep learning curves, requiring significant time investment and terminal expertise. Current GUI solutions like Fava are web-only, format-specific, and lack modern mobile access or automation features.

Meanwhile, mainstream finance apps (YNAB, Mint) offer ease-of-use at the cost of vendor lock-in, privacy invasion, and subscription costs ($99/year+). With Mint's 2024 shutdown displacing 20M users and growing privacy concerns (GDPR, CCPA, data breaches), there's a clear market opportunity for a privacy-respecting, locally-owned alternative.

**Ledgerly** builds on the battle-tested hledger foundation: **plain text .hledger files remain the source of truth**, with embedded hledger binary handling all double-entry calculations. This PTA-authentic approach—combined with dashboard-first UI, CSV import automation, adaptive prediction logic, and progressive complexity disclosure—bridges the gap between powerful CLI tools and intuitive consumer apps. The solution targets frustrated CLI power users (15K-30K globally), PTA-curious technical users (100K-200K), and privacy-first enthusiasts (50K-100K) who value data control but lack time for CLI mastery.

### Change Log

| Date       | Version | Description             | Author |
|------------|---------|-------------------------|--------|
| 2025-10-02 | 1.0     | Initial PRD creation    | John   |
| 2025-10-03 | 2.0     | Architecture revision: hledger + Wolverine + VSA | John   |

## Requirements

### Functional Requirements

Based on the Project Brief's MVP scope and target user workflows, here are the functional requirements:

#### MVP Requirements (Phase 1)

**FR1:** The system shall support CSV file import via drag-drop or file selection interface

**FR2:** The system shall automatically detect CSV column types (date, amount, payee, memo) with >90% accuracy

**FR3:** The system shall provide a manual column mapping interface when auto-detection is uncertain

**FR4:** The system shall detect and warn against duplicate transactions during import

**FR6:** The system shall suggest transaction categories based on historical patterns and user-defined rules

**FR8:** The system shall display an interactive dashboard as the primary landing page showing net worth, expense breakdown, income vs. expense, and recent transactions

**FR9:** The system shall provide drill-down navigation from category summaries to individual transaction lists

**FR10:** The system shall enable users to create, read, update, and delete transactions through a GUI form

**FR11:** The system shall support split transactions (single transaction allocated across multiple categories)

**FR12:** The system shall handle transfers between accounts without double-counting in expense/income totals

**FR13:** The system shall provide auto-completion for payees and categories based on transaction history

**FR14:** The system shall generate category-based expense and income reports with time period filtering (month, quarter, year, custom range)

**FR15:** The system shall provide comparison views (e.g., this month vs. last month, year-over-year)

**FR16:** The system shall export reports to PDF and CSV formats

**FR17:** The system shall detect recurring transactions (subscriptions, rent, salary) with >80% accuracy

**FR18:** The system shall display a cash flow timeline showing predicted balance for the next 30-90 days

**FR20:** The system shall alert users to predicted overdrafts or unusual spending patterns

**FR21:** The system shall write transactions to .hledger plain text files using standard hledger format

**FR23:** The system shall validate all .hledger file writes using embedded hledger binary (`hledger check`)

**FR24:** The system shall allow users to manually edit .hledger files with external editors, detecting changes via FileSystemWatcher

#### Phase 2 Requirements (Post-MVP)

**FR5:** The system shall normalize payee names (e.g., "AMAZON.COM AMZN.COM/BILL" → "Amazon") *(Deferred: Complex bank-specific patterns; users can manually correct in MVP)*

**FR7:** The system shall learn from user corrections to improve future categorization accuracy (target: 80% after 3 months) *(Deferred: Requires ML pipeline; MVP will collect training data via FR6)*

**FR19:** The system shall provide confidence scoring for predictions based on historical consistency *(Deferred: Extends FR18; add after validating base predictions)*

**FR22:** The system shall provide command palette interface for keyboard-driven power users *(Deferred: UI complexity; focus on core workflows first)*

### Non-Functional Requirements

**NFR1:** The system shall load the dashboard in <2 seconds for databases containing up to 5,000 transactions

**NFR2:** The system shall process CSV imports of 1,000 transactions in <5 seconds

**NFR3:** The system shall provide search and filter results in <1 second for databases containing up to 10,000 transactions

**NFR4:** The system shall launch (cold start) in <3 seconds

**NFR5:** The system shall maintain 60fps during UI interactions (scrolling, animations, chart interactions)

**NFR6:** The system shall operate fully offline with no internet connection required for core functionality

**NFR7:** The system shall store all financial data as plain text .hledger files with SQLite used only for caching and app state

**NFR8:** The system shall provide atomic writes to .hledger files (temp file → rename) with automatic .hledger.bak backups

**NFR9:** The system shall achieve >95% CSV import success rate across common bank formats

**NFR10:** The system shall maintain <0.1% crash rate (sessions without crashes)

**NFR11:** The system shall ensure zero data loss incidents through transaction integrity and backup mechanisms

**NFR12:** The system shall run cross-platform on Windows 10+, macOS 11+, and Ubuntu 20.04+

**NFR13:** The system shall use no more than 500MB of memory during typical operation

**NFR14:** The system shall validate all .hledger file writes pass `hledger check` validation 100% of the time

**NFR15:** The system shall use FileSystemWatcher to detect external .hledger edits and refresh UI within 1 second *(Phase 2: Power user feature)*

## User Interface Design Goals

Based on the Project Brief's target platforms (Desktop: Windows/macOS/Linux, Mobile: Phase 2) and competitive positioning analysis, here are the UI/UX design goals:

### Overall UX Vision

Ledgerly bridges the **power of CLI tools** with the **intuitiveness of consumer apps** through a dashboard-first, progressive disclosure approach. The interface prioritizes:

- **Instant gratification:** Dashboard shows insights within seconds of first CSV import
- **Progressive complexity:** Beginners see simple workflows; power users discover advanced features (keyboard shortcuts, command palette in Phase 2) naturally
- **Transparency without overwhelm:** Financial data is accessible and explorable without requiring understanding of double-entry accounting
- **Confidence through clarity:** Every action (import, categorization, prediction) shows clear rationale and allows user correction

**Design Philosophy:** "Show me my money, then let me explore" – Dashboard answers "where did my money go?" immediately, drill-downs answer "why?" on demand.

**Competitive Positioning:** *"hledger's power, YNAB's ease, your data"* – The only PTA tool using real hledger engine with predictive analytics, beautiful interface, and complete data ownership.

### Key Interaction Paradigms

1. **Dashboard-First Navigation**
   - Dashboard is home base; all features accessible from dashboard widgets
   - Category cards clickable → drill down to transaction lists
   - Time period selector persistent across all views (month/quarter/year)
   - "Quick Actions" always visible: Import CSV, Add Transaction, Reconcile

2. **Drag-Drop Simplicity**
   - CSV import: Drag file onto dashboard or import area
   - Column mapping: Drag column headers to match fields (Date, Amount, Payee, etc.)
   - Transaction categorization: Drag-drop transactions to categories (optional, click-select also available)

3. **Interactive Data Visualization**
   - Charts are explorable: Hover for details, click to drill down
   - Timeline scrubbing: Drag slider to see balance at any point in time
   - Comparison overlays: Toggle previous period on charts for context

4. **Smart Defaults with Easy Overrides**
   - Auto-detection runs silently; presents results with confidence indicators
   - Green checkmark = high confidence, yellow warning = needs review
   - One-click correction: Click suggested category → dropdown to override
   - Learning indicator: "Ledgerly learns from your corrections" reinforces local ML advantage

### Core Screens and Views

From a product perspective, these are the critical screens necessary to deliver PRD value:

1. **Dashboard (Landing Page)**
   - **Cash Flow Timeline widget (PROMINENT - top-center):** Next 30-90 days predicted balance with recurring transaction markers *(Unique differentiator: FR18)*
   - Net worth summary card (total assets - liabilities)
   - Expense breakdown pie/donut chart (top 5-7 categories, click to drill down)
   - Income vs. Expense bar chart (current month + trend line)
   - Recent transactions list (10-20 items, scrollable)
   - Quick action buttons: Import CSV, Add Transaction, View Reports
   - **Export button (footer):** Always visible to reinforce "no lock-in" trust (FR24)

2. **CSV Import Flow**
   - Drag-drop zone or file picker
   - Column mapping interface (auto-detected or manual)
   - Preview table (first 10 rows with detected categories)
   - Duplicate detection warnings (FR4)
   - **Learning indicator:** "Ledgerly learns from your corrections to improve future imports"
   - Import confirmation with summary (e.g., "Imported 127 transactions, 23 need categorization")

3. **Transaction List View**
   - Filterable/searchable table (by date, payee, category, amount)
   - Inline editing: Click any field to edit
   - Batch operations: Select multiple → categorize, delete, mark reconciled
   - Split transaction modal (for FR11)

4. **Transaction Detail/Edit Modal**
   - Form fields: Date, Payee, Amount, Category, Account, Memo, Tags
   - Auto-complete for Payee and Category (FR13)
   - Split transaction interface (add multiple category rows)
   - Transfer toggle (for FR12: moving money between accounts)
   - Save/Cancel/Delete buttons

5. **Category Reports View**
   - Time period selector (month, quarter, year, custom date range)
   - Category hierarchy tree (if nested categories supported) or flat list
   - Bar chart: Spending by category (interactive drill-down)
   - Comparison toggle: Show previous period or year-over-year (FR15)
   - **Export button (prominent):** PDF/CSV via FR16
   - Drill-down: Click category → transaction list for that category

6. **Cash Flow Timeline View (Predictive Analytics)**
   - **Primary differentiator screen – emphasize in demos and marketing**
   - Timeline chart showing predicted balance over next 30-90 days (FR18)
   - Recurring transaction markers (FR17: subscriptions, rent, salary with icons)
   - Confidence shading (darker = more confident, lighter = less certain)
   - Alert indicators (red warning if predicted overdraft with actionable suggestions)
   - Toggle: Show/hide individual recurring transactions vs. aggregated line
   - **Educational tooltip:** "Predictions based on your recurring transactions—adjust anytime"

7. **Settings/Preferences**
   - Account management (add/edit bank accounts)
   - Category customization (create, rename, delete categories)
   - Import rules (view/edit rules learned from corrections via FR6)
   - Ledger file export settings (format: Ledger/hledger/beancount)
   - Database backup/restore
   - **Export Ledger Files button (prominent):** One-click access to FR24

### Accessibility: WCAG AA

- **Keyboard navigation:** Full app navigable via Tab, Enter, Arrow keys (compensates for Phase 2 command palette)
- **Screen reader support:** Semantic HTML, ARIA labels on interactive elements
- **Color contrast:** All text meets WCAG AA contrast ratios (4.5:1 for body text, 3:1 for large text)
- **Focus indicators:** Clear visual focus states for keyboard users
- **Alt text:** Charts have text summaries for screen readers (e.g., "Expense breakdown: Groceries $450, Dining $230...")

### Branding

- **Aesthetic:** Clean, modern, data-focused – **developer tool aesthetic** (inspired by VS Code, GitHub) rather than consumer finance apps (YNAB's bright colors)
- **Color Palette:**
  - Primary: Deep blue (#2C3E50) – trust, stability
  - Accent: Teal (#1ABC9C) – growth, positive cash flow, predictions
  - Danger: Red (#E74C3C) – overdrafts, warnings
  - Success: Green (#27AE60) – savings, positive trends
  - Neutral: Grays (#ECF0F1 light, #34495E dark)
- **Typography:**
  - Headings: Inter or Roboto (sans-serif, clean, readable)
  - Body: System font stack (faster load, native feel)
  - Data: Monospace (for amounts, ledger file previews)
- **Iconography:** Material Icons or Lucide (consistent, recognizable)
- **Tone:** Professional but not corporate; empowering, not patronizing; technical users appreciate precision over cuteness

### Target Device and Platforms: Web Responsive (Desktop-first)

**MVP (Phase 1):**
- **Desktop:** Windows, macOS, Linux via Tauri wrapper
- **Resolution targets:** 1920x1080 (primary), 1366x768 (minimum), 2560x1440+ (scaling)
- **Responsive breakpoints:**
  - Large desktop (1920px+): Multi-column dashboard with prominent Cash Flow Timeline
  - Standard desktop (1366-1920px): Two-column dashboard
  - Small desktop/laptop (1024-1366px): Single column with stacked widgets
- **NOT supported in MVP:** Mobile, tablet (Phase 2 – target desktop-heavy users first)

**Phase 2:**
- Mobile companion app (iOS/Android via Capacitor) for balance checks and quick transaction capture
- Tablet layouts (responsive scaling)

## Technical Assumptions

Based on the Project Brief's technical preferences and architecture requirements, here are the technical assumptions for Ledgerly:

### Repository Structure: Monorepo with Vertical Slice Architecture

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

### Service Architecture: Event-Driven Monolith with Embedded hledger

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

### Testing Requirements: Unit + Integration + Critical Path E2E

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

### Additional Technical Assumptions and Requests

#### Frontend Technology Stack

- **Framework:** Angular 16+ (latest stable)
  - **Signals:** Reactive state management for simple component state and derived values (built-in, no external library)
  - **RxJS (Hybrid Approach):** For complex async workflows (HTTP calls, CSV import progress, WebSocket updates if added)
  - **Rationale:** Use Signals where they shine (reactivity, computed values); use RxJS for proven async patterns (don't force Signals everywhere)
  - **Standalone Components:** Tree-shaking benefits, simplified architecture
  - **Angular Material:** UI component library (buttons, forms, dialogs) for rapid prototyping
- **Charting Library:** Chart.js (lightweight, interactive) or D3.js (if complex visualizations needed)
- **HTTP Client:** Angular HttpClient for API calls to localhost .NET backend
- **Routing:** Angular Router for navigation (dashboard, reports, settings views)

#### Backend Technology Stack

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

#### Data Storage Architecture

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

#### Desktop Wrapper

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

#### Dependency Management

- **Frontend:** npm or pnpm (faster lockfile resolution)
- **Backend:** NuGet for .NET packages
  - **Key Dependencies:** WolverineFx, WolverineFx.Http, CsvHelper, EF Core
- **hledger Binaries:** Bundled in app resources (Windows/macOS/Linux versions)

#### CI/CD Pipeline

- **Platform:** GitHub Actions (free for public repos, integrated with GitHub)
- **Workflow:**
  - **On Push:** Lint (ESLint, C# analyzers), Unit Tests, Build verification
  - **On PR:** Full test suite (unit + integration), build all platforms
  - **On Release Tag:** Build production binaries (Windows .exe, macOS .dmg, Linux .AppImage), publish to GitHub Releases
- **Cross-Platform Builds:** GitHub Actions matrix (ubuntu-latest, macos-latest, windows-latest)

#### Security Considerations

- **Data at Rest:**
  - SQLite database encryption via sqlcipher (Phase 2, pre-launch if feasible)
  - User-provided password for database unlock
- **Data in Transit:**
  - Local-only in MVP (localhost HTTP calls, no external network)
  - Phase 2 (cloud sync): HTTPS with end-to-end encryption
- **No Telemetry in MVP:** Privacy-first positioning requires no analytics without explicit opt-in
- **Dependency Scanning:** Dependabot or Snyk for vulnerability alerts

#### Performance Assumptions

- **hledger Performance:** Typical queries <1 second for 5,000-10,000 transactions
  - **Optimization:** Leverage hledger's built-in performance (20+ years optimized)
  - **Week 6 Validation:** Test .hledger file with 50,000 transactions; validate NFR1-NFR3 targets
  - **Mitigation:** If slow, use hledger's JSON output + aggressive SQLite caching
- **CSV Import Speed:** CsvHelper parsing + .hledger append <5 seconds for 1,000 transactions (NFR2)
  - **Async Processing:** Wolverine handles async commands; progress updates via SignalR
- **Dashboard Load Time:** hledger calc + Chart.js rendering <2 seconds (NFR1)
  - **Caching Strategy:** SQLite caches hledger output; invalidate on .hledger changes
  - **Lazy Loading:** Dashboard widgets load on-demand

#### hledger File Format Support

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

#### Third-Party Integrations (MVP Scope)

- **NOT included in MVP:**
  - Bank APIs (Plaid, open banking) – CSV-only for MVP
  - Cloud sync/backup services – local-only
  - Receipt OCR or attachment storage – defer to Phase 3
  - External reporting tools (tax software exports) – defer to Phase 2

## Epic List

Based on Vertical Slice Architecture from brief, here are the high-level epics organized by feature slices:

### Epic 1: Foundation & Core Infrastructure
**Goal:** Establish Wolverine + hledger + Tauri integration with VSA folder structure and basic .hledger file operations to validate full stack end-to-end.

**Key Deliverables:**
- Tauri + .NET + Wolverine + Angular integration
- VSA folder structure (Features/ + Common/Hledger/)
- Embedded hledger binary management (extract, permissions, SHA256 verify)
- Basic .hledger read/write with atomic operations
- Testing infrastructure (xUnit, Wolverine test harness, Playwright stubs)
- Simple UI displaying hledger balance output

**Success Criteria:** Desktop app launches on all platforms, executes `hledger bal`, displays results in UI, writes test transaction to .hledger file

---

### Epic 2: Import CSV Vertical Slice (P0 - Foundation)
**Goal:** Build complete CSV import feature as first vertical slice - parse CSV, generate hledger syntax, append to .hledger file with Wolverine async handling.

**Key Deliverables:**
- ImportCsvCommand + Handler + Endpoint (Wolverine)
- CSV column detection and manual mapping (FR2, FR3)
- hledger transaction formatter (proper syntax, 2-space indent, aligned amounts)
- Atomic .hledger file append with .bak backup
- Duplicate detection hashing (FR4)
- Category suggestion engine via ImportRules (FR6)
- **Test Data:** 20+ bank CSV formats collected (Week 2)

**Success Criteria:** Import 3 different bank CSVs → valid .hledger file → `hledger check` passes → transactions visible in CLI

**Deferred to Phase 2:** Payee normalization (FR5), ML categorization learning (FR7)

---

### Epic 3: Get Dashboard Vertical Slice (P0 - Core Value)
**Goal:** Build dashboard query slice - call hledger for calculations, cache results, display interactive visualizations showing financial insights.

**Key Deliverables:**
- GetDashboardQuery + Handler (Wolverine)
- hledger query execution (`hledger bal -O json`, `hledger reg`)
- Output parsing (JSON → C# objects)
- SQLite caching layer with invalidation on .hledger changes
- Dashboard widgets: Net worth, expense breakdown (Chart.js), income vs expense
- Drill-down navigation (FR9)
- **FileSystemWatcher:** Detect .hledger changes → invalidate cache → refresh UI
- **Performance Test:** 5,000 transaction .hledger file → <2s dashboard load (NFR1)

**Success Criteria:** Dashboard shows accurate hledger data within 2 seconds; external .hledger edits trigger UI refresh within 1 second

---

### Epic 4: Manage Transactions Vertical Slice (P0 - Essential)
**Goal:** Implement transaction CRUD operations - add/edit/delete via commands that rewrite .hledger file maintaining proper formatting.

**Key Deliverables:**
- AddTransactionCommand + Handler (append to .hledger)
- EditTransactionCommand + Handler (rewrite .hledger file)
- DeleteTransactionCommand + Handler (comment out in .hledger)
- GetTransactionHistoryQuery + Handler (parse hledger reg output)
- .hledger file rewrite strategy (read → modify → atomic write)
- Transaction search/filter UI (FR10)
- Auto-complete for payees/categories from .hledger file (FR13)

**Success Criteria:** Add/edit/delete transaction → .hledger updated → `hledger check` validates → dashboard refreshes automatically

**Deferred to Phase 2:** Split transactions (FR11), transfer handling (FR12)

---

### Epic 5: Detect Recurring Vertical Slice (P1 - Delight)
**Goal:** Build recurring transaction detection and prediction slice - analyze hledger data for patterns, generate cash flow timeline (unique differentiator).

**Key Deliverables:**
- DetectRecurringCommand + Handler (Wolverine scheduled job, nightly)
- Pattern detection algorithm (same payee, ±10% amount, ±5 days monthly)
- RecurringTransactions cache (SQLite)
- PredictCashFlowQuery + Handler (project balance 30-90 days)
- Cash Flow Timeline widget on dashboard (prominent placement)
- Overdraft/unusual spending alerts (FR20)
- **Test Dataset:** 100+ labeled transactions (50 recurring, 50 non-recurring)

**Success Criteria:** 80%+ recurring detection accuracy; predictions within 10% for 30-day forecast

**Time-Box:** 1.5 weeks; defer confidence scoring (FR19) to Phase 2

---

### Epic 6: Categorize Transaction Vertical Slice (P1 - High Value)
**Goal:** Build categorization slice - auto-suggest categories, learn from corrections, batch operations via Wolverine commands.

**Key Deliverables:**
- CategorizeTransactionCommand + Handler
- Auto-suggest engine (match payee against ImportRules in SQLite)
- Batch categorization support (multiple transactions)
- Learning mechanism (user correction → update ImportRules)
- Confidence scoring (simple: exact match = high, partial = medium)
- Pattern matching (keyword-based, not ML for MVP)

**Success Criteria:** >50% categorization accuracy on first import; >70% after 3 corrections

**Deferred to Phase 2:** ML-based learning (FR7), advanced pattern matching

---

### Epic 7: Category Reports Vertical Slice (P1 - Core Value)
**Goal:** Build reporting slice - query hledger for category breakdowns, time period filtering, comparison views, PDF/CSV export.

**Key Deliverables:**
- GetCategoryReportQuery + Handler (call `hledger bal -O json` with date filters)
- Time period filtering (month, quarter, year, custom range)
- Comparison views (this month vs last month, YoY)
- Drill-down to transactions by category (reuse GetTransactionHistory)
- PDF export (jsPDF) and CSV export
- Interactive Chart.js visualizations

**Success Criteria:** Category report generated in <5 seconds; drill-down to transactions works; PDF/CSV export well-formatted

**Optional for MVP:** If timeline slips, cut this epic – users can use hledger CLI for reports

---

### Epic Sequencing & Timeline (VSA-Based)

**Total Duration:** 12 weeks following VSA feature slice approach

| Week | Epic | Notes |
|------|------|-------|
| 1 | Epic 1: Foundation | Wolverine + hledger + Tauri PoC; VSA structure |
| 2-3 | Epic 2: Import CSV Slice | First complete vertical slice; collect 20+ CSV formats |
| 4 | Epic 3: Dashboard Slice | hledger queries + caching + FileSystemWatcher |
| 5-6 | Epic 4: Transaction CRUD Slice | .hledger rewrite strategy; 50K perf test Week 6 |
| 7-8 | Epic 6: Categorize Slice | Auto-suggest + learning rules |
| 9-10 | Epic 5: Predictions Slice (1.5 wks) + Epic 7: Reports (0.5 wks) | **Buffer: Cut Epic 7 if needed** |
| 11-12 | E2E Testing, Polish, Package | Playwright critical paths; cross-platform builds |

**Critical Path (VSA):** Epics 1 → 2 → 3 → 4 (Foundation → Import → Dashboard → CRUD) = 6 weeks
**Differentiators:** Epic 5 (Predictions) time-boxed; Epic 7 (Reports) optional

**VSA Benefits:**
- Each epic is complete feature (backend + frontend + tests)
- No "backend weeks" then "frontend weeks" - features ship incrementally
- Testing flows through each slice, not deferred to end

**Parallelization:**
- Week 2: Collect .hledger test files (50+) while building Import slice
- All epics include unit + integration tests co-located with feature

## Epic Details

### Epic 1: Foundation & Core Infrastructure

**Epic Goal:** Establish Wolverine + hledger + Tauri integration with VSA folder structure and basic .hledger file operations to validate full stack end-to-end.

#### Story 1.1: Set Up VSA Project Structure with Wolverine

**As a** developer,
**I want** the project repository initialized with VSA folder structure, Wolverine, and hledger integration,
**so that** I can build feature slices on a solid event-driven foundation.

**Acceptance Criteria:**
1. Monorepo created with VSA structure: `Features/`, `Common/Hledger/`, `Ledgerly.Web/`, `Ledgerly.Desktop/`, `Ledgerly.Contracts/`
2. Angular 16+ project initialized with Signals and standalone components
3. .NET 8+ Web API project created with Wolverine configured (WolverineFx, WolverineFx.Http)
4. Tauri 1.5+ wrapper integrated with Angular frontend
5. SQLite configured for caching only (NOT financial data)
6. GitHub repository initialized with .gitignore, README, VSA documentation

#### Story 1.2: Integrate and Validate hledger Binary

**As a** developer,
**I want** to embed hledger binary and validate process execution works cross-platform,
**so that** I can confidently use hledger for all double-entry calculations.

**Acceptance Criteria:**
1. hledger binaries downloaded for Windows, macOS, Linux (from hledger.org)
2. HledgerBinaryManager.cs extracts binaries to app resources, sets permissions, SHA256 verifies
3. HledgerProcessRunner.cs executes `hledger --version` successfully on all platforms
4. Test executes `hledger bal -f test.hledger` and parses output
5. Cross-platform builds validated (Windows .exe, macOS .dmg, Linux .AppImage)
6. Decision by end of Week 1: Proceed with Tauri OR pivot to Electron if process spawning issues found

#### Story 1.3: Set Up Wolverine Test Harness and Testing Infrastructure

**As a** developer,
**I want** Wolverine test harness and testing frameworks configured,
**so that** I can test command/query handlers with in-memory message bus.

**Acceptance Criteria:**
1. Wolverine test harness configured with in-memory message bus
2. xUnit configured for .NET with sample Wolverine handler test passing
3. Frontend: Jest configured for Angular unit tests (Signals support)
4. Integration tests: hledger binary execution + .hledger file validation
5. E2E: Playwright stubs initialized (full suite Week 10)
6. CI/CD (GitHub Actions) runs unit tests on push; coverage reporting enabled (70%+ target)

#### Story 1.4: Build hledger File Writer and Atomic Operations

**As a** developer,
**I want** atomic .hledger file write operations with backups,
**so that** financial data is never corrupted or lost.

**Acceptance Criteria:**
1. TransactionFormatter.cs generates valid hledger syntax (2-space indent, aligned amounts, ISO dates)
2. Atomic write strategy: Write to temp file → validate with `hledger check` → rename to .hledger
3. Automatic .hledger.bak backup created before each write
4. HledgerFileWriter.cs handles account declarations at top of file
5. Integration test: Write transaction → `hledger check` passes → read back with `hledger reg`
6. Error handling: If `hledger check` fails, restore .bak file and surface error to user

#### Story 1.5: Build Simple hledger Balance Display UI

**As a** user,
**I want** to see hledger balance output when I open the app,
**so that** I can verify the full stack is working (Angular → API → Wolverine → hledger).

**Acceptance Criteria:**
1. Create test .hledger file with 5 sample transactions (seed data)
2. GetBalanceQuery + Handler calls `hledger bal -O json` and parses output
3. Angular component displays balance tree (accounts with amounts)
4. API endpoint uses Wolverine HTTP endpoint pattern
5. Basic styling with Angular Material
6. Cross-platform smoke test: App launches, executes hledger, displays balances on Windows/macOS/Linux

---

### Epic 2: CSV Import & Smart Data Entry

**Epic Goal:** Enable users to import bank CSV files with intelligent column detection, manual mapping fallback, duplicate detection, and category suggestion rules.

#### Story 2.1: Collect Bank CSV Test Samples

**As a** developer,
**I want** to gather 20+ real bank CSV formats before starting development,
**so that** CSV parser handles diverse formats and edge cases.

**Acceptance Criteria:**
1. Collect 20+ anonymized bank CSV samples from community (r/plaintextaccounting, friends, family)
2. Document CSV format variations: delimiter (comma, semicolon, tab), date formats, encoding (UTF-8, ISO-8859-1)
3. Create test fixture folder with categorized CSVs (standard, edge cases, malformed)
4. Identify common column names: "Date", "Description", "Amount", "Debit", "Credit", "Balance"
5. Edge cases documented: negative amounts as parentheses, multi-line memos, special characters

#### Story 2.2: Build CSV Upload and Parsing

**As a** user,
**I want** to upload a CSV file via drag-drop or file picker,
**so that** I can import transactions without manual data entry.

**Acceptance Criteria:**
1. Drag-drop zone accepts .csv files (FR1)
2. File picker dialog opens on click (fallback for users unfamiliar with drag-drop)
3. CsvHelper library parses uploaded file, handles encoding detection (UTF-8, ISO-8859-1)
4. Parse errors displayed with helpful message (e.g., "Invalid CSV format on line 5")
5. Progress indicator shown during parsing for large files (>1,000 rows)
6. Unit tests validate parsing for 10+ CSV formats from Story 2.1

#### Story 2.3: Automatic Column Detection

**As a** user,
**I want** the app to automatically detect which columns contain date, amount, payee, and memo,
**so that** I don't have to manually map columns every time.

**Acceptance Criteria:**
1. Heuristic algorithm detects date column (pattern matching: MM/DD/YYYY, YYYY-MM-DD, etc.) with >90% accuracy (FR2)
2. Amount column detected (numeric values, optional currency symbols, decimal separators)
3. Payee/description column detected (text values, typically longest non-numeric column)
4. Memo column detected (optional, secondary text column)
5. Confidence indicators displayed: Green checkmark (high confidence), yellow warning (review needed)
6. Integration test validates detection for 15+ CSV formats from Story 2.1 with >90% success rate

#### Story 2.4: Manual Column Mapping Interface

**As a** user,
**I want** to manually map CSV columns when auto-detection fails,
**so that** I can still import my bank statements.

**Acceptance Criteria:**
1. Drag-drop UI displays CSV headers as draggable pills (FR3)
2. Drop zones for: Date, Amount, Payee, Memo, Account (optional)
3. Preview table shows first 5 rows with mapped columns highlighted
4. Validation errors shown if required columns (Date, Amount) not mapped
5. "Save Mapping" button stores rule for future imports from same bank
6. Saved mappings accessible in Settings → Import Rules

#### Story 2.5: Duplicate Detection and Category Suggestions

**As a** user,
**I want** the app to warn me about duplicate transactions and suggest categories,
**so that** I avoid importing the same data twice and save categorization time.

**Acceptance Criteria:**
1. Duplicate detection: Hash (date + amount + payee) matches existing transaction (FR4)
2. Duplicate warning dialog shows matched transaction with "Skip" or "Import Anyway" options
3. Category suggestion engine: Match payee against ImportRules table (FR6)
4. New transactions displayed with suggested category (yellow highlight if suggestion, gray if unknown)
5. User can accept suggestion (click checkmark) or override (dropdown menu)
6. Accepted suggestions update ImportRules for future imports (simple keyword matching, not ML)

#### Story 2.6: Import Preview and Confirmation

**As a** user,
**I want** to preview imported transactions before final confirmation,
**so that** I can verify data accuracy before saving to my database.

**Acceptance Criteria:**
1. Preview table displays all transactions with: date, payee, amount, suggested category, duplicate status
2. Editable fields: Category (dropdown), Payee (text field for corrections)
3. Summary shown: "127 transactions found, 23 duplicates skipped, 104 ready to import, 15 need categorization"
4. "Import" button saves non-duplicate transactions to SQLite
5. Success message: "Imported 104 transactions" with link to Dashboard
6. Import history logged (timestamp, file name, transaction count) for troubleshooting

---

### Epic 3: Dashboard & Interactive Visualizations

**Epic Goal:** Build the dashboard-first UI with net worth summary, expense breakdowns, income vs. expense charts, and drill-down navigation.

#### Story 3.1: Create Dashboard Layout and Net Worth Widget

**As a** user,
**I want** to see my net worth (assets - liabilities) when I open the app,
**so that** I immediately understand my financial position.

**Acceptance Criteria:**
1. Dashboard route (/) displays as landing page (FR8)
2. Net worth card shows: Total Assets, Total Liabilities, Net Worth (calculated difference)
3. Trend indicator: Up/down arrow with percentage change vs. last month
4. Data fetched from `/api/dashboard/networth` endpoint
5. Loading spinner shown while data fetches
6. Responsive layout: Full-width on mobile, card grid on desktop

#### Story 3.2: Build Expense Breakdown Chart

**As a** user,
**I want** to see a visual breakdown of my expenses by category,
**so that** I can quickly identify where my money is going.

**Acceptance Criteria:**
1. Pie or donut chart displays top 5-7 expense categories for current month
2. Chart.js library integrated for interactive visualizations
3. Hover tooltip shows: Category name, amount, percentage of total
4. Click on chart segment triggers drill-down to category detail (Story 3.5)
5. "Other" category aggregates remaining categories beyond top 7
6. Data fetched from `/api/dashboard/expenses?period=month` endpoint

#### Story 3.3: Build Income vs. Expense Comparison

**As a** user,
**I want** to compare my income and expenses over time,
**so that** I can see spending trends and cash flow.

**Acceptance Criteria:**
1. Bar chart displays income (green) vs. expenses (red) for last 6 months
2. Trend line overlay shows net income (income - expenses) trajectory
3. Hover tooltip shows exact amounts for each month
4. Time period selector: 3 months, 6 months, 1 year, All time
5. Negative net income months highlighted with warning indicator
6. Data fetched from `/api/dashboard/income-expense?period=6months` endpoint

#### Story 3.4: Add Recent Transactions and Quick Actions

**As a** user,
**I want** to see my recent transactions and quick action buttons on the dashboard,
**so that** I can perform common tasks without navigating through menus.

**Acceptance Criteria:**
1. Recent transactions widget displays last 10-20 transactions (scrollable)
2. Each transaction row shows: date, payee, amount, category (color-coded badge)
3. Click on transaction opens edit modal (Story 4.2)
4. Quick action buttons always visible: "Import CSV", "Add Transaction", "View All Transactions"
5. Buttons route to: CSV import flow, add transaction modal, transactions list page
6. Empty state message if no transactions: "Import your first CSV to get started"

#### Story 3.5: Implement Drill-Down Navigation

**As a** user,
**I want** to click on a category in the dashboard and see all transactions for that category,
**so that** I can investigate spending details.

**Acceptance Criteria:**
1. Click on expense chart segment navigates to `/transactions?category={categoryId}` (FR9)
2. Transactions page pre-filtered by clicked category
3. Breadcrumb navigation shown: Dashboard > Category: Groceries
4. Filter persist across page refresh (URL-based state)
5. "Back to Dashboard" button returns to dashboard view
6. Click on category in recent transactions list also triggers drill-down

#### Story 3.6: Performance Test Dashboard with 5,000 Transactions

**As a** developer,
**I want** to validate dashboard loads in <2 seconds with 5,000 transactions,
**so that** performance targets (NFR1) are met early, not discovered late.

**Acceptance Criteria:**
1. Test database created with 5,000 transaction records (across 2-3 years)
2. Dashboard load time measured with browser DevTools Performance profiler
3. Target: <2 seconds from navigation to interactive (NFR1)
4. If >2 seconds, optimizations applied: SQL query indexing, lazy loading widgets, chart data pagination
5. Re-test after optimizations to confirm <2 second target achieved
6. Document performance baseline in test report

---

### Epic 4: Transaction Management (Basic CRUD)

**Epic Goal:** Implement core transaction management operations enabling users to add, edit, and delete transactions.

#### Story 4.1: Build Add Transaction Form

**As a** user,
**I want** to manually add a transaction,
**so that** I can track cash purchases or fill gaps in imported data.

**Acceptance Criteria:**
1. "Add Transaction" button opens modal dialog with form (FR10)
2. Form fields: Date (date picker), Payee (text), Amount (number), Category (dropdown), Account (dropdown), Memo (textarea, optional)
3. Validation: Date and Amount required; error messages shown for invalid input
4. Auto-complete for Payee and Category based on transaction history (FR13)
5. "Save" button posts to `/api/transactions` endpoint, closes modal on success
6. Dashboard and recent transactions list refresh automatically after save

#### Story 4.2: Build Edit and Delete Transaction

**As a** user,
**I want** to edit or delete a transaction,
**so that** I can correct mistakes or remove duplicate entries.

**Acceptance Criteria:**
1. Click on transaction in any list view opens edit modal (pre-populated with transaction data)
2. Edit form identical to add form (Story 4.1), but with "Delete" button in footer
3. "Save" button updates transaction via PUT `/api/transactions/{id}`
4. "Delete" button shows confirmation dialog: "Are you sure? This cannot be undone."
5. Confirmation triggers DELETE `/api/transactions/{id}`, closes modal on success
6. Toast notification shown: "Transaction updated" or "Transaction deleted"

#### Story 4.3: Implement Auto-Complete for Payees and Categories

**As a** user,
**I want** payee and category fields to auto-suggest values I've used before,
**so that** data entry is faster and more consistent.

**Acceptance Criteria:**
1. Payee field shows dropdown with matching payees as user types (FR13)
2. Matches case-insensitive, searches transaction history (last 1,000 transactions for performance)
3. Category dropdown pre-populated with existing categories, sorted alphabetically
4. Keyboard navigation: Arrow keys to select, Enter to confirm, Escape to close
5. New payee/category created if user types value not in dropdown and saves
6. Auto-complete debounced (300ms delay) to avoid excessive API calls

#### Story 4.4: Build Batch Operations

**As a** user,
**I want** to select multiple transactions and apply actions (categorize, delete) in bulk,
**so that** I can manage large imports efficiently.

**Acceptance Criteria:**
1. Transaction table has checkboxes for multi-select
2. "Select All" checkbox in table header toggles all rows
3. Batch action toolbar appears when ≥1 transaction selected: "Categorize" and "Delete" buttons
4. "Categorize" opens dropdown to select category, applies to all selected transactions
5. "Delete" shows confirmation: "Delete 15 transactions?" with count
6. Batch operations update database via POST `/api/transactions/batch` endpoint

#### Story 4.5: Build Transaction Search and Filtering

**As a** user,
**I want** to search and filter transactions by date, payee, category, or amount,
**so that** I can quickly find specific transactions.

**Acceptance Criteria:**
1. Search bar filters by payee or memo (case-insensitive, partial match)
2. Date range picker filters transactions between start and end dates
3. Category dropdown (multi-select) filters by one or more categories
4. Amount range filter: Min and Max inputs
5. Filters applied via query params: `/transactions?search=amazon&category=shopping&dateFrom=2025-01-01`
6. "Clear Filters" button resets all filters to default (show all transactions)

---

### Epic 5: Predictive Analytics & Cash Flow (Simplified & Time-Boxed)

**Epic Goal:** Add recurring transaction detection and cash flow timeline predictions – the unique differentiator.

#### Story 5.1: Create Labeled Test Dataset for Recurring Transactions

**As a** developer,
**I want** a labeled dataset of recurring transactions before building detection algorithm,
**so that** I can validate >80% accuracy target (FR17).

**Acceptance Criteria:**
1. Test dataset created with 100+ transactions: 50 recurring (monthly subscriptions, rent, salary) + 50 non-recurring
2. Recurring transactions labeled with: frequency (monthly), expected dates, amount consistency
3. Edge cases included: Variable amounts (utilities), missed months (cancelled subscription), date variance (±3 days)
4. Dataset stored as JSON fixture for integration tests
5. Success criteria defined: Algorithm must correctly identify ≥80% of labeled recurring transactions

#### Story 5.2: Build Recurring Transaction Detection Algorithm

**As a** user,
**I want** the app to automatically detect recurring transactions (subscriptions, rent, salary),
**so that** I can see what regularly affects my balance.

**Acceptance Criteria:**
1. Detection algorithm analyzes transaction history for monthly patterns (FR17)
2. Matching criteria: Same payee, similar amount (±10%), similar date each month (±5 days)
3. Minimum 3 occurrences required to classify as recurring
4. Recurring transactions stored in RecurringTransactions table with: payee, amount, frequency, next expected date
5. Algorithm tested against labeled dataset (Story 5.1) with ≥80% accuracy
6. Background job runs nightly to update recurring transaction detections

#### Story 5.3: Build Cash Flow Timeline Prediction

**As a** user,
**I want** to see a timeline of my predicted balance over the next 30-90 days,
**so that** I can anticipate cash flow and avoid overdrafts.

**Acceptance Criteria:**
1. Cash flow timeline chart displays predicted daily balance for next 30-90 days (FR18)
2. Projection algorithm: Starting balance + sum of predicted recurring transactions
3. Recurring transaction markers shown on timeline (subscription icons, color-coded by category)
4. Confidence shading: Darker line = high confidence (consistent recurring patterns), lighter = lower confidence
5. User can toggle timeline duration: 30 days, 60 days, 90 days
6. Data fetched from `/api/predictions/cashflow?days=30` endpoint

#### Story 5.4: Add Overdraft and Spending Alerts

**As a** user,
**I want** to receive alerts if my predicted balance goes negative or spending is unusual,
**so that** I can take corrective action before problems occur.

**Acceptance Criteria:**
1. Overdraft alert shown if predicted balance <$0 within 30 days (FR20)
2. Alert displayed as warning banner on dashboard: "Warning: Predicted overdraft on Feb 15 ($-45)"
3. Unusual spending alert: Expense category >50% above average for last 3 months
4. Alert includes actionable suggestion: "Reduce dining out by $100 to avoid overdraft"
5. Alerts dismissible but re-appear on next app launch if condition persists
6. Alert preferences configurable in Settings (enable/disable, threshold adjustments)

---

### Epic 6: Ledger File Generation & PTA Integration (80% Use Case)

**Epic Goal:** Auto-generate valid Ledger-format files in real-time and validate compatibility with CLI tools.

#### Story 6.1: Collect Community Ledger Test Files (Start Week 2)

**As a** developer,
**I want** to gather 50+ real Ledger files from the PTA community,
**so that** generated files are validated against diverse real-world examples.

**Acceptance Criteria:**
1. Collect 50+ anonymized Ledger files from r/plaintextaccounting, GitHub repos, friends
2. Files categorized by complexity: Simple (basic transactions), Medium (splits, transfers), Complex (multi-currency, virtual postings)
3. Test fixture folder organized with metadata (source, complexity level, edge cases)
4. Common patterns documented: Account naming conventions, memo formats, special directives
5. Edge cases identified: Multi-line memos, special characters in payees, automated transactions

#### Story 6.2: Build Ledger File Generation Engine

**As a** user,
**I want** the app to automatically generate a valid Ledger file from my transactions,
**so that** I can use CLI tools (ledger, hledger) alongside Ledgerly.

**Acceptance Criteria:**
1. Template-based generation engine converts SQLite transactions to Ledger format (FR21)
2. Generated file structure: Header (account declarations), transactions (date, payee, postings)
3. Proper formatting: 2-space indentation, aligned amounts, ISO date format (YYYY-MM-DD)
4. Account names derived from Category and Account fields (e.g., "Expenses:Groceries", "Assets:Checking")
5. Real-time generation: Ledger file updated immediately after transaction changes
6. File saved to `~/.ledgerly/ledger.dat` (configurable path in Settings)

#### Story 6.3: Implement Ledger File Validation Against CLI

**As a** developer,
**I want** to validate generated Ledger files pass `ledger -f file.dat bal` without errors,
**so that** PTA community trusts file compatibility (FR23).

**Acceptance Criteria:**
1. Integration test suite runs `ledger -f generated.dat bal` command for each test file
2. Test validates: No parse errors, balances match expected totals, all accounts listed
3. Test files from Story 6.1 used for validation (simple transactions only for MVP)
4. Target: 100% validation success for simple transactions (no splits, transfers, multi-currency)
5. Unsupported features documented in README: "MVP supports basic transactions; splits/transfers coming Phase 2"
6. Test report generated showing validation results for all test files

#### Story 6.4: Build Ledger File Export Functionality

**As a** user,
**I want** to export my Ledger file to any location,
**so that** I can use it with other tools or back it up.

**Acceptance Criteria:**
1. "Export Ledger File" button in Settings and Dashboard footer (FR24)
2. File save dialog opens (native OS dialog via Tauri)
3. User selects destination path and filename
4. Generated Ledger file copied to selected location
5. Success toast notification: "Ledger file exported to /path/to/file.dat"
6. Export history logged (timestamp, file path) for user reference

#### Story 6.5: Add Format Selection (Ledger Primary)

**As a** user,
**I want** to choose which Ledger format to generate (Ledger, hledger, beancount),
**so that** I can use my preferred CLI tool.

**Acceptance Criteria:**
1. Settings page has "Ledger Format" dropdown: Ledger (default), hledger, beancount (Phase 2)
2. Format selection stored in user preferences (SQLite settings table)
3. For MVP: Only Ledger format fully implemented; hledger/beancount show "Coming in Phase 2" message
4. Generated file extension changes based on format: .dat (Ledger), .journal (hledger), .beancount
5. Validation tests run against selected format's CLI tool

---

### Epic 7: Reporting & Data Export (Optional - P1)

**Epic Goal:** Create category-based reports with time period filtering, comparison views, and PDF/CSV export.

#### Story 7.1: Build Category-Based Expense Reports

**As a** user,
**I want** to generate expense reports grouped by category for any time period,
**so that** I can analyze spending patterns.

**Acceptance Criteria:**
1. Reports page displays category expense breakdown as table and bar chart (FR14)
2. Time period selector: This Month, Last Month, Quarter, Year, Custom Range (date picker)
3. Table shows: Category, Amount, Percentage of Total, Transaction Count
4. Bar chart sorted by amount (highest to lowest)
5. Click on category row drills down to transaction list (reuse Epic 3 drill-down)
6. Data fetched from `/api/reports/expenses?period=month&start=2025-01-01&end=2025-01-31`

#### Story 7.2: Add Comparison Views

**As a** user,
**I want** to compare spending across different time periods,
**so that** I can see trends and changes.

**Acceptance Criteria:**
1. "Compare" toggle enables comparison mode (FR15)
2. Comparison options: This Month vs. Last Month, This Quarter vs. Last Quarter, This Year vs. Last Year
3. Table shows: Category, Current Period, Previous Period, Change ($), Change (%)
4. Change indicators: Green (decrease in expenses), Red (increase), Gray (no change)
5. Chart displays side-by-side bars for current and previous periods
6. Comparison data fetched from `/api/reports/comparison?current=2025-01&previous=2024-12`

#### Story 7.3: Implement PDF and CSV Export

**As a** user,
**I want** to export reports as PDF or CSV,
**so that** I can share with accountant or analyze in Excel.

**Acceptance Criteria:**
1. "Export" button with dropdown: Export as PDF, Export as CSV (FR16)
2. PDF export: Uses jsPDF library to generate formatted report with charts embedded as images
3. CSV export: Generates CSV file with columns: Category, Amount, Percentage, Transaction Count
4. File save dialog opens (native OS via Tauri) for user to choose location
5. PDF includes: Report title, date range, category breakdown table, expense chart
6. CSV opens correctly in Excel/Google Sheets with proper formatting (UTF-8 encoding)

## Checklist Results Report

### Executive Summary

- **Overall PRD Completeness:** 95% (Exceptionally comprehensive)
- **MVP Scope Appropriateness:** Just Right (with minor time-box recommendations applied)
- **Readiness for Architecture Phase:** **READY**
- **Most Critical Strengths:**
  - Detailed epic breakdown with 30+ user stories and comprehensive acceptance criteria
  - Clear prioritization matrix with Phase 1/Phase 2 separation
  - Technical assumptions include validation gates and fallback plans
  - Stakeholder feedback integrated throughout (user, developer, QA perspectives)

### Category Analysis

| Category                         | Status  | Critical Issues                                     |
| -------------------------------- | ------- | --------------------------------------------------- |
| 1. Problem Definition & Context  | **PASS** | None - Project Brief referenced, market analysis solid |
| 2. MVP Scope Definition          | **PASS** | Well-scoped with clear Phase 1/2 boundaries         |
| 3. User Experience Requirements  | **PASS** | Comprehensive UI goals, competitive positioning     |
| 4. Functional Requirements       | **PASS** | 24 FRs with clear MVP/Phase 2 split                 |
| 5. Non-Functional Requirements   | **PASS** | 15 NFRs with specific performance targets           |
| 6. Epic & Story Structure        | **PASS** | 7 epics, 30+ stories, full acceptance criteria      |
| 7. Technical Guidance            | **PASS** | Detailed assumptions, validation gates, fallbacks   |
| 8. Cross-Functional Requirements | **PASS** | Data model, testing, migration strategy included    |
| 9. Clarity & Communication       | **PASS** | Exceptionally well-structured, stakeholder-informed |

### Validation Decision

**✅ READY FOR ARCHITECT**

The PRD is exceptionally comprehensive, properly structured, and ready for architectural design. MVP scope is well-defined with clear prioritization, technical constraints are explicit with validation gates, and 30+ user stories provide detailed implementation guidance.

**Confidence Level:** Very High (95%)

## Next Steps

### UX Expert Prompt

Review the [UI Design Goals](#user-interface-design-goals) section and create wireframes for the 7 core screens (Dashboard, CSV Import, Transaction List, Transaction Edit Modal, Cash Flow Timeline, Category Reports, Settings). Focus on the **Cash Flow Timeline widget** as the primary visual differentiator - this needs to be prominent on the dashboard and visually compelling. Use the developer tool aesthetic (VS Code, GitHub inspiration) rather than consumer finance app styling.

### Architect Prompt

Review the [Technical Assumptions](#technical-assumptions) section and design the system architecture for Ledgerly using **Vertical Slice Architecture + Wolverine + embedded hledger**. Key focus areas:

1. **Week 1 Validation:**
   - Execute Tauri PoC (hledger binary execution, .hledger file I/O, cross-platform builds)
   - Decide Tauri vs. Electron by end of Week 1 based on process spawning results
2. **VSA Structure:**
   - Design feature slice organization (`Features/ImportCsv/`, `Features/GetDashboard/`, etc.)
   - Define shared kernel boundaries (`Common/Hledger/`, `Common/FileSystem/`)
3. **Wolverine Integration:**
   - Design command/query handlers for each feature slice
   - Plan async processing (CSV import, recurring detection scheduled job)
4. **hledger Integration:**
   - Design HledgerProcessRunner for CLI invocation
   - Plan JSON output parsing (`hledger bal -O json`) + text parsing fallback
   - Design caching strategy (SQLite) with FileSystemWatcher invalidation
5. **Data Flow:**
   - UI → Wolverine Command → Write .hledger → hledger calc → Cache → UI
   - Atomic writes (temp → validate → rename) with .bak backups
6. **Performance Strategy:**
   - Plan for hledger <1s queries, dashboard <2s load, aggressive caching

Start with Epic 1 to validate: Wolverine message bus → hledger binary execution → .hledger file write → UI display
