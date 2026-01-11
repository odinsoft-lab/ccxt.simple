<#
.SYNOPSIS
    Unlists a specific version of CCXT.Simple package from NuGet.org.

.DESCRIPTION
    This script unlists (hides) a specific version of the CCXT.Simple package from NuGet.org.
    Unlisted packages are still available for download if the exact version is known,
    but they won't appear in search results or as the latest version.

    Note: NuGet.org does not support permanent deletion of packages.
    Unlisting is the closest alternative.

.PARAMETER Version
    The version number to unlist (required). Example: "1.1.0"

.PARAMETER ApiKey
    NuGet API key for authentication. If not provided, the script will attempt
    to use the NUGET_API_KEY environment variable.

.PARAMETER Force
    Skip confirmation prompt and unlist immediately.

.EXAMPLE
    .\unlist-nuget.ps1 -Version "1.1.0" -ApiKey "your-api-key"
    Unlists version 1.1.0 using the provided API key.

.EXAMPLE
    .\unlist-nuget.ps1 -Version "1.1.0" -Force
    Unlists version 1.1.0 without confirmation using NUGET_API_KEY environment variable.

.EXAMPLE
    $env:NUGET_API_KEY = "your-api-key"
    .\scripts\unlist-nuget.ps1 -Version "1.0.0"
    Uses API key from environment variable.

.NOTES
    File Name  : unlist-nuget.ps1
    Author     : ODINSOFT Team
    Version    : 1.0.0

    Prerequisites:
    - .NET SDK installed
    - Valid NuGet API key with unlist permissions from https://www.nuget.org/account/apikeys

    Important:
    - Unlisting cannot be undone through this script
    - The package version will still be downloadable if the exact version is known
    - Re-publishing the same version is NOT possible after unlisting

.LINK
    https://github.com/odinsoft-lab/ccxt.simple
    https://www.nuget.org/packages/CCXT.Simple
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$Version,

    [Parameter(Mandatory=$false)]
    [string]$ApiKey = "",

    [Parameter(Mandatory=$false)]
    [switch]$Force = $false
)

# Configuration
$PackageId = "CCXT.Simple"
$NuGetSource = "https://api.nuget.org/v3/index.json"

# Color output functions
function Write-SuccessMsg { param($Message) Write-Host $Message -ForegroundColor Green }
function Write-InfoMsg { param($Message) Write-Host $Message -ForegroundColor Cyan }
function Write-WarningMsg { param($Message) Write-Host $Message -ForegroundColor Yellow }
function Write-ErrorMsg { param($Message) Write-Host $Message -ForegroundColor Red }

# Banner
Write-Host ""
Write-InfoMsg "========================================="
Write-InfoMsg "  CCXT.Simple NuGet Package Unlister"
Write-InfoMsg "========================================="
Write-Host ""

# Validate version format
if ($Version -notmatch '^\d+\.\d+\.\d+(-[a-zA-Z0-9]+)?$') {
    Write-ErrorMsg "Error: Invalid version format '$Version'"
    Write-Host "  Expected format: X.Y.Z or X.Y.Z-suffix (e.g., 1.1.0 or 1.1.0-beta)"
    exit 1
}

# Check if API key is provided or exists in environment
if ([string]::IsNullOrEmpty($ApiKey)) {
    $ApiKey = $env:NUGET_API_KEY
    if ([string]::IsNullOrEmpty($ApiKey)) {
        Write-ErrorMsg "Error: NuGet API key not provided!"
        Write-Host ""
        Write-Host "Please provide API key using one of these methods:"
        Write-Host "  1. Pass as parameter: .\unlist-nuget.ps1 -Version X.Y.Z -ApiKey YOUR_KEY"
        Write-Host "  2. Set environment variable: `$env:NUGET_API_KEY = 'YOUR_KEY'"
        Write-Host ""
        Write-Host "Get your API key from: https://www.nuget.org/account/apikeys"
        exit 1
    } else {
        Write-InfoMsg "Using API key from environment variable NUGET_API_KEY"
    }
}

# Check if dotnet CLI is available
try {
    $dotnetVersion = dotnet --version
    Write-InfoMsg "Found .NET SDK version: $dotnetVersion"
} catch {
    Write-ErrorMsg "Error: .NET SDK not found! Please install from https://dotnet.microsoft.com/download"
    exit 1
}

# Display unlist information
Write-Host ""
Write-WarningMsg "WARNING: You are about to unlist a package from NuGet.org!"
Write-Host ""
Write-Host "  Package: $PackageId"
Write-Host "  Version: $Version"
Write-Host "  URL: https://www.nuget.org/packages/$PackageId/$Version"
Write-Host ""
Write-WarningMsg "Important notes:"
Write-Host "  - Unlisted packages are hidden from search results"
Write-Host "  - The package can still be downloaded with the exact version"
Write-Host "  - You CANNOT re-publish the same version number after unlisting"
Write-Host "  - This action cannot be easily reversed"
Write-Host ""

# Confirmation
if (-not $Force) {
    $confirmation = Read-Host "Are you sure you want to unlist $PackageId version $Version? (yes/N)"
    if ($confirmation -ne 'yes') {
        Write-WarningMsg "Unlist cancelled by user"
        exit 0
    }
}

# Unlist the package
Write-Host ""
Write-InfoMsg "Unlisting package $PackageId version $Version..."

$unlistCommand = "dotnet nuget delete `"$PackageId`" `"$Version`" --source `"$NuGetSource`" --api-key `"$ApiKey`" --non-interactive"
Write-Host "  Command: dotnet nuget delete $PackageId $Version --source $NuGetSource --api-key [HIDDEN] --non-interactive"

$unlistResult = Invoke-Expression $unlistCommand
if ($LASTEXITCODE -ne 0) {
    Write-ErrorMsg "Failed to unlist package!"
    Write-Host ""
    Write-Host "Common issues:"
    Write-Host "  - Invalid API key or insufficient permissions"
    Write-Host "  - Package version does not exist"
    Write-Host "  - Network connection issues"
    exit 1
}

Write-Host ""
Write-SuccessMsg "========================================="
Write-SuccessMsg "  Package unlisted successfully!"
Write-SuccessMsg "========================================="
Write-Host ""
Write-InfoMsg "Package $PackageId version $Version has been unlisted from NuGet.org"
Write-InfoMsg "Note: It may take a few minutes for the change to propagate"
Write-Host ""
