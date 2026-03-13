@echo off
chcp 65001 >nul 2>&1
title barian_Lin Unity Installer
echo.
echo ============================================================
echo   barian_Lin  Unity Project  Auto-Installer
echo ============================================================
echo.

:: Script location
SET SCRIPT_DIR=%~dp0
SET PROJECT_PATH=

echo [1/3] Searching for barian_Lin project...
echo.

:: --- Search common locations ---
FOR %%D IN (
    "%USERPROFILE%\Desktop\barian_Lin"
    "%USERPROFILE%\Documents\barian_Lin"
    "%USERPROFILE%\My Project\barian_Lin"
    "%USERPROFILE%\Documents\Unity Projects\barian_Lin"
    "C:\Users\%USERNAME%\Desktop\barian_Lin"
    "C:\Users\%USERNAME%\Documents\barian_Lin"
    "C:\Users\%USERNAME%\My Project\barian_Lin"
    "C:\Projects\barian_Lin"
    "D:\Projects\barian_Lin"
    "D:\Unity\barian_Lin"
) DO (
    IF EXIST "%%~D\Assets" (
        SET PROJECT_PATH=%%~D
        echo    Found: %%~D
        GOTO FOUND
    )
)

:: --- Not found: ask user ---
echo.
echo [!] Could not find barian_Lin project automatically.
echo.
SET /P PROJECT_PATH="Enter full path to barian_Lin project: "
IF NOT EXIST "%PROJECT_PATH%\Assets" (
    echo [ERROR] Not a valid Unity project path.
    pause
    EXIT /B 1
)

:FOUND
echo.
echo [2/3] Copying files to: %PROJECT_PATH%
echo.

:: --- Create folders ---
IF NOT EXIST "%PROJECT_PATH%\Assets\Sprites\Warrior"    MKDIR "%PROJECT_PATH%\Assets\Sprites\Warrior"
IF NOT EXIST "%PROJECT_PATH%\Assets\Scripts"             MKDIR "%PROJECT_PATH%\Assets\Scripts"
IF NOT EXIST "%PROJECT_PATH%\Assets\Scripts\Editor"     MKDIR "%PROJECT_PATH%\Assets\Scripts\Editor"

:: --- Copy sprites ---
XCOPY /Y /Q "%SCRIPT_DIR%Assets\Sprites\Warrior\*.png"  "%PROJECT_PATH%\Assets\Sprites\Warrior\"
IF ERRORLEVEL 1 (
    echo [ERROR] Failed to copy sprites. Check path.
    pause & EXIT /B 1
)
echo    [OK] Sprites copied

:: --- Copy scripts ---
XCOPY /Y /Q "%SCRIPT_DIR%Assets\Scripts\*.cs"            "%PROJECT_PATH%\Assets\Scripts\"
XCOPY /Y /Q "%SCRIPT_DIR%Assets\Scripts\Editor\*.cs"    "%PROJECT_PATH%\Assets\Scripts\Editor\"
echo    [OK] Scripts copied

echo.
echo [3/3] Done!
echo.
echo ============================================================
echo   Next steps in Unity Editor:
echo.
echo   1. Open barian_Lin project in Unity Hub
echo   2. Menu:  Tools  ^>  Warrior Setup  ^>  Run Full Setup
echo   3. Drag  Assets/Prefabs/Warrior.prefab  into scene
echo   4. Press Play!
echo.
echo   Controls:
echo     A / Left Arrow   : Move Left
echo     D / Right Arrow  : Move Right
echo     Space            : Jump
echo     Left Mouse Click : Sword Attack
echo ============================================================
echo.
pause
