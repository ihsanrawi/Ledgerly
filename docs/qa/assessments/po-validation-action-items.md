# PO Validation Action Items
**Generated:** 2025-10-05
**Source:** PO Master Checklist Validation (92% Pass Rate)
**Status:** Ready for Implementation

---

## Critical Priority - Must Complete Before Development Start

### AI-1: Add OpenAPI Specification Generation to Epic 1
**Category:** Documentation & Handoff
**Epic:** Epic 1 - Foundation & Core Infrastructure
**Effort:** 0.5 days
**Assigned To:** Developer Agent

**Description:**
Add API documentation generation to ensure frontend-backend contract is explicitly documented and discoverable.

**Acceptance Criteria:**
- [ ] Install Swashbuckle.AspNetCore NuGet package (v6.5.0+)
- [ ] Configure Swagger in `Program.cs` with XML comments enabled
- [ ] Export OpenAPI spec to `docs/api/openapi.yaml`
- [ ] Swagger UI accessible at `/swagger` endpoint
- [ ] Validate spec with Spectral linter (no errors)
- [ ] Add to Story 1.1 acceptance criteria or create new Story 1.6

**Implementation Notes:**
```csharp
// Program.cs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ledgerly API", Version = "v1" });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// Enable in .csproj
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

**Verification:**
- OpenAPI spec exists at `docs/api/openapi.yaml`
- Swagger UI loads at `https://localhost:5000/swagger`
- All endpoints documented with request/response schemas

---

### AI-2: Clarify Epic 7 (Reports) MVP Commitment
**Category:** MVP Scope Alignment
**Epic:** N/A (Planning Decision)
**Effort:** 0 days (decision only)
**Assigned To:** Product Owner / Project Stakeholder

**Description:**
Epic 7 is marked as "Optional - P1" in epic-details.md and "cuttable buffer" in epic-list.md timeline. This ambiguity needs resolution before sprint planning.

**Decision Required:**
- [ ] **Option A:** Epic 7 IS in 12-week MVP scope
  - Timeline: Week 9-10 (0.5 weeks allocated)
  - Risk: May compress testing time if earlier epics slip
  - Rationale: Reports provide essential user value

- [ ] **Option B:** Epic 7 DEFERRED to Phase 2
  - Update `docs/prd/requirements.md` to move FR14, FR15, FR16 to "Phase 2 Requirements"
  - Update `docs/prd/epic-list.md` to remove Epic 7 from timeline
  - Rationale: Users can use hledger CLI for reports; focus on core differentiators

**Recommended Decision:** **Option B (Defer to Phase 2)**
- MVP already delivers core value (import, dashboard, predictions, transaction management)
- Reports are valuable but not differentiating (hledger CLI covers this)
- Provides 1.5-week buffer for Epic 5 (predictions) - the true differentiator
- Can be added post-MVP based on user feedback priority

**Action:**
- [ ] Document decision in `docs/prd/epic-list.md` change log
- [ ] Update Epic 7 header to `## Epic 7: Reporting & Data Export (Phase 2)` if deferred

---

### AI-3: Document Code Signing Certificate Acquisition Process
**Category:** External Dependencies
**Epic:** Epic 1 or Pre-Development Checklist
**Effort:** User task (2-3 hours)
**Assigned To:** User

**Description:**
Code signing certificates are required for Windows and macOS distribution but acquisition process is not documented in stories.

**User Tasks:**
- [ ] **Windows Code Signing Certificate**
  - Vendor: Sectigo, Certum, or DigiCert
  - Type: Authenticode Certificate (EV or Standard)
  - Cost: $75-$400/year
  - Timeline: 1-7 days (identity verification required)
  - Process: Purchase → Submit identity docs → Download .pfx file

- [ ] **macOS Code Signing Certificate**
  - Vendor: Apple Developer Program
  - Type: Developer ID Application Certificate
  - Cost: $99/year
  - Timeline: Immediate after enrollment
  - Process: Enroll in Apple Developer → Generate certificate in Xcode

