# Infrastructure and Deployment

## Infrastructure as Code

**Tool:** Not applicable for desktop application - no cloud infrastructure in MVP

**Location:** `src/Ledgerly.Desktop/src-tauri/tauri.conf.json`

**Approach:** Configuration-as-code for desktop application packaging

## Deployment Strategy

**Strategy:** GitHub Releases with automated cross-platform builds

**CI/CD Platform:** GitHub Actions with **self-hosted runners** (FREE for private repos)

**Runner Configuration:**
- **Windows:** Self-hosted runner on development Windows PC
- **macOS:** Self-hosted runner on development Mac (or refurbished Mac Mini)
- **Linux:** Self-hosted runner on Linux VM or spare hardware

**Self-Hosted Runner Setup:**

```bash
# On each build machine (Windows/macOS/Linux):
# Navigate to GitHub repo → Settings → Actions → Runners → New self-hosted runner

# Linux setup:
./config.sh --url https://github.com/yourorg/ledgerly --token YOUR_TOKEN --labels linux
./run.sh

# Auto-start on boot:
sudo ./svc.sh install
sudo ./svc.sh start

# Windows setup (PowerShell as Administrator):
.\config.cmd --url https://github.com/yourorg/ledgerly --token YOUR_TOKEN --labels windows
.\svc.cmd install
.\svc.cmd start

# macOS setup:
./config.sh --url https://github.com/yourorg/ledgerly --token YOUR_TOKEN --labels macos-arm64
sudo ./svc.sh install
sudo ./svc.sh start
```

**Pipeline Configuration:** `.github/workflows/release.yml`

```yaml
jobs:
  build-tauri:
    runs-on: [self-hosted, ${{ matrix.platform.runner-label }}]
    strategy:
      matrix:
        platform:
          - runner-label: linux
            target: x86_64-unknown-linux-gnu
            artifact: '*.AppImage'
          - runner-label: windows
            target: x86_64-pc-windows-msvc
            artifact: '*.msi'
          - runner-label: macos-arm64
            target: aarch64-apple-darwin
            artifact: '*.dmg'
    # ... rest of build steps
```

**Build Artifacts:**
- Windows: `ledgerly-1.0.0-x64.msi`
- macOS (Apple Silicon): `ledgerly-1.0.0-arm64.dmg`
- macOS (Intel): `ledgerly-1.0.0-x64.dmg`
- Linux: `ledgerly-1.0.0-x86_64.AppImage` + `.deb`

**Estimated Build Time:** 30-40 minutes (faster on self-hosted hardware vs. cloud runners)

## Environments

- **Development:** Local dev with hot reload (`http://localhost:5000` + `http://localhost:4200`)
- **Staging:** Tauri dev mode (`npm run tauri dev`)
- **Production:** Installed desktop app (embedded .NET + bundled Angular)
- **Testing:** Automated CI/CD with temporary databases

## Environment Promotion Flow

```
Developer Workstation (Dev)
  ↓ (git push to main)
Automated Tests (CI)
  ↓ (all tests pass)
Build Artifacts (Self-Hosted Runners)
  ↓ (create git tag v1.0.0)
Release Build (All Platforms)
  ↓ (manual approval)
GitHub Release Published
  ↓ (auto-updater)
Users Install Update
```

## Rollback Strategy

**Primary Method:** GitHub Releases version rollback

**Trigger Conditions:**
- Critical bug (data corruption, crashes)
- Performance regression >50%
- >10 user reports in 24 hours

**Process:**
1. Identify last stable version (e.g., v1.0.2)
2. Mark current release (v1.0.3) as "Pre-release"
3. Update Tauri updater endpoint to serve v1.0.2
4. Notify users via in-app notification

**Recovery Time Objective (RTO):** <2 hours

**Limitations:**
- Desktop apps cannot force-downgrade (user acceptance required)
- Database schema migrations are **irreversible**
- Mitigation: Schema versioning + backward compatibility

## Infrastructure Costs

**MVP (Phase 1):**
- **GitHub Actions:** $0 (self-hosted runners)
- **GitHub Releases:** Free
- **Self-Hosted Runner Hardware:** $0-$300 one-time (optional refurbished Mac Mini)
- **Electricity:** ~$5/month
- **Code Signing Certificates:**
  - Windows (Authenticode): $75-$400/year
  - macOS (Apple Developer): $99/year
  - Linux: Free
- **Domain:** $12/year (optional)

**Total MVP Cost:** ~$200-$500/year

**Phase 2 (Cloud Sync):**
- AWS S3: ~$0.23/month (100 users × 10MB)
- Cloudflare CDN: Free tier
- AWS Lambda: Free tier

**Total Phase 2 Cost:** +$50-$100/year

## Monitoring and Observability

**Desktop Application Approach:**
- **No centralized logging** (privacy-first)
- **No telemetry** in MVP (opt-in only)
- **Local logging:** Serilog file sink (`%APPDATA%/Ledgerly/logs/`)
- **Log retention:** Last 7 days
- **User bug reports:** In-app "Report Bug" exports sanitized logs (manual GitHub Issue upload)

## Key Deployment Decisions

| Decision | Rationale | Trade-off |
|----------|-----------|-----------|
| **Self-hosted GitHub Actions runners** | Zero CI cost for private repo, same workflow syntax | Requires maintaining 3 build machines |
| **GitHub Releases distribution** | No CDN cost, version history, auto-updater integration | Slower downloads vs. CDN (acceptable) |
| **Tauri built-in updater** | Signed updates, delta downloads | Public key distribution complexity |
| **No cloud infrastructure** | Privacy-first, zero recurring costs | No centralized analytics |
| **Code signing required** | Windows/macOS trust | Annual certificate cost |
| **SQLCipher encryption** | Protects data at rest | <5% performance overhead |

---
