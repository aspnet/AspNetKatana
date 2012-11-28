@echo off
cd %~dp0

set EnableNuGetPackageRestore=true

.nuget\NuGet.exe install Sake -version 0.2 -o packages

packages\Sake.0.2\tools\Sake.exe -I build -f Sakefile.shade %*
