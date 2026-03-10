@echo off
REM Change to the directory where this script is located
cd /d "%~dp0"

echo ====================================
echo Building PanKek C# Scripts
echo ====================================
echo.

REM Check if dotnet is installed
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: dotnet SDK not found!
    echo Please install .NET 6.0 SDK from: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo Found dotnet SDK:
dotnet --version
echo.

REM Build ScriptCore (we're already in Scripts folder)
echo Building ScriptCore...
dotnet build ScriptCore.csproj -c Release
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: ScriptCore build failed!
    pause
    exit /b 1
)
echo ScriptCore built successfully!
echo.

echo ====================================
echo Build Complete!
echo ====================================
echo.
echo Output DLL:
echo   - ScriptCore.dll (in this folder)
echo   - Contains: Entity API + TestScript + your game scripts
echo.
echo You can now run your engine and attach TestScript to entities!
echo.
pause
