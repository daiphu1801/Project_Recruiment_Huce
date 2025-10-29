@echo off
echo Testing website...
echo.

REM Test if website is running
echo Checking if IIS Express is running...
tasklist | findstr iisexpress >nul
if %ERRORLEVEL% neq 0 (
    echo [ERROR] IIS Express is not running. Please run start-iis-express.bat first.
    pause
    exit /b 1
)

echo IIS Express is running.
echo.

REM Test HTTP response
echo Testing HTTP response...
powershell -Command "try { $response = Invoke-WebRequest -Uri 'http://localhost:44300' -Method Head; Write-Host 'Status:' $response.StatusCode; Write-Host 'Content-Type:' $response.Headers['Content-Type']; Write-Host 'Website is working!' } catch { Write-Host 'Error:' $_.Exception.Message }"

echo.
echo Opening website in browser...
start http://localhost:44300

pause
