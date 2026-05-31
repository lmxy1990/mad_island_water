param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [string]$Runtime = "win-x64",

    [switch]$SelfContained,

    [string]$Output = ""
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot "MadIslandWater\MadIslandWater.csproj"

if ([string]::IsNullOrWhiteSpace($Output)) {
    $Output = Join-Path $repoRoot "publish\MadIslandWater"
}

dotnet publish $project `
    -c $Configuration `
    -r $Runtime `
    --self-contained:$($SelfContained.IsPresent.ToString().ToLowerInvariant()) `
    -o $Output

$sourceDlc = Join-Path $repoRoot "dlc"
if (Test-Path -LiteralPath $sourceDlc) {
    $targetDlc = Join-Path $Output "dlc"
    New-Item -ItemType Directory -Force -Path $targetDlc | Out-Null
    Copy-Item -LiteralPath (Join-Path $sourceDlc "dlc_00.zip") -Destination $targetDlc -Force
}

Write-Host "Published to $Output"
