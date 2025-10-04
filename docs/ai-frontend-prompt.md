# AI-Optimized Frontend Generation Prompt for Ledgerly

This document contains a comprehensive, copy-paste-ready prompt for AI UI generation tools (v0.dev, Lovable.ai, etc.) to scaffold Ledgerly's frontend application.

---

## 📋 COMPLETE AI FRONTEND PROMPT

```markdown
# Ledgerly - Personal Finance Manager (Dashboard & Core UI Components)

## HIGH-LEVEL GOAL
Create a responsive, local-first personal finance manager dashboard with interactive data visualizations, CSV import workflow, and transaction management. The application uses a **developer tool aesthetic** (VS Code/GitHub-inspired) rather than consumer finance app styling, prioritizing clean typography, dark mode support, and professional polish.

## DETAILED STEP-BY-STEP INSTRUCTIONS

### 1. Project Setup & Tech Stack
1. Create a new Angular 17+ standalone component project
2. Use Angular Material for base UI components (buttons, forms, dialogs, tables)
3. Install Chart.js for interactive data visualizations
4. Configure Tailwind CSS for utility-first styling
5. Set up Angular Signals for reactive state management
6. Use Angular HttpClient for API communication (all endpoints localhost-based)
7. Configure routing with Angular Router for SPA navigation

### 2. Dashboard Landing Page (Primary View)
1. Create a **Dashboard Component** as the default landing route (`/`)
2. Implement a **responsive grid layout** with the following widgets (all visible without scrolling on desktop):
   - **Net Worth Summary Card** (top-left): Display Total Assets, Total Liabilities, Net Worth with trend indicator (↑/↓ arrow + % change vs last month)
   - **Cash Flow Timeline Widget** (top-center, PROMINENT): Line chart showing predicted balance for next 30-90 days with recurring transaction markers (subscriptions, rent, salary icons). Include confidence shading (darker = high confidence, lighter = uncertain predictions)
   - **Expense Breakdown Chart** (mid-left): Pie/donut chart of top 5-7 expense categories for current month. Clicking a segment drills down to filtered transaction list
   - **Income vs Expense Bar Chart** (mid-right): Bar chart for last 6 months with green (income) and red (expense) bars, trend line overlay
   - **Recent Transactions List** (bottom): Table showing last 10-20 transactions (date, payee, amount, category badge)
3. Add **Floating Action Button (FAB)** - bottom-right corner with two stacked buttons:
   - Primary: "Import CSV" (teal accent color, larger)
   - Secondary: "Add Transaction" (smaller, below primary)
4. Implement **time period selector** (persistent dropdown): This Month, Last Month, Quarter, Year, Custom Range
5. All charts should be **interactive**: hover tooltips, click to drill down to detail views
6. **Empty state**: If no transactions exist, show "Import your first CSV to get started" message with prominent Import button

### 3. CSV Import Flow (Multi-Step Wizard)
1. Create **CSV Import Modal/Page** with 4 steps:

   **Step 1: Upload**
   - Drag-and-drop zone accepting `.csv` files (support multiple files simultaneously)
   - File picker button as fallback
   - Show file names and sizes after upload
   - Progress indicator for parsing large files (>1000 rows)

   **Step 2: Column Mapping**
   - Auto-detect columns (Date, Amount, Payee, Memo) with confidence indicators:
     - Green checkmark = high confidence (>90%)
     - Yellow warning = needs review
   - If low confidence, show **manual mapping UI**: Draggable column headers to drop zones (Date, Amount, Payee, Memo fields)
   - Preview table showing first 5 rows with mapped columns highlighted
   - "Save Mapping" option to store rule for future imports

   **Step 3: Duplicate Detection & Preview**
   - Display preview table with ALL transactions (200-300 rows, virtualized scrolling)
   - Show duplicate warnings: Banner "23 potential duplicates detected" with expandable list
   - Each duplicate shows side-by-side comparison (existing vs new) with differences highlighted in yellow
   - Confidence badges: Green (exact match), Yellow (likely), Gray (uncertain)
   - User actions per duplicate: "Skip This", "Import Anyway", "Skip All Remaining"
   - Category suggestions column with confidence indicators and editable dropdown
   - Summary footer: "127 found, 23 duplicates skipped, 104 ready to import, 15 need categorization"

   **Step 4: Confirmation**
   - Final review summary with transaction count breakdown
   - "Import" button triggers API call (`POST /api/import/csv`)
   - Success toast: "Imported 198 transactions" with link to Dashboard
   - Learning indicator message: "Ledgerly learns from your corrections to improve future imports"

### 4. Transaction Management Views
1. **Transaction List Page** (`/transactions`):
   - Filterable data table with columns: Date, Payee, Amount, Category (colored badge), Account, Actions
   - Search bar (filters by payee/memo, case-insensitive)
   - Filter controls: Date range picker, category multi-select dropdown, amount range (min/max)
   - Checkbox column for multi-select with "Select All" in header
   - Batch action toolbar (appears when ≥1 selected): "Categorize", "Delete" buttons
   - Click any row to open edit modal
   - Pagination or virtual scrolling for large datasets
   - Breadcrumb when accessed via drill-down: `Dashboard > Expenses: Groceries`

2. **Transaction Edit Modal** (overlay):
   - Form fields: Date (date picker), Payee (text with autocomplete), Amount (number), Category (dropdown with autocomplete), Account (dropdown), Memo (textarea)
   - Validation: Date and Amount required (show error messages)
   - Autocomplete suggestions based on history (last 1000 transactions)
   - Footer buttons: "Save", "Delete" (with confirmation dialog), "Cancel"
   - Toast notifications: "Transaction updated" / "Transaction deleted"

3. **Add Transaction Modal** (same form as edit, but blank):
   - Opened via FAB or "Add Transaction" quick action
   - Same fields as edit modal
   - Auto-focus on Date field when opened

### 5. Cash Flow Timeline View (Unique Differentiator)
1. Create **Cash Flow Page** (`/cash-flow`) as primary navigation item
2. Implement **interactive timeline chart** (Chart.js line chart):
   - X-axis: Dates (next 30-90 days)
   - Y-axis: Predicted balance
   - Confidence shading: Gradient from darker (high confidence) to lighter (uncertain)
   - Recurring transaction markers: Icons on timeline (subscription 📱, rent 🏠, salary 💰) with color-coded category badges
3. **Duration toggle buttons**: 30 Days, 60 Days, 90 Days (selected state highlighted)
4. **Hover tooltips**: Show exact balance on hover ("Balance: $8,340 on Feb 28")
5. **Overdraft alerts**: Red warning banner if balance goes negative ("Predicted overdraft on Feb 15 ($-45)") with actionable suggestion ("Reduce dining by $100 to avoid overdraft")
6. **Recurring transaction detail modal**: Click any marker to open modal showing:
   - Payee, Amount, Frequency (monthly/weekly), Next Expected Date
   - Edit actions: Change amount, Mark as cancelled
   - Real-time timeline update when edited
7. **Empty/insufficient data state**: "No recurring transactions found. Import 3+ months of data for predictions."

### 6. Category Reports View
1. Create **Reports Page** (`/reports`):
   - Time period selector (This Month, Last Month, Quarter, Year, Custom Date Range)
   - Category breakdown table: Category, Amount, % of Total, Transaction Count
   - Bar chart sorted by amount (highest to lowest)
   - "Compare" toggle for previous period comparison (side-by-side bars: current vs previous)
   - Comparison table adds columns: Previous Period, Change ($), Change (%)
   - Change indicators: Green (decrease), Red (increase), Gray (no change)
   - Export buttons: "Export PDF", "Export CSV" (prominent, always visible)
   - Click category row to drill down to filtered transaction list

### 7. Settings Page
1. Create **Settings Page** (`/settings`) with tabs:
   - **Accounts**: Add/edit bank accounts (name, type, initial balance)
   - **Categories**: Create, rename, delete custom categories (tree view if nested supported)
   - **Import Rules**: View/edit learned categorization rules (table: Payee Pattern, Category, Confidence)
   - **.hledger File Settings**: Show real-time .hledger file preview with syntax highlighting (monospace font), "Copy .hledger File" button (file save dialog)
   - **Export**: One-click "Export .hledger File" button (prominent placement)

### 8. Navigation & Layout
1. **Left Sidebar Navigation** (persistent, always visible on desktop):
   - Icon + label for each item (collapse to icon-only on screens <1366px)
   - Items: Dashboard (home icon), Transactions (list icon), Reports (chart-bar icon), Cash Flow (trending-up icon), Settings (gear icon)
   - Active state: Highlighted background + teal accent border
   - Hover states for non-active items

2. **Top Header Bar**:
   - App logo/name: "Ledgerly" (left side)
   - Time period selector (center, persistent across views)
   - Dark mode toggle (right side)
   - No cloud/sync icons (reinforces offline-first commitment)

3. **Mobile Responsive Breakpoints**:
   - Large desktop (1920px+): Multi-column dashboard, sidebar expanded
   - Standard desktop (1366-1920px): Two-column dashboard, sidebar expanded
   - Small desktop/laptop (1024-1366px): Single column, sidebar icon-only
   - Tablet/Mobile: Out of scope for MVP (show message: "Desktop version recommended")

### 9. Visual Design System
1. **Color Palette**:
   - Primary: Deep blue (#2C3E50) for headers, primary actions
   - Accent: Teal (#1ABC9C) for growth indicators, FAB, positive cash flow
   - Danger: Red (#E74C3C) for overdrafts, warnings, negative values
   - Success: Green (#27AE60) for savings, positive trends, increase indicators
   - Neutral: Light gray (#ECF0F1), Dark gray (#34495E) for backgrounds/borders

2. **Typography**:
   - Headings: Inter or Roboto (sans-serif, weights 500-700)
   - Body text: System font stack for native feel
   - Data/amounts: Monospace font (JetBrains Mono or Consolas) for alignment
   - .hledger file preview: Monospace with syntax highlighting

3. **Component Styling**:
   - Rounded corners: 4px (buttons), 8px (cards)
   - Shadows: Subtle elevation (Material Design depth)
   - Spacing: 8px base unit, generous whitespace
   - Focus states: 2px teal outline for keyboard navigation (WCAG AA)
   - No cutesy illustrations or mascots - professional, data-focused aesthetic

4. **Dark Mode Support**:
   - Toggle in header (persistent across sessions)
   - Dark backgrounds: #1E1E1E (VS Code-inspired)
   - Light text: #E0E0E0
   - Muted chart colors for dark mode (adjust saturation)

### 10. Accessibility (WCAG AA Compliance)
1. Full keyboard navigation: Tab, Enter, Arrow keys, Escape
2. ARIA labels on all interactive elements (charts, buttons, form fields)
3. Color contrast: 4.5:1 for body text, 3:1 for large text
4. Focus indicators: Clear 2px teal outline
5. Screen reader support: Chart text summaries (e.g., "Expense breakdown: Groceries $450, Dining $230...")
6. Skip to main content link for keyboard users

## CODE EXAMPLES, DATA STRUCTURES & CONSTRAINTS

### API Endpoints (All Localhost)
```typescript
// Dashboard
GET /api/dashboard/networth → { assets: number, liabilities: number, netWorth: number, trend: number }
GET /api/dashboard/expenses?period=month → { categories: Array<{name, amount, percentage}> }
GET /api/dashboard/income-expense?period=6months → { months: Array<{month, income, expenses}> }
GET /api/dashboard/recent → Array<{id, date, payee, amount, category}>

