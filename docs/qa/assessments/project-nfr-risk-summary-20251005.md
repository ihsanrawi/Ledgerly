# Project-Wide NFR Risk Summary
## Ledgerly - Early Architecture Quality Assessment

**Date:** 2025-10-05
**Reviewer:** Quinn (QA Test Architect)
**Scope:** All 7 Epics - Pre-Development Risk Analysis
**Assessment Type:** Proactive / Preventative

---

## Executive Dashboard

### Overall Project Health

| Metric | Score | Status |
|--------|-------|--------|
| **Overall Quality Score** | **77/100** | 🟡 MODERATE RISK |
| **Security Posture** | **82/100** | ✅ ACCEPTABLE |
| **Performance Posture** | **70/100** | ⚠️ NEEDS ATTENTION |
| **Reliability Posture** | **72/100** | ⚠️ NEEDS ATTENTION |
| **Maintainability Posture** | **90/100** | ✅ STRONG |

### Epic-Level Quality Scores

| Epic | Quality Score | Risk Level | Critical Issues | Status |
|------|--------------|-----------|----------------|--------|
| Epic 1: Foundation | 80/100 | 🟡 MEDIUM | 3 | ⚠️ Address before Epic 2 |
| Epic 2: Import CSV | **70/100** | 🔴 **HIGH** | **7** | 🚨 **REQUIRES REWORK** |
| Epic 3: Dashboard | 80/100 | 🟡 MEDIUM | 4 | ⚠️ Monitor performance |
| Epic 4: Transactions | 80/100 | 🟡 MEDIUM | 5 | ✅ Manageable |
| Epic 5: Predictions | **60/100** | 🔴 **HIGH** | **4** | 🚨 **ACCURACY RISK** |
| Epic 6: Ledger Gen | 90/100 | 🟢 LOW | 2 | ✅ On track |
| Epic 7: Reports | 90/100 | 🟢 LOW | 1 | ✅ Optional epic |

---

## 🚨 Top 5 Project-Blocking Issues

### 1. Epic 2: CSV Injection Vulnerability (RCE)
**Severity:** 🔴 CRITICAL
**NFR Impact:** Security, NFR9 (95% import success), NFR11 (zero data loss)
**Risk:** Remote code execution via formula injection (=cmd|'/c calc')
**Attack Vector:** Import malicious CSV → export to Excel → code executes
**Timeline Impact:** +3 hours to Epic 2
**Mitigation:**
```csharp
public string SanitizeCsvCell(string value) {
    if (value.StartsWith("=") || value.StartsWith("+") ||
        value.StartsWith("-") || value.StartsWith("@") || value.StartsWith("|"))
        return "'" + value; // Force text interpretation
    return value;
}
```
**Test Coverage Required:** 100% of injection vectors (`=`, `+`, `-`, `@`, `|`)

---

### 2. Epic 5: 80% Accuracy Target Not Enforceable
**Severity:** 🔴 CRITICAL
**NFR Impact:** FR17 (recurring detection >80%), Product Differentiator at Risk
**Risk:** Algorithm ships with 50% accuracy, users lose trust in predictions
**Timeline Impact:** +1 hour to Epic 5
**Mitigation:**
```csharp
[Fact]
public async Task RecurringDetection_LabeledDataset_Achieves80PercentAccuracy() {
    var dataset = LoadLabeledDataset(); // Story 5.1
    var predictions = await _detector.DetectRecurring(dataset.Transactions);
    var accuracy = CalculateAccuracy(predictions, dataset.Labels);
    Assert.True(accuracy >= 0.80,
        $"Accuracy {accuracy:P} below 80% threshold (FR17)");
}
```
**Acceptance Criteria Addition:** Story 5.2 AC #5 MUST include accuracy assertion

---

### 3. Epic 2: No Rollback for Partial Import Failures
**Severity:** 🔴 HIGH
**NFR Impact:** NFR11 (zero data loss), NFR10 (crash rate)
**Risk:** Import fails at txn 500/1000 → first 499 committed, .hledger inconsistent
**Timeline Impact:** +4 hours to Epic 2
**Mitigation:** Wrap import in database transaction with .hledger rollback
**Test Coverage Required:**
- Simulate DB failure mid-import
- Simulate disk full during .hledger write
- Verify all-or-nothing guarantee

---

