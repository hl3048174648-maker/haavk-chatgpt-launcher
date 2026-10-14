# Builds dist\HAAVK-ChatGPT.exe with the C# compiler that ships with Windows (.NET Framework 4.x).
# Usage:  powershell -ExecutionPolicy Bypass -File build.ps1 [-Video path\to\haavk_02.mp4]
param([string]$Video = "")
$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
$fw   = Join-Path $env:WINDIR "Microsoft.NET\Framework64\v4.0.30319"
$csc  = Join-Path $fw "csc.exe"
$out  = Join-Path $root "dist"
if (-not (Test-Path $csc)) { throw "csc.exe not found: $csc" }
New-Item -ItemType Directory -Force $out | Out-Null

& $csc -nologo -codepage:65001 -target:winexe -optimize+ `
    "-win32icon:$root\assets\haavk.ico" `
    "-out:$out\HAAVK-ChatGPT.exe" `
    "-r:$fw\WPF\PresentationFramework.dll" `
    "-r:$fw\WPF\PresentationCore.dll" `
    "-r:$fw\WPF\WindowsBase.dll" `
    "-r:$fw\System.Xaml.dll" `
    "$root\src\HaavkChatGPT.cs"
if ($LASTEXITCODE -ne 0) { throw "build failed" }

Copy-Item "$root\assets\haavk.ico" $out -Force
Copy-Item "$root\create-desktop-shortcut.vbs" $out -Force
if ($Video) { Copy-Item $Video (Join-Path $out "haavk_02.mp4") -Force }
Write-Host "built $out\HAAVK-ChatGPT.exe"
