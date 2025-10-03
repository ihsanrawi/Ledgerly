# Project Brief: Ledgerly

## Executive Summary

**Ledgerly** is a modern personal finance manager that bridges the gap between powerful Plain Text Accounting (PTA) tools and intuitive consumer finance apps. Built on the hledger PTA software foundation, Ledgerly provides the precision of double-entry bookkeeping with a dashboard-first, user-friendly interface designed for individuals and families who want financial control without CLI complexity.

**Primary Problem:** Double-entry accounting software for personal finance is either prohibitively expensive (commercial tools) or has a steep learning curve requiring CLI expertise (Ledger, hledger, beancount). There's no accessible solution that combines PTA's power—data ownership, transparency, portability—with modern UX expectations.

**Target Market:** Technical individuals (developers, engineers), privacy-conscious users, and financial enthusiasts (15,000-200,000 globally) who value data ownership but are frustrated by CLI friction or unwilling to invest the time to master command-line PTA tools.

**Key Value Proposition:** "Git for Money" - version-controllable, transparent financial tracking with CSV-as-source architecture, adaptive prediction logic, and powerful reporting—all wrapped in an intuitive Angular interface that respects user intelligence without requiring terminal expertise.

**Architectural Approach:** Event-driven architecture using Wolverine for messaging, Vertical Slice Architecture for feature organization, and embedded hledger as the double-entry calculation engine. Plain text .hledger files are the source of truth, maintaining pure PTA philosophy while adding modern UX.

---

## Problem Statement

### Current State & Pain Points

**The Double-Entry Dilemma:**

Most individuals managing personal finances face an uncomfortable choice:

1. **Expensive Commercial Tools** (Quicken, Moneydance) - Provide double-entry precision but cost $50-150/year with vendor lock-in, proprietary formats, and limited customization
2. **CLI PTA Tools** (Ledger, hledger, beancount) - Offer complete control, data ownership, and transparency but demand significant technical expertise, CLI comfort, and patience for steep learning curves
3. **Mainstream Consumer Apps** (YNAB $99/year, dying Mint) - Ease of use comes at the cost of privacy invasion, data selling, cloud dependency, and inflexible workflows

**Specific Pain Points Identified:**

**For Current CLI PTA Users (15K-30K users globally):**
- Visualizations and reporting require additional tools or scripting
- Mobile access non-existent or cumbersome
- Reconciliation workflows are tedious
- Onboarding spouse/partner is nearly impossible
- Transaction categorization and import mapping require manual rule writing

**For PTA-Curious Technical Users (100K-200K potential users):**
- Aware of PTA benefits (data ownership, portability, version control) but perceive time investment as prohibitive
- Frustrated with current tool's vendor lock-in or subscription fatigue (YNAB, spreadsheets)
- Want modern UX (keyboard shortcuts, command palette, interactive charts) without sacrificing control
- Need gradual onboarding rather than "dedicate a weekend to learning CLI"

**For Privacy-First Finance Enthusiasts (50K-100K users):**
- Refuse to share bank credentials with aggregators (Plaid, Yodlee)
- Distrust cloud-based apps that sell data or have breach histories
- Seek local-first, self-hosted, or verifiably secure options
- Willing to trade some convenience for privacy and control

### Impact of the Problem

**Quantified Impact:**
- **Time waste:** Current CLI users spend 1-3 hours/month on manual reconciliation, categorization, and report generation that could be automated
- **Adoption barrier:** 90%+ of people interested in detailed finance tracking abandon PTA tools within first week due to complexity
- **Relationship friction:** Household finance management requires shared access; CLI tools make collaboration nearly impossible
- **Privacy risk:** Mainstream app users unknowingly expose financial data to third-party monetization and potential breaches

**Why Existing Solutions Fall Short:**

**CLI PTA Tools (Ledger/hledger/beancount):**
- No native visualization or interactive reporting
- Mobile workflows require brittle workarounds
- CSV import rules require manual scripting
- Steep documentation learning curve
- **Result:** Powerful but accessible only to dedicated technical users

**Fava (Beancount GUI):**
- Web-only, requires running local server
- Limited mobile support
- Beancount-format locked (no Ledger/hledger compatibility)
- Minimal import automation or prediction
- **Result:** Improves visualization but doesn't solve onboarding or workflow efficiency