### 4. Epic 2: Performance Target (NFR2) Not Tested
**Severity:** 🔴 HIGH
**NFR Impact:** NFR2 (import <5s for 1,000 txns)
**Risk:** Discover performance issues in Week 8 (too late to refactor)
**Timeline Impact:** +4 hours to Epic 2
**Mitigation:** Add performance test to Story 2.6
**Test Required:**
```csharp
[Fact]
public async Task ImportCsv_1000Transactions_CompletesUnder5Seconds() {
    var csv = Generate1000TransactionCsv();
    var stopwatch = Stopwatch.StartNew();
    await _handler.Handle(new ImportCsvCommand(csv));
    stopwatch.Stop();
    Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(5),
        $"Import took {stopwatch.Elapsed.TotalSeconds}s (NFR2 requires <5s)");
}
```

---

### 5. Epic 2: O(n²) Duplicate Detection
**Severity:** 🔴 HIGH
**NFR Impact:** NFR2 (import time), NFR5 (60fps UI)
**Risk:** 1,000 imported × 5,000 existing = 5M hash comparisons
**Timeline Impact:** +2 hours to Epic 2
**Mitigation:** Use hash dictionary (O(1) lookup)
**Performance Impact:** Reduces 5M comparisons to 1,000 lookups (5000x faster)

---

## NFR Compliance Scorecard

### Security (ISO 25010: Confidentiality, Integrity, Authenticity)

| Epic | Security Status | Critical Gaps | Compliance % |
|------|----------------|---------------|--------------|
| Epic 1 | 🟡 CONCERNS | Binary verification gaps | 85% |
| Epic 2 | 🔴 **CONCERNS** | **CSV injection (RCE)** | **70%** |
| Epic 3 | 🟢 PASS | None | 100% |
| Epic 4 | 🟡 CONCERNS | Input validation | 85% |
| Epic 5 | 🟢 PASS | None | 100% |
| Epic 6 | 🟢 PASS | None | 100% |
| Epic 7 | 🟡 CONCERNS | Export injection | 90% |

**Overall Security Compliance:** 82%
**Target:** 95%
**Gap Analysis:** 13% gap driven by Epic 2 injection risks

**Recommendations:**
1. Mandatory: Fix Epic 2 CSV injection before release
2. Important: Add input validation to Epic 4 (amount, date, memo)
3. Nice-to-have: Reuse Epic 2 sanitization in Epic 7 CSV export

---

### Performance (ISO 25010: Time Behavior, Resource Utilization)

| NFR | Requirement | Tested In | Status | Evidence |
|-----|-------------|-----------|--------|----------|
| NFR1 | Dashboard <2s (5K txns) | Epic 3 Story 3.6 | ⚠️ AT RISK | Missing indexes |
| NFR2 | Import <5s (1K txns) | **Epic 2** | 🔴 **NOT TESTED** | **No test exists** |
| NFR3 | Search <1s (10K txns) | Epic 4 Story 4.5 | 🔴 **NOT TESTED** | **No test exists** |
| NFR4 | Cold start <3s | Epic 1 Story 1.5 | ✅ TESTABLE | Smoke test |
| NFR5 | 60fps interactions | All epics | ⚠️ AT RISK | Large charts, sync parsing |
| NFR13 | Memory <500MB | Not tested | ⏸️ DEFERRED | Manual testing Week 11 |

**Overall Performance Compliance:** 70%
**Target:** 100%
**Gap Analysis:**
- **CRITICAL:** 2 NFRs (NFR2, NFR3) have no tests (33% of perf requirements)
- **HIGH:** 2 NFRs (NFR1, NFR5) at risk due to missing optimizations

**Recommendations:**
1. **Mandatory (Epic 2):** Add NFR2 performance test (4 hours)
2. **Mandatory (Epic 4):** Add NFR3 performance test (3 hours)
3. **Critical (Epic 3):** Add database indexes before Story 3.6 test (1 hour)
4. **Important (Epic 5):** Optimize O(n²) pattern matching to O(n log n) (4 hours)

---

### Reliability (ISO 25010: Maturity, Fault Tolerance, Recoverability)

| Epic | Reliability Status | Critical Gaps | Compliance % |
|------|-------------------|---------------|--------------|
| Epic 1 | 🟡 CONCERNS | Binary recovery | 80% |
| Epic 2 | 🔴 **CONCERNS** | **No rollback** | **70%** |
| Epic 3 | 🟡 CONCERNS | Cache sync | 85% |
| Epic 4 | 🟢 PASS | None | 95% |
| Epic 5 | 🔴 **FAIL** | **Accuracy not enforceable** | **50%** |
| Epic 6 | 🟡 CONCERNS | Limited compatibility | 85% |
| Epic 7 | 🟢 PASS | None | 95% |

