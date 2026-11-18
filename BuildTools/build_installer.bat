@echo off
setlocal
cd /d "%~dp0"

set VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe
for /f "usebackq delims=" %%i in (`"%VSWHERE%" -latest -prerelease -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do set MSBUILD=%%i
if not defined MSBUILD (
    echo MSBuild not found, install Visual Studio 2022 or Build Tools for Visual Studio 2022.
    goto end
)

"%MSBUILD%" build_output.xml -nologo -v:minimal || goto end
"%MSBUILD%" build_installer.xml -nologo -v:minimal || goto end

:end
pause
