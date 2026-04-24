@echo off
setlocal

set ROOT=%~dp0
set DIST=%ROOT%\dist

REM Clean dist
if exist "%DIST%" rmdir /s /q "%DIST%"
mkdir "%DIST%\ZoneFbx.GUI"
mkdir "%DIST%\ZoneFbx.GUI\ZoneFbxCLI"

REM Build
dotnet build "%ROOT%\ZoneFbx\ZoneFbx.csproj" -c GithubRelease -p:Platform=x64
dotnet build "%ROOT%\ZoneFbx.GUI\ZoneFbx.GUI.csproj" -c GithubRelease -p:Platform=x64

REM Copy GUI
xcopy "%ROOT%\ZoneFbx.GUI\bin\x64\GithubRelease\net10.0-windows\*" "%DIST%\ZoneFbx.GUI\" /E /I /Y

REM Copy CLI into subfolder
xcopy "%ROOT%\ZoneFbx\bin\x64\GithubRelease\net10.0-windows\*" "%DIST%\ZoneFbx.GUI\ZoneFbxCLI\" /E /I /Y /D

REM Copy CLI into subfolder
xcopy "%ROOT%\x64\Debug\*" "%DIST%\ZoneFbx.GUI\ZoneFbxCLI\" /E /I /Y /D

echo Done.
pause
