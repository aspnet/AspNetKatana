@echo off
cd %~dp0

set EnableNuGetPackageRestore=true

.nuget\NuGet.exe install Sake -version 0.1.4 -o packages

packages\Sake.0.1.4\tools\Sake.exe -I build -f Sakefile.shade %*
