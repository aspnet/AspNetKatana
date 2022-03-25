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
    public partial class MiddlewaresAtDifferentStagesTest
    {
        [Fact, Trait("FunctionalTests", "General")]
        public void MiddlewaresAtDifferentStages()
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(HostType.IIS, MiddlewaresAtDifferentStagesConfiguration);
                WebDeployer webDeployer = (WebDeployer)deployer.Application;
                webDeployer.Application.Deploy("IntegratedPipelineTest.aspx", File.ReadAllText("RequirementFiles\\IntegratedPipelineTest.aspx"));

                string responseText = HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "/IntegratedPipelineTest.aspx");
                Assert.True(responseText.Contains("IntegratedPipelineTest"), "IntegratedPipelineTest.aspx not returned");
                Assert.True(responseText.Contains("0;1;2;3;4;5;6;7;8;9;10"), "Pipeline order incorrect");
            }
        }

        internal void MiddlewaresAtDifferentStagesConfiguration(IAppBuilder app)
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
                            new Tuple<RequestNotification, PipelineStage>(RequestNotification.AuthenticateRequest, PipelineStage.PostAuthenticate),
                            new Tuple<RequestNotification, PipelineStage>(RequestNotification.AuthorizeRequest, PipelineStage.Authorize),
                            new Tuple<RequestNotification, PipelineStage>(RequestNotification.AuthorizeRequest, PipelineStage.PostAuthorize),
                            new Tuple<RequestNotification, PipelineStage>(RequestNotification.ResolveRequestCache, PipelineStage.ResolveCache),
                            new Tuple<RequestNotification, PipelineStage>(RequestNotification.ResolveRequestCache, PipelineStage.PostResolveCache),
                            new Tuple<RequestNotification, PipelineStage>(RequestNotification.MapRequestHandler, PipelineStage.MapHandler),
                            new Tuple<RequestNotification, PipelineStage>(RequestNotification.MapRequestHandler, PipelineStage.PostMapHandler),
                            new Tuple<RequestNotification, PipelineStage>(RequestNotification.AcquireRequestState, PipelineStage.AcquireState),
                            new Tuple<RequestNotification, PipelineStage>(RequestNotification.AcquireRequestState, PipelineStage.PostAcquireState),
                            new Tuple<RequestNotification, PipelineStage>(RequestNotification.PreExecuteRequestHandler, PipelineStage.PreHandlerExecute),
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