- [ ] **Certificate Storage**
  - Store certificates securely (password manager, not in git)
  - Document certificate paths in `.env.local` (gitignored)
  - Add to CI/CD secrets for automated builds

**Timeline:**
Start Week 10 (before packaging in Week 11-12)

**Documentation Location:**
Add to `docs/prd/epic-details.md` as user responsibility or create `docs/setup/code-signing.md`

**Acceptance Criteria:**
- [ ] Windows certificate acquired and tested with `signtool.exe`
- [ ] macOS certificate acquired and imported to Keychain
- [ ] Tauri build configuration updated with certificate paths
- [ ] Test signed build on each platform (SmartScreen/Gatekeeper validation)

---

## High Priority - Should Complete for Quality

### AI-4: Document CSS/Styling Strategy
**Category:** UI/UX Considerations
**Epic:** Epic 1 - Foundation
**Effort:** 1 day
**Assigned To:** Developer Agent

**Description:**
Styling approach (Angular Material theming vs. custom CSS modules) is not explicitly documented, which may lead to inconsistent implementation.

**Tasks:**
- [ ] Create `docs/architecture/frontend-styling.md` OR add section to `coding-standards.md`
- [ ] Document CSS strategy decision:
  - Primary: Angular Material theming system
  - Custom styles: CSS Modules or global SCSS
  - Component-specific: Inline styles, styleUrls, or both
- [ ] Define color palette (primary, accent, warn colors for Material theme)
- [ ] Document responsive breakpoint strategy (desktop-first per PRD)
- [ ] Define naming conventions for custom CSS classes (e.g., BEM, utility-first)

**Recommended Strategy:**
```typescript
// Angular Material Theme Configuration
// src/Ledgerly.Web/src/styles/theme.scss
@use '@angular/material' as mat;

$ledgerly-primary: mat.define-palette(mat.$indigo-palette);
$ledgerly-accent: mat.define-palette(mat.$teal-palette);
$ledgerly-warn: mat.define-palette(mat.$red-palette);

$ledgerly-theme: mat.define-light-theme((
  color: (
    primary: $ledgerly-primary,
    accent: $ledgerly-accent,
    warn: $ledgerly-warn,
  ),
  typography: mat.define-typography-config(),
  density: 0,
));

@include mat.all-component-themes($ledgerly-theme);

// Custom utility classes for domain-specific styling
.transaction-income { color: mat.get-color-from-palette($ledgerly-primary, 600); }
.transaction-expense { color: mat.get-color-from-palette($ledgerly-warn, 600); }
```

**Acceptance Criteria:**
- [ ] Styling strategy documented with examples
- [ ] Material theme configuration file created
- [ ] Responsive breakpoints defined (desktop-first: 1200px, 768px, 480px)
- [ ] CSS naming convention established

---

### AI-5: Design First-Time User Onboarding Flow
**Category:** UI/UX Considerations
**Epic:** Epic 3 - Dashboard OR Epic 1 - Foundation
**Effort:** 0.5 days
**Assigned To:** Developer Agent

**Description:**
First-time user experience is not explicitly designed. Users may be confused on initial app launch with empty dashboard.

**Proposed Flow:**
1. **First Launch Detection**
   - Check SQLite for existing transactions count
   - If count = 0, show onboarding wizard

2. **Onboarding Steps**
   - **Step 1:** Welcome screen
     - "Welcome to Ledgerly - Your Privacy-First Finance Manager"
     - Brief value proposition (3 bullet points)
     - CTA: "Get Started"

   - **Step 2:** Import Your First CSV
     - Redirect to CSV import flow (Epic 2)
     - Helper text: "Import your bank statement to see your finances visualized"

   - **Step 3:** View Dashboard
     - After successful import, show dashboard with data
     - Highlight key widgets: Net Worth, Cash Flow Timeline, Recent Transactions
     - CTA: "Explore Your Dashboard"

3. **Dismissal & Re-Access**
   - "Don't show this again" checkbox
   - Settings → Help → "Show Onboarding Again"

