// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
    public class OrderOfExecutionTest
    {
        [Fact, Trait("FunctionalTests", "General")]
        public void OrderOfExecution()
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(HostType.IIS, OrderOfExecutionConfiguration);
                WebDeployer webDeployer = (WebDeployer)deployer.Application;
                webDeployer.Application.Deploy("IntegratedPipelineTest.aspx", File.ReadAllText("RequirementFiles\\IntegratedPipelineTest.aspx"));

                string responseText = HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "/IntegratedPipelineTest.aspx");
                Assert.True(responseText.Contains("IntegratedPipelineTest"), "IntegratedPipelineTest.aspx not returned");
                Assert.True(responseText.Contains("0;1;2;3"), "Pipeline order incorrect");
            }
        }

        internal void OrderOfExecutionConfiguration(IAppBuilder app)
        {
            app.Use((context, next) =>
            {
                //Create a custom object in the dictionary to push middlewareIds. 
                context.Set<Stack<int>>("stack", new Stack<int>());
                return next.Invoke();
            });

            var stageTuples = new Tuple<RequestNotification, PipelineStage>[]
            {
                new Tuple<RequestNotification, PipelineStage>(RequestNotification.AuthenticateRequest, PipelineStage.Authenticate),
                new Tuple<RequestNotification, PipelineStage>(RequestNotification.AuthenticateRequest, PipelineStage.Authorize),
                new Tuple<RequestNotification, PipelineStage>(RequestNotification.AuthenticateRequest, PipelineStage.Authenticate),
                new Tuple<RequestNotification, PipelineStage>(RequestNotification.AuthorizeRequest, PipelineStage.Authorize)
            };

            for (int middlewareId = 0; middlewareId < stageTuples.Length; middlewareId++)
            {
                var stage = stageTuples[middlewareId];
                app.Use<IntegratedPipelineMiddleware>(stage.Item1, middlewareId);
                app.UseStageMarker(stage.Item2);
            }
        }
    }
}
