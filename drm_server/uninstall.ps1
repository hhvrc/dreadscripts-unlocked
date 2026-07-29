#Requires -RunAsAdministrator
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$installDir = "C:\ProgramData\DreadScriptsDRM"
$exeDst     = Join-Path $installDir "drm_server.exe"

Write-Host ""
Write-Host "=== DreadScripts DRM Server — Uninstaller ===" -ForegroundColor Cyan
Write-Host ""

# ── 1. Stop and remove service ──────────────────────────────────────────────
if (Test-Path $exeDst) {
    Write-Host "[1/3] Removing Windows service ..." -ForegroundColor Yellow
    Push-Location $installDir
    try { & $exeDst uninstall-service } catch { Write-Host "      (not installed)" -ForegroundColor DarkGray }
    Pop-Location
    Write-Host "      OK" -ForegroundColor Green
} else {
    Write-Host "[1/3] Binary not found — skipping service removal" -ForegroundColor DarkGray
}

# ── 2. Remove hosts-file entry ──────────────────────────────────────────────
Write-Host "[2/3] Removing hosts-file redirect ..." -ForegroundColor Yellow
$hostsPath = "$env:SystemRoot\System32\drivers\etc\hosts"
$hostname  = "us-central1-dreadscripts-c6b62.cloudfunctions.net"
$content   = Get-Content $hostsPath -Raw
$cleaned   = ($content -split "`r?`n" | Where-Object { $_ -notmatch [regex]::Escape($hostname) }) -join "`r`n"
Set-Content $hostsPath $cleaned -Encoding ASCII -NoNewline
Write-Host "      OK" -ForegroundColor Green

# ── 3. Remove install directory ─────────────────────────────────────────────
Write-Host "[3/3] Deleting $installDir ..." -ForegroundColor Yellow
if (Test-Path $installDir) {
    Remove-Item -Recurse -Force $installDir
    Write-Host "      OK" -ForegroundColor Green
} else {
    Write-Host "      (not found, skipping)" -ForegroundColor DarkGray
}

Write-Host ""
Write-Host "=== Uninstall complete ===" -ForegroundColor Cyan
Write-Host "Note: the TLS cert remains in the Windows Root store."
Write-Host "To remove it: certmgr.msc → Trusted Root CAs → find 'DreadScripts DRM Server' → Delete"
Write-Host ""
