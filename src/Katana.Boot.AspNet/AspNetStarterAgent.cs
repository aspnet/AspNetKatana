// <copyright file="AspNetStarterAgent.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using System.Web.Hosting;
using System.Xml;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Hosting.Engine;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Utilities;

namespace Katana.Boot.AspNet
{
    public class AspNetStarterAgent : MarshalByRefObject, IRegisteredObject
    {
        private AspNetStarterProxy _proxy;

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Start must not throw")]
        public IDisposable Start(AspNetStarterProxy proxy, StartOptions options)
        {
            _proxy = proxy;
            try
            {
                HostingEnvironment.RegisterObject(this);
            }
            catch
            {
                // Notification not always supported
            }

            var context = StartContext.Create(options);

            IServiceProvider services = ServicesFactory.Create(context.Options.Settings);

            var builderFactory = services.GetService<IAppBuilderFactory>();
            context.Builder = new AppBuilderWrapper(builderFactory.Create());

            IHostingEngine engine = services.GetService<IHostingEngine>();

            return engine.Start(context);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Stop must not throw")]
        public void Stop(bool immediate)
        {
            try
            {
                HostingEnvironment.UnregisterObject(this);
            }
            catch
            {
                // ignored error
            }
            _proxy.StopDomain(immediate);
        }
    }
}
