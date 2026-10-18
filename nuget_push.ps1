<#
.SYNOPSIS
    Signs, packs, signs and pushes the MeaMod.Utilities NuGet packages.

.DESCRIPTION
    PowerShell replacement for the old nuget_push.bat. Code signing goes through the
    Signotaur signing server instead of a certificate in the local store with signtool
    and nuget sign.

    Stages:
      1. Pre-flight: $env:signkey and $env:signserver (Signotaur API key and server URL),
         SignotaurTool.exe, dotnet, project files.
      2. dotnet build of the solution (Release) unless -SkipBuild.
      3. Sign every MeaMod.*.dll under the three library projects' bin/{Configuration}
         folders (all target frameworks) with Signotaur. Files that already carry a
         signature are skipped, which replaces the old signtool verify check.
      4. dotnet pack --no-build the three library projects.
      5. Sign the .nupkg files with Signotaur (replaces nuget sign).
      6. dotnet nuget push each package to nuget.org with --skip-duplicate, unless -NoPush.
      7. Move the pushed packages into nugetpackagearchive/.

    The old nuget.exe push used the API key stored with "nuget setapikey". dotnet nuget
    push reads that same NuGet.Config entry when no key is given, so existing setups keep
    working; -ApiKey or $env:NUGET_API_KEY override it.

.PARAMETER Configuration
    Build configuration. Default: Release.

.PARAMETER SkipBuild
    Sign and pack whatever is already in bin/{Configuration} instead of building first.

.PARAMETER NoPush
    Sign and pack only. Packages are left in bin/{Configuration} and not archived.

.PARAMETER ApiKey
    nuget.org API key. Defaults to $env:NUGET_API_KEY; when neither is set the key stored
    in NuGet.Config is used.

.PARAMETER DryRun
    Print what would happen without building, signing, packing, pushing or moving anything.

.EXAMPLE
    $env:signkey = "<signotaur api key>"
    $env:signserver = "https://<signotaur host>:443"
    pwsh ./nuget_push.ps1

.EXAMPLE
    pwsh ./nuget_push.ps1 -DryRun
#>

#Requires -Version 7.0

