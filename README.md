# Welcome to Katana
Katana is a flexible set of components for building and hosting OWIN-based web applications.

This repo is the home for the Katana host, server, and middleware source code and documentation. Official releases of Katana components (including prerelease versions) can be found on http://nuget.org.  

These products are developed by the Katana team from Microsoft in collaboration with a community of open source developers.

See the list of [Packages](https://github.com/aspnet/AspNetKatana/wiki/Packages) for information about individual components.

## Source Code
To build and run the tests from a command prompt, run `build.cmd` (found in the root directory). Note: As part of building you may need to obtain NuGet packages from the NuGet.org public feed*.

*_By running build, you will be initiating the download of other software packages from a NuGet-based feed that is owned by the Outercurve Foundation. You are responsible for locating, reading and complying with the license terms that accompany each such package. Each package is licensed to you by its owner. Microsoft is not responsible for, nor does it grant any licenses to, third-party packages._

See Katana.sln in the root directory to open the solution in Visual Studio.

## Signed Nightly Builds
If you do not want to build the source, nightly builds are available via a private NuGet feed. Nightly builds are meant for developers to try out new features or bug fixes ahead of an official prerelease or final build. We strongly urge you to only use official builds for production.

To use the nightly build:

## In your Package Manager settings add one of the following package sources: 
* Release branch: [url:https://dotnet.myget.org/f/katana-release/]
* Dev branch: [url:https://dotnet.myget.org/f/katana-dev/]
* IdentityModel dependencies: [url:http://www.myget.org/F/azureadwebstacknightly/]

## Contribute
There are lots of ways to [contribute](https://github.com/aspnet/Home/blob/dev/CONTRIBUTING.md) to the project, and we appreciate our contributors.
You can contribute by reviewing and sending feedback on code commits, suggesting and trying out new features as they are implemented, submit bugs and help us verify fixes as they are checked in, as well as submit code fixes or code contributions of your own. Note that all code submissions will be rigorously reviewed and tested by the Katana team, and only those that meet an extremely high bar for both quality and design/roadmap appropriateness will be merged into the source.

## Roadmap
For details on our planned features and future direction, please refer to our [roadmap](https://github.com/aspnet/AspNetKatana/wiki/Roadmap).
