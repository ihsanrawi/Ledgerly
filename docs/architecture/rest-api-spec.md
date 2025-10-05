# REST API Spec

**Internal REST API for Ledgerly desktop application.**

Backend: ASP.NET Core 8.0 with Minimal APIs
Frontend: Angular 17+ with Signals

All endpoints are local-only (`http://localhost:5000`) with no external authentication required for MVP (desktop app security boundary).

**Full OpenAPI 3.0 specification available at:** `docs/api/openapi.yaml` (extracted for readability)

## Key Endpoints Summary

**Import Endpoints:**
- `POST /api/import/preview` - Upload CSV and preview import
- `POST /api/import/confirm` - Confirm and execute CSV import
- `GET /api/import/mappings` - Get saved column mappings

**Transaction Endpoints:**
- `GET /api/transactions` - Get transactions (pagination, filtering, search)
- `POST /api/transactions` - Add new transaction
- `PUT /api/transactions/{id}` - Update transaction
- `DELETE /api/transactions/{id}` - Delete transaction (comment out in .hledger)
- `POST /api/transactions/batch` - Batch categorize transactions

**Dashboard Endpoints:**
- `GET /api/dashboard` - Get all dashboard data
- `GET /api/dashboard/networth` - Get current net worth
- `GET /api/dashboard/cashflow-timeline?days={30|60|90}` - **Cash flow predictions (PRIMARY DIFFERENTIATOR)**

**Predictions Endpoints:**
- `GET /api/predictions/recurring` - Get recurring transaction patterns
- `POST /api/predictions/recurring/{id}/confirm` - Confirm pattern
- `DELETE /api/predictions/recurring/{id}/reject` - Reject pattern
- `POST /api/predictions/detect` - Trigger detection (normally scheduled nightly)

**Categorization Endpoints:**
- `GET /api/categorize/suggestions/{transactionId}` - Get category suggestions
- `POST /api/categorize/confirm` - Confirm suggestion (triggers learning)
- `POST /api/categorize/correct` - Correct suggestion (triggers negative learning)

**Reports Endpoints:**
- `GET /api/reports/expenses` - Get expense report (time filtering, grouping)
- `POST /api/reports/export/pdf` - Export report data for client-side PDF generation

**Accounts & Categories:**
- `GET /api/accounts` - Get account hierarchy
- `GET /api/categories` - Get all categories

## API Design Decisions

**Key Decisions:**
1. **Local-only endpoints**: No authentication for MVP (Tauri security boundary)
2. **Minimal API style**: RESTful conventions with simple paths
3. **Pagination built-in**: All list endpoints support paging
4. **Client-side PDF**: `/reports/export/pdf` returns data, not binary PDF (jsPDF renders in Angular)
5. **Adaptive learning endpoints**: Explicit confirm/correct endpoints trigger learning algorithms
6. **UUID identifiers**: All entities use GUIDs for client-side ID generation compatibility

**Error Response Format:**
```json
{
  "errorCode": "HLEDGER_VALIDATION_FAILED",
  "message": "Transaction validation failed",
  "details": "...",
  "validationErrors": {},
  "timestamp": "2025-01-15T14:30:00Z",
  "traceId": "abc-123-def-456"
}
```

---
