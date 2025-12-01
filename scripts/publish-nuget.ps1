<#
.SYNOPSIS
    Builds and publishes the CCXT.Simple NuGet package to NuGet.org.

.DESCRIPTION
    This script automates the NuGet package publishing process for CCXT.Simple.
    It performs the following steps:
    1. Validates NuGet API key (from parameter or environment variable)
    2. Cleans previous package files
    3. Builds the project in Release mode
    4. Runs tests to ensure quality
    5. Creates NuGet package (.nupkg) and symbol package (.snupkg)
    6. Publishes to NuGet.org with user confirmation

.PARAMETER ApiKey
    NuGet API key for authentication. If not provided, the script will attempt
    to use the NUGET_API_KEY environment variable.

.PARAMETER SkipBuild
    Skip the build step and use existing compiled binaries.

.PARAMETER SkipTests
    Skip running tests before publishing.

.PARAMETER DryRun
    Perform all steps except the actual publish to NuGet.org.
    Useful for testing the script without publishing.

.EXAMPLE
    .\publish-nuget.ps1 -ApiKey "your-api-key"
    Builds, tests, and publishes the package using the provided API key.

.EXAMPLE
    .\publish-nuget.ps1 -DryRun
    Performs all steps except publishing. Uses NUGET_API_KEY environment variable.

.EXAMPLE
    .\publish-nuget.ps1 -SkipBuild -SkipTests
    Publishes using existing build without rebuilding or testing.

.EXAMPLE
    $env:NUGET_API_KEY = "your-api-key"
    .\scripts\publish-nuget.ps1
    Uses API key from environment variable.

.NOTES
    File Name  : publish-nuget.ps1
    Author     : ODINSOFT Team
    Version    : 1.1.0

    Prerequisites:
    - .NET SDK installed
    - Valid NuGet API key from https://www.nuget.org/account/apikeys

    Security:
    - Never commit API keys to version control
    - Use environment variables for CI/CD pipelines

.LINK
    https://github.com/odinsoft-lab/ccxt.simple
    https://www.nuget.org/packages/CCXT.Simple
#>

param(
    [Parameter(Mandatory=$false)]
    [string]$ApiKey = "",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipBuild = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipTests = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$DryRun = $false
)

# Configuration
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir
$ProjectPath = Join-Path $RootDir "src\ccxt.simple.csproj"
$Configuration = "Release"
$OutputDirectory = Join-Path $RootDir "src\bin\Release"
$NuGetSource = "https://api.nuget.org/v3/index.json"

# Color output functions (using custom names to avoid conflicts with built-in cmdlets)
function Write-SuccessMsg { param($Message) Write-Host $Message -ForegroundColor Green }
function Write-InfoMsg { param($Message) Write-Host $Message -ForegroundColor Cyan }
function Write-WarningMsg { param($Message) Write-Host $Message -ForegroundColor Yellow }
function Write-ErrorMsg { param($Message) Write-Host $Message -ForegroundColor Red }

# Banner
Write-Host ""
Write-InfoMsg "========================================="
Write-InfoMsg "  CCXT.Simple NuGet Package Publisher"
Write-InfoMsg "========================================="
Write-Host ""

