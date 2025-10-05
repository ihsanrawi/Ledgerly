# Epic Details

## Epic 1: Foundation & Core Infrastructure

**Epic Goal:** Establish Wolverine + hledger + Tauri integration with VSA folder structure and basic .hledger file operations to validate full stack end-to-end.

### Story 1.1: Set Up VSA Project Structure with Wolverine

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
7. OpenAPI specification configured with Swashbuckle.AspNetCore, spec exported to `docs/api/openapi.yaml`, Swagger UI accessible at `/swagger`

### Story 1.2: Integrate and Validate hledger Binary

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

### Story 1.3: Set Up Wolverine Test Harness and Testing Infrastructure

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

### Story 1.4: Build hledger File Writer and Atomic Operations

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

### Story 1.5: Build Simple hledger Balance Display UI

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

## Epic 2: CSV Import & Smart Data Entry

**Epic Goal:** Enable users to import bank CSV files with intelligent column detection, manual mapping fallback, duplicate detection, and category suggestion rules.

### Story 2.1: Collect Bank CSV Test Samples

**As a** developer,
**I want** to gather 20+ real bank CSV formats before starting development,
**so that** CSV parser handles diverse formats and edge cases.

**Acceptance Criteria:**
1. Collect 20+ anonymized bank CSV samples from community (r/plaintextaccounting, friends, family)
2. Document CSV format variations: delimiter (comma, semicolon, tab), date formats, encoding (UTF-8, ISO-8859-1)
3. Create test fixture folder with categorized CSVs (standard, edge cases, malformed)
4. Identify common column names: "Date", "Description", "Amount", "Debit", "Credit", "Balance"
5. Edge cases documented: negative amounts as parentheses, multi-line memos, special characters

### Story 2.2: Build CSV Upload and Parsing

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

### Story 2.3: Automatic Column Detection

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

### Story 2.4: Manual Column Mapping Interface

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

### Story 2.5: Duplicate Detection and Category Suggestions

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

### Story 2.6: Import Preview and Confirmation

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

## Epic 3: Dashboard & Interactive Visualizations

**Epic Goal:** Build the dashboard-first UI with net worth summary, expense breakdowns, income vs. expense charts, and drill-down navigation.

### Story 3.1: Create Dashboard Layout and Net Worth Widget

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

### Story 3.2: Build Expense Breakdown Chart

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

### Story 3.3: Build Income vs. Expense Comparison

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

### Story 3.4: Add Recent Transactions and Quick Actions

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

### Story 3.5: Implement Drill-Down Navigation

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

### Story 3.6: Performance Test Dashboard with 5,000 Transactions

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

## Epic 4: Transaction Management (Basic CRUD)

**Epic Goal:** Implement core transaction management operations enabling users to add, edit, and delete transactions.

### Story 4.1: Build Add Transaction Form

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

### Story 4.2: Build Edit and Delete Transaction

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

### Story 4.3: Implement Auto-Complete for Payees and Categories

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

### Story 4.4: Build Batch Operations

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

### Story 4.5: Build Transaction Search and Filtering

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

## Epic 5: Predictive Analytics & Cash Flow (Simplified & Time-Boxed)

**Epic Goal:** Add recurring transaction detection and cash flow timeline predictions – the unique differentiator.

### Story 5.1: Create Labeled Test Dataset for Recurring Transactions

**As a** developer,
**I want** a labeled dataset of recurring transactions before building detection algorithm,
**so that** I can validate >80% accuracy target (FR17).

**Acceptance Criteria:**
1. Test dataset created with 100+ transactions: 50 recurring (monthly subscriptions, rent, salary) + 50 non-recurring
2. Recurring transactions labeled with: frequency (monthly), expected dates, amount consistency
3. Edge cases included: Variable amounts (utilities), missed months (cancelled subscription), date variance (±3 days)
4. Dataset stored as JSON fixture for integration tests
5. Success criteria defined: Algorithm must correctly identify ≥80% of labeled recurring transactions

### Story 5.2: Build Recurring Transaction Detection Algorithm

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

### Story 5.3: Build Cash Flow Timeline Prediction

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

### Story 5.4: Add Overdraft and Spending Alerts

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

## Epic 6: Ledger File Generation & PTA Integration (80% Use Case)

**Epic Goal:** Auto-generate valid Ledger-format files in real-time and validate compatibility with CLI tools.

### Story 6.1: Collect Community Ledger Test Files (Start Week 2)

**As a** developer,
**I want** to gather 50+ real Ledger files from the PTA community,
**so that** generated files are validated against diverse real-world examples.

**Acceptance Criteria:**
1. Collect 50+ anonymized Ledger files from r/plaintextaccounting, GitHub repos, friends
2. Files categorized by complexity: Simple (basic transactions), Medium (splits, transfers), Complex (multi-currency, virtual postings)
3. Test fixture folder organized with metadata (source, complexity level, edge cases)
4. Common patterns documented: Account naming conventions, memo formats, special directives
5. Edge cases identified: Multi-line memos, special characters in payees, automated transactions

### Story 6.2: Build Ledger File Generation Engine

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

### Story 6.3: Implement Ledger File Validation Against CLI

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

### Story 6.4: Build Ledger File Export Functionality

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

### Story 6.5: Add Format Selection (Ledger Primary)

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

## Epic 7: Reporting & Data Export (Phase 2 - Deferred)

**Deferral Decision Date:** 2025-10-05
**Rationale:** Deferred to Phase 2 to allocate full 2 weeks to Epic 5 (predictions/recurring detection - the core differentiator). Users can use hledger CLI for reports in MVP. This epic will be reintroduced post-MVP based on user feedback priority.

**Epic Goal:** Create category-based reports with time period filtering, comparison views, and PDF/CSV export.

### Story 7.1: Build Category-Based Expense Reports

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

### Story 7.2: Add Comparison Views

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

### Story 7.3: Implement PDF and CSV Export

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