// Transactions
GET /api/transactions?search=&category=&dateFrom=&dateTo= → Array<Transaction>
POST /api/transactions → Transaction
PUT /api/transactions/:id → Transaction
DELETE /api/transactions/:id → void
POST /api/transactions/batch → { categorized: number, deleted: number }

// CSV Import
POST /api/import/csv → { imported: number, duplicates: number, needsCategorization: number }

// Cash Flow Predictions
GET /api/predictions/cashflow?days=30 → { timeline: Array<{date, balance, confidence}>, recurring: Array<RecurringTransaction> }
GET /api/predictions/recurring → Array<{id, payee, amount, frequency, nextDate}>
PUT /api/predictions/recurring/:id → RecurringTransaction

// Reports
GET /api/reports/expenses?period=month&start=&end= → { categories: Array<{name, amount, percentage, count}> }
GET /api/reports/comparison?current=2025-01&previous=2024-12 → { categories: Array<{name, current, previous, change}> }
```

### TypeScript Interfaces
```typescript
interface Transaction {
  id: string;
  date: string; // ISO 8601
  payee: string;
  amount: number; // Negative for expenses, positive for income
  category: string;
  account: string;
  memo?: string;
}

interface RecurringTransaction {
  id: string;
  payee: string;
  amount: number;
  frequency: 'weekly' | 'monthly' | 'quarterly' | 'yearly';
  nextExpectedDate: string;
  confidence: number; // 0-100
}

