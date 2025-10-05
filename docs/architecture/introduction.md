# Introduction

This document outlines the overall project architecture for Ledgerly, including backend systems, shared services, and non-UI specific concerns. Its primary goal is to serve as the guiding architectural blueprint for AI-driven development, ensuring consistency and adherence to chosen patterns and technologies.

**Relationship to Frontend Architecture:**
If the project includes a significant user interface, a separate Frontend Architecture Document will detail the frontend-specific design and MUST be used in conjunction with this document. Core technology stack choices documented herein (see "Tech Stack") are definitive for the entire project, including any frontend components.

## Starter Template or Existing Project

**Decision:** **No standard starter template** - This is a **greenfield project** with a highly specialized architecture.

**Analysis:**

Your PRD specifies a custom tech stack:
- **Backend:** .NET 8+ with Wolverine (event-driven messaging)
- **Frontend:** Angular 17+ with Signals
- **Desktop Wrapper:** Tauri 1.6+ (with Electron fallback)
- **Architecture:** Vertical Slice Architecture (VSA)

**Why No Template:**

1. **Wolverine + VSA pattern**: No standard template exists for this combination
2. **Embedded hledger binary**: Custom integration requirement
3. **Tauri wrapper**: Requires custom configuration with Angular + .NET backend
4. **Event-driven monolith**: Specific architectural pattern not in standard templates

**Recommendation:** Build from scratch using project scaffolding:
- `dotnet new webapi` for ASP.NET Core foundation
- `ng new` for Angular app
- `cargo create-tauri-app` for desktop wrapper
- Manual VSA structure setup (Features/ folders)

**Rationale:** The architectural uniqueness (VSA + Wolverine + embedded CLI tool + Tauri) makes a custom structure more appropriate than adapting a generic template. This gives you full control over the feature slice organization.

---
