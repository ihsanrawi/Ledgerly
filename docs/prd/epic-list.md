# Epic List

Based on Vertical Slice Architecture from brief, here are the high-level epics organized by feature slices:

## Epic 1: Foundation & Core Infrastructure
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

## Epic 2: Import CSV Vertical Slice (P0 - Foundation)
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

## Epic 3: Get Dashboard Vertical Slice (P0 - Core Value)
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

## Epic 4: Manage Transactions Vertical Slice (P0 - Essential)
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

## Epic 5: Detect Recurring Vertical Slice (P1 - Delight)
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

## Epic 6: Categorize Transaction Vertical Slice (P1 - High Value)
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

## Epic 7: Reporting & Data Export (Phase 2 - Deferred)
**Goal:** Build reporting slice - query hledger for category breakdowns, time period filtering, comparison views, PDF/CSV export.

**Deferral Rationale:** Epic 7 is deferred to Phase 2 to provide additional buffer for Epic 5 (predictions - the core differentiator). Users can use hledger CLI for reports in MVP. This epic will be reintroduced post-MVP based on user feedback priority.

**Key Deliverables:**
- GetCategoryReportQuery + Handler (call `hledger bal -O json` with date filters)
- Time period filtering (month, quarter, year, custom range)
- Comparison views (this month vs last month, YoY)
- Drill-down to transactions by category (reuse GetTransactionHistory)
- PDF export (jsPDF) and CSV export
- Interactive Chart.js visualizations

**Success Criteria:** Category report generated in <5 seconds; drill-down to transactions works; PDF/CSV export well-formatted

---

## Epic Sequencing & Timeline (VSA-Based)

**Total Duration:** 12 weeks following VSA feature slice approach

| Week | Epic | Notes |
|------|------|-------|
| 1 | Epic 1: Foundation | Wolverine + hledger + Tauri PoC; VSA structure |
| 2-3 | Epic 2: Import CSV Slice | First complete vertical slice; collect 20+ CSV formats |
| 4 | Epic 3: Dashboard Slice | hledger queries + caching + FileSystemWatcher |
| 5-6 | Epic 4: Transaction CRUD Slice | .hledger rewrite strategy; 50K perf test Week 6 |
| 7-8 | Epic 6: Categorize Slice | Auto-suggest + learning rules |
| 9-10 | Epic 5: Predictions Slice (2 wks) | Epic 7 deferred to Phase 2; full 2 weeks allocated to predictions (core differentiator) |
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
