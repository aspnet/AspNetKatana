Getting started with Microsoft OWIN self-host libraries:

This package contains libraries for hosting OWIN compatible HTTP components in in your own process.

First define a Startup class and a Configuration method where your OWIN HTTP pipeline is constructed. See the following example:
https://aspnet.codeplex.com/SourceControl/latest#Samples/Katana/Embedded/Startup.cs

Then call that Startup class from your application as follows:
https://aspnet.codeplex.com/SourceControl/latest#Samples/Katana/Embedded/Program.cs

For additional information see:
http://katanaproject.codeplex.com/
http://aspnet.codeplex.com/
http://www.owin.org/