**YNAB/Mainstream Apps:**
- Vendor lock-in with proprietary formats
- Cloud-dependent with privacy concerns
- Inflexible workflows (opinionated budgeting methodology)
- Expensive subscriptions ($99/year+)
- Limited power-user features or customization
- **Result:** Easy to start but constraining over time

### Urgency & Importance

**Why Now?**

1. **Privacy Awakening:** GDPR, CCPA, high-profile breaches driving demand for local-first, privacy-respecting tools
2. **Mint Shutdown (2024):** Intuit's closure of Mint displaced 20M users seeking alternatives—perfect moment to offer privacy-first option
3. **Developer Tool Mainstreaming:** Git → GitHub Desktop/GitKraken trajectory proves viable path from "CLI tool" to "accessible GUI"—timing is right for PTA to follow
4. **Open Banking Maturation:** PSD2 in EU, emerging US APIs enable reliable transaction import without screen-scraping aggregators
5. **Financial Independence Movement:** FIRE communities (2M+ r/financialindependence members) demand detailed tracking beyond simple budgeting apps

**Market Timing:**
- Plain Text Accounting community growing 15-20% annually
- Developer population expanding 5% annually with rising incomes ($100K+ median)
- Privacy-first software movement accelerating (Obsidian, Signal, ProtonMail success)
- Subscription fatigue creating demand for one-time purchase or self-hosted alternatives

---

## Proposed Solution

### Core Concept & Approach

**Ledgerly** is a **local-first, cross-platform personal finance manager** that uses hledger as its double-entry calculation engine. The application provides a dashboard-first, goal-oriented interface with adaptive prediction logic, removing CLI barriers while maintaining full PTA transparency and control.

**Paradigm Shift:**
- **Traditional PTA:** Ledger file is primary → manually edit transactions → generate reports from CLI
- **Ledgerly:** CSV imports and GUI → write to .hledger file → hledger calculates → interactive dashboards

**Architectural Philosophy:**
- **Local-first:** Data lives on user's device as plain text .hledger file
- **PTA-authentic:** Real hledger binary for calculations (battle-tested, 20+ years)
- **Event-driven:** Wolverine handles async workflows (import, categorization, analytics)
- **Vertical slices:** Features self-contained, easy to build and maintain
- **Progressive complexity:** Start simple → reveal power features as users grow
- **Escape hatches:** Full access to .hledger file, manual editing, CLI compatibility

### Key Differentiators from Existing Solutions

**vs. CLI PTA Tools (Ledger/hledger/beancount):**
- ✅ Interactive dashboards and visualizations out-of-box (no scripting required)
- ✅ Smart CSV import with learning mapper (auto-categorization improves over time)
- ✅ Command palette interface (keyboard-driven power without CLI complexity)
- ✅ Beginner-friendly onboarding with instant gratification (see insights on day one)
- ✅ **Maintains:** Data ownership, plain text format, version control compatibility, full transparency
- ✅ **Uses real hledger:** Not custom double-entry implementation

**vs. Fava (Beancount GUI):**
- ✅ Native desktop app (not web-only)
- ✅ hledger format (widely used, mature)
- ✅ CSV-as-source with predictive analytics
- ✅ Offline-first (no local server required)
- ✅ Event-driven async processing

**vs. YNAB/Mainstream Apps:**
- ✅ Data ownership (plain text files you control)
- ✅ No cloud requirement (local-first)
- ✅ Privacy-respecting (no data selling, no aggregator credentials)
- ✅ Flexible workflows (not opinionated envelope budgeting)
- ✅ **Maintains:** Ease of use for beginners, automated import

### High-Level Product Vision

**MVP (Months 1-4):**
- CSV import with intelligent categorization learning
- Interactive dashboard (net worth, expense breakdown, predictions)
- Transaction management (add, edit, categorize, reconcile)
- Category-based reporting with drill-down
- Basic predictive analytics (recurring detection, cash flow timeline)
- Desktop app (Angular + .NET + hledger, Windows/Mac/Linux)

