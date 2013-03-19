---
title: Getting started
order: 1
template: section.jade
---

So you've decided to use Katana for your web application. Great job! That was the hard part.

Now let's look a the easy part - how do you add Katana's Microsoft.Owin components to your Web Application project type? And what can you do from there?

### Hello, World!

First, you need a class

```csharp
using Owin;

namespace MyApplication
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            System.Console.WriteLine("Hello, world!");
        }
    }
}
```

### How to run this on IIS with ASP.NET

NuGet to the rescue!

```
Install-Package Microsoft.Owin.Host.SystemWeb
```

