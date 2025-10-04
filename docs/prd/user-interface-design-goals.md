# User Interface Design Goals

Based on the Project Brief's target platforms (Desktop: Windows/macOS/Linux, Mobile: Phase 2) and competitive positioning analysis, here are the UI/UX design goals:

## Overall UX Vision

Ledgerly bridges the **power of CLI tools** with the **intuitiveness of consumer apps** through a dashboard-first, progressive disclosure approach. The interface prioritizes:

- **Instant gratification:** Dashboard shows insights within seconds of first CSV import
- **Progressive complexity:** Beginners see simple workflows; power users discover advanced features (keyboard shortcuts, command palette in Phase 2) naturally
- **Transparency without overwhelm:** Financial data is accessible and explorable without requiring understanding of double-entry accounting
- **Confidence through clarity:** Every action (import, categorization, prediction) shows clear rationale and allows user correction

**Design Philosophy:** "Show me my money, then let me explore" – Dashboard answers "where did my money go?" immediately, drill-downs answer "why?" on demand.

**Competitive Positioning:** *"hledger's power, YNAB's ease, your data"* – The only PTA tool using real hledger engine with predictive analytics, beautiful interface, and complete data ownership.

## Key Interaction Paradigms

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

## Core Screens and Views

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

## Accessibility: WCAG AA

- **Keyboard navigation:** Full app navigable via Tab, Enter, Arrow keys (compensates for Phase 2 command palette)
- **Screen reader support:** Semantic HTML, ARIA labels on interactive elements
- **Color contrast:** All text meets WCAG AA contrast ratios (4.5:1 for body text, 3:1 for large text)
- **Focus indicators:** Clear visual focus states for keyboard users
- **Alt text:** Charts have text summaries for screen readers (e.g., "Expense breakdown: Groceries $450, Dining $230...")

## Branding

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

## Target Device and Platforms: Web Responsive (Desktop-first)

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
