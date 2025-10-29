@echo off
setlocal

REM ---- Đường dẫn MSBuild (Visual Studio Community 2022) ----
set "MSBUILD=C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
if not exist "%MSBUILD%" (
  echo [ERROR] MSBuild not found at Community path. Trying BuildTools...
  set "MSBUILD=C:\Program Files\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
  if not exist "%MSBUILD%" (
    echo [ERROR] MSBuild not found. Please install Visual Studio 2022 Community or Build Tools.
    pause
    exit /b 1
  )
)

REM ---- Restore NuGet packages ----
"%MSBUILD%" Project_Recruiment_Huce.sln /t:Restore

REM ---- Build Debug (đổi Configuration/Platform nếu cần) ----
"%MSBUILD%" Project_Recruiment_Huce.sln /p:Configuration=Debug /m

exit /b %ERRORLEVEL%