**Implementation Options:**
- **Option A:** Extend Story 1.5 (Simple UI) to include empty state + onboarding trigger
- **Option B:** Add Story 3.7 "Build First-Time User Onboarding"
- **Option C:** Defer to Epic 11-12 (Polish phase)

**Recommended:** Option A (integrate into Story 1.5)

**Acceptance Criteria:**
- [ ] Empty dashboard shows onboarding wizard on first launch
- [ ] Wizard steps guide user through: Welcome → Import → Dashboard
- [ ] User can dismiss and re-access from Settings
- [ ] Onboarding state persisted in SQLite

---

### AI-6: Expand Self-Hosted Runner Documentation
**Category:** Documentation & Handoff
**Epic:** Infrastructure
**Effort:** 0.5 days
**Assigned To:** Developer Agent

**Description:**
Self-hosted runner setup is documented but lacks prerequisite software lists, which may block first-time setup.

**Tasks:**
- [ ] Expand `docs/architecture/infrastructure-and-deployment.md` section "Self-Hosted Runner Setup"
- [ ] Add prerequisite software table for each platform
- [ ] Document troubleshooting for common issues
- [ ] Add verification steps to confirm runner is working

**Required Prerequisites by Platform:**

**Linux (Ubuntu 20.04+):**
```bash
# System dependencies
sudo apt-get update
sudo apt-get install -y \
    build-essential \
    libssl-dev \
    libwebkit2gtk-4.0-dev \
    libgtk-3-dev \
    libayatana-appindicator3-dev \
    librsvg2-dev

# Rust toolchain
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh
source $HOME/.cargo/env

# .NET 8 SDK
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0

# Node.js 20 LTS
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt-get install -y nodejs
```

**macOS:**
```bash
# Xcode Command Line Tools
xcode-select --install

# Homebrew
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# Rust toolchain
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh

# .NET 8 SDK
brew install dotnet@8

# Node.js 20 LTS
brew install node@20
```

**Windows:**
```powershell
# Visual Studio 2022 Build Tools (required for Rust)
# Download from: https://visualstudio.microsoft.com/downloads/
# Select: Desktop development with C++

# Rust toolchain
# Download from: https://www.rust-lang.org/tools/install
# Run: rustup-init.exe

# .NET 8 SDK
winget install Microsoft.DotNet.SDK.8

# Node.js 20 LTS
winget install OpenJS.NodeJS.LTS
```

**Acceptance Criteria:**
- [ ] Prerequisite software documented for all 3 platforms
- [ ] Verification commands provided (e.g., `rustc --version`, `dotnet --version`)
- [ ] Common troubleshooting scenarios documented
- [ ] Estimated setup time: 30-45 minutes per platform

---

### AI-7: Add Performance Logging to Monitoring Strategy
**Category:** Post-MVP Considerations
**Epic:** Epic 3 - Dashboard
**Effort:** Integrated (no additional time)
**Assigned To:** Developer Agent

**Description:**
Story 3.6 validates initial performance but no ongoing performance monitoring exists for user self-diagnosis.

**Implementation:**
- [ ] Add Serilog performance logging to key operations:
  - hledger query execution times
  - Dashboard load times
  - CSV import processing times

- [ ] Log format:
  ```csharp
  Log.Information("Dashboard loaded in {ElapsedMs}ms for {TransactionCount} transactions",
      elapsed.TotalMilliseconds, transactionCount);
  ```

- [ ] User-accessible logs:
  - Location: `%APPDATA%/Ledgerly/logs/performance.log`
  - Retention: 7 days (configurable)
  - Privacy: No financial data, only metrics

**Acceptance Criteria:**
- [ ] Performance metrics logged for: dashboard load, hledger queries, CSV import
- [ ] Logs accessible from Settings → Advanced → View Logs
- [ ] Log rotation configured (7-day retention)
- [ ] No sensitive data (amounts, payees) in logs

**Integration Point:**
Add to Story 3.1, 3.2, 3.3 acceptance criteria (no separate story needed)