interface DuplicateDetection {
  existingTransaction: Transaction;
  newTransaction: Transaction;
  confidence: 'exact' | 'likely' | 'possible'; // exact=100%, likely=95%, possible=80%
  differences: string[]; // ["amount", "date"]
}
```

### Styling Constraints
- Use **Tailwind CSS utility classes** for all styling (no custom CSS unless absolutely necessary)
- Angular Material components MUST be restyled to match Ledgerly color palette (override theme variables)
- Chart.js configuration: `responsive: true`, `maintainAspectRatio: false`, `interaction.mode: 'nearest'`
- DO NOT use bright pastels, gradients, or playful colors (this is not YNAB or Mint)
- DO NOT add emojis or illustrations in UI (only in recurring transaction icons where specified)
- Dark mode toggle MUST persist selection to localStorage: `theme: 'light' | 'dark'`

### Data Validation Rules
- Date format: ISO 8601 (YYYY-MM-DD) sent to API
- Amount precision: 2 decimal places (e.g., $45.23, not $45.2 or $45.234)
- Category names: Max 50 characters, alphanumeric + spaces/dashes only
- Payee names: Max 100 characters
- CSV file size limit: 10MB (show error if exceeded)
- Transaction history autocomplete: Last 1000 transactions max (performance)

## STRICT SCOPE DEFINITION

### Files to Create (All New - No Existing Codebase)
1. **Components:**
   - `app/dashboard/dashboard.component.ts` (+ .html, .css)
   - `app/transactions/transaction-list.component.ts` (+ .html, .css)
   - `app/transactions/transaction-edit-modal.component.ts` (+ .html, .css)
   - `app/csv-import/csv-import-wizard.component.ts` (+ .html, .css)
   - `app/cash-flow/cash-flow-timeline.component.ts` (+ .html, .css)
   - `app/reports/category-reports.component.ts` (+ .html, .css)
   - `app/settings/settings.component.ts` (+ .html, .css)
   - `app/shared/sidebar-nav.component.ts` (+ .html, .css)
   - `app/shared/header.component.ts` (+ .html, .css)

2. **Services:**
   - `app/services/api.service.ts` (HttpClient wrapper for all API calls)
   - `app/services/theme.service.ts` (dark mode toggle logic)
   - `app/services/transaction.service.ts` (transaction state management with Signals)

3. **Routing:**
   - `app/app.routes.ts` (Angular standalone routes)

4. **Configuration:**
   - `tailwind.config.js` (Ledgerly color palette)
   - `angular.json` (Chart.js, Angular Material theming)

### DO NOT Create/Modify
- Backend API code (out of scope - assume localhost endpoints exist)
- Database schemas or SQLite logic
- .hledger file generation logic (handled by backend)
- Authentication/login (MVP is single-user local app)
- Mobile-specific layouts (MVP is desktop-only)
- E2E tests (focus on functional UI only)

### Critical Constraints
- **Mobile-First Approach**: Design all components with mobile-first responsive patterns (start with mobile layout, then scale up for tablet/desktop breakpoints)
- **Local-First**: No cloud/sync features visible in UI (no "Sync" buttons, no cloud icons, no loading spinners for external APIs)
- **Offline-First**: All features must work without internet connection (no external CDN dependencies in production build)
- **Performance**: Dashboard MUST render in <2 seconds for 5,000 transactions (use lazy loading, virtual scrolling, memoization)
- **Accessibility**: WCAG AA compliance is non-negotiable (test with keyboard navigation and screen reader)

---

## MOBILE-FIRST RESPONSIVE DESIGN SPECIFICATIONS

### Mobile Layout (320px - 767px)
1. **Navigation**: Bottom tab bar instead of sidebar (4 tabs: Dashboard, Transactions, Cash Flow, More)
2. **Dashboard**: Single column, stacked widgets, FAB repositioned to bottom-center
3. **Charts**: Full-width, scroll vertically, touch-optimized interactions (tap instead of hover)
4. **Transaction List**: Card-based layout (not table), swipe actions for edit/delete
5. **CSV Import**: Full-screen wizard (not modal), step indicator at top

### Tablet Layout (768px - 1023px)
1. **Navigation**: Left sidebar (icon + label), auto-hide on scroll
2. **Dashboard**: Two-column grid for widgets
3. **Charts**: Side-by-side for comparison views
4. **Transaction List**: Table with fewer columns (hide Account, Memo on smaller screens)

### Desktop Layout (1024px+)
1. **Navigation**: Persistent left sidebar
2. **Dashboard**: Multi-column grid (3 columns on 1920px+)
3. **Charts**: Full-featured tooltips, keyboard navigation
4. **Transaction List**: Full table with all columns

### Breakpoint-Specific Tailwind Classes
```html
<!-- Example: Responsive Dashboard Grid -->
<div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
  <div class="col-span-1 md:col-span-2 xl:col-span-2">Cash Flow Timeline (Prominent)</div>
  <div class="col-span-1">Net Worth Card</div>
  <div class="col-span-1">Expense Breakdown</div>
  <div class="col-span-1">Income vs Expense</div>
  <div class="col-span-1 md:col-span-2 xl:col-span-3">Recent Transactions</div>
