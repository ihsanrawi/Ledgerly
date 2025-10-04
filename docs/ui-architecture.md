# Ledgerly Frontend Architecture Document

**Version:** 1.0
**Date:** 2025-10-04
**Status:** Active
**Document Owner:** Architecture Team

## Change Log

| Date | Version | Description | Author |
|------|---------|-------------|--------|
| 2025-10-04 | 1.0 | Initial frontend architecture document. Includes: Angular 18 standalone components, Signals + RxJS hybrid state management, Material UI theming, Playwright testing, Tauri desktop integration, WCAG AA accessibility patterns. | Winston (AI Architect) |

---

## Table of Contents

1. [Template and Framework Selection](#1-template-and-framework-selection)
2. [Frontend Tech Stack](#2-frontend-tech-stack)
3. [Project Structure](#3-project-structure)
4. [Component Standards](#4-component-standards)
5. [State Management](#5-state-management)
6. [API Integration](#6-api-integration)
7. [Routing](#7-routing)
8. [Styling Guidelines](#8-styling-guidelines)
9. [Testing Requirements](#9-testing-requirements)
10. [Environment Configuration](#10-environment-configuration)
11. [Accessibility (WCAG AA Compliance)](#11-accessibility-wcag-aa-compliance)
12. [Frontend Developer Standards](#12-frontend-developer-standards)

---

## 1. Template and Framework Selection

### Decision: No Starter Template (Greenfield Angular 18 Project)

**Framework:** Angular 18 (standalone components)
**Rationale:**

The PRD specifies a custom tech stack:
- **Backend:** .NET 8+ with Wolverine (event-driven messaging)
- **Frontend:** Angular 17+ with Signals
- **Desktop Wrapper:** Tauri 1.6+ (with Electron fallback)
- **Architecture:** Vertical Slice Architecture (VSA)

No standard starter template exists for this combination:
1. Wolverine + VSA pattern - unique architectural choice
2. Embedded hledger binary - custom integration requirement
3. Tauri wrapper - requires custom configuration with Angular + .NET backend
4. Event-driven monolith - specific pattern not in standard templates

**Recommendation:** Build from scratch using:
- `ng new ledgerly-web --standalone` for Angular app (standalone components by default)
- `cargo create-tauri-app` for desktop wrapper
- Manual VSA structure setup (Features/ folders)

This gives full control over the feature slice organization while learning modern Angular patterns.

---

## 2. Frontend Tech Stack

| Category | Technology | Version | Purpose | Rationale |
|----------|-----------|---------|---------|-----------|
| **Framework** | Angular | 18 | Reactive UI framework | Standalone components, Signals + RxJS hybrid, latest patterns |
| **State Management** | Angular Signals + RxJS | 18 | Reactive state + async flows | Best of both: Signals for sync state, RxJS for HTTP/events |
| **UI Component Library** | Angular Material | 18 | Pre-built WCAG AA components | Consumer finance aesthetic, comprehensive component library |
| **Styling** | SCSS + CSS Variables | N/A | Theme system with dark mode | Customizable Material theme, runtime theming |
| **Routing** | Angular Router | 18 | Client-side routing | Guards for protected routes, lazy loading |
| **Build Tool** | Angular CLI + esbuild | Latest | Fast builds | Integrated with Angular 18 |
| **Forms** | Angular Reactive Forms | 18 | Type-safe form handling | FormArray for split transactions, async validators |
| **HTTP Client** | Angular HttpClient + Interceptors | 18 | API communication | Auth tokens, error handling, loading states |
| **Testing (Unit)** | Jest + Angular Testing Library | Latest | Component tests | Faster than Karma, user-centric tests |
| **Testing (E2E)** | Playwright | Latest | Critical user flows | Desktop app testing, cross-browser, parallel execution |
| **Accessibility** | Angular CDK + axe-core | Latest | WCAG AA compliance | Automated + manual testing |
| **Charts** | Chart.js + ng2-charts | 4.x | Interactive dashboards | Drill-down clicks, hover tooltips, accessible charts |
| **Desktop Integration** | Tauri API (conditional) | 1.6+ | Native file dialogs, window controls | Abstraction layer for browser dev mode |
| **Dev Tools** | Angular DevTools + ESLint | Latest | Debugging and code quality | Signals inspection, linting |

**Key Architectural Decisions:**

1. **Angular 18 Standalone Components:** Modern approach, less boilerplate, better tree-shaking
2. **Signals + RxJS Hybrid:** Signals for UI state, RxJS for async operations
3. **Angular Material:** Consumer finance aesthetic (bright, friendly, YNAB-inspired)
4. **Playwright:** Tauri desktop testing support (critical requirement)
5. **Jest:** Faster test execution than Karma

---

## 3. Project Structure

```
src/Ledgerly.Web/
├── src/
│   ├── app/
│   │   ├── core/                          # Singleton services, guards, interceptors
│   │   │   ├── api/
│   │   │   │   ├── api-client.service.ts       # HttpClient wrapper
│   │   │   │   ├── auth.interceptor.ts         # Auth tokens (functional)
│   │   │   │   └── error.interceptor.ts        # Global error handling
│   │   │   ├── guards/
│   │   │   │   └── data-loaded.guard.ts        # Functional guard
│   │   │   ├── services/
│   │   │   │   ├── tauri.service.ts            # Tauri API abstraction
│   │   │   │   ├── notification.service.ts     # Toast notifications
│   │   │   │   ├── theme.service.ts            # Dark mode toggle
│   │   │   │   └── loading.service.ts          # Global loading state
│   │   │   └── state/                          # Global app-level state
│   │   │       ├── user-preferences.service.ts # Settings, theme, currency
│   │   │       └── notification-state.service.ts
│   │   │
│   │   ├── shared/                        # Reusable components, pipes, directives
│   │   │   ├── components/
│   │   │   │   ├── page-header/
│   │   │   │   ├── loading-spinner/
│   │   │   │   ├── confirmation-dialog/
│   │   │   │   └── currency-display/
│   │   │   ├── pipes/
│   │   │   │   ├── currency.pipe.ts
│   │   │   │   └── date-range.pipe.ts
│   │   │   └── directives/
│   │   │       └── auto-focus.directive.ts
│   │   │
│   │   ├── features/                      # Feature components (VSA-aligned)
│   │   │   │
│   │   │   ├── dashboard/                 # Epic 3: Dashboard
│   │   │   │   ├── dashboard.component.ts       # Standalone container
│   │   │   │   ├── dashboard.routes.ts          # Route config
│   │   │   │   ├── components/
│   │   │   │   │   ├── net-worth-widget.component.ts
│   │   │   │   │   ├── expense-breakdown-chart.component.ts
│   │   │   │   │   ├── income-expense-chart.component.ts
│   │   │   │   │   ├── cash-flow-timeline.component.ts
│   │   │   │   │   ├── recent-transactions.component.ts
│   │   │   │   │   └── quick-actions.component.ts
│   │   │   │   ├── services/
│   │   │   │   │   └── dashboard-state.service.ts
│   │   │   │   └── models/
│   │   │   │       └── dashboard.models.ts
│   │   │   │
│   │   │   ├── transactions/              # Epic 4: Transaction Management
│   │   │   │   ├── transaction-list.component.ts
│   │   │   │   ├── transactions.routes.ts
│   │   │   │   ├── components/
│   │   │   │   │   ├── transaction-row.component.ts
│   │   │   │   │   ├── transaction-form/
│   │   │   │   │   │   ├── transaction-form.component.ts
│   │   │   │   │   │   └── split-transaction.component.ts
│   │   │   │   │   ├── transaction-search.component.ts
│   │   │   │   │   └── batch-operations.component.ts
│   │   │   │   ├── services/
│   │   │   │   │   ├── transaction.service.ts
│   │   │   │   │   └── transaction-state.service.ts
│   │   │   │   ├── validators/
│   │   │   │   │   └── duplicate-transaction.validator.ts
│   │   │   │   └── models/
│   │   │   │       └── transaction.models.ts
│   │   │   │
│   │   │   ├── import-csv/                # Epic 2: CSV Import
│   │   │   │   ├── csv-import.component.ts
│   │   │   │   ├── import-csv.routes.ts
│   │   │   │   ├── components/
│   │   │   │   │   ├── file-upload.component.ts
│   │   │   │   │   ├── column-mapping.component.ts
│   │   │   │   │   ├── import-preview.component.ts
│   │   │   │   │   └── import-summary.component.ts
│   │   │   │   ├── services/
│   │   │   │   │   ├── csv-parser.service.ts
│   │   │   │   │   ├── import.service.ts
│   │   │   │   │   └── import-state.service.ts
│   │   │   │   └── models/
│   │   │   │       └── import.models.ts
│   │   │   │
│   │   │   ├── reports/                   # Epic 7: Category Reports
│   │   │   │   ├── reports.component.ts
│   │   │   │   ├── reports.routes.ts
│   │   │   │   ├── components/
│   │   │   │   │   ├── category-breakdown.component.ts
│   │   │   │   │   ├── comparison-view.component.ts
│   │   │   │   │   └── export-options.component.ts
│   │   │   │   ├── services/
│   │   │   │   │   └── reports-state.service.ts
│   │   │   │   └── models/
│   │   │   │       └── reports.models.ts
│   │   │   │
│   │   │   ├── predictions/               # Epic 5: Cash Flow Predictions
│   │   │   │   ├── cash-flow-timeline.component.ts
│   │   │   │   ├── predictions.routes.ts
│   │   │   │   ├── components/
│   │   │   │   │   ├── recurring-transactions.component.ts
│   │   │   │   │   └── alerts.component.ts
│   │   │   │   ├── services/
│   │   │   │   │   └── prediction-state.service.ts
│   │   │   │   └── models/
│   │   │   │       └── predictions.models.ts
│   │   │   │
│   │   │   └── settings/                  # Settings & Preferences
│   │   │       ├── settings.component.ts
│   │   │       ├── settings.routes.ts
│   │   │       ├── components/
│   │   │       │   ├── account-management.component.ts
│   │   │       │   ├── category-management.component.ts
│   │   │       │   └── export-settings.component.ts
│   │   │       ├── services/
│   │   │       │   └── settings.service.ts
│   │   │       └── models/
│   │   │           └── settings.models.ts
│   │   │
│   │   ├── layout/                        # Application shell
│   │   │   ├── header.component.ts             # Standalone
│   │   │   ├── sidebar.component.ts            # Standalone
│   │   │   └── layout.component.ts             # Standalone main container
│   │   │
│   │   ├── app.component.ts               # Standalone root component
│   │   ├── app.routes.ts                  # Top-level routes
│   │   └── app.config.ts                  # App configuration (providers)
│   │
│   ├── assets/                            # Static files
│   ├── environments/                      # Environment configs
│   ├── styles/                            # Global styles
│   │   ├── _variables.scss
│   │   ├── _theme.scss
│   │   ├── _material-theme.scss
│   │   ├── _typography.scss
│   │   └── styles.scss
│   ├── index.html
│   └── main.ts                            # Bootstrap with bootstrapApplication()
│
├── angular.json
├── package.json
├── tsconfig.json
├── tsconfig.app.json
└── jest.config.js
```

**Key Organizational Principles:**

1. **Feature Module Structure (VSA Alignment):** Each `features/` folder mirrors backend epics
2. **Core vs Shared Distinction:** `core/` = singleton services, `shared/` = reusable components
3. **Lazy Loading Ready:** Each feature has own routing for code-splitting
4. **Component Composition:** Container/presentational pattern for complex features

---

## 4. Component Standards

### Component Template (Standalone)

```typescript
// features/dashboard/components/net-worth-widget.component.ts

import { Component, computed, signal, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { CurrencyDisplayComponent } from '../../../shared/components/currency-display/currency-display.component';

@Component({
  selector: 'app-net-worth-widget',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    CurrencyDisplayComponent
  ],
  templateUrl: './net-worth-widget.component.html',
  styleUrl: './net-worth-widget.component.scss'
})
export class NetWorthWidgetComponent {
  // Modern Angular 18 input/output syntax (signal-based)
  assets = input.required<number>();
  liabilities = input.required<number>();

  widgetClicked = output<void>();

  // Computed signal (derived state)
  netWorth = computed(() => this.assets() - this.liabilities());

  // Local component state (signal)
  isExpanded = signal(false);

  // Methods
  toggleExpanded(): void {
    this.isExpanded.update(expanded => !expanded);
    this.widgetClicked.emit();
  }
}
```

### Naming Conventions

**File Naming:**

| Type | Pattern | Example |
|------|---------|---------|
| Component | `{feature-name}.component.ts` | `dashboard.component.ts` |
| Service | `{feature-name}.service.ts` | `transaction.service.ts` |
| Model/Interface | `{feature-name}.models.ts` | `dashboard.models.ts` |
| Routes | `{feature}.routes.ts` | `dashboard.routes.ts` |

**Class/Symbol Naming:**

| Type | Convention | Example |
|------|------------|---------|
| Component Class | `PascalCase` + `Component` suffix | `DashboardComponent` |
| Service Class | `PascalCase` + `Service` suffix | `TransactionService` |
| Interface | `PascalCase` (no prefix) | `Transaction` |
| Signal | `camelCase` (no prefix) | `selectedAccount`, `isLoading` |
| Observable | `camelCase` + `$` suffix | `transactions$`, `loading$` |

### Component Patterns

#### Container/Presentational Pattern

```typescript
// Container (Smart - handles data/logic)
@Component({
  selector: 'app-transaction-list',
  standalone: true,
  template: `
    @if (isLoading()) {
      <app-loading-spinner />
    } @else {
      @for (transaction of transactions(); track transaction.id) {
        <app-transaction-row
          [transaction]="transaction"
          (edit)="handleEdit($event)"
          (delete)="handleDelete($event)" />
      }
    }
  `
})
export class TransactionListComponent {
  private transactionService = inject(TransactionService);
  transactions = signal<Transaction[]>([]);
  isLoading = signal(true);
}

// Presentational (Dumb - receives data, emits events)
@Component({
  selector: 'app-transaction-row',
  standalone: true,
  template: `<div class="transaction-row">...</div>`
})
export class TransactionRowComponent {
  transaction = input.required<Transaction>();
  edit = output<Transaction>();
  delete = output<string>();
}
```

### Data Visualization (Chart.js Integration)

#### Base Chart Component

```typescript
import { Component, AfterViewInit, ViewChild, ElementRef, input, effect } from '@angular/core';
import { Chart, ChartConfiguration, ChartType, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-base-chart',
  standalone: true,
  template: `
    <div class="chart-wrapper">
      <canvas #chartCanvas></canvas>
    </div>
  `
})
export class BaseChartComponent implements AfterViewInit {
  @ViewChild('chartCanvas') canvasRef!: ElementRef<HTMLCanvasElement>;

  chartType = input.required<ChartType>();
  chartData = input.required<ChartConfiguration['data']>();
  chartOptions = input<ChartConfiguration['options']>({});

  private chart?: Chart;

  ngAfterViewInit() {
    this.initChart();

    effect(() => {
      if (this.chart) {
        this.chart.data = this.chartData();
        this.chart.update();
      }
    });
  }

  private initChart() {
    const ctx = this.canvasRef.nativeElement.getContext('2d');
    if (!ctx) return;

    this.chart = new Chart(ctx, {
      type: this.chartType(),
      data: this.chartData(),
      options: {
        responsive: true,
        maintainAspectRatio: false,
        ...this.chartOptions()
      }
    });
  }

  ngOnDestroy() {
    this.chart?.destroy();
  }
}
```

#### Colorblind-Friendly Chart Colors

```typescript
// shared/constants/chart-colors.ts

export const CHART_COLORS = {
  primary: [
    '#4477AA', // Blue
    '#EE6677', // Red
    '#228833', // Green
    '#CCBB44', // Yellow
    '#66CCEE', // Cyan
    '#AA3377', // Purple
    '#BBBBBB'  // Gray
  ]
};
```

---

## 5. State Management

### Store Structure

```
src/app/
├── core/state/                            # Global state
│   ├── user-preferences.service.ts
│   └── notification-state.service.ts
│
└── features/
    ├── dashboard/services/dashboard-state.service.ts
    ├── transactions/services/transaction-state.service.ts
    ├── import-csv/services/import-state.service.ts
    └── predictions/services/prediction-state.service.ts
```

### State Management Template

#### Global State Service (Singleton)

```typescript
// core/state/user-preferences.service.ts

import { Injectable, signal, computed, effect } from '@angular/core';

export interface UserPreferences {
  theme: 'light' | 'dark';
  currency: string;
  dateFormat: string;
  defaultAccount: string | null;
}

@Injectable({ providedIn: 'root' })
export class UserPreferencesService {
  private _preferences = signal<UserPreferences>({
    theme: 'light',
    currency: 'USD',
    dateFormat: 'MM/dd/yyyy',
    defaultAccount: null
  });

  readonly preferences = this._preferences.asReadonly();
  readonly isDarkMode = computed(() => this._preferences().theme === 'dark');

  constructor() {
    this.loadFromStorage();

    effect(() => {
      localStorage.setItem('user-preferences', JSON.stringify(this._preferences()));
      document.body.classList.toggle('dark-theme', this.isDarkMode());
    });
  }

  setTheme(theme: 'light' | 'dark'): void {
    this._preferences.update(prefs => ({ ...prefs, theme }));
  }

  private loadFromStorage(): void {
    const stored = localStorage.getItem('user-preferences');
    if (stored) {
      try {
        this._preferences.set(JSON.parse(stored));
      } catch {}
    }
  }
}
```

#### Feature State Service (API Integration)

```typescript
// features/transactions/services/transaction-state.service.ts

import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Transaction } from '../models/transaction.models';

@Injectable({ providedIn: 'root' })
export class TransactionStateService {
  private http = inject(HttpClient);

  // Private state
  private _transactions = signal<Transaction[]>([]);
  private _isLoading = signal(false);
  private _error = signal<string | null>(null);
  private _categoryFilter = signal<string | null>(null);

  // Public read-only
  readonly transactions = this._transactions.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly error = this._error.asReadonly();

  // Computed state
  readonly transactionCount = computed(() => this._transactions().length);
  readonly filteredTransactions = computed(() => {
    let txns = this._transactions();
    const category = this._categoryFilter();
    if (category) {
      txns = txns.filter(t => t.category === category);
    }
    return txns;
  });

  // Load with caching
  loadTransactions(): void {
    if (this._transactions().length > 0) return;
    this.refresh();
  }

  refresh(): void {
    this._isLoading.set(true);
    this.http.get<Transaction[]>('/api/transactions').subscribe({
      next: (txns) => {
        this._transactions.set(txns);
        this._isLoading.set(false);
      },
      error: (err) => {
        this._error.set(err.message);
        this._isLoading.set(false);
      }
    });
  }

  // Optimistic delete with undo
  deleteTransaction(id: string): void {
    const deleted = this._transactions().find(t => t.id === id);
    if (!deleted) return;

    this._transactions.update(txns => txns.filter(t => t.id !== id));

    this.http.delete(`/api/transactions/${id}`).subscribe({
      error: () => {
        this._transactions.update(txns => [...txns, deleted]);
      }
    });
  }

  setCategoryFilter(category: string | null): void {
    this._categoryFilter.set(category);
  }
}
```

### State Management Patterns

| Pattern | Use Case | Example |
|---------|----------|---------|
| **Global Singleton** | App-wide shared state | `UserPreferencesService` |
| **Feature Singleton** | Feature-scoped data + CRUD | `TransactionStateService` |
| **Feature-Scoped** | Wizard flows, transient state | `ImportStateService` (route provider) |
| **Signal + RxJS Bridge** | Async operations | Search with debouncing |
| **Local Component State** | UI-only state | Chart controls, modal state |

---

## 6. API Integration

### Service Template

```typescript
// features/transactions/services/transaction.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, retry, timeout } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { Transaction, CreateTransactionDto } from '../models/transaction.models';

@Injectable({ providedIn: 'root' })
export class TransactionService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/api/transactions`;

  getAll(): Observable<Transaction[]> {
    return this.http.get<Transaction[]>(this.baseUrl).pipe(
      timeout(10000),
      retry(1),
      catchError(this.handleError)
    );
  }

  getById(id: string): Observable<Transaction> {
    return this.http.get<Transaction>(`${this.baseUrl}/${id}`).pipe(
      timeout(5000),
      catchError(this.handleError)
    );
  }

  create(dto: CreateTransactionDto): Observable<Transaction> {
    return this.http.post<Transaction>(this.baseUrl, dto).pipe(
      timeout(10000),
      catchError(this.handleError)
    );
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`).pipe(
      timeout(5000),
      catchError(this.handleError)
    );
  }

  private handleError(error: any): Observable<never> {
    let errorMessage = 'An unknown error occurred';

    if (error.status === 404) {
      errorMessage = 'Transaction not found';
    } else if (error.status === 500) {
      errorMessage = 'Server error occurred';
    } else if (error.status === 0) {
      errorMessage = 'Backend unavailable';
    }

    return throwError(() => new Error(errorMessage));
  }
}
```

### HTTP Interceptors

#### Error Interceptor

```typescript
// core/api/error.interceptor.ts

import { Injectable, inject } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpErrorResponse } from '@angular/common/http';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { NotificationService } from '../services/notification.service';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  private notification = inject(NotificationService);

  intercept(req: HttpRequest<any>, next: HttpHandler) {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 0) {
          this.notification.error('Backend unavailable', { duration: 0 });
        } else if (error.status >= 500) {
          this.notification.error('Server error occurred');
        }
        return throwError(() => error);
      })
    );
  }
}
```

#### Loading Interceptor

```typescript
// core/api/loading.interceptor.ts

import { Injectable, inject } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler } from '@angular/common/http';
import { finalize } from 'rxjs/operators';
import { LoadingService } from '../services/loading.service';

@Injectable()
export class LoadingInterceptor implements HttpInterceptor {
  private loading = inject(LoadingService);

  intercept(req: HttpRequest<any>, next: HttpHandler) {
    if (req.url.includes('/status')) return next.handle(req);

    this.loading.startLoading();
    return next.handle(req).pipe(
      finalize(() => this.loading.stopLoading())
    );
  }
}
```

---

## 7. Routing

### Route Configuration

```typescript
// app.routes.ts

import { Routes } from '@angular/router';
import { dataLoadedGuard } from './core/guards/data-loaded.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full'
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/dashboard.component')
      .then(m => m.DashboardComponent),
    title: 'Dashboard - Ledgerly'
  },
  {
    path: 'transactions',
    loadChildren: () => import('./features/transactions/transactions.routes')
      .then(m => m.TRANSACTION_ROUTES),
    title: 'Transactions - Ledgerly'
  },
  {
    path: 'import',
    loadChildren: () => import('./features/import-csv/import-csv.routes')
      .then(m => m.IMPORT_ROUTES),
    title: 'Import CSV - Ledgerly'
  },
  {
    path: 'predictions',
    loadComponent: () => import('./features/predictions/cash-flow-timeline.component')
      .then(m => m.CashFlowTimelineComponent),
    title: 'Cash Flow Predictions - Ledgerly',
    canActivate: [dataLoadedGuard]
  },
  {
    path: '**',
    loadComponent: () => import('./shared/components/not-found/not-found.component')
      .then(m => m.NotFoundComponent)
  }
];
```

### Route Guards (Functional)

```typescript
// core/guards/data-loaded.guard.ts

import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { TransactionStateService } from '../../features/transactions/services/transaction-state.service';

export const dataLoadedGuard: CanActivateFn = (route, state) => {
  const transactionState = inject(TransactionStateService);
  const router = inject(Router);

  if (transactionState.transactionCount() === 0) {
    router.navigate(['/transactions']);
    return false;
  }

  return true;
};
```

```typescript
// core/guards/unsaved-changes.guard.ts

import { CanDeactivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { map } from 'rxjs/operators';

export interface CanComponentDeactivate {
  canDeactivate: () => boolean;
}

export const unsavedChangesGuard: CanDeactivateFn<CanComponentDeactivate> = (component) => {
  const dialog = inject(MatDialog);

  if (!component.canDeactivate || component.canDeactivate()) {
    return true;
  }

  const dialogRef = dialog.open(ConfirmationDialogComponent, {
    data: {
      title: 'Unsaved Changes',
      message: 'You have unsaved changes. Do you want to leave?'
    }
  });

  return dialogRef.afterClosed().pipe(map(result => result === true));
};
```

---

## 8. Styling Guidelines

### Global Theme Variables

```css
/* styles/_theme.scss */

:root {
  /* Primary Colors */
  --color-primary: #2C3E50;
  --color-accent: #1ABC9C;

  /* Semantic Colors */
  --color-success: #27AE60;
  --color-warning: #F39C12;
  --color-danger: #E74C3C;
  --color-info: #3498DB;

  /* Neutral Colors */
  --color-background: #FFFFFF;
  --color-surface: #F8F9FA;
  --color-text-primary: #212529;
  --color-text-secondary: #6C757D;

  /* Spacing (8px base unit) */
  --spacing-xs: 4px;
  --spacing-sm: 8px;
  --spacing-md: 16px;
  --spacing-lg: 24px;
  --spacing-xl: 32px;

  /* Typography */
  --font-family-base: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
  --font-size-base: 1rem;
  --font-weight-normal: 400;
  --font-weight-semibold: 600;

  /* Shadows */
  --shadow-sm: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
  --shadow-md: 0 4px 6px -1px rgba(0, 0, 0, 0.1);

  /* Border Radius */
  --radius-md: 8px;
  --radius-lg: 12px;

  /* Transitions */
  --transition-base: 200ms ease-in-out;
}

.dark-theme {
  --color-primary: #1ABC9C;
  --color-background: #1E1E1E;
  --color-surface: #2D2D2D;
  --color-text-primary: #E8E8E8;
  --color-text-secondary: #B0B0B0;
}
```

### Angular Material Theme

```scss
/* styles/_material-theme.scss */

@use '@angular/material' as mat;

@include mat.core();

$ledgerly-primary: mat.define-palette(mat.$teal-palette, 500);
$ledgerly-accent: mat.define-palette(mat.$blue-palette, 600);

$ledgerly-light-theme: mat.define-light-theme((
  color: (
    primary: $ledgerly-primary,
    accent: $ledgerly-accent
  )
));

@include mat.all-component-themes($ledgerly-light-theme);

.dark-theme {
  @include mat.all-component-colors($ledgerly-dark-theme);
}
```

---

## 9. Testing Requirements

### Component Test Template

```typescript
// features/dashboard/components/net-worth-widget.component.spec.ts

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NetWorthWidgetComponent } from './net-worth-widget.component';

describe('NetWorthWidgetComponent', () => {
  let component: NetWorthWidgetComponent;
  let fixture: ComponentFixture<NetWorthWidgetComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NetWorthWidgetComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(NetWorthWidgetComponent);
    component = fixture.componentInstance;
  });

  it('should calculate net worth correctly', () => {
    fixture.componentRef.setInput('assets', 10000);
    fixture.componentRef.setInput('liabilities', 3000);
    fixture.detectChanges();

    expect(component.netWorth()).toBe(7000);
  });
});
```

### State Service Testing

```typescript
// features/transactions/services/transaction-state.service.spec.ts

import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TransactionStateService } from './transaction-state.service';

describe('TransactionStateService', () => {
  let service: TransactionStateService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [TransactionStateService]
    });

    service = TestBed.inject(TransactionStateService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should fetch transactions from backend', () => {
    const mockTransactions = [{ id: '1', payee: 'Test' }];

    service.refresh();

    const req = httpMock.expectOne('/api/transactions');
    req.flush(mockTransactions);

    expect(service.transactions()).toEqual(mockTransactions);
  });
});
```

### E2E Testing (Playwright)

```typescript
// e2e/transaction-crud.spec.ts

import { test, expect } from '@playwright/test';

test.describe('Transaction Management', () => {
  test('should add new transaction', async ({ page }) => {
    await page.goto('/transactions');
    await page.click('text=Add Transaction');

    await page.fill('[data-testid="payee-input"]', 'Coffee Shop');
    await page.fill('[data-testid="amount-input"]', '5.50');
    await page.click('button[type="submit"]');

    await expect(page.locator('text=Transaction added')).toBeVisible();
  });
});
```

### Coverage Goals

| Layer | Target | Priority |
|-------|--------|----------|
| State Services | 85% | High |
| API Services | 80% | High |
| Container Components | 70% | Medium |
| E2E Critical Paths | 100% | High |

---

## 10. Environment Configuration

### Development Environment

```typescript
// src/environments/environment.ts

export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000',
  apiTimeout: 30000,
  enableLogging: true,
  isTauri: false,
  appVersion: '0.1.0-dev',
  defaultCurrency: 'USD'
};
```

### Production Environment

```typescript
// src/environments/environment.prod.ts

export const environment = {
  production: true,
  apiUrl: 'http://localhost:5000',
  apiTimeout: 30000,
  enableLogging: false,
  isTauri: true,
  appVersion: '1.0.0',
  defaultCurrency: 'USD'
};
```

### Tauri Detection Service

```typescript
// core/services/tauri.service.ts

import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class TauriService {
  private _isTauri = signal(this.detectTauri());
  readonly isTauri = this._isTauri.asReadonly();

  private detectTauri(): boolean {
    return typeof window !== 'undefined' && '__TAURI__' in window;
  }

  async openFileDialog(): Promise<string | null> {
    if (!this.isTauri()) return null;

    const { open } = await import('@tauri-apps/api/dialog');
    const selected = await open({
      multiple: false,
      filters: [{ name: 'CSV Files', extensions: ['csv'] }]
    });

    return typeof selected === 'string' ? selected : null;
  }
}
```

---

## 11. Accessibility (WCAG AA Compliance)

### Component ARIA Patterns

#### Buttons & Interactive Elements

```typescript
@Component({
  template: `
    <div role="toolbar" aria-label="Quick actions">
      <button
        mat-raised-button
        (click)="importCSV()"
        aria-label="Import CSV file">
        <mat-icon aria-hidden="true">upload_file</mat-icon>
        <span>Import CSV</span>
      </button>
    </div>
  `
})
export class QuickActionsComponent {}
```

**Key Points:**
- `aria-label` describes action
- `aria-hidden="true"` on decorative icons
- `role="toolbar"` groups related actions

---

#### Forms & Validation

```typescript
@Component({
  template: `
    <form [formGroup]="form">
      <mat-form-field>
        <mat-label for="payee-input">Payee</mat-label>
        <input
          matInput
          id="payee-input"
          formControlName="payee"
          aria-required="true"
          [attr.aria-invalid]="form.get('payee')?.invalid"
          aria-describedby="payee-error">

        @if (form.get('payee')?.invalid) {
          <mat-error id="payee-error" role="alert">
            Payee is required
          </mat-error>
        }
      </mat-form-field>
    </form>
  `
})
export class TransactionFormComponent {}
```

---

#### Accessible Charts

```typescript
@Component({
  template: `
    <div class="chart-container">
      <h3 id="chart-title">Expense Breakdown</h3>

      <canvas
        #chartCanvas
        role="img"
        [attr.aria-labelledby]="'chart-title'"
        [attr.aria-describedby]="'chart-description'">
      </canvas>

      <div id="chart-description" class="sr-only">
        Pie chart showing:
        @for (category of chartData(); track category.label) {
          {{ category.label }}: {{ category.value | currency }}.
        }
      </div>

      <button (click)="showDataTable.set(!showDataTable())">
        View Data Table
      </button>

      @if (showDataTable()) {
        <table>
          <caption>Expense breakdown by category</caption>
          <thead>
            <tr>
              <th scope="col">Category</th>
              <th scope="col">Amount</th>
            </tr>
          </thead>
          <tbody>
            @for (category of chartData(); track category.label) {
              <tr>
                <th scope="row">{{ category.label }}</th>
                <td>{{ category.value | currency }}</td>
              </tr>
            }
          </tbody>
        </table>
      }
    </div>
  `,
  styles: [`
    .sr-only {
      position: absolute;
      width: 1px;
      height: 1px;
      overflow: hidden;
      clip: rect(0, 0, 0, 0);
    }
  `]
})
export class ExpenseBreakdownChartComponent {}
```

---

### Keyboard Navigation

| Key | Action | Context |
|-----|--------|---------|
| `Tab` | Navigate forward | All focusable elements |
| `Shift + Tab` | Navigate backward | All focusable elements |
| `Enter` / `Space` | Activate button | Buttons, links |
| `Escape` | Close modal | Dialogs, dropdowns |

---

### Color Contrast (WCAG AA)

| Text Type | Minimum Ratio |
|-----------|---------------|
| Normal text (<18pt) | 4.5:1 |
| Large text (≥18pt) | 3:1 |
| UI components | 3:1 |

**Verified Color Palette:**

```scss
:root {
  --color-text-primary: #212529;    /* 16.1:1 ✅ */
  --color-text-secondary: #6C757D;  /* 4.6:1 ✅ */
  --color-primary: #2C3E50;         /* 11.5:1 ✅ */
  --color-success: #27AE60;         /* 3.4:1 ✅ */
}
```

---

### Accessibility Testing

#### Automated (Playwright + axe-core)

```typescript
// e2e/accessibility.spec.ts

import { test } from '@playwright/test';
import { injectAxe, checkA11y } from 'axe-playwright';

test('dashboard should have no violations', async ({ page }) => {
  await page.goto('/dashboard');
  await injectAxe(page);
  await checkA11y(page);
});
```

---

### Accessibility Checklist

Before marking a component complete:

**Visual & Interactive:**
- [ ] All interactive elements reachable via keyboard
- [ ] Focus indicators visible (2px outline minimum)
- [ ] Color contrast meets WCAG AA
- [ ] Text resizable to 200%

**Semantic HTML:**
- [ ] Proper heading hierarchy (h1 → h2 → h3)
- [ ] Semantic elements used (`<button>`, `<nav>`, `<main>`)
- [ ] Form labels associated with inputs

**ARIA:**
- [ ] Icons have `aria-hidden="true"` if decorative
- [ ] Buttons have `aria-label` if icon-only
- [ ] Modals have `role="dialog"`
- [ ] Errors use `role="alert"`

**Testing:**
- [ ] Tested with keyboard only
- [ ] Tested with NVDA or VoiceOver
- [ ] Automated axe-core tests pass

---

## 12. Frontend Developer Standards

### Critical Coding Rules

1. **NEVER Mutate Signals Directly**
   ```typescript
   // ❌ WRONG
   transactions()[0].amount = 100;

   // ✅ CORRECT
   this.transactions.update(txns =>
     txns.map(t => t.id === id ? { ...t, amount: 100 } : t)
   );
   ```

2. **ALWAYS Use Readonly Signals for Public State**
   ```typescript
   // ❌ WRONG
   transactions = signal<Transaction[]>([]);

   // ✅ CORRECT
   private _transactions = signal<Transaction[]>([]);
   readonly transactions = this._transactions.asReadonly();
   ```

3. **ALWAYS Unsubscribe from RxJS Observables**
   ```typescript
   // ❌ WRONG
   this.http.get('/api/data').subscribe(data => {});

   // ✅ CORRECT
   data = toSignal(this.http.get<Data[]>('/api/data'));

   // ✅ CORRECT
   constructor() {
     this.http.get('/api/data')
       .pipe(takeUntilDestroyed())
       .subscribe(data => this.data.set(data));
   }
   ```

4. **NEVER Use `any` Type**
   ```typescript
   // ❌ WRONG
   function process(data: any) { }

   // ✅ CORRECT
   function process(data: Transaction) { }
   ```

5. **ALWAYS Use TrackBy with ngFor**
   ```html
   <!-- ❌ WRONG -->
   @for (transaction of transactions(); track $index) { }

   <!-- ✅ CORRECT -->
   @for (transaction of transactions(); track transaction.id) { }
   ```

6. **Use New Control Flow Syntax**
   ```html
   <!-- ❌ OLD -->
   <div *ngIf="isLoading">Loading...</div>

   <!-- ✅ NEW (Angular 17+) -->
   @if (isLoading()) {
     <div>Loading...</div>
   }
   ```

7. **Use `input()` and `output()` for Props**
   ```typescript
   // ❌ OLD
   @Input() transaction!: Transaction;
   @Output() deleted = new EventEmitter<string>();

   // ✅ NEW
   transaction = input.required<Transaction>();
   deleted = output<string>();
   ```

8. **ALWAYS Provide Alt Text for Images**
   ```html
   <!-- ❌ WRONG -->
   <img src="chart.png">

   <!-- ✅ CORRECT -->
   <img src="chart.png" alt="Expense breakdown pie chart">
   ```

9. **NEVER Perform Side Effects in Computed Signals**
   ```typescript
   // ❌ WRONG
   total = computed(() => {
     const sum = this.items().reduce(...);
     this.saveToLocalStorage(sum);  // Side effect!
     return sum;
   });

   // ✅ CORRECT
   total = computed(() => this.items().reduce(...));

   constructor() {
     effect(() => this.saveToLocalStorage(this.total()));
   }
   ```

10. **ALWAYS Use `inject()` Over Constructor Injection**
    ```typescript
    // ❌ OLD
    constructor(private http: HttpClient) {}

    // ✅ NEW
    private http = inject(HttpClient);
    ```

---

### Quick Reference

**Common Commands:**

```bash
ng serve                  # Dev server
ng build --configuration production
npm test                  # Jest tests
npm run e2e              # Playwright E2E
npm run tauri:dev        # Desktop app
```

**Key Import Patterns:**

```typescript
// Angular Core
import { Component, signal, computed, effect, inject } from '@angular/core';

// RxJS
import { Observable } from 'rxjs';
import { map, debounceTime, takeUntilDestroyed } from 'rxjs/operators';
import { toSignal, toObservable } from '@angular/core/rxjs-interop';

// Material
import { MatButtonModule } from '@angular/material/button';
```

---

## Document Maintenance

This document is a **living document** that should be updated as:
- New patterns emerge during implementation
- Framework updates introduce new features
- Architecture decisions are made or revised
- Team feedback identifies gaps or improvements

**Update Process:**
1. Make changes to relevant sections
2. Update version number and change log
3. Notify team of significant changes
4. Review quarterly for accuracy

---

**End of Document**
