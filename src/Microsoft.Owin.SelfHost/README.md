# Getting started with Microsoft OWIN self-host libraries:

This package contains libraries for hosting OWIN compatible HTTP components in your own process.

An example Startup class is included below. The Startup class can be called from your application as follows:

```c#
using (WebApp.Start<Startup>("http://localhost:12345"))
{
    Console.ReadLine();
}

public class Startup
{
    public void Configuration(IAppBuilder app)
    {
#if DEBUG
        app.UseErrorPage();
#endif
        app.UseWelcomePage("/");
    }
}
```

For additional information see:
https://github.com/aspnet/aspnetkatana
http://www.owin.org/