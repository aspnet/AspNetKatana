@echo off
cd %~dp0

set EnableNuGetPackageRestore=true
".nuget\NuGet.exe" install Sake -version 0.1.3 -o packages
for /f "tokens=*" %%G in ('dir /AD /ON /B "packages\Sake.*"') do set __sake__=%%G
"packages\%__sake__%\tools\Sake.exe" -I src/build -f Sakefile.shade %*
set __sake__=