**Phase 2 (Months 5-8):**
- Command palette interface for power users
- Mobile companion app (balance checks, quick transaction capture)
- Goal tracking aligned with predictions
- Enhanced categorization (ML-based)
- Budget creation and monitoring

**Phase 3 (Months 9-12):**
- Multi-user collaboration (family finance with permissions)
- Advanced reporting (custom reports, tax summaries)
- Investment tracking integration
- Receipt attachment and OCR

**Long-Term Vision (1-2 years):**
- Natural language query interface
- External integrations (bank APIs, investment feeds)
- Business/freelancer features
- Self-hosted version for privacy purists

---

## Target Users

### Primary User Segment: Frustrated CLI Power Users

**Profile:**
- **Demographics:** Software developers, data engineers, systems administrators; 25-45 years old; 80%+ male; $80K-$200K income
- **Current Behavior:** Already tracking finances with Ledger/hledger/beancount (1-5+ years); command-line comfortable; open-source advocates
- **Psychographics:** Value data ownership and transparency; willing to invest time for control; privacy-conscious; technically sophisticated

**Specific Needs:**
- Better visualization without scripting
- Easier CSV import and categorization
- Mobile balance checks without SSH
- Way to onboard spouse/partner
- Backup/sync respecting privacy

**Pain Points:**
- "I love plain text files but hate CLI for basic tasks"
- "My partner refuses to use terminal"
- "I spend 2 hours monthly categorizing"
- "Can't check balance from phone"

**Goals:**
- Maintain precise records with minimal time
- Share tracking with household
- Make data-driven decisions using visualizations
- Preserve data ownership and portability

### Secondary User Segment: PTA-Curious Technical Users

**Profile:**
- **Demographics:** Software developers, DevOps engineers, technical PMs; 25-40 years old; 65% male; $70K-$180K income
- **Current Behavior:** Using YNAB, spreadsheets, or basic apps; aware of PTA but haven't adopted CLI tools; interested in "quantified self"
- **Psychographics:** Value privacy and control but unwilling to dedicate weekend to CLI learning; prefer GUI with keyboard shortcuts

**Specific Needs:**
- PTA benefits without CLI complexity
- Gradual onboarding path
- Modern UX expectations met
- Git integration for versioning
- Privacy-respecting import

**Pain Points:**
- "Want PTA control but don't have time to learn CLI"
- "YNAB is $99/year and locks my data"
- "Tried hledger, gave up after 3 hours"
- "Hate sharing bank passwords with aggregators"

**Goals:**
- Migrate from YNAB/spreadsheet to privacy-respecting solution
- Track finances with precision without becoming hobbyist
- Version control financial data like code
- Avoid vendor lock-in

---

## Goals & Success Metrics

### Business Objectives

**Side Project Goals (Not-for-Profit Initially):**
- Build sustainable tool for PTA community
- Learn modern tech stack (Wolverine, VSA, Angular Signals, Tauri)
- Contribute to PTA ecosystem
- Create tool worthy of daily use

**If Commercialized (Future):**
- **Year 1:** 1,000-3,000 paying users, $60K-$250K revenue
- **Year 2:** 3,500-10,500 users, $210K-$840K revenue
- **Year 3-5:** 2-5% of TAM = $900K-$5.6M revenue

### User Success Metrics

**Onboarding:**
- Time to first insight: <30 minutes
- First week retention: 70%+
- Spouse/partner adoption: 25%+

**Engagement:**
- Weekly active usage: 60-80%
- Monthly reconciliation completion: 70%+
- Feature adoption: 40%+ use predictions

**Value:**
- Time saved: 50%+ reduction vs previous solution
- Financial awareness: 80%+ report better understanding
- Workflow satisfaction: NPS 40+

### Key Performance Indicators

**Product KPIs:**
- CSV import success rate: >95%
- Categorization accuracy: >80% after 3 months
- Dashboard load time: <2 seconds for 5,000 transactions
- Crash rate: <0.1%
- Data integrity: Zero data loss

**Growth KPIs (If Commercialized):**
- MAU growth: 10-20% month-over-month
- Trial downloads: 500-2,000/month by month 6
- Organic acquisition: >80%
- Community engagement: 200+ active forum members

---

## Technical Considerations

### Platform Requirements

