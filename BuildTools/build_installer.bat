set PATH=%PATH%;%WINDIR%\Microsoft.Net\Framework64\v4.0.30319.

rem go to current folder
cd %~dp0

msbuild write_VersionInfo.xml
msbuild build_output.xml /property:OutputTemp=..\OutputTemp /property:BuildDir=..\Output /property:Lib=..\Lib /property:Help=..\FreePIE.Core.Plugins\Help
msbuild build_installer.xml /property:InstallerTemp=..\InstallerTemp /property:Lib=..\Lib
pause