</div>
```

---

## ADDITIONAL CONTEXT & RATIONALE

### Why This Approach?
- **Structured Framework**: Four-part prompt (Goal → Instructions → Examples → Scope) maximizes AI code generation quality by providing clear boundaries and context
- **Mobile-First**: Ensures responsive design from the start, preventing desktop-only layouts that break on smaller screens
- **Developer Aesthetic**: Differentiates Ledgerly from consumer finance apps (YNAB, Mint) by targeting technical users who value clean, data-focused UI
- **PTA Authenticity**: .hledger file visibility and export prominence builds trust with Plain Text Accounting community
- **Predictive Analytics**: Cash Flow Timeline is unique selling point - must be visually prominent and easy to understand

### Key Design Decisions
1. **Dashboard-First Navigation**: Users see insights immediately (not transaction lists) - aligns with "Show me my money, then let me explore" principle
2. **Confidence Indicators**: Green/yellow/gray badges build trust in AI suggestions and duplicate detection
3. **Learning Indicator**: "Ledgerly learns from your corrections" message reinforces competitive advantage over static rule-based categorization
4. **Offline-First UI**: No cloud icons or sync buttons in MVP - visual cue of privacy commitment
5. **Monospace for Data**: Financial amounts and .hledger previews use monospace fonts for readability and professional aesthetic

---

## ⚠️ IMPORTANT REMINDER

**All AI-generated code requires careful human review, testing, and refinement to be considered production-ready.**

After generation:
1. **Review** all component logic for edge cases (empty states, error handling)
2. **Test** accessibility (keyboard navigation, screen reader, color contrast)
3. **Validate** responsive breakpoints on real devices
4. **Optimize** performance (lazy loading, virtual scrolling for large datasets)
5. **Refine** dark mode styling (test all components in both themes)
6. **Audit** API integration points (ensure proper error handling for network failures)

This prompt provides a comprehensive foundation, but human expertise is essential for production quality.
```

