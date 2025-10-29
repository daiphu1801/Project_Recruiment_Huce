@echo off
setlocal enabledelayedexpansion

REM ---- Kill iisexpress cũ nếu đang chạy ----
taskkill /IM iisexpress.exe /F >nul 2>&1

set "SITE_PATH=%~dp0Project_Recruiment_Huce"
set "PORT=44300"
set "IIS_EXPRESS=%ProgramFiles(x86)%\IIS Express\iisexpress.exe"

if not exist "%IIS_EXPRESS%" (
  echo [ERROR] IIS Express not found.
  exit /b 1
)

if not exist "%SITE_PATH%\web.config" (
  echo [ERROR] web.config not found in "%SITE_PATH%".
  exit /b 1
)

pushd "%SITE_PATH%"
echo Starting IIS Express on http://localhost:%PORT% ...
"%IIS_EXPRESS%" /path:"%SITE_PATH%" /port:%PORT% /systray:true
popd
