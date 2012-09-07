@echo off

rem This file needs to be executed from an ELEVATED, BVT or VS command prompt

call SetVariables.bat

echo ------ Install Server Certificate
rem The certificate file was created using the following command.  Then go into certificate management concole in MMC (local machine) and export the certificate to the file HttpsServerTestCertificate.pfx
rem makecert -r -pe -n "CN=HttpsServerTestCertificate" -b 01/01/2008 -e 01/01/2025 -eku 1.3.6.1.5.5.7.3.1 -ss my -sr localmachine -sky exchange -sp "Microsoft RSA SChannel Cryptographic Provider" -sy 12

"%HttpServer_MsBuildPath%" /nologo /verbosity:minimal /target:rebuild ..\Tools\CertificateInstaller\CertificateInstaller.sln
if %ERRORLEVEL% == 0 (echo SUCCESS: Built CertInstaller.exe.) else (echo ERROR: Unable to build CertInstaller.exe. MSBuild.exe is required: Make sure to run this batch file from a BVT console window or a VS console window.)
..\Tools\CertificateInstaller\CertInstaller\Bin\Debug\CertInstaller.exe install %HttpServer_HttpsCertFileName% %HttpServer_HttpsCertFilePassword%
if %ERRORLEVEL% == 0 (echo SUCCESS: Certificate installed successfully.) else (echo ERROR: Unable to install certificate '%HttpServer_HttpsCertFileName%'.)


rem Determine the OS version
ver | find " 5." > nul
if %ERRORLEVEL% == 0 goto PreVista
goto VistaAndLater


:VistaAndLater

echo ------ Setup for: Vista, Win7, or later

echo ------ Add URL reservation for %HttpServer_HttpPort%
netsh http show urlacl | find ":%HttpServer_HttpPort%/"
if %ERRORLEVEL% == 0 (echo WARNING: There is already a URL reservation for port %HttpServer_HttpPort%.) else (netsh http add urlacl url=http://*:%HttpServer_HttpPort%/ user=Users)
netsh http show urlacl | find "http://*:%HttpServer_HttpPort%/"
if %ERRORLEVEL% == 0 (echo SUCCESS: URL reservation for port %HttpServer_HttpPort% completed successfully.) else (echo ERROR: URL reservation for port %HttpServer_HttpPort% failed.)

echo ------ Add URL reservation for %HttpServer_HttpsPort%
netsh http show urlacl | find ":%HttpServer_HttpsPort%/"
if %ERRORLEVEL% == 0 (echo WARNING: There is already a URL reservation for port %HttpServer_HttpsPort%.) else (netsh http add urlacl url=https://*:%HttpServer_HttpsPort%/ user=Users)
netsh http show urlacl | find "https://*:%HttpServer_HttpsPort%/"
if %ERRORLEVEL% == 0 (echo SUCCESS: URL reservation for port %HttpServer_HttpsPort% completed successfully.) else (echo ERROR: URL reservation for port %HttpServer_HttpsPort% failed.)

echo ------ Register Server Certificate
netsh http show sslcert | find ":%HttpServer_HttpsPort%"
if %ERRORLEVEL% == 0 (echo WARNING: There is already a certificate registered for port %HttpServer_HttpsPort%.) else (netsh http add sslcert ipport=0.0.0.0:%HttpServer_HttpsPort% certhash=%HttpServer_CertHash% appid={00000000-0000-0000-0000-000000000000})
netsh http show sslcert | find "0.0.0.0:%HttpServer_HttpsPort%"
if %ERRORLEVEL% == 0 (echo SUCCESS: Certificate for port %HttpServer_HttpsPort% registered successfully.) else (echo ERROR: Certificate registration for port %HttpServer_HttpsPort% failed.)

goto End


:PreVista

echo ------ Setup for: XP, 2003

echo ------ Add URL reservation for %HttpServer_HttpPort%
..\Tools\XPTools\httpcfg.exe query urlacl | find ":%HttpServer_HttpPort%/"
if %ERRORLEVEL% == 0 (echo WARNING: There is already a URL reservation for port %HttpServer_HttpPort%.) else (..\Tools\XPTools\httpcfg.exe set urlacl /u http://*:%HttpServer_HttpPort%/ /a "D:(A;;GX;;;WD)")
..\Tools\XPTools\httpcfg.exe query urlacl | find "http://*:%HttpServer_HttpPort%/"
if %ERRORLEVEL% == 0 (echo SUCCESS: URL reservation for port %HttpServer_HttpPort% completed successfully.) else (echo ERROR: URL reservation for port %HttpServer_HttpPort% failed.)

echo ------ Add URL reservation for %HttpServer_HttpsPort%
..\Tools\XPTools\httpcfg.exe query urlacl | find ":%HttpServer_HttpsPort%/"
if %ERRORLEVEL% == 0 (echo WARNING: There is already a URL reservation for port %HttpServer_HttpsPort%.) else (..\Tools\XPTools\httpcfg.exe set urlacl /u https://*:%HttpServer_HttpsPort%/ /a "D:(A;;GX;;;WD)")
..\Tools\XPTools\httpcfg.exe query urlacl | find "https://*:%HttpServer_HttpsPort%/"
if %ERRORLEVEL% == 0 (echo SUCCESS: URL reservation for port %HttpServer_HttpsPort% completed successfully.) else (echo ERROR: URL reservation for port %HttpServer_HttpsPort% failed.)

echo ------ Register Server Certificate
..\Tools\XPTools\httpcfg.exe query ssl | find ":%HttpServer_HttpsPort%"
if %ERRORLEVEL% == 0 (echo WARNING: There is already a certificate registered for port %HttpServer_HttpsPort%.) else (..\Tools\XPTools\httpcfg.exe set ssl -i 0.0.0.0:%HttpServer_HttpsPort% -f 2 -c "MY" -h %HttpServer_CertHash%)
..\Tools\XPTools\httpcfg.exe query ssl | find "0.0.0.0:%HttpServer_HttpsPort%"
if %ERRORLEVEL% == 0 (echo SUCCESS: Certificate for port %HttpServer_HttpsPort% registered successfully.) else (echo ERROR: Certificate registration for port %HttpServer_HttpsPort% failed.)

goto End


:End