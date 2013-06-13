Getting started with Microsoft OWIN self-host libraries:

This package contains libraries for hosting OWIN compatible HTTP components in in your own process.

An example Startup class is included. The Startup class can be called from your application as follows:
using (WebApp.Start<Startup>("http://localhost:12345"))
{
    Console.ReadLine();
}

For additional information see:
http://katanaproject.codeplex.com/
http://aspnet.codeplex.com/SourceControl/latest#Samples/Katana/Embedded/
http://aspnet.codeplex.com/
http://www.owin.org/