---

## Medium Priority - Consider for Improvement

### AI-8: Add Code Coverage Enforcement in CI
**Category:** Testing Infrastructure
**Epic:** Epic 1 - Foundation
**Effort:** 1 day
**Assigned To:** Developer Agent

**Description:**
Story 1.3 mentions "70%+ target" but coverage is not enforced in CI pipeline, which may lead to undertesting.

**Tasks:**
- [ ] Configure Coverlet for .NET code coverage
- [ ] Configure Jest coverage for Angular
- [ ] Add GitHub Actions workflow step to enforce coverage thresholds
- [ ] Generate coverage reports and upload to artifacts

**Implementation:**
```yaml
# .github/workflows/build.yml
- name: Run .NET tests with coverage
  run: |
    dotnet test --collect:"XPlat Code Coverage" \
      --results-directory ./coverage \
      /p:Threshold=70 \
      /p:ThresholdType=line

- name: Run Angular tests with coverage
  run: |
    cd src/Ledgerly.Web
    npm run test:coverage -- --coverage-threshold=70

- name: Upload coverage reports
  uses: actions/upload-artifact@v3
  with:
    name: coverage-reports
    path: coverage/
```

**Acceptance Criteria:**
- [ ] Coverage threshold set to 70% for both backend and frontend
- [ ] CI build fails if coverage drops below threshold
- [ ] Coverage reports generated as GitHub Actions artifacts
- [ ] Optionally: Integrate with Codecov or Coveralls for visualization

**Timeline:**
Add to Story 1.3 acceptance criteria or Week 2 infrastructure hardening

---

### AI-9: Add Accessibility Testing Automation
**Category:** UI/UX Considerations
**Epic:** Epic 11-12 (E2E Testing & Polish)
**Effort:** 1 day
**Assigned To:** Developer Agent

**Description:**
WCAG AA compliance is a requirement but no automated accessibility testing is mentioned.

**Tasks:**
- [ ] Install axe-core library for Playwright
- [ ] Add accessibility tests to E2E suite
- [ ] Validate WCAG AA compliance automatically
- [ ] Generate accessibility report

**Implementation:**
```typescript
// tests/e2e/accessibility.spec.ts
import { test, expect } from '@playwright/test';
import { injectAxe, checkA11y, getViolations } from 'axe-playwright';

test.describe('Accessibility - WCAG AA Compliance', () => {
  test('Dashboard page meets WCAG AA', async ({ page }) => {
    await page.goto('/');
    await injectAxe(page);

    const violations = await checkA11y(page, null, {
      detailedReport: true,
      detailedReportOptions: { html: true },
    });

    expect(violations.length).toBe(0);
  });

  test('Import CSV page meets WCAG AA', async ({ page }) => {
    await page.goto('/import');
    await injectAxe(page);
    const violations = await checkA11y(page);
    expect(violations.length).toBe(0);
  });
});
```

**Acceptance Criteria:**
- [ ] axe-core integrated into Playwright tests
- [ ] Accessibility tests run for key pages: Dashboard, Import, Transactions, Reports
- [ ] Tests fail if WCAG AA violations detected
- [ ] Accessibility report generated (HTML format)

**Timeline:**
Week 11-12 (E2E testing phase)

---

### AI-10: Document hledger Binary Update Process
**Category:** Documentation & Handoff
**Epic:** Maintenance Documentation
**Effort:** 0.5 days
**Assigned To:** Developer Agent

**Description:**
Story 1.2 embeds hledger 1.32.3 but no process documented for updating to future versions (e.g., 1.33, 1.34).

**Tasks:**
- [ ] Create `docs/maintenance/hledger-update-checklist.md`
- [ ] Document step-by-step update process
- [ ] Define regression testing requirements
- [ ] Document rollback procedure if new version breaks compatibility