# Check if API key is provided or exists in environment
if ([string]::IsNullOrEmpty($ApiKey)) {
    $ApiKey = $env:NUGET_API_KEY
    if ([string]::IsNullOrEmpty($ApiKey)) {
        Write-ErrorMsg "Error: NuGet API key not provided!"
        Write-Host ""
        Write-Host "Please provide API key using one of these methods:"
        Write-Host "  1. Pass as parameter: .\publish-nuget.ps1 -ApiKey YOUR_KEY"
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

# Clean previous packages
Write-Host ""
Write-InfoMsg "Cleaning previous packages..."
if (Test-Path $OutputDirectory) {
    Remove-Item "$OutputDirectory\*.nupkg" -Force -ErrorAction SilentlyContinue
    Remove-Item "$OutputDirectory\*.snupkg" -Force -ErrorAction SilentlyContinue
}

# Build the project
if (-not $SkipBuild) {
    Write-Host ""
    Write-InfoMsg "Building project in $Configuration mode..."

    $buildCommand = "dotnet build `"$ProjectPath`" --configuration $Configuration"
    Write-Host "  Command: $buildCommand"

    $buildResult = Invoke-Expression $buildCommand
    if ($LASTEXITCODE -ne 0) {
        Write-ErrorMsg "Build failed! Please fix build errors and try again."
        exit 1
    }
    Write-SuccessMsg "Build completed successfully!"
} else {
    Write-WarningMsg "Skipping build step (using existing build)"
}

# Run tests (optional)
if (-not $SkipTests) {
    Write-Host ""
    Write-InfoMsg "Running tests..."
    $TestPath = Join-Path $RootDir "tests\CCXT.Simple.Tests.csproj"

    # Build test project first
    $testBuildCommand = "dotnet build `"$TestPath`" --configuration $Configuration"
    Write-Host "  Building tests: $testBuildCommand"
    $testBuildResult = Invoke-Expression $testBuildCommand

    if ($LASTEXITCODE -eq 0) {
        # Run tests
        $testCommand = "dotnet test `"$TestPath`" --configuration $Configuration --no-build"
        Write-Host "  Running tests: $testCommand"
        $testResult = Invoke-Expression $testCommand

        if ($LASTEXITCODE -ne 0) {
            Write-WarningMsg "Tests failed! Consider fixing tests before publishing."
            $confirmation = Read-Host "Do you want to continue anyway? (y/N)"
            if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
                Write-WarningMsg "Publication cancelled"
                exit 1
            }
        } else {
            Write-SuccessMsg "All tests passed!"
        }
    } else {
        Write-WarningMsg "Test build failed. Skipping tests."
    }
} else {
    Write-WarningMsg "Skipping tests (use -SkipTests flag to suppress this warning)"
}

# Create NuGet package
Write-Host ""
Write-InfoMsg "Creating NuGet package..."

$packCommand = "dotnet pack `"$ProjectPath`" --configuration $Configuration --no-build --include-symbols --include-source -p:SymbolPackageFormat=snupkg"
Write-Host "  Command: $packCommand"

$packResult = Invoke-Expression $packCommand
if ($LASTEXITCODE -ne 0) {
    Write-ErrorMsg "Package creation failed!"
    exit 1
}
Write-SuccessMsg "Package created successfully!"

# Find the created package (search recursively for multi-target builds)
$packageFile = Get-ChildItem -Path $OutputDirectory -Filter "*.nupkg" -Recurse |
                Where-Object { $_.Name -notlike "*.symbols.nupkg" } |
                Sort-Object LastWriteTime -Descending |
                Select-Object -First 1

if ($null -eq $packageFile) {
    Write-ErrorMsg "No package file found in $OutputDirectory"
    exit 1
}

$packagePath = $packageFile.FullName
$packageName = $packageFile.Name

# Find symbol package if exists
$symbolPackageFile = Get-ChildItem -Path $OutputDirectory -Filter "*.snupkg" -Recurse |
                      Sort-Object LastWriteTime -Descending |
                      Select-Object -First 1
$symbolPackagePath = if ($null -ne $symbolPackageFile) { $symbolPackageFile.FullName } else { $null }

# Extract version from package name (e.g., CCXT.Simple.1.2.3.nupkg -> 1.2.3)
$packageVersion = if ($packageName -match '\.(\d+\.\d+\.\d+(-[a-zA-Z0-9]+)?)\.nupkg$') { $Matches[1] } else { "unknown" }

Write-Host ""
Write-SuccessMsg "Package created: $packageName"
Write-InfoMsg "Package location: $packagePath"
if ($null -ne $symbolPackagePath) {
    Write-InfoMsg "Symbol package: $($symbolPackageFile.Name)"
}

# Get package info
Write-Host ""
Write-InfoMsg "Package Information:"
Write-Host "  Name: CCXT.Simple"
Write-Host "  Version: $packageVersion"
Write-Host "  Size: $([math]::Round($packageFile.Length / 1KB, 2)) KB"
if ($null -ne $symbolPackageFile) {
    Write-Host "  Symbol Size: $([math]::Round($symbolPackageFile.Length / 1KB, 2)) KB"
}

# Publish to NuGet
if ($DryRun) {
    Write-Host ""
    Write-WarningMsg "DRY RUN MODE - Package will NOT be published"
    Write-InfoMsg "To publish, run without -DryRun flag"
} else {
    Write-Host ""
    Write-WarningMsg "About to publish package to NuGet.org"
    Write-Host "  Package: $packageName"
    Write-Host "  Version: $packageVersion"
    if ($null -ne $symbolPackagePath) {
        Write-Host "  Symbol Package: $($symbolPackageFile.Name)"
    }
    Write-Host ""

    $confirmation = Read-Host "Do you want to continue? (y/N)"
    if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
        Write-WarningMsg "Publication cancelled by user"
        exit 0
    }

    Write-Host ""
    Write-InfoMsg "Publishing package to NuGet.org..."

    # Push main package
    $pushCommand = "dotnet nuget push `"$packagePath`" --api-key `"$ApiKey`" --source `"$NuGetSource`" --skip-duplicate"
    Write-Host "  Command: dotnet nuget push [package] --api-key [HIDDEN] --source $NuGetSource --skip-duplicate"

    $pushResult = Invoke-Expression $pushCommand
    if ($LASTEXITCODE -ne 0) {
        Write-ErrorMsg "Package publication failed!"
        Write-Host ""
        Write-Host "Common issues:"
        Write-Host "  - Invalid API key"
        Write-Host "  - Package version already exists"
        Write-Host "  - Network connection issues"
        exit 1
    }

    # Push symbol package if exists
    if ($null -ne $symbolPackagePath) {
        Write-Host ""
        Write-InfoMsg "Publishing symbol package..."
        $pushSymbolCommand = "dotnet nuget push `"$symbolPackagePath`" --api-key `"$ApiKey`" --source `"$NuGetSource`" --skip-duplicate"
        Write-Host "  Command: dotnet nuget push [symbol-package] --api-key [HIDDEN] --source $NuGetSource --skip-duplicate"

        $pushSymbolResult = Invoke-Expression $pushSymbolCommand
        if ($LASTEXITCODE -ne 0) {
            Write-WarningMsg "Symbol package publication failed (non-critical)"
        } else {
            Write-SuccessMsg "Symbol package published successfully!"
        }
    }

    Write-Host ""
    Write-SuccessMsg "========================================="
    Write-SuccessMsg "  Package published successfully!"
    Write-SuccessMsg "========================================="
    Write-Host ""
    Write-InfoMsg "Package URL: https://www.nuget.org/packages/CCXT.Simple/$packageVersion"
    Write-InfoMsg "It may take a few minutes for the package to appear on NuGet.org"
}

Write-Host ""
Write-SuccessMsg "Process completed!"