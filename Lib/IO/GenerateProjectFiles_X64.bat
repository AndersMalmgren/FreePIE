@echo off

::Clean up the old build folder
IF EXIST build (
  del /f /s /q build > nul
  rmdir /s /q build
)

:: Create the build folder
mkdir build
cd build

echo "Rebuilding FreePIE IO x64 Project files..."
cmake .. -G "Visual Studio 14 2015 Win64"
IF %ERRORLEVEL% NEQ 0 (
  echo "Error generating FreePIE IO 64-bit project files"
  goto failure
)

EXIT /B 0

:failure
pause
EXIT /B 1