**Target Platforms:**
- **Desktop (MVP):** Windows, macOS, Linux
  - Native application with embedded hledger binary
  - Offline-first (no internet required)
  - Local file system access
  - Bundle includes hledger (~10MB per platform)

- **Mobile (Phase 2):** iOS, Android
  - Companion app (read-heavy, quick capture)
  - Sync via file sync or custom solution

**Performance Requirements:**
- Dashboard load: <2 seconds for 5,000 transactions
- Event processing: 1,000 transactions in <3 seconds
- CSV import: 1,000 transactions in <5 seconds
- Search/filter: <1 second for 10,000 transactions
- App launch: <3 seconds cold start
- UI: 60fps interactions

---

### Technology Stack

**Frontend:**
- **Framework:** Angular 16+ (Signals, Standalone components)
- **State Management:** Angular Signals + RxJS
- **UI:** Angular Material
- **Charts:** Chart.js
- **Real-time:** SignalR

**Backend:**
- **Framework:** .NET 8 + ASP.NET Core
- **Messaging:** Wolverine (command/event handling, local queues)
- **Architecture:** Vertical Slice Architecture (VSA)
- **Double-Entry Engine:** hledger (embedded binary)
- **Database:** SQLite (caching, app state only)
- **API:** RESTful with minimal APIs

**Desktop:**
- **Wrapper:** Tauri (preferred) or Electron
- **Binaries:** Bundled hledger for all platforms

**Development:**
- **OS:** Linux (primary), macOS, Windows
- **IDE:** JetBrains Rider (primary), Cursor AI (assistant)
- **Source Control:** Git + GitHub
- **CI/CD:** GitHub Actions

---

### Architecture Overview

**Core Philosophy:**
1. **.hledger file is source of truth** (plain text, version-controllable)
2. **hledger binary for calculations** (battle-tested, 20+ years)
3. **Event-driven workflows** (Wolverine for async processing)
4. **Vertical slice organization** (features self-contained)
5. **SQLite for caching only** (not financial data)

**Architecture Pattern:**
```
Angular UI → ASP.NET Core API → Wolverine Commands/Queries
                                      ↓
                              Command/Query Handlers
                                      ↓
                    Append/Read .hledger file (plain text)
                                      ↓
                              hledger CLI (embedded)
                                      ↓
                         Parse output → Cache → UI
```

**Key Architectural Benefits:**
- ✅ PTA-authentic (real hledger, not custom)
- ✅ Battle-tested double-entry (20+ years development)
- ✅ Plain text files (version-controllable, portable)
- ✅ Event-driven async (better UX, cancellable operations)
- ✅ VSA organization (features isolated, easy maintenance)
- ✅ Simpler than full event sourcing (no PostgreSQL complexity)

---

### Repository Structure - Vertical Slice Architecture

**Monorepo organized by feature slices:**

```
Ledgerly.sln
│
├── src/
│   ├── Ledgerly.Api/                    # ASP.NET Core + Wolverine
│   │   ├── Features/                    # Vertical slices (CQRS)
│   │   │   ├── ImportCsv/
│   │   │   │   ├── ImportCsvCommand.cs
│   │   │   │   ├── ImportCsvHandler.cs
│   │   │   │   ├── ImportCsvValidator.cs
│   │   │   │   ├── CsvParser.cs
│   │   │   │   ├── ImportCsvEndpoint.cs
│   │   │   │   └── ImportCsvTests.cs
│   │   │   │
│   │   │   ├── CategorizeTransaction/
│   │   │   ├── GetDashboard/
│   │   │   ├── ReconcileAccount/
│   │   │   ├── DetectRecurring/
│   │   │   ├── GenerateLedgerFile/
│   │   │   └── GetTransactionHistory/
│   │   │
│   │   ├── Common/                      # Shared kernel
│   │   │   ├── Hledger/
│   │   │   │   ├── HledgerBinaryManager.cs
│   │   │   │   ├── HledgerProcessRunner.cs
│   │   │   │   ├── HledgerOutputParser.cs
│   │   │   │   └── TransactionFormatter.cs
│   │   │   ├── FileSystem/
│   │   │   ├── Caching/
│   │   │   └── Validation/
│   │   │
│   │   ├── Infrastructure/
│   │   └── Program.cs
│   │
│   ├── Ledgerly.Web/                    # Angular frontend
│   ├── Ledgerly.Desktop/                # Tauri wrapper
│   └── Ledgerly.Contracts/              # Shared DTOs
│
└── tests/
    └── Ledgerly.Api.Tests/
```

