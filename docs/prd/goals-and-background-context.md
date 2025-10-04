# Goals and Background Context

## Goals

- Deliver a **local-first, dashboard-driven personal finance manager** that makes Plain Text Accounting accessible to technical users without CLI expertise
- Enable **CSV-based transaction imports** with intelligent categorization that learns from user corrections over time
- Provide **interactive visualizations and predictive analytics** that reveal spending patterns and cash flow forecasts
- Maintain **full PTA transparency** through auto-generated Ledger files that preserve data ownership, portability, and version control compatibility
- Support **household finance management** by enabling non-technical users (spouses/partners) to access insights without terminal skills
- Reduce **monthly reconciliation time by 50%+** compared to CLI-only PTA workflows
- Achieve **1,000-3,000 paying users** within 12 months, validating product-market fit

## Background Context

The Plain Text Accounting ecosystem (Ledger, hledger, beancount) provides powerful double-entry bookkeeping with complete data ownership and transparency. However, these CLI-only tools present steep learning curves, requiring significant time investment and terminal expertise. Current GUI solutions like Fava are web-only, format-specific, and lack modern mobile access or automation features.

Meanwhile, mainstream finance apps (YNAB, Mint) offer ease-of-use at the cost of vendor lock-in, privacy invasion, and subscription costs ($99/year+). With Mint's 2024 shutdown displacing 20M users and growing privacy concerns (GDPR, CCPA, data breaches), there's a clear market opportunity for a privacy-respecting, locally-owned alternative.

**Ledgerly** builds on the battle-tested hledger foundation: **plain text .hledger files remain the source of truth**, with embedded hledger binary handling all double-entry calculations. This PTA-authentic approach—combined with dashboard-first UI, CSV import automation, adaptive prediction logic, and progressive complexity disclosure—bridges the gap between powerful CLI tools and intuitive consumer apps. The solution targets frustrated CLI power users (15K-30K globally), PTA-curious technical users (100K-200K), and privacy-first enthusiasts (50K-100K) who value data control but lack time for CLI mastery.

## Change Log

| Date       | Version | Description             | Author |
|------------|---------|-------------------------|--------|
| 2025-10-02 | 1.0     | Initial PRD creation    | John   |
| 2025-10-03 | 2.0     | Architecture revision: hledger + Wolverine + VSA | John   |