[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [switch]$SkipBuild,
    [switch]$NoPush,
    [string]$ApiKey = $env:NUGET_API_KEY,
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# --- Signotaur settings --------------------------------------------------------------
# The signing key and the server address both come from the environment so that neither
# is published with this script: $env:signkey and $env:signserver (e.g. https://host:443).
$signKey             = $Env:signkey
$signServer          = $Env:signserver
# "CN=James Weston" issued by Sectigo Public Code Signing CA R36, held on the Signotaur
# server. nuget.org only accepts package signatures from a certificate registered on the
# publishing account, so register a replacement certificate there before changing this.
$signCert            = "221209138096417926222BAF6374AF291F612247"
$signDesc            = "MeaMod.Utilities"
$signDescUrl         = "https://www.meamod.com"
$signTimestamp       = "http://timestamp.digicert.com"
$signTimestampBackup = "http://timestamp.sectigo.com"
$signDigest          = "SHA256"

# --- Paths ---------------------------------------------------------------------------
$repoRoot    = $PSScriptRoot
$solution    = Join-Path $repoRoot "MeaMod.Utilities.sln"
$archiveDir  = Join-Path $repoRoot "nugetpackagearchive"
$nugetSource = "https://api.nuget.org/v3/index.json"

# Packable projects, in the order they are packed and pushed.
$projects = @(
    [PSCustomObject]@{ Id = "MeaMod.Utilities";             Dir = "MeaMod.Utilities" }
    [PSCustomObject]@{ Id = "MeaMod.Utilities.Security";    Dir = "MeaMod.Utilities.Security" }
    [PSCustomObject]@{ Id = "MeaMod.Utilities.UI.WinForms"; Dir = "MeaMod.Utilities.UI.WinForms" }
)
foreach ($p in $projects) {
    $p | Add-Member Csproj (Join-Path $repoRoot (Join-Path $p.Dir "$($p.Id).csproj"))
    $p | Add-Member BinDir (Join-Path $repoRoot (Join-Path $p.Dir "bin/$Configuration"))
}

# --- Helpers -------------------------------------------------------------------------
function Section($msg) { Write-Host "==> $msg" -ForegroundColor Cyan }
function Step($msg)    { Write-Host "  - $msg" -ForegroundColor DarkGray }
function Ok($msg)      { Write-Host "    $msg" -ForegroundColor Green }
function Fail($msg)    { Write-Error $msg; exit 1 }

function Invoke-Tool([string]$context, [string]$exe, [string[]]$arguments) {
    if ($DryRun) {
        Step "[dry-run] $exe $($arguments -join ' ')"
        return
    }
    & $exe @arguments
    if ($LASTEXITCODE -ne 0) {
        Fail "$context failed (exit $LASTEXITCODE)"
    }
}

function Get-ProjectTargetFrameworks([string]$csproj) {
    # bin/{Configuration} accumulates folders for frameworks that were targeted in the
    # past; only the ones the project targets today are built, packed and worth signing.
    $tfms = (& dotnet msbuild $csproj -getProperty:TargetFrameworks --nologo 2>$null | Out-String).Trim()
    if ([string]::IsNullOrWhiteSpace($tfms)) {
        $tfms = (& dotnet msbuild $csproj -getProperty:TargetFramework --nologo 2>$null | Out-String).Trim()
    }
    if ([string]::IsNullOrWhiteSpace($tfms)) {
        Fail "Could not read the target frameworks of $csproj"
    }
    return @($tfms -split ';' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
}

function Invoke-Signotaur($files, $context) {
    # A single-element pipeline yields a scalar string (splatting a string passes it one
    # character per argument) and an empty one yields a lone null. Force a clean array.
    $files = @($files | Where-Object { -not [string]::IsNullOrEmpty($_) })
    if ($files.Count -eq 0) {
        Step "No files to sign for $context - skipping."
        return
    }
    $missing = @($files | Where-Object { -not (Test-Path $_ -PathType Leaf) })
    if ($missing.Count -gt 0) {
        Fail "Invoke-Signotaur ($context): $($missing.Count) path(s) do not exist, e.g. '$($missing[0])'"
    }
    Step "Signing $($files.Count) file(s) with Signotaur ($context)..."
    if ($DryRun) {
        $files | ForEach-Object { Step "[dry-run]   $_" }
        return
    }
    & SignotaurTool.exe sign `
        -a $signKey -s $signServer -t $signCert `
        -d "$signDesc" --du $signDescUrl `
        --tr $signTimestamp --fb $signTimestampBackup `
        --td $signDigest --fd $signDigest `
        --ph --vf --vc --skip-signed `
        @files `
        --verify-cert-chain
    if ($LASTEXITCODE -ne 0) {
        Fail "Signotaur failed for $context (exit $LASTEXITCODE)"
    }
    Ok "Signed: $context"
}

# --- Pre-flight ----------------------------------------------------------------------
Section "Pre-flight"

if (-not $DryRun) {
    if ([string]::IsNullOrWhiteSpace($signKey)) {
        Fail '$env:signkey must be set before running this script.'
    }
    Ok "signkey present"

    if ([string]::IsNullOrWhiteSpace($signServer)) {
        Fail '$env:signserver (the Signotaur server URL) must be set before running this script.'
    }
    Ok "signserver present"

    if (-not (Get-Command SignotaurTool.exe -ErrorAction SilentlyContinue)) {
        Fail "SignotaurTool.exe not found on PATH."
    }
    Ok "SignotaurTool.exe on PATH"
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Fail "dotnet not found on PATH."
}
if (-not (Test-Path $solution)) {
    Fail "Solution file not found: $solution"
}
foreach ($p in $projects) {
    if (-not (Test-Path $p.Csproj)) {
        Fail "Project file not found: $($p.Csproj)"
    }
}
Ok "dotnet, solution and $($projects.Count) project files found"

if (-not $NoPush -and [string]::IsNullOrWhiteSpace($ApiKey)) {
    Step "No API key given: dotnet nuget push will use the key stored in NuGet.Config."
}

# --- Build ---------------------------------------------------------------------------
if ($SkipBuild) {
    Section "Build skipped"
    Step "Signing and packing the existing bin/$Configuration output."
} else {
    Section "Build solution ($Configuration)"
    Invoke-Tool "dotnet build" "dotnet" @("build", $solution, "-c", $Configuration, "--nologo")
    Ok "Build complete"
}

# --- Sign assemblies -----------------------------------------------------------------
# Only our own assemblies, and only for the frameworks each project targets today:
# dependencies copied next to them are already signed by their publishers, and only
# MeaMod.*.dll for the current frameworks ends up inside the packages.
Section "Sign assemblies"

$assemblies = @(foreach ($p in $projects) {
    $tfms = Get-ProjectTargetFrameworks $p.Csproj
    Step "$($p.Id): $($tfms -join ', ')"
    foreach ($tfm in $tfms) {
        $tfmDir = Join-Path $p.BinDir $tfm
        if (Test-Path $tfmDir) {
            Get-ChildItem -Path $tfmDir -File -Filter "MeaMod.*.dll"
        } elseif (-not $DryRun) {
            Fail "Expected build output folder missing: $tfmDir"
        }
    }
}) | Sort-Object FullName -Unique

if ($assemblies.Count -eq 0 -and -not $DryRun) {
    Fail "No MeaMod.*.dll found under bin/$Configuration. Build first, or drop -SkipBuild."
}
Step "$($assemblies.Count) assemblies to sign"
Invoke-Signotaur ($assemblies | ForEach-Object { $_.FullName }) "MeaMod assemblies"

# --- Pack ----------------------------------------------------------------------------
Section "Pack ($Configuration, no build)"

foreach ($p in $projects) {
    # A package left behind by an aborted run must not be re-signed and pushed as if it
    # were this run's output.
    Get-ChildItem -Path $p.BinDir -Filter "*.nupkg" -File -ErrorAction SilentlyContinue | ForEach-Object {
        Step "Removing stale $($_.Name)"
        if (-not $DryRun) { Remove-Item -LiteralPath $_.FullName -Force }
    }
    Invoke-Tool "dotnet pack ($($p.Id))" "dotnet" @("pack", $p.Csproj, "-c", $Configuration, "--no-build", "--nologo")
}

$packages = @(foreach ($p in $projects) {
    Get-ChildItem -Path $p.BinDir -Filter "*.nupkg" -File -ErrorAction SilentlyContinue
})
if ($packages.Count -eq 0 -and -not $DryRun) {
    Fail "dotnet pack produced no .nupkg files."
}
foreach ($pkg in $packages) { Step $pkg.Name }

# --- Sign packages -------------------------------------------------------------------
Section "Sign packages"
Invoke-Signotaur ($packages | ForEach-Object { $_.FullName }) "NuGet packages"

# --- Push ----------------------------------------------------------------------------
if ($NoPush) {
    Section "NoPush set"
    Step "Signed packages left in place:"
    foreach ($pkg in $packages) { Step "  $($pkg.FullName)" }
    Section "Done"
    exit 0
}

Section "Push to nuget.org"
foreach ($pkg in $packages) {
    Step "Pushing $($pkg.Name)"
    if ($DryRun) {
        Step "[dry-run] dotnet nuget push $($pkg.FullName) --source $nugetSource --skip-duplicate"
        continue
    }
    $pushArgs = @("nuget", "push", $pkg.FullName, "--source", $nugetSource, "--skip-duplicate")
    if (-not [string]::IsNullOrWhiteSpace($ApiKey)) {
        $pushArgs += @("--api-key", $ApiKey)
    }
    & dotnet @pushArgs
    if ($LASTEXITCODE -ne 0) {
        Fail "dotnet nuget push failed for $($pkg.Name) (exit $LASTEXITCODE)"
    }
    Ok "Pushed $($pkg.Name)"
}

# --- Archive -------------------------------------------------------------------------
Section "Archive"
if (-not (Test-Path $archiveDir) -and -not $DryRun) {
    New-Item -ItemType Directory -Path $archiveDir | Out-Null
}
foreach ($pkg in $packages) {
    Step "$($pkg.Name) -> nugetpackagearchive/"
    if (-not $DryRun) {
        Move-Item -LiteralPath $pkg.FullName -Destination (Join-Path $archiveDir $pkg.Name) -Force
    }
}

Section "Done"
