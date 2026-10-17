@echo off
setlocal

echo ========================================================
echo   NIGHTBOUND - Building Windows x64
echo ========================================================

set "PROJECT_PATH=%~dp0"

if defined UNITY_EDITOR_PATH (
    set "UNITY_PATH=%UNITY_EDITOR_PATH%"
) else (
    set "UNITY_PATH=C:\Program Files\Unity\Hub\Editor\6000.6.4f1\Editor\Unity.exe"
)

if not exist "%UNITY_PATH%" (
    echo [ERROR] Unity Editor not found: "%UNITY_PATH%"
    echo Set UNITY_EDITOR_PATH to your Unity.exe path and try again.
    endlocal & exit /b 1
)

set "BUILD_LOG=%PROJECT_PATH%build-windows.log"
"%UNITY_PATH%" -batchmode -quit -projectPath "%PROJECT_PATH%" -executeMethod BuildScript.BuildWindows -logFile "%BUILD_LOG%"
set "BUILD_EXIT_CODE=%ERRORLEVEL%"

if not "%BUILD_EXIT_CODE%"=="0" (
    echo [ERROR] Windows build failed. See "%BUILD_LOG%".
    endlocal & exit /b %BUILD_EXIT_CODE%
)

if not exist "%PROJECT_PATH%Builds\Nightbound.exe" (
    echo [ERROR] Unity returned success, but Builds\Nightbound.exe was not created.
    echo See "%BUILD_LOG%".
    endlocal & exit /b 1
)

echo [SUCCESS] Built "%PROJECT_PATH%Builds\Nightbound.exe"
endlocal & exit /b 0
