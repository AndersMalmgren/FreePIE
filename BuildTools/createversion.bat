@ECHO off

FOR /F "tokens=1,2 delims=:M" %%A in ('svnversion ../ -c') do SET PART_1=%%A&SET PART_2=%%B

SET file=../VersionInfo.cs

IF NOT DEFINED PART_2 (
SET SVN_REV=%PART_1%
)

IF NOT DEFINED SVN_REV (
SET SVN_REV=%PART_2%
)

set result=0

for /f "tokens=3" %%f in ('find /c /i ".%SVN_REV%." %file%') do set result=%%f

IF %result% gtr 0 (
GOTO end
)

ECHO using System.Reflection; > %file%
ECHO [assembly: AssemblyVersion("0.2.%SVN_REV%.0")]     >>  %file%
ECHO [assembly: AssemblyFileVersion("0.2.%SVN_REV%.0")] >> %file%

:end