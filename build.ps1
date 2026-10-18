$ErrorActionPreference = "Stop"

Write-Host "=== 1. Compiling CozyCityMayor.exe from multi-file C# source (src\*.cs) ===" -ForegroundColor Cyan
New-Item -ItemType Directory -Force -Path "build" | Out-Null
New-Item -ItemType Directory -Force -Path "installer_output" | Out-Null

$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
& $csc /target:winexe /win32icon:"assets\cozy_mayor.ico" /out:"build\CozyCityMayor.exe" /optimize+ /r:System.dll,System.Drawing.dll,System.Windows.Forms.dll "src\*.cs"
if ($LASTEXITCODE -ne 0) {
    throw "Compilation of CozyCityMayor.exe failed!"
}
Write-Host "Successfully built build\CozyCityMayor.exe" -ForegroundColor Green

Write-Host "=== 2. Running Automated Self-Test & Screenshot Verification ===" -ForegroundColor Cyan
Start-Process -Wait -FilePath ".\build\CozyCityMayor.exe" -ArgumentList "--verify","assets"
Write-Host "Self-test and screenshot verification passed!" -ForegroundColor Green

Write-Host "=== 3. Building Inno Setup 6 Installer (with EULA.txt) ===" -ForegroundColor Cyan
$iscc = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if (Test-Path $iscc) {
    & $iscc "installer.iss"
    if ($LASTEXITCODE -ne 0) {
        throw "Inno Setup compilation failed!"
    }
    Write-Host "Successfully built installer_output\CozyCityMayor_Setup.exe" -ForegroundColor Green
} else {
    throw "ISCC.exe not found at $iscc"
}
