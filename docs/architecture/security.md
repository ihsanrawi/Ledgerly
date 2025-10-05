# Security

## Input Validation

**Validation Library:** FluentValidation 11.9.1

**Validation Location:** All command/query inputs validated at API boundary

**Required Rules:**
- All external inputs MUST be validated before processing
- Validation at API boundary (before Wolverine handler)
- Whitelist approach preferred over blacklist
- Reject invalid input with 400 Bad Request + detailed error messages

## Authentication & Authorization

**Auth Method:** None for MVP (desktop app, single-user, Tauri security boundary)

**Rationale:** MVP targets solo power-users (Persona 1). Multi-user read-only access for household members (Persona 3) deferred to Phase 2.

**Future (Phase 2 - Multi-User Support):**
- Local authentication (PIN/password) for household member read-only access
- Role-based authorization (Editor vs. Viewer roles)
- Read-only UI mode for non-technical users
- OAuth 2.0 / OpenID Connect for cloud sync services

**Required Patterns (MVP):**
- File system permissions check before .hledger access
- SQLite cache unencrypted (aligns with plaintext .hledger PTA philosophy)
- No network requests (except update check) - offline-first

## Secrets Management

**Development:** `.env` files (gitignored) with local configuration only

**Production:** Environment variables managed by Tauri

**Code Requirements:**
- NEVER hardcode secrets (API keys, passwords, connection strings)
- Access configuration via `IConfiguration` (.NET) or environment variables
- No secrets in logs or error messages
- No secrets in version control (`.gitignore` enforced)

**SQLCipher Encryption Key:**
- Generated on first app launch
- Stored in OS-specific secure storage:
  - Windows: DPAPI (Data Protection API)
  - macOS: Keychain
  - Linux: Secret Service API / libsecret

## API Security

**Rate Limiting:** Not applicable (local-only API)

**CORS Policy:** Not applicable (no cross-origin requests in Tauri)

**Security Headers:** Configured in Tauri CSP (Content Security Policy)
```json
"csp": "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:;"
```

**HTTPS Enforcement:** Not applicable (localhost HTTP for dev, no network in production)

## Data Protection

**Encryption at Rest:**
- **SQLite Cache:** Unencrypted in MVP (aligns with plaintext .hledger PTA philosophy)
- **.hledger Files:** Plain text (PTA requirement; user's choice to encrypt with git-crypt, VeraCrypt, BitLocker, etc.)
- **.hledger.bak Files:** Plain text backups (consistent with source files)

**Rationale:** PTA tools prioritize transparency and interoperability. .hledger files must be readable by CLI tools. Encrypting only SQLite cache (but not .hledger files) creates security inconsistency. Phase 2 may add full-disk encryption option (all files encrypted).

**Encryption in Transit:** Not applicable (no network transmission of financial data in MVP)

**PII Handling:**
- Payee names: User financial data - plaintext in SQLite cache and .hledger files
- Transaction memos: Treated as sensitive - plaintext (PTA requirement)
- No external transmission: All data stays local
- User responsibility: Filesystem encryption (OS-level) recommended for sensitive data

**Logging Restrictions:**
- Never log: Full file paths (use filename only)
- Never log: Transaction amounts or payee names in production
- Hash: Machine IDs before logging (SHA256)

## Dependency Security

**Scanning Tool:**
- **Backend:** `dotnet list package --vulnerable` + Dependabot
- **Frontend:** `npm audit` + Dependabot

**Update Policy:**
- Critical vulnerabilities: Patch within 24 hours
- High vulnerabilities: Patch within 1 week
- Medium/Low: Review and patch in next minor release

**Approval Process:**
- All new dependencies must be reviewed for:
  - Known vulnerabilities (CVE database)
  - License compatibility (MIT, Apache 2.0 preferred; GPLv3 acceptable for separate binaries)
  - Maintenance status (last commit <6 months)
  - Download count / community trust

**hledger GPL Licensing:**
- **hledger License:** GPLv3 (copyleft)
- **Distribution Model:** Separate subprocess (not linked or embedded in executable)
- **GPL Compliance:** Ledgerly spawns hledger via `ProcessStartInfo` (IPC boundary = GPL firewall)
- **Legal Analysis:** GPL applies to derivative works, not separate programs communicating via CLI/stdin/stdout
- **Precedent:** VS Code (MIT) bundles GPL extensions as separate processes
- **Bundling Strategy:** hledger binaries distributed in Tauri app resources folder for user convenience
- **Attribution:** LICENSE file includes hledger GPLv3 attribution and separates Ledgerly code (MIT/proprietary) from hledger binary licensing
- **Source Code:** hledger source available at hledger.org (GPL requirement satisfied)

## Security Testing

**SAST Tool:** GitHub CodeQL (free for public repos)

**DAST Tool:** Not applicable (no web endpoints exposed)

**Penetration Testing:** Not planned for MVP (desktop app, no network attack surface)

**Vulnerability Disclosure:**
- Email: security@ledgerly.com
- GPG key published for encrypted reports
- Acknowledgment within 48 hours
- Patch within 7 days for critical issues

## Security Best Practices

**Tauri Specific:**
- Allowlist minimal APIs: Only enable required Tauri APIs (fs, shell, dialog)
- CSP enforcement: Strict Content Security Policy in `tauri.conf.json`
- No eval(): Never use `eval()` or `Function()` constructor
- Sanitize IPC: Validate all messages between frontend and Rust backend

**.NET Specific:**
- Parameterized queries: EF Core prevents SQL injection (no raw SQL)
- CSRF protection: Not needed (no web forms, desktop app)
- XSS protection: Angular sanitizes templates automatically

**File System:**
- Path traversal: Validate all file paths before access
- Atomic writes: Use temp file → rename to prevent corruption
- Permissions: Check write permissions before attempting file operations

---