**Update Checklist Template:**
```markdown
# hledger Binary Update Checklist

## Preparation
- [ ] Check hledger release notes for breaking changes
- [ ] Download new binaries for all platforms (Windows, macOS, Linux)
- [ ] Verify SHA256 checksums against official release

## Testing
- [ ] Update `Story 1.2` version reference
- [ ] Replace binaries in `src/Ledgerly.Api/Resources/Binaries/`
- [ ] Run integration tests: `dotnet test --filter "Category=HledgerIntegration"`
- [ ] Test on all platforms: Windows, macOS (Intel + ARM), Linux
- [ ] Validate with test .hledger files from Story 6.1 collection

## Regression Testing
- [ ] Test `hledger bal` output parsing
- [ ] Test `hledger reg` output parsing
- [ ] Test `hledger check` validation
- [ ] Test JSON output format (`-O json`)
- [ ] Performance benchmark: Compare query times vs. previous version

## Deployment
- [ ] Update tech stack documentation with new version
- [ ] Create git commit: "chore: Update hledger binary to vX.X.X"
- [ ] Tag release with updated binary version
- [ ] Monitor user bug reports for 2 weeks post-release

## Rollback Procedure
If new version causes issues:
- [ ] Revert binaries to previous version
- [ ] Update documentation to reflect rollback
- [ ] Create GitHub issue to track investigation
```

**Acceptance Criteria:**
- [ ] Update checklist document created
- [ ] Process tested with hledger 1.32.3 → 1.33 (when available)
- [ ] Linked from `docs/architecture/tech-stack.md`

---

## Summary

### Effort Breakdown

| Priority | Item | Effort | Status |
|----------|------|--------|--------|
| **CRITICAL** | AI-1: OpenAPI Spec | 0.5 days | 🔴 Blocking |
| **CRITICAL** | AI-2: Epic 7 Decision | 0 days | 🔴 Blocking |
| **CRITICAL** | AI-3: Code Signing Docs | 2-3 hours | 🟡 User Task |
| **HIGH** | AI-4: CSS Strategy | 1 day | 🟢 Quality |
| **HIGH** | AI-5: Onboarding Flow | 0.5 days | 🟢 Quality |
| **HIGH** | AI-6: Runner Docs | 0.5 days | 🟢 Quality |
| **HIGH** | AI-7: Performance Logging | Integrated | 🟢 Quality |
| **MEDIUM** | AI-8: Coverage Enforcement | 1 day | 🔵 Optional |
| **MEDIUM** | AI-9: A11y Testing | 1 day | 🔵 Optional |
| **MEDIUM** | AI-10: hledger Update Docs | 0.5 days | 🔵 Optional |

**Total Critical Path Impact:** 1 day (AI-1 only)
**Total Recommended Additions:** 3.5 days (AI-4, AI-5, AI-6, AI-7)
**Total Optional Improvements:** 3 days (AI-8, AI-9, AI-10)

### Next Steps

1. **Immediate Actions** (Before Epic 1 Start)
   - [ ] Complete AI-1: Add OpenAPI generation to Story 1.1 or 1.6
   - [ ] Complete AI-2: Make Epic 7 decision and update documentation
   - [ ] Complete AI-3: Document code signing acquisition process

2. **Epic 1 Integration** (Week 1)
   - [ ] Integrate AI-4, AI-5, AI-7 into existing stories
   - [ ] Update Story 1.1 AC to include styling strategy documentation
   - [ ] Update Story 1.5 AC to include onboarding flow

3. **Infrastructure Hardening** (Week 2)
   - [ ] Complete AI-6: Expand runner documentation
   - [ ] Optionally complete AI-8: Add coverage enforcement

4. **Polish Phase** (Week 11-12)
   - [ ] Optionally complete AI-9: Accessibility testing automation
   - [ ] Complete AI-10: hledger update documentation

---

**Validation Status:** ✅ **APPROVED FOR DEVELOPMENT**
**With Conditions:** Complete AI-1, AI-2, AI-3 before Epic 1 kickoff

**Generated by:** Sarah (Product Owner Agent)
**Assessment Date:** 2025-10-05
**Next Review:** Post-Epic 1 Completion (Week 1)
