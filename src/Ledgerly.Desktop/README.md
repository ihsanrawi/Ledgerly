# Ledgerly Desktop - Tauri Wrapper

## Prerequisites

This component requires Rust to be installed.

### Install Rust

```bash
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh
```

After installation, restart your terminal and run:

```bash
rustup update
```

### Install Tauri CLI

```bash
cargo install tauri-cli --version "^1.6"
```

## Development

Once Rust is installed, initialize the Tauri project:

```bash
cd src/Ledgerly.Desktop
cargo tauri init
```

Configuration will point to the built Angular app at `../Ledgerly.Web/dist/ledgerly.web`.

## Build

```bash
cargo tauri build
```

## Note

Tauri integration will be completed once Rust is installed. A placeholder structure has been created for now.
