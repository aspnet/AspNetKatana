// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using FunctionalTests.Common;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.StaticFiles
{
    public class IfNoneMatchTests
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        public void Static_IfNoneMatch(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                var applicationUrl = deployer.Deploy(hostType, IfNoneMatchConfiguration);
                var httpClient = new HttpClient() { BaseAddress = new Uri(applicationUrl) };

                //Initial request to get the E-tag
                var response = httpClient.GetAsync("RequirementFiles/Dir1/Default.html").Result;
                httpClient.DefaultRequestHeaders.IfNoneMatch.Add(response.Headers.ETag);

                //Add the E-tag to the successive request to the same entity. Expect a Not modified 304.
                for (int count = 0; count < 10; count++)
                {
                    response = httpClient.GetAsync("RequirementFiles/Dir1/Default.html").Result;
                    Assert.Equal<HttpStatusCode>(HttpStatusCode.NotModified, response.StatusCode);
                }

                //Modify the file to see if the body is fetched fully again
                string fileContent = File.ReadAllText(@"RequirementFiles/Dir1/Default.html");
                File.WriteAllText(@"RequirementFiles/Dir1/Default.html", fileContent);

                //Verify if the fully body is sent after the file modification
                response = httpClient.GetAsync("RequirementFiles/Dir1/Default.html").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);

                //Add a non-matching E-tag to see if the body is fetched again. 
                httpClient.DefaultRequestHeaders.IfNoneMatch.Clear();
                httpClient.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue("\"etag1\""));
                response = httpClient.GetAsync("RequirementFiles/Dir1/Default.html").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);

                //Duplicate 4x e-tags
                httpClient.DefaultRequestHeaders.IfNoneMatch.Add(response.Headers.ETag);
                httpClient.DefaultRequestHeaders.IfNoneMatch.Add(response.Headers.ETag);
                httpClient.DefaultRequestHeaders.IfNoneMatch.Add(response.Headers.ETag);
                httpClient.DefaultRequestHeaders.IfNoneMatch.Add(response.Headers.ETag);
                response = httpClient.GetAsync("RequirementFiles/Dir1/Default.html").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.NotModified, response.StatusCode);

                //Etag=* should always get a 304. 
                httpClient.DefaultRequestHeaders.IfNoneMatch.Clear();
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("If-None-Match", "*");
                response = httpClient.GetAsync("RequirementFiles/Dir1/Default.html").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.NotModified, response.StatusCode);
            }
        }

        public void IfNoneMatchConfiguration(IAppBuilder app)
        {
            app.UseStaticFiles();
        }
    }
}
