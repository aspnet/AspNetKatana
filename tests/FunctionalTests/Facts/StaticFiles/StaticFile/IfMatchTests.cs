// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using FunctionalTests.Common;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.StaticFiles
{
    public class IfMatchTests
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        public void Static_IfMatch(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                var applicationUrl = deployer.Deploy(hostType, IfMatchConfiguration);
                var httpClient = new HttpClient() { BaseAddress = new Uri(applicationUrl) };

                //Initial request to get the E-tag
                var response = httpClient.GetAsync("RequirementFiles/Dir1/Default.html").Result;
                httpClient.DefaultRequestHeaders.IfMatch.Add(response.Headers.ETag);

                //Add the E-tag to the successive request to the same entity. Expect the body is fully fetched again. 
                for (int count = 0; count < 10; count++)
                {
                    response = httpClient.GetAsync("RequirementFiles/Dir1/Default.html").Result;
                    Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                }

                //Modify the file to see if the body is fetched fully again
                string fileContent = File.ReadAllText(@"RequirementFiles/Dir1/Default.html");
                File.WriteAllText(@"RequirementFiles/Dir1/Default.html", fileContent);
                //Sometimes the test is flaky returning OK status code as the file change is not immediately recognized by the server. So give it a while.
                Thread.Sleep(500);

                //If the E-tag sent failed to match the entity's etag, then expect a Precondition failed 412. 
                response = httpClient.GetAsync("RequirementFiles/Dir1/Default.html").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.PreconditionFailed, response.StatusCode);

                //Duplicate 4x e-tags
                httpClient.DefaultRequestHeaders.IfMatch.Clear();
                response = httpClient.GetAsync("RequirementFiles/Dir1/Default.html").Result;
                httpClient.DefaultRequestHeaders.IfMatch.Add(response.Headers.ETag);
                httpClient.DefaultRequestHeaders.IfMatch.Add(response.Headers.ETag);
                httpClient.DefaultRequestHeaders.IfMatch.Add(response.Headers.ETag);
                httpClient.DefaultRequestHeaders.IfMatch.Add(response.Headers.ETag);
                response = httpClient.GetAsync("RequirementFiles/Dir1/Default.html").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);

                //Etag=* should always get a 304. 
                httpClient.DefaultRequestHeaders.IfMatch.Clear();
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("If-Match", "*");
                response = httpClient.GetAsync("RequirementFiles/Dir1/Default.html").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
            }
        }

        internal void IfMatchConfiguration(IAppBuilder app)
        {
            app.UseStaticFiles();
        }
    }
}
