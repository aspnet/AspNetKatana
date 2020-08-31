Set-PSDebug -Trace 1
[System.Net.ServicePointManager]::SecurityProtocol
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::TLS12
[System.Net.ServicePointManager]::SecurityProtocol
try
{
  $client = New-Object System.Net.WebClient
  $client.DownloadFile('https://dist.nuget.org/win-x86-commandline/latest/nuget.exe', '.nuget\NuGet.exe')
}
catch [System.Exception]
{
  $Error[0].Exception.ToString()
}