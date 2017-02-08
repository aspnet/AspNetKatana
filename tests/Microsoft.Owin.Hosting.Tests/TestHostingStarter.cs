// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.Hosting.Engine;
using Microsoft.Owin.Hosting.Starter;
using Microsoft.Owin.Hosting.Tests;

[assembly: HostingStarter(typeof(TestHostingStarter))]

namespace Microsoft.Owin.Hosting.Tests
{
    public class TestHostingStarter : IHostingStarter
    {
        private readonly IHostingEngine _engine;

        public TestHostingStarter(IHostingEngine engine)
        {
            _engine = engine;
        }

        public IDisposable Start(StartOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
