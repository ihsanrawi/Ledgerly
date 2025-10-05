# Coding Standards

**Purpose:** MANDATORY rules for AI agents and human developers. These standards directly control code generation behavior.

## Core Standards

**Languages & Runtimes:**
- **Backend:** C# 12 with .NET 8.0.4 LTS
- **Frontend:** TypeScript 5.3.3 with Angular 17.3.8
- **Desktop:** Rust (Tauri 1.6.1)

**Style & Linting:**
- **C#:** Roslyn Analyzers + StyleCop rules (configured in `.editorconfig`)
- **TypeScript:** ESLint 8.57.0 with Angular recommended rules
- **Formatting:** Prettier 3.2.5 for TypeScript/JavaScript/HTML/CSS

**Test Organization:**
- **Backend:** Co-located with features (`Features/ImportCsv/ImportCsv.Tests/`)
- **Frontend:** `tests/unit/` for Jest, `tests/e2e/` for Playwright
- **Naming:** `{Class}Tests.cs` or `{Component}.spec.ts`

## Critical Rules

**MANDATORY - AI must follow these rules:**

1. **Logging Only:** Never use `console.log` in production code - use `Serilog` (backend) or Angular's logging service (frontend)

2. **Money Precision:** All monetary amounts MUST use `Money` value object (INTEGER cents storage) - never raw decimal

3. **hledger Validation:** ALL .hledger file writes MUST call `hledger check` validation before committing

4. **Correlation IDs:** Every API request MUST include correlation ID for tracing

5. **Optimistic Concurrency:** All entity updates MUST check `RowVersion` to prevent data loss

6. **Error Handling:** Throw specific exceptions (`HledgerValidationException`, `ConcurrencyException`) - never generic `Exception`

7. **Nullable Reference Types:** Always enable and respect C# nullable annotations

8. **Async/Await:** Use `async`/`await` for all I/O operations - never `.Result` or `.Wait()`

9. **Dependency Injection:** Constructor injection only - no service locator pattern

10. **FluentValidation:** All command/query inputs MUST have validators

11. **Atomic File Operations:** `.hledger` writes use temp file → validate → atomic rename pattern

12. **Cache Invalidation:** `.hledger` file changes MUST trigger cache synchronization

---
