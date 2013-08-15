// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Owin.Hosting.Tests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    // Used for the DefaultServerFactoryLoaderTests discovery
    public class InstanceServerFactory
    {
        public void Create(AppFunc appFunc, IDictionary<string, object> properties)
        {
            properties["create.server"] = GetType().FullName;
        }
    }
}
