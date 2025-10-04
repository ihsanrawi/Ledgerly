# Next Steps

## UX Expert Prompt

Review the [UI Design Goals](#user-interface-design-goals) section and create wireframes for the 7 core screens (Dashboard, CSV Import, Transaction List, Transaction Edit Modal, Cash Flow Timeline, Category Reports, Settings). Focus on the **Cash Flow Timeline widget** as the primary visual differentiator - this needs to be prominent on the dashboard and visually compelling. Use the developer tool aesthetic (VS Code, GitHub inspiration) rather than consumer finance app styling.

## Architect Prompt

Review the [Technical Assumptions](#technical-assumptions) section and design the system architecture for Ledgerly using **Vertical Slice Architecture + Wolverine + embedded hledger**. Key focus areas:

1. **Week 1 Validation:**
   - Execute Tauri PoC (hledger binary execution, .hledger file I/O, cross-platform builds)
   - Decide Tauri vs. Electron by end of Week 1 based on process spawning results
2. **VSA Structure:**
   - Design feature slice organization (`Features/ImportCsv/`, `Features/GetDashboard/`, etc.)
   - Define shared kernel boundaries (`Common/Hledger/`, `Common/FileSystem/`)
3. **Wolverine Integration:**
   - Design command/query handlers for each feature slice
   - Plan async processing (CSV import, recurring detection scheduled job)
4. **hledger Integration:**
   - Design HledgerProcessRunner for CLI invocation
   - Plan JSON output parsing (`hledger bal -O json`) + text parsing fallback
   - Design caching strategy (SQLite) with FileSystemWatcher invalidation
5. **Data Flow:**
   - UI → Wolverine Command → Write .hledger → hledger calc → Cache → UI
   - Atomic writes (temp → validate → rename) with .bak backups
6. **Performance Strategy:**
   - Plan for hledger <1s queries, dashboard <2s load, aggressive caching

Start with Epic 1 to validate: Wolverine message bus → hledger binary execution → .hledger file write → UI display
