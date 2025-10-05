# External APIs

Since Ledgerly is architected as a **local-first desktop application** with the hledger binary embedded, there are **no external APIs required for MVP functionality**. All accounting calculations, validations, and data processing occur locally through the hledger CLI.

**Future External API Considerations (Phase 2):**

If cloud sync or additional features are implemented in Phase 2, potential external APIs might include:
- **Cloud Storage APIs** (AWS S3, Azure Blob, Cloudflare R2) - For .hledger file backup/sync
- **Financial Institution APIs** (Plaid, Yodlee) - For direct bank transaction fetching (beyond CSV import)
- **Exchange Rate APIs** - For multi-currency support

**For MVP, this section is marked N/A.**

**Rationale:**
- **Offline-first by design**: NFR6 requires full functionality without internet
- **Data privacy**: No external transmission of financial data
- **hledger is embedded**: Not an external API—bundled binary
- **Simplifies architecture**: No API keys, rate limits, network error handling

---