**Overall Reliability Compliance:** 72%
**Target:** 95%
**Gap Analysis:**
- **CRITICAL:** Epic 5 accuracy target (FR17) not enforceable in tests
- **HIGH:** Epic 2 partial import failures could corrupt data (NFR11 violation)

**NFR Traceability:**

| NFR | Requirement | Epic Coverage | Status |
|-----|-------------|---------------|--------|
| NFR8 | Atomic writes + backups | Epic 1, 2, 3, 4 | ✅ IMPLEMENTED |
| NFR10 | <0.1% crash rate | All epics | ⚠️ PARTIAL (error handling defined, telemetry deferred) |
| NFR11 | Zero data loss | Epic 1, 2 | 🔴 **AT RISK** (Epic 2 rollback missing) |
| NFR14 | 100% hledger validation | Epic 1, 6 | ✅ IMPLEMENTED |
| NFR15 | FileSystemWatcher <1s | Epic 3 | ⚠️ AT RISK (race conditions) |

**Recommendations:**
1. **Mandatory (Epic 5):** Add accuracy assertion to Story 5.2 (1 hour)
2. **Mandatory (Epic 2):** Add transactional import with rollback (4 hours)
3. **Critical (Epic 3):** Add cache hash validation (3 hours)

---

### Maintainability (ISO 25010: Modularity, Testability, Modifiability)

| Epic | Maintainability Status | Strengths | Score |
|------|----------------------|-----------|-------|
| Epic 1 | ✅ PASS | VSA structure, test harness | 95% |
| Epic 2 | ✅ PASS | 20+ CSV test files | 90% |
| Epic 3 | ✅ PASS | Reusable Chart.js components | 90% |
| Epic 4 | ✅ PASS | Reusable CRUD patterns | 90% |
| Epic 5 | 🟡 CONCERNS | Algorithm not unit-testable | 80% |
| Epic 6 | ✅ PASS | Template-based generation | 95% |
| Epic 7 | ✅ PASS | Reuses Epic 3 components | 95% |

**Overall Maintainability Compliance:** 90%
**Target:** 80%
**Status:** ✅ EXCEEDS TARGET

**Strengths:**
- VSA folder structure enforces modularity
- Test-first mindset (Epic 2 Story 2.1 collects data before coding)
- Clear coverage targets (80% unit, 70% frontend)
- Co-located tests (`{Feature}.Tests/`)

**Recommendations:**
1. **Nice-to-have (Epic 5):** Extract prediction algorithm to pure function (4 hours)
2. **Nice-to-have (Epic 3):** Extract Chart.js wrappers to shared library (3 hours)

---

## Risk Heatmap (Probability × Impact)

### Security Risks

| Risk | Probability | Impact | Score | Epic |
|------|------------|--------|-------|------|
| CSV injection (RCE) | HIGH | CRITICAL | 🔴 **9** | Epic 2 |
| Binary substitution | LOW | HIGH | 🟡 4 | Epic 1 |
| Path traversal | MEDIUM | HIGH | 🟡 6 | Epic 2 |
| Export injection | LOW | MEDIUM | 🟢 3 | Epic 7 |

**Top Security Risk:** CSV injection (Epic 2) - **MUST FIX**

---

### Performance Risks

| Risk | Probability | Impact | Score | Epic |
|------|------------|--------|-------|------|
| Import exceeds 5s | HIGH | HIGH | 🔴 **9** | Epic 2 |
| Dashboard exceeds 2s | MEDIUM | HIGH | 🟡 6 | Epic 3 |
| Search exceeds 1s | MEDIUM | MEDIUM | 🟡 5 | Epic 4 |
| Prediction job blocks UI | MEDIUM | MEDIUM | 🟡 5 | Epic 5 |

**Top Performance Risk:** Import time unknown (Epic 2) - **ADD TEST**

---

### Reliability Risks

| Risk | Probability | Impact | Score | Epic |
|------|------------|--------|-------|------|
| Accuracy <80% ships | HIGH | CRITICAL | 🔴 **9** | Epic 5 |
| Partial import corruption | MEDIUM | CRITICAL | 🔴 **8** | Epic 2 |
| Cache desync shows stale data | MEDIUM | HIGH | 🟡 6 | Epic 3 |
| Binary not found (no recovery) | LOW | HIGH | 🟡 4 | Epic 1 |

**Top Reliability Risk:** Accuracy target not enforced (Epic 5) - **MUST FIX**

---

## Timeline Impact Analysis

### Original Timeline (12 weeks)

