OwinHost Readme:
OwinHost enables self-hosting of OWIN applications without requiring the 
developer to write that host process. The OwinHost package contains the 
OwinHost.exe host process as well as assemblies containing default loader 
logic.

OwinHost.exe:
When installing the OwinHost package from within Visual Studio, OwinHost.exe 
can be found in the package's 'tools' directory. In a typical NuGet 
configuration, this will be located at 
<solution root>/packages/OwinHost.(version)/tools. While this package 
installation approach enables use of the Visual Studio Package Manager dialog, 
the placement of OwinHost.exe relative to the project directory can prove 
cumbersome over time if regularly run from the command line. In this case, the 
package can also be installed to a common location on the development machine 
(using NuGet.exe) and that location's directory can be added to the PATH 
environment variable. This approach enables OwinHost.exe to be run from the 
project directory without any path qualifiers. Some third party tools, such as 
Chocolatey automate this process by setting up a central location for binaries 
and adding that location to the PATH.

Launching OwinHost:
Self-hosting an OWIN application with OwinHost is as simple as running 
OwinHost.exe from your Web application's project directory. Project directory 
is defined here as the parent directory of ./bin which contains the 
application's assemblies as well as the selected server assembly. By default, 
when running OwinHost.exe with no additional parameters, the host will attempt 
to locate and load the application's startup class and the OWIN HttpListener 
server. After constructing the OWIN pipeline with the help of the startup 
class, it will then begin listening on port 5000. All of these default 
behaviors can be easily changed using parameters to OwinHost.exe, as described 
below.

Launching OwinHost from Within Visual Studio 2013:
In Visual Studio 2013, OwinHost can be launched directly from within the IDE 
using the F5 gesture. This is accomplished by a new feature wherein Visual 
Studio can run custom Web servers that have been registered in a Web 
application project. For Visual Studio 2013 Web application projects, the 
OwinHost NuGet package will automatically register OwinHost.exe as a custom Web 
server. To use it, navigate to the Web tab of project properties and select 
OwinHost from the dropdown list of available Web servers. Additional command 
line settings can optionally be specified using the form fields underneath the 
server list. After setting OwinHost as the Web server, the project can be run 
using OwinHost.exe by pressing F5.

OwinHost Parameters:
There are a variety of ways to customize the default behavior of OwinHost. For 
example, to select an alternate OWIN-compatible server, run the following:

OwinHost.exe -s <Custom.Server.Assembly>

The full list of options can be seen by running:

OwinHost.exe /?