// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Owin.Host.HttpListener.Tests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ServerFactoryTests
    {
        private readonly AppFunc _notImplemented = env => { throw new NotImplementedException(); };

        [Fact]
        public void InitializeNullProperties_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => OwinServerFactory.Initialize(null));
        }

        [Fact]
        public void Initialize_PopulatesExpectedFields()
        {
            var properties = new Dictionary<string, object>();
            OwinServerFactory.Initialize(properties);

            Assert.Equal("1.0", properties["owin.Version"]);
            Assert.IsType<OwinHttpListener>(properties["Microsoft.Owin.Host.HttpListener.OwinHttpListener"]);
            Assert.IsType<System.Net.HttpListener>(properties["System.Net.HttpListener"]);
        }

        [Fact]
        public void CreateNullAppFunc_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => OwinServerFactory.Create(null, new Dictionary<string, object>()));
        }

        [Fact]
        public void CreateNullProperties_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => OwinServerFactory.Create(_notImplemented, null));
        }

        [Fact]
        public void CreateEmptyProperties_Success()
        {
            OwinServerFactory.Create(_notImplemented, new Dictionary<string, object>());
        }
    }
}
