@echo off
cd /d "%~dp0"

set MAPLE_PATH=%MAPLESTORY_PATH%
if "%MAPLE_PATH%"=="" set MAPLE_PATH=V:\Nexon\maplestory

set FOUND=0
for /r "%MAPLE_PATH%" %%F in (Base.wz Data.wz) do (
    if exist "%%F" set FOUND=1
)
if not "%FOUND%"=="1" (
    echo ERROR: MapleStory WZ files not found under %MAPLE_PATH%
    echo Please ensure MapleStory is installed or set the MAPLESTORY_PATH environment variable.
    pause
    exit /b 1
)

echo Building MapleNecrocer...
dotnet build MapleNecrocer.sln -c Release > build.log 2>&1
if %ERRORLEVEL% neq 0 (
    echo Build failed. See build.log for details.
    pause
    exit /b %ERRORLEVEL%
)

echo Starting MapleNecrocer...
dotnet run -c Release --project MapleNecrocer --no-build --maplePath "%MAPLE_PATH%"
if %ERRORLEVEL% neq 0 (
    echo Application exited with error code %ERRORLEVEL%.
    pause
)