**VSA Benefits:**
- Feature cohesion (all code together)
- Easy navigation (no hunting across layers)
- Parallel development (features don't collide)
- Clear boundaries (shared kernel prevents coupling)
- Testing (co-located with features)
- CQRS natural fit (commands/queries explicit)

---

### Wolverine vs Akka.NET (For Your Experience)

| Akka.NET Concept | Wolverine Equivalent | Notes |
|------------------|----------------------|-------|
| Actor | Handler Class | Stateless, simpler |
| Tell | SendAsync() | Fire & forget |
| Ask | InvokeAsync<T>() | Request-reply |
| Supervision | Error Policies | Built-in retry |
| Hierarchy | Auto-discovery | Convention-based |
| Cluster | N/A (Local MVP) | Can add distributed later |
| Router | Local Queue | Work distribution |
| Scheduler | Scheduled Jobs | [Schedule] attribute |

**Key Differences:**
1. **Stateless by default:** Handlers don't maintain state like actors
2. **No location transparency:** Local-first, can add distributed later
3. **Convention over configuration:** Auto-discovery vs explicit Props
4. **Simpler routing:** Type-based, named queues

**For Ledgerly:** Wolverine is simpler and sufficient for local desktop app

---

### Development Environment (Linux + Rider + Cursor)

**Your Setup:**
- **OS:** Linux (Ubuntu 20.04+)
- **IDE:** JetBrains Rider (primary for .NET/C#)
- **AI Assistant:** Cursor IDE (rapid generation, exploration)
- **Platform Priority:** Linux first → macOS → Windows

**Quick Environment Setup:**

```bash
# 1. Install .NET 8
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0

# 2. Install Node.js 20
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt-get install -y nodejs

# 3. Download hledger
wget https://hledger.org/bin/hledger-linux-x64.zip
unzip hledger-linux-x64.zip
chmod +x hledger
./hledger --version

# 4. Create project
mkdir ~/Projects/Ledgerly && cd ~/Projects/Ledgerly
dotnet new sln -n Ledgerly
dotnet new webapi -n Ledgerly.Api -o src/Ledgerly.Api
dotnet sln add src/Ledgerly.Api/Ledgerly.Api.csproj

# 5. Add Wolverine
cd src/Ledgerly.Api
dotnet add package WolverineFx.Http
dotnet add package WolverineFx

# 6. Open in Rider
rider ~/Projects/Ledgerly/Ledgerly.sln
```

**Rider + Cursor Workflow:**
- **Rider:** Structure, debugging, testing, running
- **Cursor:** Quick generation, boilerplate, exploration
- **Pattern:** Build structure in Rider, accelerate with Cursor

**Useful Cursor Prompts:**
```
"Create Wolverine command handler for ImportCsv that appends to .hledger file"
"Write unit tests for CsvParser using xUnit and Moq"
"Parse hledger balance JSON output into C# objects"
"Generate minimal API endpoint for dashboard query"
```

---

### hledger Integration

**Binary Distribution:**
- Bundle hledger binaries for Windows, macOS, Linux
- Extract to app resources on launch
- Set executable permissions (Linux/macOS)
- SHA256 verification for integrity

**Process Execution:**
```csharp
// Common/Hledger/HledgerProcessRunner.cs
public async Task<string> RunAsync(string arguments, CancellationToken ct)
{
    var psi = new ProcessStartInfo
    {
        FileName = _hledgerBinaryPath,
        Arguments = $"-f \"{_ledgerFilePath}\" {arguments}",
        RedirectStandardOutput = true,
        UseShellExecute = false
    };
    
    using var process = Process.Start(psi);
    var output = await process.StandardOutput.ReadToEndAsync(ct);
    await process.WaitForExitAsync(ct);
    
    if (process.ExitCode != 0)
        throw new HledgerException("hledger failed");
    
    return output;
}
```

**Output Parsing:**
- Prefer JSON: `hledger bal -O json`
- Parse text tables when needed
- Cache results in SQLite
- Invalidate cache on .hledger file changes

**File Management:**
- Atomic writes (temp file → rename)
- Automatic backups (.hledger.bak)
- FileSystemWatcher for external edits
- Validation: `hledger check` after mutations

---

## MVP Scope (VSA Feature-Based)

### Six Core Vertical Slices

**Priority Order:**

**1. Import CSV (P0 - Foundation)**
- Parse CSV with column detection
- Generate hledger syntax
- Append to .hledger file
- Preview before commit
- Learn from corrections

**2. Get Dashboard (P0 - Core Value)**
- Net worth summary
- Expense breakdown by category
- Recent transactions
- Call hledger for calculations
- Cache results

**3. Manage Transactions (P0 - Essential)**
- Add transaction (CRUD)
- Edit transaction (file rewrite)
- Delete transaction (comment out)
- Transaction history query
- Search/filter

**4. Categorize Transactions (P1 - High Value)**
- Auto-suggest categories
- Learn from corrections
- Confidence scoring
- Batch categorization
- Pattern matching

**5. Category Reports (P1 - Core Value)**
- Expense by category
- Time period filtering
- Comparison views
- Drill-down to transactions
- Export (PDF, CSV)

**6. Detect Recurring (P2 - Delight)**
- Find recurring patterns
- Cash flow timeline
- Subscription detection
- Overdraft warnings
- Nightly background job

### Out of Scope for MVP

❌ Mobile app  
❌ Multi-user collaboration  
❌ Natural language query  
❌ Bank API integration  
❌ Investment tracking  
❌ Receipt OCR  
❌ Budgets and goals  
❌ Multi-currency  
❌ Command palette  
❌ Cloud sync  

---

## Development Timeline

### Month 1: Backend Foundation

**Week 1-2: Setup + First Slice**
- Environment setup (Rider, .NET, hledger)
- Wolverine configuration
- VSA folder structure
- Import CSV slice (backend complete)

**Week 3-4: Core Slices**
- Dashboard slice (hledger integration)
- Transaction management (CRUD)

### Month 2: Feature Completion

**Week 5-6: Automation**
- Categorize transactions
- Category learning

**Week 7-8: Reporting + Analytics**
- Category reports
- Recurring detection

### Month 3: Frontend

**Week 9-10: Angular Foundation**
- Angular + Signals setup
- Dashboard UI
- Charts with Chart.js

**Week 11-12: Feature UIs**
- CSV import UI
- Transaction management UI
- Category reports UI

### Month 4: Polish + Deploy

**Week 13: Desktop Packaging**
- Tauri wrapper
- Bundle hledger binaries
- Multi-platform testing

**Week 14-16: Testing + Docs**
- Comprehensive testing
- File watching
- Documentation
- Alpha release prep

**Total: 4 months comfortable pace**

---

## Constraints & Assumptions

### Constraints

**Budget:** $0 (bootstrap side project)

**Timeline:** Flexible (no deadline, learning-focused)
- 4 months realistic estimate
- Your advantage: 60% stack familiarity

**Resources:** Solo developer (senior engineer)
- Already knows: event-driven, CQRS, Angular, containers
- Learning: Wolverine (1 week), hledger integration (1-2 weeks), Angular Signals (3-5 days)

**Technical:**
- Cross-platform (Windows/macOS/Linux)
- Offline-first
- hledger GPL license (process boundary likely safe)
- VSA reduces complexity vs layers

### Key Assumptions

**Product:**
- CSV import sufficient for MVP
- hledger format acceptable to community
- Plain text aligns with user values
- Dashboard-first paradigm resonates

**Technical:**
- hledger performance adequate (<1s typical queries)
- Process spawning overhead acceptable (<100ms)
- Text/JSON parsing robust
- SQLite sufficient for caching
- VSA scales well for solo dev
- Wolverine simpler than Akka.NET for this case

**Architecture:**
- Vertical slices prevent entanglement
- Shared kernel sufficient for cross-cutting
- hledger eliminates double-entry complexity
- Feature-based testing simpler

---

## Key Risks

**1. hledger Integration**
- Binary distribution across platforms
- Output parsing brittleness
- Platform-specific bugs
- **Mitigation:** JSON output, SHA256 verification, multi-platform CI/CD

**2. File Concurrency**
- External edits during operation
- Conflicts, data loss
- **Mitigation:** FileSystemWatcher, atomic writes, automatic backups

**3. VSA Governance**
- Shared kernel bloat
- Feature duplication
- **Mitigation:** Clear boundaries, accept some duplication, Rule of Three

**4. Learning Curve**
- Multiple new technologies
- Cognitive overload risk
- **Mitigation:** 60% familiar, staged learning, no deadline pressure

**5. Performance**
- Large transaction sets (50K+)
- UI sluggishness
- **Mitigation:** hledger is fast, aggressive caching, background refresh

**6. GPL License**
- hledger GPL compatibility
- Licensing conflicts
- **Mitigation:** Process boundary (likely safe), consider open-sourcing

**7. Solo Developer**
- Burnout, bus factor
- Project abandonment
- **Mitigation:** Sustainable pace, open-source from start, modular VSA

**8. Community Reception**
- PTA purists reject GUI
- Low adoption
- **Mitigation:** Real hledger (authentic), manual editing supported, CLI compatible

---

## Success Criteria

**Feature Completeness:**
- ✅ All 6 core slices working
- ✅ No critical bugs or data loss

**Performance:**
- ✅ Dashboard <2s for 5,000 transactions
- ✅ CSV import <5s for 1,000 transactions
- ✅ hledger queries <1s typical
- ✅ 60fps UI

**Quality:**
- ✅ 90%+ CSV import success
- ✅ 80%+ categorization accuracy after learning
- ✅ <0.1% crash rate
- ✅ Zero data loss

**User Validation:**
- ✅ 5-10 alpha testers successful
- ✅ Positive feedback on usability
- ✅ No major missing features

---

## Next Steps

### Week 1: Environment + First Handler

```bash
# Day 1: Setup (2-3 hours)
mkdir ~/Projects/Ledgerly && cd ~/Projects/Ledgerly
dotnet new sln -n Ledgerly
dotnet new webapi -n Ledgerly.Api -o src/Ledgerly.Api
cd src/Ledgerly.Api
dotnet add package WolverineFx.Http

# Day 2: VSA Structure
mkdir -p Features/ImportCsv
mkdir -p Common/Hledger

# Day 3-4: First Handler
# Create ImportCsvCommand + Handler
# Test with curl

# Day 5-7: hledger Integration
# Download binary, test execution
# Build HledgerProcessRunner
# Test bal/reg commands
```

### Week 2-4: Core Slices

- Complete Import CSV (end-to-end)
- Build Dashboard (hledger queries)
- Implement Transaction CRUD

### Month 2-4: Follow Timeline

See detailed week-by-week plan in Development Timeline section

---

## Appendices

### A. Research Summary

This brief incorporates:
- Brainstorming sessions (2025-10-02)
- Market research (TAM: $30M-$180M)
- Competitive analysis (CLI tools, Fava, mainstream apps)
- Community insights (r/plaintextaccounting, PTA forums)

### B. Stakeholder Input

**Primary Stakeholder:** Ihsan (Solo Developer)
- Senior software engineer
- Experience: event-driven, CQRS, Akka.NET, Angular, PostgreSQL, containers
- Setup: Linux + Rider + Cursor
- Motivation: Learning project, contribute to PTA community
- Values: Data ownership, privacy, quality architecture

### C. References

**PTA Community:**
- r/plaintextaccounting
- hledger.org
- plaintextaccounting.org

**Technical:**
- Wolverine documentation
- Angular Signals guide
- Tauri documentation

**Project Documents:**
- brainstorming-session-results.md
- market-research.md
- architecture diagrams (to be created)

---

**Document Version:** 2.0 (VSA + Wolverine + hledger Architecture)  
**Created:** 2025-10-02  
**Updated:** 2025-10-03  
**Author:** Mary (Business Analyst) with Ihsan  
**Status:** Complete - Ready for Development  
**Next Milestone:** Environment setup, first vertical slice

*Built with Claude using BMad-Method analyst agent*
