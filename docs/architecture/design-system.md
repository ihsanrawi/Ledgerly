# Ledgerly Design System

**Purpose:** Definitive UI design system for Ledgerly, establishing visual language, component patterns, and styling standards across the application.

**Source of Truth:** Based on the design samples in [docs/design-samples/](../../docs/design-samples/) covering all pages: Dashboard, Transactions, Reports, Cash Flow, Settings, and CSV Import.

**Authority:** MANDATORY for all frontend development. AI agents and human developers MUST follow these design standards.

---

## ⚠️ IMPORTANT: Design Samples vs. Implementation

**Design Samples (Reference Only):**
- **Framework:** React 18+ with TypeScript
- **Styling:** Tailwind CSS
- **Components:** shadcn/ui (Radix UI primitives)
- **Icons:** Lucide React
- **Charts:** Recharts

**Actual Implementation (Production):**
- **Framework:** Angular 17.3.8 with TypeScript
- **Styling:** SCSS + CSS Custom Properties
- **Components:** Angular Material
- **Icons:** Material Icons
- **Charts:** Chart.js / ng2-charts

**How to Use This Document:**
1. **Extract design principles** - Colors, spacing, typography, layouts
2. **Translate to Angular** - Use Angular Material equivalents for shadcn/ui components
3. **Apply patterns** - Grid layouts, responsive breakpoints, component structure
4. **Ignore framework-specific code** - React hooks, JSX syntax, Tailwind classes are reference only

**Example Translation:**

React Sample:
```tsx
<Button className="bg-accent hover:bg-accent/90">
  <Upload className="mr-2 h-5 w-5" />
  Import CSV
</Button>
```

Angular Implementation:
```html
<button mat-flat-button color="accent">
  <mat-icon class="mr-2">upload</mat-icon>
  Import CSV
</button>
```

---

**Tech Stack for Design Samples (Reference):** React 18+, TypeScript, Tailwind CSS, shadcn/ui, Lucide React, Recharts

---

## Table of Contents

