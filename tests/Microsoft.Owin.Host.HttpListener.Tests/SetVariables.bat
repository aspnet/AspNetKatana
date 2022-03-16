echo ------ Set required variables
set HttpServer_HttpPort=8080
set HttpServer_HttpsPort=9090
set HttpServer_HttpsCertFileName=HttpsServerTestCertificate.pfx
rem [SuppressMessage("Microsoft.Security", "CS002:SecretInNextLine", Justification="Unit test dummy credentials.")]
set HttpServer_HttpsCertFilePassword=katana
set HttpServer_CertHash=c7306ce743cf189e984cf804eb1d14fb62bd4f4e
rem If this file is not executed from a BVT console, assume msbuild.exe is in the path (i.e. SDK console)
if "%DD_NdpInstallPath%"=="" (set HttpServer_MsBuildPath=msbuild.exe) else (set HttpServer_MsBuildPath=%DD_NdpInstallPath%\msbuild.exe)
