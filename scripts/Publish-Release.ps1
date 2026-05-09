param(
    [Parameter()]
    [string]$Version = "local",

    [Parameter()]
    [string[]]$RuntimeIdentifiers = @("win-x64", "win-arm64")
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Path $PSScriptRoot -Parent
$projectPath = Join-Path $projectRoot "TwentyTwentyTray.csproj"
$outputRoot = Join-Path $projectRoot (Join-Path "dist\release" $Version)

if (Test-Path $outputRoot)
{
    Remove-Item $outputRoot -Recurse -Force
}

New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null

foreach ($runtimeIdentifier in $RuntimeIdentifiers)
{
    $publishDirectory = Join-Path $outputRoot $runtimeIdentifier
    $zipPath = Join-Path $outputRoot ("TwentyTwentyTray-{0}-{1}.zip" -f $Version, $runtimeIdentifier)

    dotnet publish $projectPath `
        -c Release `
        -r $runtimeIdentifier `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:EnableCompressionInSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -o $publishDirectory

    if ($LASTEXITCODE -ne 0)
    {
        throw "Publish failed for runtime '$runtimeIdentifier'."
    }

    if (Test-Path $zipPath)
    {
        Remove-Item $zipPath -Force
    }

    Compress-Archive -Path (Join-Path $publishDirectory "*") -DestinationPath $zipPath -Force
    Write-Host "Created $zipPath"
}

Write-Host "Release packages available in $outputRoot"