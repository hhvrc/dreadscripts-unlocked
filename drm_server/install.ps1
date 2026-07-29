#Requires -RunAsAdministrator
<#
.SYNOPSIS
    One-command setup for the DreadScripts local DRM server.

.DESCRIPTION
    Copies drm_server.exe to C:\ProgramData\DreadScriptsDRM\, then:
      1. Adds the hosts-file redirect (DNS spoof)
      2. Installs the self-signed TLS cert into Windows Trusted Root
      3. Registers a Windows auto-start service
      4. Starts the service immediately

.EXAMPLE
    # Run from an elevated PowerShell prompt in the drm_server folder:
    .\install.ps1

    # Or run directly from anywhere:
    powershell -ExecutionPolicy Bypass -File .\install.ps1
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$installDir = "C:\ProgramData\DreadScriptsDRM"
$exeSrc     = Join-Path $PSScriptRoot "drm_server.exe"
$exeDst     = Join-Path $installDir  "drm_server.exe"

Write-Host ""
Write-Host "=== DreadScripts DRM Server — Installer ===" -ForegroundColor Cyan
Write-Host ""

# ── 1. Copy binary ──────────────────────────────────────────────────────────
Write-Host "[1/5] Copying binary to $installDir ..." -ForegroundColor Yellow
if (-not (Test-Path $exeSrc)) {
    Write-Error "drm_server.exe not found next to install.ps1. Build it first:  go build -o drm_server.exe ."
}
New-Item -ItemType Directory -Force -Path $installDir | Out-Null
Copy-Item -Force $exeSrc $exeDst
Write-Host "      OK: $exeDst" -ForegroundColor Green

# ── 2. Patch hosts file ─────────────────────────────────────────────────────
Write-Host "[2/5] Adding hosts-file redirect ..." -ForegroundColor Yellow
& $exeDst patch-hosts
Write-Host "      OK" -ForegroundColor Green

# ── 3. Install TLS cert ─────────────────────────────────────────────────────
Write-Host "[3/5] Installing self-signed TLS certificate ..." -ForegroundColor Yellow
# Run from installDir so cert PEM files are stored there (not PSScriptRoot)
Push-Location $installDir
& $exeDst install-cert
Pop-Location
Write-Host "      OK" -ForegroundColor Green

# ── 4. Install Windows service ──────────────────────────────────────────────
Write-Host "[4/5] Registering Windows service (auto-start) ..." -ForegroundColor Yellow
Push-Location $installDir
try {
    & $exeDst install-service
} catch {
    # Already installed — that's fine
    Write-Host "      (service already registered, skipping)" -ForegroundColor DarkGray
}
Pop-Location
Write-Host "      OK" -ForegroundColor Green

# ── 5. Start service ────────────────────────────────────────────────────────
Write-Host "[5/5] Starting service ..." -ForegroundColor Yellow
Push-Location $installDir
& $exeDst start-service
Pop-Location
Write-Host "      OK" -ForegroundColor Green

Write-Host ""
Write-Host "=== Installation complete! ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "The DRM server is now running and will start automatically on every boot."
Write-Host "Logs: Windows Event Viewer → Windows Logs → Application → source 'DreadScriptsDRM'"
Write-Host ""
Write-Host "To uninstall:"
Write-Host "  $exeDst uninstall-service" -ForegroundColor DarkGray
Write-Host "  # Then remove the hosts line and delete $installDir" -ForegroundColor DarkGray
Write-Host ""
