@echo off
chcp 65001 > nul
echo ============================================================
echo   barian_Lin Unity Project Installer (Windows)
echo ============================================================
echo.

:: 현재 스크립트 위치
SET SCRIPT_DIR=%~dp0

:: barian_Lin 프로젝트 자동 탐색
SET PROJECT_PATH=

echo [1/3] barian_Lin 프로젝트 탐색 중...

:: 일반적인 위치들 탐색
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
        echo    발견: %%~D
        GOTO FOUND
    )
)

:: 못 찾으면 직접 입력
echo.
echo ❌ 자동으로 찾지 못했습니다.
echo.
SET /P PROJECT_PATH="barian_Lin 프로젝트 전체 경로를 입력하세요: "
IF NOT EXIST "%PROJECT_PATH%\Assets" (
    echo ❌ 올바른 Unity 프로젝트 경로가 아닙니다.
    pause
    EXIT /B 1
)

:FOUND
echo.
echo [2/3] 파일 복사 중...
echo    대상: %PROJECT_PATH%

:: 폴더 생성
IF NOT EXIST "%PROJECT_PATH%\Assets\Sprites\Warrior" MKDIR "%PROJECT_PATH%\Assets\Sprites\Warrior"
IF NOT EXIST "%PROJECT_PATH%\Assets\Scripts"          MKDIR "%PROJECT_PATH%\Assets\Scripts"
IF NOT EXIST "%PROJECT_PATH%\Assets\Scripts\Editor"   MKDIR "%PROJECT_PATH%\Assets\Scripts\Editor"

:: 스프라이트 복사
XCOPY /Y /Q "%SCRIPT_DIR%unity_project\Assets\Sprites\Warrior\*.png"  "%PROJECT_PATH%\Assets\Sprites\Warrior\"
echo    ✓ 스프라이트 복사 완료

:: C# 스크립트 복사
XCOPY /Y /Q "%SCRIPT_DIR%unity_project\Assets\Scripts\*.cs"            "%PROJECT_PATH%\Assets\Scripts\"
XCOPY /Y /Q "%SCRIPT_DIR%unity_project\Assets\Scripts\Editor\*.cs"     "%PROJECT_PATH%\Assets\Scripts\Editor\"
echo    ✓ 스크립트 복사 완료

echo.
echo [3/3] 완료!
echo.
echo ============================================================
echo   Unity Editor에서 다음을 실행하세요:
echo.
echo   메뉴: Tools ^> Warrior Setup ^> Run Full Setup
echo   그 다음: Assets/Prefabs/Warrior.prefab 씬에 배치
echo.
echo   조작:  A/D=이동  Space=점프  마우스좌클릭=공격
echo ============================================================
echo.
pause
