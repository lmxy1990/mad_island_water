param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $repoRoot "MadIslandWater.sln"

dotnet restore $solution
dotnet build $solution -c $Configuration --no-restore
