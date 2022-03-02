// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FunctionalTests.Common;
using Owin;
using Xunit;
using Xunit.Extensions;
using SendFileFunc = System.Func<string, long, long?, System.Threading.CancellationToken, System.Threading.Tasks.Task>;

namespace FunctionalTests.Facts.StaticFiles
{
    public class CustomSendFileFunc
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        public void Static_CustomSendFileFunc(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                var applicationUrl = deployer.Deploy(hostType, CustomSendFileFuncConfiguration);
                var httpClient = new HttpClient() { BaseAddress = new Uri(applicationUrl) };

                var response = httpClient.GetAsync("RequirementFiles/Dir1/Default.html").Result;
                Assert.Equal("MyCustomSendFileAsync", response.Content.ReadAsStringAsync().Result);
            }
        }

        internal void CustomSendFileFuncConfiguration(IAppBuilder app)
        {
            app.Use((context, next) =>
                {
                    Func<string, long, long?, CancellationToken, Task> sendFileFunc = (fileName, offset, count, cancel) =>
                        {
                            context.Response.ContentLength = "MyCustomSendFileAsync".Length;
                            context.Response.WriteAsync("MyCustomSendFileAsync");
                            return Task.Factory.StartNew(() => { });
                        };

                    context.Set<SendFileFunc>("sendfile.SendAsync", sendFileFunc);
                    return next.Invoke();
                });

            app.UseStaticFiles();
        }
    }
}