| Week | Epic | Original Plan |
|------|------|---------------|
| 1 | Epic 1 | Foundation |
| 2-3 | Epic 2 | Import CSV |
| 4 | Epic 3 | Dashboard |
| 5-6 | Epic 4 | Transactions |
| 7-8 | Epic 6 | Categorization |
| 9-10 | Epic 5 + 7 | Predictions (1.5w) + Reports (0.5w) |
| 11-12 | Testing | E2E, Polish, Package |

### Recommended Timeline (13 weeks)

| Week | Epic | Adjusted Plan | Change |
|------|------|---------------|--------|
| 1 | Epic 1 | Foundation + 3 fixes | +6 hours (fits in week) |
| **2-4** | Epic 2 | Import CSV + 7 fixes | **+1 week** (+17.5 hours) |
| 5 | Epic 3 | Dashboard + 4 fixes | +9 hours (fits in week) |
| 6-7 | Epic 4 | Transactions + 5 fixes | +12 hours (fits in 2 weeks) |
| 8-9 | Epic 6 | Categorization + 2 fixes | +1.5 hours (fits in 2 weeks) |
| 10-11 | Epic 5 + 7 | Predictions + 4 fixes, Reports | +11 hours (fits in time-box) |
| 12-13 | Testing | E2E, Polish, Package | No change |

**Total Timeline Impact:** +1 week (driven by Epic 2)
**New Target:** 13 weeks (acceptable within 12-week original scope + buffer)

---

## Fix Effort Summary

### By Epic

| Epic | Issues | Total Effort | Priority |
|------|--------|-------------|----------|
| Epic 1 | 3 | 6 hours | 🟡 MEDIUM |
| Epic 2 | 7 | **17.5 hours** | 🔴 **CRITICAL** |
| Epic 3 | 4 | 9 hours | 🟡 MEDIUM |
| Epic 4 | 5 | 12 hours | 🟡 MEDIUM |
| Epic 5 | 4 | 10 hours | 🔴 **HIGH** |
| Epic 6 | 2 | 1.5 hours | 🟢 LOW |
| Epic 7 | 1 | 1 hour | 🟢 LOW |

**Total Fix Effort:** 57 hours (~1.5 weeks)

### By Severity

| Severity | Count | Total Effort |
|----------|-------|-------------|
| CRITICAL | 2 | 4 hours |
| HIGH | 10 | 28 hours |
| MEDIUM | 9 | 18 hours |
| LOW | 5 | 7 hours |

---

## Quality Gates Recommendations

### Epic 1 Gate Criteria
- [ ] Post-extraction SHA256 verification test passes
- [ ] Cross-platform permission validation test passes
- [ ] Tauri process execution decision documented (PASS/FAIL thresholds)

### Epic 2 Gate Criteria (STRICT)
- [ ] CSV injection sanitization implemented + tested (100% coverage)
- [ ] Path traversal protection implemented
- [ ] Performance test added (NFR2: <5s for 1,000 txns)
- [ ] Transactional import with rollback implemented
- [ ] O(n²) duplicate detection refactored to O(n)
- [ ] File size limit validation added

### Epic 3 Gate Criteria
- [ ] Database indexes added (category, date, amount)
- [ ] Cache hash validation on dashboard load
- [ ] FileSystemWatcher debouncing (500ms)
- [ ] Performance test passes (NFR1: <2s for 5,000 txns)

### Epic 5 Gate Criteria (STRICT)
- [ ] Accuracy assertion added to Story 5.2 (MUST be ≥80%)
- [ ] Integration test runs against labeled dataset (Story 5.1)
- [ ] Pattern matching algorithm optimized (O(n log n) or better)

---

## Strategic Recommendations

### 1. Add 1 Week to Epic 2 Timeline
**Rationale:** Epic 2 has 7 critical issues requiring 17.5 hours of fixes
**Impact:** Total project timeline: 12 → 13 weeks
**Benefit:** Prevents technical debt accumulation and late-stage refactoring

### 2. Enforce Strict Quality Gates for Epic 2 and Epic 5
**Rationale:** Both epics have CRITICAL reliability/security risks
**Implementation:**
- Epic 2: No merge to main until all 6 gate criteria pass
- Epic 5: Accuracy test MUST assert ≥80% (FR17 requirement)

### 3. Front-Load Performance Testing
**Rationale:** 2 of 6 performance NFRs have no tests (NFR2, NFR3)
**Implementation:**
- Add NFR2 test to Epic 2 Story 2.6 (mandatory)
- Add NFR3 test to Epic 4 Story 4.5 (mandatory)
- Document baselines for regression detection

