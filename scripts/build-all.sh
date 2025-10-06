#!/bin/bash

# Build script for Ledgerly - Cross-platform Tauri builds
# Usage: ./scripts/build-all.sh [windows|macos|linux|all]

set -e  # Exit on error

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$PROJECT_ROOT"

echo "=== Ledgerly Build Script ==="
echo "Project root: $PROJECT_ROOT"
echo ""

# Function to build for specific target
build_target() {
    local target=$1
    local platform_name=$2

    echo ">>> Building for $platform_name (target: $target)..."

    # Build .NET API first
    echo "Building .NET API..."
    dotnet build src/Ledgerly.Api/Ledgerly.Api.csproj --configuration Release

    # Build Tauri app for target
    echo "Building Tauri app for $target..."
    cd src/Ledgerly.Desktop
    cargo tauri build --target "$target" || {
        echo "WARNING: Build failed for $target. This may be due to missing cross-compilation toolchain."
        echo "For cross-platform builds, you need the target toolchain installed:"
        echo "  rustup target add $target"
        return 1
    }
    cd "$PROJECT_ROOT"

    echo "✓ Build completed for $platform_name"
    echo ""
}

# Parse command line argument
BUILD_TARGET="${1:-current}"

case "$BUILD_TARGET" in
    windows)
        build_target "x86_64-pc-windows-msvc" "Windows x64"
        ;;
    macos)
        build_target "aarch64-apple-darwin" "macOS ARM64"
        ;;
    macos-intel)
        build_target "x86_64-apple-darwin" "macOS x64 (Intel)"
        ;;
    linux)
        build_target "x86_64-unknown-linux-gnu" "Linux x64"
        ;;
    current)
        echo "Building for current platform..."
        dotnet build src/Ledgerly.Api/Ledgerly.Api.csproj --configuration Release
        cd src/Ledgerly.Desktop
        cargo tauri build
        cd "$PROJECT_ROOT"
        echo "✓ Build completed for current platform"
        ;;
    all)
        echo "Building for all supported platforms..."
        echo "NOTE: This requires cross-compilation toolchains to be installed."
        echo ""

        build_target "x86_64-unknown-linux-gnu" "Linux x64" || true
        build_target "x86_64-pc-windows-msvc" "Windows x64" || true
        build_target "aarch64-apple-darwin" "macOS ARM64" || true
        build_target "x86_64-apple-darwin" "macOS x64 (Intel)" || true
        ;;
    *)
        echo "Usage: $0 [windows|macos|macos-intel|linux|current|all]"
        echo ""
        echo "  windows      - Build for Windows x64"
        echo "  macos        - Build for macOS ARM64 (Apple Silicon)"
        echo "  macos-intel  - Build for macOS x64 (Intel)"
        echo "  linux        - Build for Linux x64"
        echo "  current      - Build for current platform (default)"
        echo "  all          - Build for all platforms (requires cross-compilation)"
        exit 1
        ;;
esac

echo ""
echo "=== Build Summary ==="
echo "Build artifacts location:"
echo "  Tauri: src/Ledgerly.Desktop/src-tauri/target/release/"
echo "  .NET:  src/Ledgerly.Api/bin/Release/net8.0/"
echo ""
echo "Done!"
