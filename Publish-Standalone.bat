@echo off
setlocal
cd /d "%~dp0"

where dotnet >nul 2>nul
if errorlevel 1 (
  echo .NET 10 SDK was not found.
  echo Install the .NET 10 SDK, then run this file again.
  pause
  exit /b 1
)

dotnet publish RobsMusicCopier.csproj -c Release -r win-x64 --self-contained true
if errorlevel 1 (
  echo.
  echo Publish failed.
  pause
  exit /b 1
)

echo.
echo Standalone EXE created here:
echo %CD%\bin\Release\net10.0-windows\win-x64\publish\RobsMusicCopier-v1.05.exe
explorer "%CD%\bin\Release\net10.0-windows\win-x64\publish"
pause