---

## 📊 Prompt Structure Overview

This prompt follows the **four-part structured framework**:

1. ✅ **High-Level Goal**: Dashboard-driven finance manager with developer aesthetic
2. ✅ **Detailed, Step-by-Step Instructions**: 10 sections with granular implementation steps
3. ✅ **Code Examples, Data Structures & Constraints**: TypeScript interfaces, API endpoints, validation rules
4. ✅ **Strict Scope Definition**: Files to create, what NOT to modify, critical constraints

---

## 🎯 How to Use This Prompt

### Option 1: Full Application Generation
Copy the entire prompt (from "Ledgerly - Personal Finance Manager" through "IMPORTANT REMINDER") and paste into your AI UI generation tool.

### Option 2: Component-by-Component Generation
Generate components incrementally in this recommended order:

1. **Start with Dashboard** (Section 2)
   - Validates overall layout, navigation, and design system
   - Tests Chart.js integration early

2. **CSV Import Flow** (Section 3)
   - Most complex workflow - better to tackle early
   - Tests form handling, multi-step wizard patterns

3. **Transaction Management** (Section 4)
   - Builds on dashboard drill-down navigation
   - Tests CRUD operations and autocomplete

4. **Cash Flow Timeline** (Section 5)
   - Unique differentiator - requires custom Chart.js configuration
   - Tests prediction visualization patterns

