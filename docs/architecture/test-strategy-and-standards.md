# Test Strategy and Standards

## Testing Philosophy

**Approach:** Test-after with strong coverage requirements (not strict TDD for solo developer)

**Coverage Goals:**
- **Unit Tests:** 80% code coverage minimum
- **Integration Tests:** All critical paths (CSV import, transaction CRUD, cash flow predictions)
- **E2E Tests:** Core user journeys only (import → categorize → dashboard)

**Test Pyramid:**
- **70% Unit Tests:** Fast, isolated, comprehensive
- **20% Integration Tests:** Database + hledger CLI integration
- **10% E2E Tests:** Critical happy paths only

## Test Types and Organization

### Unit Tests (Backend)

**Framework:** xUnit 2.7.0

**File Convention:** `{Class}Tests.cs`

**Location:** Co-located in `{Feature}.Tests/` folders

**Mocking Library:** NSubstitute 5.1.0

**Coverage Requirement:** 80% minimum

**AI Agent Requirements:**
- Generate tests for all public methods
- Cover edge cases and error conditions
- Follow AAA pattern (Arrange, Act, Assert)
- Mock all external dependencies (database, hledger CLI, file I/O)

### Unit Tests (Frontend)

**Framework:** Jest 29.7.0

**File Convention:** `{Component}.spec.ts`

**Location:** `tests/unit/` directory

**Coverage Requirement:** 70% minimum

**Testing Strategy:**
- Test component logic, not Angular internals
- Mock `HttpClient` responses
- Test Signal state changes
- Snapshot tests for complex templates

### Integration Tests

**Scope:** Feature slices with real dependencies (SQLite + hledger binary)

**Location:** `tests/Integration.Tests/`

**Test Infrastructure:**
- **Database:** In-memory SQLite for fast tests
- **hledger Binary:** Real binary execution with test `.hledger` files
- **File System:** Temporary directories cleaned after tests

### End-to-End Tests

**Framework:** Playwright 1.42.1

**Scope:** Critical user paths only

**Environment:** Full Tauri app with test database

**Critical Paths:**
1. CSV Import Flow: Upload CSV → Map columns → Review suggestions → Confirm → Verify dashboard
2. Manual Transaction: Add transaction → Validate → Check in dashboard
3. Cash Flow Timeline: View predictions → Confirm recurring pattern → Check updated timeline

## Test Data Management

**Strategy:** Fixture files + factories

**Fixtures:** `tests/TestData/`
- `sample.hledger` - Pre-populated ledger with 100 transactions
- `chase-sample.csv` - Real bank CSV format
- `bofa-sample.csv` - Bank of America format

**Factories:** Bogus 35.5.0 for generating realistic test data

**Cleanup:** Delete test `.hledger` and `.db` files after each test

## Continuous Testing

**CI Integration:**
- **On PR:** Run all unit tests (backend + frontend)
- **On merge to main:** Run unit + integration tests
- **On tag (release):** Run all tests including E2E

**Performance Tests:**
- Dashboard load: <2s for 5,000 transactions (NFR1)
- CSV import: <5s for 1,000 transactions (NFR2)
- Search/filter: <1s for 10,000 transactions (NFR3)

**Security Tests:**
- **SAST:** CodeQL on every PR
- **Dependency scanning:** Dependabot weekly
- **Secret scanning:** GitHub secret scanner enabled

---
