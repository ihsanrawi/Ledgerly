# Requirements

## Functional Requirements

Based on the Project Brief's MVP scope and target user workflows, here are the functional requirements:

### MVP Requirements (Phase 1)

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

### Phase 2 Requirements (Post-MVP)

**FR5:** The system shall normalize payee names (e.g., "AMAZON.COM AMZN.COM/BILL" → "Amazon") *(Deferred: Complex bank-specific patterns; users can manually correct in MVP)*

**FR7:** The system shall learn from user corrections to improve future categorization accuracy (target: 80% after 3 months) *(Deferred: Requires ML pipeline; MVP will collect training data via FR6)*

**FR19:** The system shall provide confidence scoring for predictions based on historical consistency *(Deferred: Extends FR18; add after validating base predictions)*

**FR22:** The system shall provide command palette interface for keyboard-driven power users *(Deferred: UI complexity; focus on core workflows first)*

**FR25:** The system shall support multi-user read-only access for household members (Persona 3) *(Deferred to Phase 2: Requires authentication, authorization, and role-based UI; MVP focuses on solo power-user workflows)*

## Non-Functional Requirements

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
