// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Web;
using FunctionalTests.Common;
using LTAF.Infrastructure;
using Microsoft.Owin.Extensions;
using Owin;
using Xunit;

namespace FunctionalTests.Facts.IntegratedPipeline
{
    public partial class IntegratedPipelineFacts
    {
        [Fact, Trait("FunctionalTests", "General")]
        public void IntegratedPipelineWithMapMiddleware()
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(HostType.IIS, WithMapMiddleware);
                WebDeployer webDeployer = (WebDeployer)deployer.Application;

                Directory.CreateDirectory(Path.Combine(webDeployer.Application.VirtualDirectories[0].PhysicalPath, "Branch1"));
                Directory.CreateDirectory(Path.Combine(webDeployer.Application.VirtualDirectories[0].PhysicalPath, "Branch2"));

                webDeployer.Application.Deploy("Branch1\\IntegratedPipelineTest.aspx", File.ReadAllText("RequirementFiles\\IntegratedPipelineTest.aspx"));
                webDeployer.Application.Deploy("Branch2\\IntegratedPipelineTest.aspx", File.ReadAllText("RequirementFiles\\IntegratedPipelineTest.aspx"));

                Assert.True(HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "/Branch1/IntegratedPipelineTest.aspx").Contains("1;11"), "Pipeline order incorrect");
                Assert.True(HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "/Branch2/IntegratedPipelineTest.aspx").Contains("1;21"), "Pipeline order incorrect");
            }
        }

        public void WithMapMiddleware(IAppBuilder app)
        {
            app.Use((context, next) =>
            {
                //Create a custom object in the dictionary to push middlewareIds. 
                context.Set<Stack<int>>("stack", new Stack<int>());
                return next.Invoke();
            });

            app.Use<IntegratedPipelineMiddleware>(RequestNotification.AuthenticateRequest, 1);
            app.UseStageMarker(PipelineStage.Authenticate);

            app.Map("/Branch1", branch1 =>
            {
                branch1.Use<IntegratedPipelineMiddleware>(RequestNotification.ResolveRequestCache, 11);
                branch1.UseStageMarker(PipelineStage.ResolveCache);
            });

            app.Map("/Branch2", branch1 =>
            {
                branch1.Use<IntegratedPipelineMiddleware>(RequestNotification.PreExecuteRequestHandler, 21);
                branch1.UseStageMarker(PipelineStage.PreHandlerExecute);
            });
        }
    }
}