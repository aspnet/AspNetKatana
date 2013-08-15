// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.Hosting.Loader;
using Microsoft.Owin.Hosting.Services;

namespace Microsoft.Owin.Hosting.Tests.Containers
{
    public class DefaultServicesTests : ContainerTestsBase
    {
        public override Func<Type, object> CreateContainer()
        {
            IServiceProvider services = ServicesFactory.Create(reg => reg
                .Add<IAppLoaderFactory, TestAppLoader1>()
                .Add<IAppLoaderFactory, TestAppLoader2>());
            return services.GetService;
        }
    }
}
