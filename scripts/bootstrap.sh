#!/bin/bash
# Bootstrap development environment for Ledgerly

set -e

echo "=== Ledgerly Development Environment Setup ==="
echo ""

# Check prerequisites
echo "Checking prerequisites..."

# Check .NET
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    echo "✓ .NET SDK: $DOTNET_VERSION"
else
    echo "✗ .NET SDK not found. Please install .NET 8 SDK from https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi

# Check Node.js
if command -v node &> /dev/null; then
    NODE_VERSION=$(node --version)
    echo "✓ Node.js: $NODE_VERSION"
else
    echo "✗ Node.js not found. Please install Node.js 20 LTS from https://nodejs.org/"
    exit 1
fi

# Check npm
if command -v npm &> /dev/null; then
    NPM_VERSION=$(npm --version)
    echo "✓ npm: $NPM_VERSION"
else
    echo "✗ npm not found"
    exit 1
fi

# Check Rust (optional for Tauri)
if command -v cargo &> /dev/null; then
    RUST_VERSION=$(rustc --version | cut -d' ' -f2)
    echo "✓ Rust: $RUST_VERSION"
    TAURI_AVAILABLE=true
else
    echo "⚠ Rust not found. Desktop app (Tauri) will not be available."
    echo "  Install from: https://rustup.rs/"
    TAURI_AVAILABLE=false
fi

echo ""
echo "Installing backend dependencies..."
dotnet restore src/Ledgerly.Api/Ledgerly.Api.csproj
dotnet restore src/Ledgerly.Contracts/Ledgerly.Contracts.csproj

echo ""
echo "Installing frontend dependencies..."
npm install --prefix src/Ledgerly.Web

echo ""
echo "Installing global tools..."
if ! command -v dotnet-ef &> /dev/null; then
    dotnet tool install --global dotnet-ef --version 8.0.4
    echo "✓ dotnet-ef installed"
else
    echo "✓ dotnet-ef already installed"
fi

if [ "$TAURI_AVAILABLE" = true ]; then
    echo ""
    echo "Setting up Tauri..."
    cd src/Ledgerly.Desktop
    if [ ! -f "src-tauri/Cargo.toml" ]; then
        echo "Initializing Tauri project..."
        cargo tauri init --app-name Ledgerly --window-title Ledgerly --dist-dir ../../Ledgerly.Web/dist/ledgerly.web --dev-path http://localhost:4200
    else
        echo "✓ Tauri already initialized"
    fi
    cd ../..
fi

echo ""
echo "=== Setup Complete ==="
echo ""
echo "Next steps:"
echo "  1. Start backend:  dotnet run --project src/Ledgerly.Api"
echo "  2. Start frontend: cd src/Ledgerly.Web && npm start"
if [ "$TAURI_AVAILABLE" = true ]; then
    echo "  3. Start desktop:  cd src/Ledgerly.Desktop && cargo tauri dev"
fi
echo ""
echo "API Swagger UI: http://localhost:5000/swagger"
echo "Frontend:       http://localhost:4200"