1. [Design Philosophy](#design-philosophy)
2. [Design Tokens](#design-tokens)
3. [Typography](#typography)
4. [Color System](#color-system)
5. [Spacing & Layout](#spacing--layout)
6. [Component Patterns](#component-patterns)
7. [Page Layouts](#page-layouts)
8. [Responsive Design](#responsive-design)
9. [Accessibility](#accessibility)
10. [Implementation Guide](#implementation-guide)

---

## Design Philosophy

### Core Principles

Ledgerly's design embodies these principles from the [front-end-spec.md](front-end-spec.md):

1. **Developer Tool Aesthetic** - Clean, professional interface inspired by VS Code, GitHub, Linear
2. **No Patronizing Language** - Precise, professional tone (not "Woohoo!" or emojis)
3. **Progressive Disclosure** - Complex features revealed gradually through clear UI affordances
4. **Transparency Without Overwhelm** - Show confidence indicators and reasoning, but don't force engagement
5. **Monospace for Financial Data** - Monetary amounts and code use monospace fonts for clarity
6. **Data Portability First** - .hledger export prominently featured, no vendor lock-in

### Visual Language

**What Makes Ledgerly's UI Distinctive:**
- **Deep blue primary color** - Professional, trustworthy (inspired by financial institutions)
- **Teal accent** - Modern, positive (growth, savings, positive actions)
- **Subtle shadows** - Minimal elevation with `shadow-sm` for card hierarchy
- **Rounded corners** - Consistent `rounded-lg` (0.5rem / 8px) for approachability
- **Generous whitespace** - Breathing room with `space-y-6` patterns
- **Confidence indicators** - Color-coded badges/trends (green=positive, red=negative)
- **VS Code-inspired dark mode** - Deep slate backgrounds (#1C1E26) with easy theme toggle
- **Icon-driven navigation** - Lucide icons with consistent sizing

---

## Design Tokens

### CSS Custom Properties (HSL Color System)

All design tokens are defined as HSL values in `src/index.css` with automatic dark mode support via `.dark` class.

**Why HSL?** Enables programmatic color manipulation (lightness/saturation adjustments) and better dark mode support.

```css
:root {
  /* Surfaces */
  --background: 0 0% 100%;           /* White background */
  --foreground: 210 11% 15%;         /* Dark text */
  --card: 0 0% 100%;                 /* Card background */
  --card-foreground: 210 11% 15%;    /* Card text */

  /* Brand Colors */
  --primary: 210 29% 24%;            /* Deep blue - professional, trustworthy */
  --primary-foreground: 0 0% 100%;   /* White text on primary */

  --secondary: 210 17% 95%;          /* Light neutral */
  --secondary-foreground: 210 11% 15%;

  --muted: 210 17% 95%;              /* Muted backgrounds */
  --muted-foreground: 215 14% 34%;   /* Muted text */

  /* Accent Colors */
  --accent: 168 76% 42%;             /* Teal - positive actions, growth */
  --accent-foreground: 0 0% 100%;

  /* Semantic Colors */
  --destructive: 0 65% 51%;          /* Red - warnings, overdrafts, negative trends */
  --destructive-foreground: 0 0% 100%;

  --success: 145 63% 42%;            /* Green - success, savings, positive trends */
  --success-foreground: 0 0% 100%;

  /* Borders & Inputs */
  --border: 210 20% 90%;
  --input: 210 20% 90%;
  --ring: 168 76% 42%;               /* Focus ring uses accent teal */

  /* Border Radius */
  --radius: 0.5rem;                  /* 8px - consistent across all components */

  /* Chart Colors */
  --chart-1: 168 76% 42%;            /* Teal */
  --chart-2: 210 29% 24%;            /* Deep blue */
  --chart-3: 145 63% 42%;            /* Green */
  --chart-4: 0 65% 51%;              /* Red */
  --chart-5: 280 60% 50%;            /* Purple */

  /* Sidebar */
  --sidebar-background: 0 0% 100%;
  --sidebar-foreground: 210 11% 15%;
  --sidebar-primary: 210 29% 24%;
  --sidebar-accent: 210 17% 95%;
  --sidebar-border: 210 20% 90%;
}

.dark {
  /* VS Code Inspired Dark Mode */
  --background: 220 13% 12%;         /* Deep slate #1C1E26 */
  --foreground: 210 20% 88%;         /* Light text */

  --card: 220 13% 15%;               /* Slightly lighter slate */
  --card-foreground: 210 20% 88%;

  --primary: 168 76% 42%;            /* Teal becomes primary in dark mode */
  --primary-foreground: 220 13% 12%;

  --secondary: 220 13% 18%;
  --secondary-foreground: 210 20% 88%;

  --muted: 220 13% 18%;
  --muted-foreground: 215 14% 65%;

  --accent: 168 76% 42%;             /* Teal accent consistent */
  --accent-foreground: 220 13% 12%;

  --border: 220 13% 22%;
  --input: 220 13% 22%;

  /* Chart colors adjusted for dark mode */
  --chart-2: 210 50% 60%;            /* Lighter blue for contrast */
  --chart-5: 280 60% 60%;            /* Lighter purple */

  /* Sidebar dark mode */
  --sidebar-background: 220 13% 10%;
  --sidebar-foreground: 210 20% 88%;
  --sidebar-primary: 168 76% 42%;
  --sidebar-accent: 220 13% 18%;
  --sidebar-border: 220 13% 22%;
}
```

### Accessing Colors in Components

**Tailwind Classes:**
```tsx
<div className="bg-background text-foreground">
<Card className="bg-card text-card-foreground">
<Button className="bg-accent text-accent-foreground">
```

**Inline Styles (for dynamic colors):**
```tsx
<div style={{ backgroundColor: `hsl(var(--chart-1))` }}>
```

---

## Typography

### Font Stack

```css
body {
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto,
               "Helvetica Neue", Arial, sans-serif;
}

/* Monospace for financial data and code */
.font-mono {
  font-family: ui-monospace, SFMono-Regular, "SF Mono", Menlo, Consolas,
               "Liberation Mono", monospace;
}
```

**Usage:**
- **System fonts** - Fast loading, native feel, cross-platform consistency
- **Monospace (font-mono)** - Financial amounts, dates, .hledger file previews, technical data

### Type Scale

| Element | Tailwind Class | Size | Weight | Usage |
|---------|----------------|------|--------|-------|
| **Page Title** | `text-3xl font-bold` | 1.875rem (30px) | 700 | Dashboard, main page headings |
| **Card Title** | `text-2xl font-semibold` | 1.5rem (24px) | 600 | Card headers, dialog titles |
| **Section Heading** | `text-xl font-semibold` | 1.25rem (20px) | 600 | Subsections |
| **Body** | `text-base` | 1rem (16px) | 400 | Default text |
| **Small** | `text-sm` | 0.875rem (14px) | 400 | Supporting text, table cells, labels |
| **Extra Small** | `text-xs` | 0.75rem (12px) | 400 | Metadata, badges, captions |
| **Amount** | `text-lg font-mono font-semibold` | 1.125rem (18px) | 600 | Monetary values |

### Typography Patterns

```tsx
{/* Page header */}
<div>
  <h1 className="text-3xl font-bold text-foreground">Dashboard</h1>
  <p className="text-muted-foreground mt-1">Your financial overview at a glance</p>
</div>

{/* Card title */}
<CardTitle className="text-2xl font-semibold">Net Worth</CardTitle>
<CardDescription>Total assets minus liabilities</CardDescription>

{/* Amount display */}
<span className="text-lg font-mono font-semibold text-success">
  ${netWorth.toLocaleString()}
</span>

{/* Table cells - small text */}
<td className="p-4 text-sm font-medium">{transaction.payee}</td>

{/* Date - monospace */}
<td className="p-4 text-sm font-mono">{transaction.date}</td>

{/* Small metadata */}
<span className="text-xs text-muted-foreground">{transaction.date}</span>
```

---

## Color System

### Primary Colors

| Color | HSL Value | Hex (Approx) | Usage |
|-------|-----------|--------------|-------|
| **Deep Blue** | `210 29% 24%` | `#2C3E50` | Primary buttons, headings (light mode) |
| **Teal** | `168 76% 42%` | `#1ABC9C` | Accent color, primary in dark mode, active states |
| **Success Green** | `145 63% 42%` | `#22C55E` | Positive amounts, savings, downward expense trends |
| **Destructive Red** | `0 65% 51%` | `#EF4444` | Warnings, overdrafts, upward expense trends |

### Semantic Color Usage

| State | Tailwind Class | Usage | Example |
|-------|---------------|-------|---------|
| **Success** | `text-success` `bg-success` | Positive amounts, completed actions, downward expense trends | Net worth increase, import success, reduced spending |
| **Destructive** | `text-destructive` `bg-destructive` | Negative amounts, warnings, upward expense trends | Expenses, overdraft alerts, increased spending |
| **Accent** | `text-accent` `bg-accent` | Primary actions, active states, emphasis | Import CSV button, active nav item, info banners |
| **Muted** | `text-muted-foreground` `bg-muted` | Supporting text, backgrounds | Descriptions, secondary info, table headers |

### Category Colors (Badges)

**Pattern:** Colored backgrounds with 10% opacity for distinction

```tsx
const getCategoryColor = (category: string) => {
  const colors: Record<string, string> = {
    Income: "bg-success/10 text-success border-success/20",
    Groceries: "bg-chart-1/10 text-chart-1 border-chart-1/20",
    Transport: "bg-chart-2/10 text-chart-2 border-chart-2/20",
    Entertainment: "bg-chart-4/10 text-chart-4 border-chart-4/20",
    Utilities: "bg-chart-5/10 text-chart-5 border-chart-5/20",
    Dining: "bg-accent/10 text-accent border-accent/20",
  };
  return colors[category] || "bg-muted text-muted-foreground";
};

// Usage
<Badge variant="outline" className={getCategoryColor(transaction.category)}>
  {transaction.category}
</Badge>
```

### Confidence Indicators (Badges)

**Pattern:** Color-coded badges show system confidence in predictions.

```tsx
{/* High confidence - Green */}
<Badge className="bg-success text-white gap-1">
  <CheckCircle2 className="h-3 w-3" />
  Exact Match
</Badge>

{/* Medium confidence - Yellow */}
<Badge className="bg-yellow-500 text-white gap-1">
  <AlertCircle className="h-3 w-3" />
  Likely Match
</Badge>

{/* Low confidence - Gray */}
<Badge className="bg-muted text-muted-foreground gap-1">
  <HelpCircle className="h-3 w-3" />
  Possible Match
</Badge>
```

### Alert/Banner Colors

```tsx
{/* Info banner (accent teal) */}
<Card className="border-accent bg-accent/5">
  <CardContent className="p-4">
    <div className="flex items-center gap-3">
      <TrendingUp className="h-5 w-5 text-accent" />
      <div>
        <p className="font-medium text-foreground">On track for positive growth</p>
        <p className="text-sm text-muted-foreground">Predicted +12% increase</p>
      </div>
    </div>
  </CardContent>
</Card>

{/* Warning alert (yellow) */}
<div className="bg-yellow-500/10 border border-yellow-500/20 text-yellow-800 dark:text-yellow-200 px-4 py-3 rounded-lg">
  <AlertCircle className="h-5 w-5" />
  <p className="font-semibold">{duplicates.length} potential duplicates detected</p>
</div>
```

### Trend Indicators

**Pattern:** Green for down (good), Red for up (bad) - expense context

```tsx
{/* Expense trend - down is good */}
<div className={`flex items-center gap-1 ${
  change < 0 ? "text-success" : "text-destructive"
}`}>
  {change < 0 ? (
    <TrendingDown className="h-4 w-4" />
  ) : (
    <TrendingUp className="h-4 w-4" />
  )}
  <span className="font-medium">{Math.abs(change).toFixed(1)}%</span>
</div>
```

### Chart Colors

**Consistent color palette across all charts:**

```tsx
const CHART_COLORS = {
  chart1: "hsl(var(--chart-1))", // Teal - primary data
  chart2: "hsl(var(--chart-2))", // Deep blue - secondary data
  chart3: "hsl(var(--chart-3))", // Green - positive/income
  chart4: "hsl(var(--chart-4))", // Red - negative/expenses
  chart5: "hsl(var(--chart-5))", // Purple - tertiary data
};

// Usage in Recharts
<Line dataKey="balance" stroke="hsl(var(--accent))" strokeWidth={3} />
<Bar dataKey="income" fill="hsl(var(--success))" />
<Bar dataKey="expenses" fill="hsl(var(--destructive))" />
```

---

## Spacing & Layout

### Spacing Scale (Tailwind)

Ledgerly uses Tailwind's default spacing scale (0.25rem base unit).

| Token | Value | Tailwind Class | Usage |
|-------|-------|----------------|-------|
| `1` | 0.25rem (4px) | `space-y-1`, `gap-1` | Tight spacing |
| `2` | 0.5rem (8px) | `space-y-2`, `gap-2` | Icon-text gaps, small elements |
| `3` | 0.75rem (12px) | `space-y-3`, `gap-3` | List items, form fields |
| `4` | 1rem (16px) | `space-y-4`, `gap-4` | Component gaps, card content |
| `6` | 1.5rem (24px) | `space-y-6`, `gap-6` | Section spacing, grid gaps |
| `8` | 2rem (32px) | `p-8` | Large padding |
| `12` | 3rem (48px) | `py-12` | Major section dividers |

### Common Patterns

```tsx
{/* Page container */}
<div className="p-6 space-y-6 animate-fade-in">
  {/* Page header */}
  <div>
    <h1 className="text-3xl font-bold">Dashboard</h1>
    <p className="text-muted-foreground mt-1">Description</p>
  </div>

  {/* Grid layout */}
  <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
    {/* Cards */}
  </div>
</div>

{/* Card padding */}
<CardHeader className="p-6">
<CardContent className="p-6 pt-0">

{/* List spacing */}
<div className="space-y-3">
  {items.map(item => <div key={item.id}>...</div>)}
</div>

{/* Recurring transaction list */}
<div className="space-y-4">
  {transactions.map(txn => <div key={txn.id}>...</div>)}
</div>
```

### Border Radius

**Consistent `rounded-lg` (0.5rem / 8px) across all components:**

| Element | Class | Usage |
|---------|-------|-------|
| **Cards** | `rounded-lg` | All Card components |
| **Buttons** | `rounded-md` | Default button radius |
| **Badges** | `rounded-full` | Fully rounded pills |
| **Inputs** | `rounded-md` | Form fields |
| **Dialogs** | `rounded-lg` | Modals, popovers |
| **Tables** | `rounded-lg` | Table containers with `overflow-hidden` |
| **Category Pills** | `rounded-lg` | Settings category display |

---

## Component Patterns

### 1. Cards (Information Containers)

**Base Pattern:** shadcn/ui Card component

```tsx
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

<Card>
  <CardHeader>
    <CardTitle className="flex items-center gap-2">
      <DollarSign className="h-5 w-5 text-accent" />
      Net Worth
    </CardTitle>
    <CardDescription>Total assets minus liabilities</CardDescription>
  </CardHeader>
  <CardContent>
    {/* Card content */}
  </CardContent>
</Card>
```

**Key Features:**
- Automatic shadow, border, rounded corners via shadcn
- `CardHeader` has built-in spacing (`p-6`)
- `CardContent` continues spacing (`p-6 pt-0` - no top padding)
- Title icons: `h-5 w-5 text-accent`

**Grid Layouts:**
```tsx
{/* Responsive grid */}
<div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
  <Card>{/* 1 column on mobile, 1/3 width on large screens */}</Card>
  <Card className="lg:col-span-2">{/* Spans 2 columns on large */}</Card>
</div>

{/* Full width card */}
<Card className="lg:col-span-3">
```

**Alert Card (Colored):**
```tsx
<Card className="border-accent bg-accent/5">
  <CardContent className="p-4">
    <div className="flex items-center gap-3">
      <TrendingUp className="h-5 w-5 text-accent" />
      <div>
        <p className="font-medium text-foreground">On track</p>
        <p className="text-sm text-muted-foreground">Details...</p>
      </div>
    </div>
  </CardContent>
</Card>
```

---

### 2. Tables

**Pattern:** Bordered table with hover states

```tsx
<div className="rounded-lg border border-border overflow-hidden">
  <table className="w-full">
    <thead className="bg-muted/50">
      <tr>
        <th className="text-left p-4 font-medium text-sm">Date</th>
        <th className="text-left p-4 font-medium text-sm">Payee</th>
        <th className="text-right p-4 font-medium text-sm">Amount</th>
      </tr>
    </thead>
    <tbody className="divide-y divide-border">
      {items.map(item => (
        <tr key={item.id} className="hover:bg-muted/30 transition-colors">
          <td className="p-4 text-sm font-mono">{item.date}</td>
          <td className="p-4 text-sm font-medium">{item.payee}</td>
          <td className="p-4 text-sm font-mono font-semibold text-right">
            ${item.amount.toFixed(2)}
          </td>
        </tr>
      ))}
    </tbody>
  </table>
</div>
```

**Key Features:**
- Container: `rounded-lg border overflow-hidden`
- Header: `bg-muted/50` with `font-medium text-sm`
- Rows: `hover:bg-muted/30 transition-colors`
- Dividers: `divide-y divide-border`
- Dates/amounts: `font-mono`
- Padding: `p-4` consistent
- Action buttons: `flex gap-2 justify-end` with icon buttons

---

### 3. Buttons

**Variants:** Default, destructive, outline, secondary, ghost, link

```tsx
import { Button } from "@/components/ui/button";

{/* Primary action - accent teal background */}
<Button className="bg-accent hover:bg-accent/90 text-accent-foreground">
  <Upload className="mr-2 h-5 w-5" />
  Import CSV
</Button>

{/* Secondary action - outlined */}
<Button variant="outline">
  <Plus className="mr-2 h-5 w-5" />
  Add Transaction
</Button>

{/* Destructive action - red background */}
<Button variant="destructive">
  Delete Transaction
</Button>

{/* Icon button */}
<Button variant="ghost" size="icon" className="h-8 w-8">
  <Edit className="h-4 w-4" />
</Button>

{/* Sizes */}
<Button size="sm">Small</Button>
<Button size="default">Default</Button>
<Button size="lg">Large</Button>
```

**Button with Icons:**
- **Left icon:** `className="mr-2 h-5 w-5"` (large button) or `h-4 w-4` (small button)
- **Right icon:** `className="ml-2 h-5 w-5"`
- **Icon-only:** `size="icon"` with `h-8 w-8` or `h-10 w-10`

**Button Groups:**
```tsx
<div className="flex gap-2">
  <Button variant="outline">30 Days</Button>
  <Button variant="outline">60 Days</Button>
  <Button variant="outline">90 Days</Button>
</div>
```

---

### 4. Navigation (Sidebar)

**Pattern:** Fixed sidebar with nav items and action buttons

```tsx
<aside className="w-64 bg-sidebar border-r border-sidebar-border flex flex-col h-screen sticky top-0">
  {/* Logo/Brand */}
  <div className="p-6 border-b border-sidebar-border">
    <h1 className="text-2xl font-bold text-sidebar-foreground">Ledgerly</h1>
    <p className="text-xs text-muted-foreground mt-1">Personal Finance Manager</p>
  </div>

  {/* Nav items */}
  <nav className="flex-1 p-4 space-y-1">
    <NavLink
      to="/"
      className={({ isActive }) => cn(
        "flex items-center gap-3 px-4 py-3 rounded-lg transition-all",
        "hover:bg-sidebar-accent text-sidebar-foreground",
        isActive && "bg-sidebar-accent border-l-4 border-accent font-medium"
      )}
    >
      <Home className="h-5 w-5" />
      <span>Dashboard</span>
    </NavLink>
  </nav>

  {/* Action buttons */}
  <div className="p-4 space-y-2 border-t border-sidebar-border">
    <Button className="w-full bg-accent hover:bg-accent/90" size="lg">
      <Upload className="mr-2 h-5 w-5" />
      Import CSV
    </Button>
    <Button variant="outline" className="w-full" size="lg">
      <Plus className="mr-2 h-5 w-5" />
      Add Transaction
    </Button>
  </div>
</aside>
```

**Key Features:**
- Width: `w-64` (256px)
- Active state: `border-l-4 border-accent` (4px left border in teal)
- Icons: `h-5 w-5` consistent size
- Hover: `hover:bg-sidebar-accent`
- Sticky: `sticky top-0 h-screen`
- Bottom actions: `border-t` separator

---

### 5. Header (Top Bar)

**Pattern:** Sticky header with period selector and theme toggle

```tsx
<header className="h-16 border-b border-border bg-background sticky top-0 z-10 flex items-center justify-between px-6">
  {/* Period Selector */}
  <Select defaultValue="this-month">
    <SelectTrigger className="w-[180px]">
      <SelectValue placeholder="Select period" />
    </SelectTrigger>
    <SelectContent>
      <SelectItem value="this-month">This Month</SelectItem>
      <SelectItem value="last-month">Last Month</SelectItem>
      <SelectItem value="quarter">Quarter</SelectItem>
      <SelectItem value="year">Year</SelectItem>
    </SelectContent>
  </Select>

  {/* Theme Toggle */}
  <Button
    variant="ghost"
    size="icon"
    onClick={() => setTheme(theme === "dark" ? "light" : "dark")}
  >
    {theme === "dark" ? <Sun className="h-5 w-5" /> : <Moon className="h-5 w-5" />}
  </Button>
</header>
```

**Key Features:**
- Height: `h-16` (64px)
- Sticky: `sticky top-0 z-10`
- Border bottom: `border-b border-border`
- Flexbox: `flex items-center justify-between`

---

### 6. Badges

**Pattern:** Inline labels for categories, confidence, status, frequency

```tsx
import { Badge } from "@/components/ui/badge";

{/* Default badge */}
<Badge>{transaction.category}</Badge>

{/* Outline variant with color */}
<Badge variant="outline" className="bg-success/10 text-success border-success/20">
  Income
</Badge>

{/* Frequency badge */}
<Badge variant="outline" className="text-xs">
  monthly
</Badge>

{/* With icon */}
<Badge className="bg-success text-white gap-1">
  <CheckCircle2 className="h-3 w-3" />
  Exact Match
</Badge>
```

---

### 7. Search & Filter

**Pattern:** Input with icon and filter button

```tsx
<div className="flex gap-2">
  <div className="relative">
    <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
    <Input placeholder="Search transactions..." className="pl-10 w-[250px]" />
  </div>
  <Button variant="outline" size="icon">
    <Filter className="h-4 w-4" />
  </Button>
</div>
```

**Key Features:**
- Icon positioning: `absolute left-3 top-1/2 -translate-y-1/2`
- Input padding: `pl-10` to account for icon
- Icon size: `h-4 w-4 text-muted-foreground`

---

### 8. Tabs (Settings)

**Pattern:** shadcn/ui Tabs for multi-section pages

```tsx
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";

<Tabs defaultValue="accounts" className="space-y-6">
  <TabsList>
    <TabsTrigger value="accounts" className="gap-2">
      <Database className="h-4 w-4" />
      Accounts
    </TabsTrigger>
    <TabsTrigger value="categories" className="gap-2">
      <Tag className="h-4 w-4" />
      Categories
    </TabsTrigger>
  </TabsList>

  <TabsContent value="accounts" className="space-y-4">
    <Card>
      {/* Account management content */}
    </Card>
  </TabsContent>

  <TabsContent value="categories" className="space-y-4">
    <Card>
      {/* Category management content */}
    </Card>
  </TabsContent>
</Tabs>
```

**Key Features:**
- Icons in triggers: `h-4 w-4` with `gap-2`
- Content spacing: `className="space-y-4"`

---

### 9. Forms

**Pattern:** Label + Input with proper spacing

```tsx
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";

<div className="space-y-4">
  <div className="space-y-2">
    <Label htmlFor="account-name">Account Name</Label>
    <Input id="account-name" placeholder="e.g., Chase Checking" />
  </div>

  <div className="space-y-2">
    <Label htmlFor="initial-balance">Initial Balance</Label>
    <Input id="initial-balance" type="number" placeholder="0.00" className="font-mono" />
  </div>

  <div className="space-y-2">
    <Label>File Preview</Label>
    <Textarea
      value={hledgerPreview}
      readOnly
      className="font-mono text-sm h-[300px] bg-muted"
    />
  </div>

  <Button className="bg-accent hover:bg-accent/90">Submit</Button>
</div>
```

**Key Features:**
- Field wrapper: `space-y-2`
- Form wrapper: `space-y-4`
- Numeric inputs: `type="number" className="font-mono"`
- Read-only code: `readOnly className="font-mono text-sm bg-muted"`

---

### 10. Transaction Lists

**Pattern:** Hoverable rows with metadata (Dashboard style)

```tsx
<div className="space-y-3">
  {transactions.map((transaction) => (
    <div
      key={transaction.id}
      className="flex items-center justify-between p-3 rounded-lg border border-border hover:bg-muted/50 transition-colors"
    >
      <div className="flex-1">
        <p className="font-medium">{transaction.payee}</p>
        <div className="flex items-center gap-3 mt-1">
          <span className="text-xs text-muted-foreground">{transaction.date}</span>
          <Badge variant="outline">{transaction.category}</Badge>
        </div>
      </div>
      <span className={`text-lg font-mono font-semibold ${
        transaction.amount > 0 ? "text-success" : "text-foreground"
      }`}>
        {transaction.amount > 0 ? "+" : ""}${Math.abs(transaction.amount).toFixed(2)}
      </span>
    </div>
  ))}
</div>
```

**Key Features:**
- Hover state: `hover:bg-muted/50 transition-colors`
- Border: `border border-border rounded-lg`
- Amounts: `font-mono font-semibold`
- Positive amounts: `text-success` with `+` prefix

---

### 11. Recurring Transaction Items

**Pattern:** Icon + badge + metadata

```tsx
<div className="space-y-4">
  {recurringTransactions.map((transaction) => {
    const IconComponent = transaction.icon;
    return (
      <div
        key={transaction.id}
        className="flex items-center justify-between p-4 rounded-lg border border-border hover:bg-muted/50 transition-colors"
      >
        <div className="flex items-center gap-4">
          {/* Colored icon container */}
          <div className={`p-2 rounded-lg bg-muted ${transaction.color}`}>
            <IconComponent className="h-5 w-5" />
          </div>
          <div>
            <p className="font-medium">{transaction.payee}</p>
            <div className="flex items-center gap-2 mt-1">
              <Badge variant="outline" className="text-xs">
                {transaction.frequency}
              </Badge>
              <span className="text-xs text-muted-foreground">
                Next: {transaction.nextDate}
              </span>
            </div>
          </div>
        </div>
        <span className={`text-lg font-mono font-semibold ${
          transaction.amount > 0 ? "text-success" : "text-foreground"
        }`}>
          {transaction.amount > 0 ? "+" : ""}${Math.abs(transaction.amount).toLocaleString()}
        </span>
      </div>
    );
  })}
</div>
```

**Key Features:**
- Icon container: `p-2 rounded-lg bg-muted` with dynamic color class
- Two-line metadata: Badge + small text
- Spacing: `space-y-4` between items

---

### 12. Charts (Recharts Integration)

**Line Chart (Cash Flow):**
```tsx
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, ReferenceLine } from "recharts";

<ResponsiveContainer width="100%" height={400}>
  <LineChart data={cashFlowData}>
    {/* Gradient fill */}
    <defs>
      <linearGradient id="confidenceGradient" x1="0" y1="0" x2="0" y2="1">
        <stop offset="0%" stopColor="hsl(var(--accent))" stopOpacity={0.3} />
        <stop offset="100%" stopColor="hsl(var(--accent))" stopOpacity={0.05} />
      </linearGradient>
    </defs>

    <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
    <XAxis dataKey="date" stroke="hsl(var(--muted-foreground))" />
    <YAxis stroke="hsl(var(--muted-foreground))" />
    <Tooltip
      contentStyle={{
        backgroundColor: "hsl(var(--popover))",
        border: "1px solid hsl(var(--border))",
        borderRadius: "var(--radius)",
      }}
    />
    <ReferenceLine y={0} stroke="hsl(var(--destructive))" strokeDasharray="3 3" />
    <Line
      type="monotone"
      dataKey="balance"
      stroke="hsl(var(--accent))"
      strokeWidth={3}
      dot={{ fill: "hsl(var(--accent))", r: 5 }}
      fill="url(#confidenceGradient)"
    />
  </LineChart>
</ResponsiveContainer>
```

**Bar Chart (Vertical):**
```tsx
<BarChart data={categoryData} layout="vertical">
  <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
  <XAxis type="number" stroke="hsl(var(--muted-foreground))" />
  <YAxis dataKey="category" type="category" stroke="hsl(var(--muted-foreground))" width={120} />
  <Tooltip
    contentStyle={{
      backgroundColor: "hsl(var(--popover))",
      border: "1px solid hsl(var(--border))",
      borderRadius: "var(--radius)",
    }}
  />
  <Bar dataKey="amount" fill="hsl(var(--accent))" radius={[0, 4, 4, 0]} />
</BarChart>
```

**Bar Chart (Horizontal - Income vs Expense):**
```tsx
<BarChart data={incomeExpenseData}>
  <Bar dataKey="income" fill="hsl(var(--success))" radius={[4, 4, 0, 0]} />
  <Bar dataKey="expenses" fill="hsl(var(--destructive))" radius={[4, 4, 0, 0]} />
</BarChart>
```

**Pie Chart (Donut):**
```tsx
<PieChart>
  <Pie
    data={expenseData}
    cx="50%"
    cy="50%"
    innerRadius={60}
    outerRadius={90}
    paddingAngle={2}
    dataKey="value"
  >
    {expenseData.map((entry, index) => (
      <Cell key={`cell-${index}`} fill={entry.color} />
    ))}
  </Pie>
</PieChart>
```

**Chart Theming Checklist:**
- Grid: `stroke="hsl(var(--border))"`
- Axes: `stroke="hsl(var(--muted-foreground))"`
- Tooltip: Match popover styling
- Always use `ResponsiveContainer`
- Heights: 250px (cards), 350-400px (main charts)

---

### 13. Dialogs/Modals (Multi-Step Wizards)

**Pattern:** shadcn/ui Dialog with progress indicator

```tsx
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";

<Dialog open={open} onOpenChange={onOpenChange}>
  <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
    <DialogHeader>
      <DialogTitle className="text-2xl font-bold">Upload CSV</DialogTitle>

      {/* Progress indicator */}
      <div className="flex items-center gap-2 mt-4">
        {steps.map((step, index) => (
          <div key={step} className="flex items-center flex-1">
            <div className={`h-2 flex-1 rounded-full ${
              currentStep === step ? "bg-accent" :
              index < currentStepIndex ? "bg-success" : "bg-muted"
            }`} />
          </div>
        ))}
      </div>
    </DialogHeader>

    {/* Step content */}
  </DialogContent>
</Dialog>
```

**Progress Bar:**
- Current: `bg-accent`
- Completed: `bg-success`
- Future: `bg-muted`
- Height: `h-2`

---

## Page Layouts

### Dashboard Page

**Structure:** Page header + responsive grid of cards

```tsx
<div className="p-6 space-y-6 animate-fade-in">
  {/* Page header */}
  <div>
    <h1 className="text-3xl font-bold text-foreground">Dashboard</h1>
    <p className="text-muted-foreground mt-1">Your financial overview at a glance</p>
  </div>

  {/* Grid */}
  <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
    {/* 1-column card */}
    <Card>...</Card>

    {/* 2-column card */}
    <Card className="lg:col-span-2">...</Card>

    {/* Full-width card */}
    <Card className="lg:col-span-3">...</Card>
  </div>
</div>
```

### Transactions Page

**Structure:** Page header + single card with table

```tsx
<div className="p-6 space-y-6 animate-fade-in">
  <div>
    <h1 className="text-3xl font-bold">Transactions</h1>
    <p className="text-muted-foreground mt-1">View and manage all your transactions</p>
  </div>

  <Card>
    <CardHeader>
      <div className="flex flex-col md:flex-row gap-4 items-start md:items-center justify-between">
        <div>
          <CardTitle>All Transactions</CardTitle>
          <CardDescription>Filter and search through your transaction history</CardDescription>
        </div>
        {/* Search + Filter */}
        <div className="flex gap-2">...</div>
      </div>
    </CardHeader>
    <CardContent>
      {/* Table */}
    </CardContent>
  </Card>
</div>
```

### Reports Page

**Structure:** Header with actions + chart card + table card

```tsx
<div className="p-6 space-y-6 animate-fade-in">
  <div className="flex items-center justify-between">
    <div>
      <h1 className="text-3xl font-bold">Category Reports</h1>
      <p className="text-muted-foreground mt-1">Analyze your spending patterns</p>
    </div>
    <div className="flex gap-2">
      <Button variant="outline">
        <FileText className="mr-2 h-4 w-4" />
        Export PDF
      </Button>
      <Button variant="outline">
        <Download className="mr-2 h-4 w-4" />
        Export CSV
      </Button>
    </div>
  </div>

  {/* Chart card */}
  <Card>...</Card>

  {/* Table card */}
  <Card>...</Card>
</div>
```

### Cash Flow Page

**Structure:** Header with buttons + alert banner + charts + recurring transactions

```tsx
<div className="p-6 space-y-6 animate-fade-in">
  <div className="flex items-center justify-between">
    <div>
      <h1 className="text-3xl font-bold">Cash Flow Timeline</h1>
      <p className="text-muted-foreground mt-1">Predicted balance and recurring transactions</p>
    </div>
    <div className="flex gap-2">
      <Button variant="outline">30 Days</Button>
      <Button variant="outline">60 Days</Button>
      <Button variant="outline">90 Days</Button>
    </div>
  </div>

  {/* Alert banner */}
  <Card className="border-accent bg-accent/5">...</Card>

  {/* Main chart */}
  <Card>...</Card>

  {/* Recurring transactions */}
  <Card>...</Card>
</div>
```

### Settings Page

**Structure:** Page header + tabs with cards

```tsx
<div className="p-6 space-y-6 animate-fade-in">
  <div>
    <h1 className="text-3xl font-bold">Settings</h1>
    <p className="text-muted-foreground mt-1">Manage your accounts, categories, and export options</p>
  </div>

  <Tabs defaultValue="accounts" className="space-y-6">
    <TabsList>...</TabsList>

    <TabsContent value="accounts" className="space-y-4">
      <Card>...</Card>
    </TabsContent>
  </Tabs>
</div>
```

---

## Responsive Design

### Breakpoints

Ledgerly uses Tailwind's default breakpoints:

| Size | Min Width | Usage |
|------|-----------|-------|
| **sm** | 640px | Small tablets |
| **md** | 768px | Tablets, flex direction changes |
| **lg** | 1024px | Desktop, grid changes, sidebar visible |
| **xl** | 1280px | Large desktop |
| **2xl** | 1536px | Extra large |

### Mobile-First Patterns

**Grid to Stack:**
```tsx
{/* 1 column mobile, 3 columns desktop */}
<div className="grid grid-cols-1 lg:grid-cols-3 gap-6">

{/* 2 columns tablet, 3 desktop */}
<div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">

{/* Span columns on large */}
<Card className="lg:col-span-2">
```

**Flex Direction:**
```tsx
{/* Stack on mobile, row on tablet+ */}
<div className="flex flex-col md:flex-row gap-4">
```

**Sidebar Collapse:**
```tsx
{/* Hide sidebar on mobile */}
<aside className="hidden lg:flex w-64">

{/* Mobile menu button */}
<Button className="lg:hidden" variant="ghost" size="icon">
  <Menu className="h-6 w-6" />
</Button>
```

**Responsive Typography:**
```tsx
{/* Smaller on mobile */}
<h1 className="text-2xl lg:text-3xl font-bold">
```

---

## Accessibility

### Focus Indicators

**shadcn/ui components have built-in focus styles:**
```css
focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2
```

### ARIA Labels

```tsx
{/* Icon buttons */}
<Button variant="ghost" size="icon" aria-label="Delete transaction">
  <Trash2 className="h-4 w-4" />
</Button>

{/* Theme toggle */}
<Button aria-label="Toggle theme" onClick={toggleTheme}>
  {theme === "dark" ? <Sun /> : <Moon />}
</Button>
```

### Keyboard Navigation

**All shadcn/ui components support keyboard:**
- Dialogs: `Esc` to close
- Dropdowns: Arrow keys
- Buttons: `Enter`/`Space`
- Tables: Tab navigation

### Color Contrast

**WCAG AA compliance:**
- Primary text: 14.8:1 (AAA)
- Muted text: 5.9:1 (AA)
- All interactive elements meet 3:1 minimum

---

## Implementation Guide

### For Developers

**1. Use Tailwind + shadcn/ui (No Custom CSS)**

✅ **Correct:**
```tsx
<div className="bg-card border border-border rounded-lg p-6">
```

❌ **Incorrect:**
```tsx
<div style={{ backgroundColor: '#fff' }}>
```

**2. Use HSL Colors**

✅ **Correct:**
```tsx
<Line stroke="hsl(var(--accent))" />
```

❌ **Incorrect:**
```tsx
<Line stroke="#1ABC9C" />
```

**3. Monospace for Data**

✅ **Correct:**
```tsx
<td className="font-mono">{date}</td>
<span className="font-mono font-semibold">${amount}</span>
```

**4. Consistent Icon Sizing**

```tsx
{/* Button icons */}
<Upload className="mr-2 h-5 w-5" />

{/* Table action icons */}
<Edit className="h-4 w-4" />

{/* Badge icons */}
<CheckCircle2 className="h-3 w-3" />

{/* Empty state icons */}
<CheckCircle2 className="h-16 w-16" />
```

**5. Category Colors Pattern**

```tsx
const getCategoryColor = (category: string) => {
  const colors: Record<string, string> = {
    Income: "bg-success/10 text-success border-success/20",
    // ... 10% opacity backgrounds
  };
  return colors[category] || "bg-muted text-muted-foreground";
};
```

---

### For AI Agents

**Component Checklist:**
- [ ] Uses shadcn/ui components
- [ ] Tailwind classes only
- [ ] HSL colors for theming
- [ ] Monospace for amounts/dates/code
- [ ] Lucide React icons (`h-5 w-5` buttons, `h-4 w-4` tables)
- [ ] Responsive (`grid-cols-1 lg:grid-cols-3`)
- [ ] ARIA labels
- [ ] Follows page layout patterns
- [ ] Tables use bordered pattern
- [ ] Charts use HSL theming

**Quick Start:**

```tsx
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { DollarSign } from "lucide-react";

export function MyPage() {
  return (
    <div className="p-6 space-y-6 animate-fade-in">
      <div>
        <h1 className="text-3xl font-bold">Page Title</h1>
        <p className="text-muted-foreground mt-1">Description</p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <DollarSign className="h-5 w-5 text-accent" />
              Title
            </CardTitle>
          </CardHeader>
          <CardContent>
            <span className="text-lg font-mono font-semibold text-success">
              $1,234.56
            </span>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
```

---

## Angular Implementation Guide

### Component Translation Table

| React/shadcn Component | Angular Material Equivalent | Notes |
|------------------------|----------------------------|-------|
| `<Card>` | `<mat-card>` | Use `mat-card-header`, `mat-card-content` |
| `<Button>` | `<button mat-flat-button>` | Variants: `mat-flat-button`, `mat-stroked-button`, `mat-icon-button` |
| `<Badge>` | `<mat-chip>` | Use `mat-chip-list` for multiple |
| `<Input>` | `<mat-form-field><input matInput>` | Always wrap in `mat-form-field` |
| `<Select>` | `<mat-select>` | Use with `mat-option` |
| `<Tabs>` | `<mat-tab-group>` | Use `mat-tab` for each tab |
| `<Dialog>` | `MatDialog service` | Open with `dialog.open(Component)` |
| `<Textarea>` | `<textarea matInput>` | In `mat-form-field` |
| Lucide Icons | Material Icons | Use `<mat-icon>` with icon name |

### Color Translation

**React (Tailwind):**
```tsx
<div className="bg-accent text-accent-foreground">
```

**Angular (CSS Custom Properties):**
```scss
// In styles.scss or component.scss
.accent-bg {
  background: var(--accent-color);
  color: var(--accent-foreground);
}
```

```html
<div class="accent-bg">
```

### Layout Translation

**React (Tailwind Grid):**
```tsx
<div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
  <Card className="lg:col-span-2">
</div>
```

**Angular (CSS Grid):**
```scss
.dashboard-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1.5rem;

  @media (min-width: 1024px) {
    grid-template-columns: repeat(3, 1fr);
  }
}

.span-2 {
  @media (min-width: 1024px) {
    grid-column: span 2;
  }
}
```

```html
<div class="dashboard-grid">
  <mat-card class="span-2">
</div>
```

### Spacing Translation

**React (Tailwind):**
```tsx
<div className="p-6 space-y-6">
```

**Angular (SCSS):**
```scss
.page-container {
  padding: 1.5rem;

  > * + * {
    margin-top: 1.5rem;
  }
}
```

### Typography Translation

**React (Tailwind):**
```tsx
<h1 className="text-3xl font-bold text-foreground">
<p className="text-muted-foreground mt-1">
<span className="font-mono font-semibold">
```

**Angular (SCSS):**
```scss
.page-title {
  font-size: 1.875rem;
  font-weight: 700;
  color: var(--text-primary);
}

.page-subtitle {
  color: var(--text-secondary);
  margin-top: 0.25rem;
}

.amount {
  font-family: 'Courier New', monospace;
  font-weight: 600;
}
```

### Chart Translation

**React (Recharts):**
```tsx
<ResponsiveContainer width="100%" height={250}>
  <LineChart data={data}>
    <Line stroke="hsl(var(--accent))" />
  </LineChart>
</ResponsiveContainer>
```

**Angular (Chart.js/ng2-charts):**
```typescript
// component.ts
public lineChartOptions: ChartConfiguration['options'] = {
  responsive: true,
  maintainAspectRatio: false,
};

public lineChartData: ChartData<'line'> = {
  datasets: [{
    data: [...],
    borderColor: getComputedStyle(document.documentElement)
      .getPropertyValue('--accent-color'),
    backgroundColor: 'rgba(26, 188, 156, 0.1)',
  }]
};
```

```html
<div style="height: 250px">
  <canvas baseChart
    [data]="lineChartData"
    [options]="lineChartOptions"
    [type]="'line'">
  </canvas>
</div>
```

### Button Variants Translation

| React/Tailwind | Angular Material | Description |
|----------------|------------------|-------------|
| `<Button>` default | `mat-flat-button color="primary"` | Solid background |
| `<Button variant="outline">` | `mat-stroked-button` | Outlined |
| `<Button variant="ghost">` | `mat-button` | Text only |
| `<Button variant="destructive">` | `mat-flat-button color="warn"` | Destructive action |
| `<Button size="icon">` | `mat-icon-button` | Icon only |

### Form Translation

**React:**
```tsx
<div className="space-y-4">
  <div className="space-y-2">
    <Label htmlFor="name">Name</Label>
    <Input id="name" />
  </div>
</div>
```

**Angular:**
```html
<div class="form-container">
  <mat-form-field appearance="outline">
    <mat-label>Name</mat-label>
    <input matInput id="name" />
  </mat-form-field>
</div>
```

```scss
.form-container {
  mat-form-field {
    width: 100%;
    margin-bottom: 1rem;
  }
}
```

### Key Differences to Remember

1. **No Tailwind utility classes** - Use SCSS/CSS instead
2. **Angular Material theming** - Configure in `angular.json` and custom theme file
3. **CSS Custom Properties** - Use for colors (already defined in your codebase)
4. **Flexbox/Grid** - Write explicit CSS instead of utility classes
5. **Icons** - Use Material Icons instead of Lucide
6. **Charts** - Use Chart.js instead of Recharts

---

## Change Log

| Date | Version | Description | Author |
|------|---------|-------------|--------|
| 2025-10-18 | 3.0 | Comprehensive design system covering all pages | Winston (Architect) |
| 2025-10-18 | 3.1 | Added Angular implementation guidance - clarified React samples are reference only | Winston (Architect) |

---

**Complete Page Coverage:**
- ✅ Dashboard - Grid layouts, charts, transaction lists
- ✅ Transactions - Tables, search/filter, action buttons
- ✅ Reports - Bar charts, trend indicators, export buttons
- ✅ Cash Flow - Line charts with gradients, recurring transactions, alert banners
- ✅ Settings - Tabs, forms, code preview
- ✅ CSV Import - Dialogs, wizards, duplicate detection
- ✅ Header - Period selector, theme toggle
- ✅ Sidebar - Navigation, branding

**Next Steps:**
1. Apply design principles to Angular components using translation guide
2. Document additional Angular-specific patterns as they're built
3. Create Storybook for visual component library (Phase 2)
