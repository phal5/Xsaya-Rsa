# Xsaya-Rsa 데모 런처(자동 정리) 빌드 스크립트
# 사용법:  powershell -ExecutionPolicy Bypass -File build.ps1
$ErrorActionPreference = "Stop"

$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$proj = Join-Path $here "src\DemoLauncher.csproj"
$out  = Join-Path $here "dist"

Write-Host "빌드 중..." -ForegroundColor Cyan
dotnet publish $proj -c Release -o $out

$exe = Join-Path $out "Xsaya-Rsa.exe"
Write-Host ""
if (Test-Path $exe) {
    $size = [math]::Round((Get-Item $exe).Length / 1MB, 1)
    Write-Host "완료: $exe  ($size MB)" -ForegroundColor Green
} else {
    Write-Host "실패: exe가 생성되지 않았습니다." -ForegroundColor Red
    exit 1
}