### 4. Prioritize Security Fixes
**Rationale:** CSV injection is CRITICAL severity (RCE vulnerability)
**Implementation:**
- Epic 2 CSV injection: Fix before Story 2.6 completion
- Epic 7 CSV export: Reuse Epic 2 sanitization logic
- Epic 4 input validation: Add FluentValidation rules

### 5. Defer Optional Features if Timeline Slips
**Rationale:** Epic 7 (Reports) is optional, can be cut
**Fallback Plan:**
- Week 9-10: If Epic 5 exceeds time-box, cut Epic 7
- Users can use hledger CLI for reports (acceptable workaround)

---

## Success Metrics (Post-Implementation)

### Security Success Criteria
- [ ] Zero CRITICAL vulnerabilities (CodeQL passes)
- [ ] Zero HIGH vulnerabilities in CSV import flow
- [ ] 100% input validation coverage for user-facing forms

### Performance Success Criteria
- [ ] NFR1: Dashboard loads <2s (5,000 txns) ✅ Measured in Epic 3
- [ ] NFR2: CSV import <5s (1,000 txns) ✅ Measured in Epic 2
- [ ] NFR3: Search <1s (10,000 txns) ✅ Measured in Epic 4
- [ ] NFR4: Cold start <3s ✅ Measured in Epic 1
- [ ] NFR5: 60fps UI interactions ✅ Manual testing Week 11

### Reliability Success Criteria
- [ ] NFR8: 100% atomic writes + backups (auto-tested)
- [ ] NFR11: Zero data loss incidents (0 failed rollbacks in tests)
- [ ] NFR14: 100% hledger validation pass rate (auto-tested)
- [ ] FR17: ≥80% recurring detection accuracy (Epic 5 gate criteria)

### Maintainability Success Criteria
- [ ] 80%+ backend test coverage (xUnit)
- [ ] 70%+ frontend test coverage (Jest)
- [ ] All critical paths covered by E2E tests (Playwright)

---

## Assessment Artifacts

**Generated Files:**
1. [Epic 1 NFR Assessment](docs/qa/assessments/epic1-nfr-20251005.md) - 3 issues, 6h fixes
2. [Epic 2 NFR Assessment](docs/qa/assessments/epic2-nfr-20251005.md) - 7 issues, 17.5h fixes, **HIGHEST RISK**
3. [Epic 3 NFR Assessment](docs/qa/assessments/epic3-nfr-20251005.md) - 4 issues, 9h fixes
4. [Epic 4-7 NFR Assessment](docs/qa/assessments/epic4-7-nfr-20251005.md) - 12 issues, 24h fixes
5. [Project Risk Summary](docs/qa/assessments/project-nfr-risk-summary-20251005.md) - This file

**Total Assessment Time:** ~3 hours (pre-development risk analysis)
**Estimated Value:** 2-3 weeks saved (prevented late-stage refactoring)

---

## Next Steps

### Immediate Actions (Before Development Starts)

1. **Review with Team** (1 hour)
   - Present Epic 2 risk profile (highest risk)
   - Get buy-in for +1 week timeline adjustment
   - Prioritize CRITICAL fixes (CSV injection, accuracy assertion)

2. **Update Story Acceptance Criteria** (2 hours)
   - Epic 2 Story 2.6: Add CSV sanitization AC
   - Epic 2 Story 2.6: Add performance test AC
   - Epic 5 Story 5.2: Add accuracy assertion AC

3. **Create Quality Gate Checklist** (1 hour)
   - Epic 2 gate: 6 mandatory criteria
   - Epic 5 gate: 3 mandatory criteria (including 80% accuracy)

### During Development

4. **Weekly NFR Review** (30 min/week)
   - Check NFR test results (pass/fail)
   - Update risk heatmap
   - Escalate if any CRITICAL issue found

5. **Epic Completion Gates** (15 min/epic)
   - Verify all gate criteria met
   - Document actual vs. target performance
   - Update technical debt backlog

---

**Assessment Status:** ✅ COMPLETE
**Recommendation:** 🟡 PROCEED WITH CAUTION
**Focus Areas:** Epic 2 (CSV Import), Epic 5 (Predictions)

---

_This assessment was performed using the nfr-assess task workflow as part of the Ledgerly project's proactive quality strategy. All findings are based on story acceptance criteria analysis and architecture document review._

**Reviewer:** Quinn (QA Test Architect)
**Date:** 2025-10-05
**Next Review:** End of Epic 2 (Week 4)