5. **Reports & Settings** (Sections 6-7)
   - Lower priority - can be deferred if timeline slips

### Option 3: Focused Prompts
For specific components, extract relevant sections:
- **Dashboard only**: Sections 1, 2, 8, 9
- **CSV Import only**: Sections 1, 3, 8, 9
- **Cash Flow Timeline only**: Sections 1, 5, 8, 9

---

## 🔄 Next Steps After Generation

1. **Cross-Reference with Front-End Spec**
   - Compare generated UI to user flows in [docs/front-end-spec.md](./front-end-spec.md)
   - Validate all screens from Site Map (Section: Information Architecture) are covered

2. **Accessibility Audit**
   - Run axe DevTools or Lighthouse
   - Test keyboard navigation (Tab, Enter, Escape)
   - Validate WCAG AA contrast ratios

3. **Performance Baseline**
   - Test dashboard with 5,000 transaction mock data
   - Target: <2 seconds load time (per PRD NFR1)
   - Use Chrome DevTools Performance profiler

4. **Responsive Validation**
   - Test all breakpoints: 320px, 768px, 1024px, 1366px, 1920px
   - Validate mobile-first approach scales correctly

5. **Dark Mode Testing**
   - Toggle theme and verify all components adapt
   - Check chart color palettes in dark mode

---

## 📚 Related Documentation

- **[PRD](./prd.md)**: Full product requirements and functional specifications
- **[Front-End Spec](./front-end-spec.md)**: Detailed UI/UX specifications, user flows, wireframes
- **Architecture Docs** (TBD): Backend API contracts, hledger integration patterns

---

## 🛠️ Customization Guide

If you need to modify the prompt for different AI tools or specific requirements:

### For v0.dev (Vercel)
- Add at the beginning: "Use Next.js 14 App Router instead of Angular" (if switching frameworks)
- Emphasize shadcn/ui components instead of Angular Material

### For Lovable.ai
- Emphasize React + TypeScript instead of Angular
- Specify Recharts instead of Chart.js (Lovable has better Recharts support)

### For Claude/GPT Code Generation
- Break into smaller prompts (one component per conversation)
- Add "Generate only the code, no explanations" if you want concise output
- Use "Explain your implementation choices" if you want rationale

### For Different Tech Stacks
Replace Section 1 (Tech Stack) with your preferred framework:
- **React**: "Create React 18+ functional components with hooks"
- **Vue**: "Create Vue 3 composition API components"
- **Svelte**: "Create Svelte 4+ components with stores"

Keep Sections 2-10 (UI specifications) largely unchanged - they're framework-agnostic design requirements.

---

*Generated by Sally, UX Expert Agent - [Powered by BMAD™ Core](https://github.com/anthropics/claude-code)*
