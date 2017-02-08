// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Owin.Hosting.Tests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    // Used to test the ServerFactoryLoader auto-discovery
    public class OwinServerFactory
    {
        public void Create(AppFunc appFunc, IDictionary<string, object> properties)
        {
            properties["create.server"] = GetType().FullName;
        }
    }
}
