@echo off

rem This file needs to be executed from an ELEVATED, BVT or VS command prompt

call SetVariables.bat

echo ------ Uninstall Server Certificate
"%HttpServer_MsBuildPath%" /nologo /verbosity:minimal /target:rebuild ..\Tools\CertificateInstaller\CertificateInstaller.sln
if %ERRORLEVEL% == 0 (echo SUCCESS: Built CertInstaller.exe.) else (echo ERROR: Unable to build CertInstaller.exe. MSBuild.exe is required: Make sure to run this batch file from a BVT console window or a VS console window.)
..\Tools\CertificateInstaller\CertInstaller\Bin\Debug\CertInstaller.exe uninstall %HttpServer_HttpsCertFileName% %HttpServer_HttpsCertFilePassword%
if %ERRORLEVEL% == 0 (echo SUCCESS: Certificate uninstalled successfully.) else (echo ERROR: Unable to uninstall certificate '%HttpServer_HttpsCertFileName%'.)


rem Determine the OS version
ver | find " 5." > nul
if %ERRORLEVEL% == 0 goto PreVista
goto VistaAndLater


:VistaAndLater

echo ------ Setup for: Vista, Win7, or later

echo ------ Delete URL reservation for %HttpServer_HttpPort%
netsh http show urlacl | find ":%HttpServer_HttpPort%/"
if %ERRORLEVEL% == 0 (netsh http delete urlacl url=http://*:%HttpServer_HttpPort%/) else (echo WARNING: There is no URL reservation for port %HttpServer_HttpPort%.)
netsh http show urlacl | find "http://*:%HttpServer_HttpPort%/"
if %ERRORLEVEL% == 0 (echo ERROR: URL reservation for port %HttpServer_HttpPort% could not be removed.) else (echo SUCCESS: URL reservation for port %HttpServer_HttpPort% deleted successfully.)

echo ------ Delete URL reservation for %HttpServer_HttpsPort%
netsh http show urlacl | find ":%HttpServer_HttpsPort%/"
if %ERRORLEVEL% == 0 (netsh http delete urlacl url=https://*:%HttpServer_HttpsPort%/) else (echo WARNING: There is no URL reservation for port %HttpServer_HttpsPort%.)
netsh http show urlacl | find "https://*:%HttpServer_HttpsPort%/"
if %ERRORLEVEL% == 0 (echo ERROR: URL reservation for port %HttpServer_HttpsPort% could not be removed.) else (echo SUCCESS: URL reservation for port %HttpServer_HttpsPort% deleted successfully.)

echo ------ Unregister Server Certificate
netsh http show sslcert | find ":%HttpServer_HttpsPort%"
if %ERRORLEVEL% == 0 (netsh http delete sslcert ipport=0.0.0.0:%HttpServer_HttpsPort%) else (echo WARNING: There is no certificate registered for port %HttpServer_HttpsPort%.)
netsh http show sslcert | find "0.0.0.0:%HttpServer_HttpsPort%"
if %ERRORLEVEL% == 0 (echo ERROR: Certificate registration for port %HttpServer_HttpsPort% could not be deleted.) else (echo SUCCESS: Certificate registration for port %HttpServer_HttpsPort% deleted successfully.)

goto End


:PreVista

echo ------ Setup for: XP, 2003

echo ------ Delete URL reservation for %HttpServer_HttpPort%
..\Tools\XPTools\httpcfg.exe query urlacl | find ":%HttpServer_HttpPort%/"
if %ERRORLEVEL% == 0 (..\Tools\XPTools\httpcfg.exe delete urlacl /u http://*:%HttpServer_HttpPort%/) else (echo WARNING: There is no URL reservation for port %HttpServer_HttpPort%.)
..\Tools\XPTools\httpcfg.exe query urlacl | find "http://*:%HttpServer_HttpPort%/"
if %ERRORLEVEL% == 0 (echo ERROR: URL reservation for port %HttpServer_HttpPort% could not be removed.) else (echo SUCCESS: URL reservation for port %HttpServer_HttpPort% deleted successfully.)

echo ------ Delete URL reservation for %HttpServer_HttpsPort%
..\Tools\XPTools\httpcfg.exe query urlacl | find ":%HttpServer_HttpsPort%/"
if %ERRORLEVEL% == 0 (..\Tools\XPTools\httpcfg.exe delete urlacl /u https://*:%HttpServer_HttpsPort%/) else (echo WARNING: There is no URL reservation for port %HttpServer_HttpsPort%.)
..\Tools\XPTools\httpcfg.exe query urlacl | find "https://*:%HttpServer_HttpsPort%/"
if %ERRORLEVEL% == 0 (echo ERROR: URL reservation for port %HttpServer_HttpsPort% could not be removed.) else (echo SUCCESS: URL reservation for port %HttpServer_HttpsPort% deleted successfully.)

echo ------ Unregister Server Certificate
..\Tools\XPTools\httpcfg.exe query ssl | find ":%HttpServer_HttpsPort%"
if %ERRORLEVEL% == 0 (..\Tools\XPTools\httpcfg.exe delete ssl -i 0.0.0.0:%HttpServer_HttpsPort%) else (echo WARNING: There is no certificate registered for port %HttpServer_HttpsPort%.)
..\Tools\XPTools\httpcfg.exe query ssl | find "0.0.0.0:%HttpServer_HttpsPort%"
if %ERRORLEVEL% == 0 (echo ERROR: Certificate registration for port %HttpServer_HttpsPort% could not be deleted.) else (echo SUCCESS: Certificate registration for port %HttpServer_HttpsPort% deleted successfully.)

goto End


:End