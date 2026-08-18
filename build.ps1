$ErrorActionPreference = 'Stop'

# Keep this script ASCII-only so Windows PowerShell 5.1 can parse it correctly
# even when the repository itself is stored in a path containing Chinese text.
$projectDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$source = Join-Path $projectDir 'QixiRomanticHeartParticles.cs'
$icon = Join-Path $projectDir 'heart.ico'
$dist = Join-Path $projectDir 'dist'
$outputName = [Text.Encoding]::UTF8.GetString(
    [Convert]::FromBase64String('5LiD5aSV5rWq5ryrM0TniLHlv4PnspLlrZAuZXhl')
)
$output = Join-Path $dist $outputName

if (-not (Test-Path -LiteralPath $compiler)) {
    throw "C# compiler not found: $compiler"
}

New-Item -ItemType Directory -Path $dist -Force | Out-Null

& $compiler `
    /nologo `
    /target:winexe `
    /optimize+ `
    /platform:anycpu `
    /reference:System.dll `
    /reference:System.Drawing.dll `
    /reference:System.Windows.Forms.dll `
    "/win32icon:$icon" `
    "/out:$output" `
    $source

if ($LASTEXITCODE -ne 0) {
    throw "Build failed. Compiler exit code: $LASTEXITCODE"
}

Write-Host "Build complete: $output"
