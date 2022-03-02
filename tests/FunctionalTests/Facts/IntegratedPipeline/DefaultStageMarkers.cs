// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Web;
using FunctionalTests.Common;
using LTAF.Infrastructure;
using Owin;
using Xunit;

namespace FunctionalTests.Facts.IntegratedPipeline
{
    public class DefaultStageMarkers
    {
        [Fact, Trait("FunctionalTests", "General")]
        public void DefaultStageMarkersTest()
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(HostType.IIS, DefaultStageMarkersTestConfiguration);
                ((WebDeployer)deployer.Application).Application.Deploy("IntegratedPipelineTest.aspx", File.ReadAllText("RequirementFiles\\IntegratedPipelineTest.aspx"));

                string responseText = HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "/IntegratedPipelineTest.aspx");
                Assert.True(responseText.Contains("IntegratedPipelineTest"), "IntegratedPipelineTest.aspx not returned");
                Assert.True(responseText.Contains("0;1;2;3;4;5;6;7;8;9"), "Pipeline order incorrect");
            }
        }

        internal void DefaultStageMarkersTestConfiguration(IAppBuilder app)
        {
            app.Use((context, next) =>
            {
                //Create a custom object in the dictionary to push middlewareIds. 
                context.Set<Stack<int>>("stack", new Stack<int>());
                return next.Invoke();
            });

            for (int middlewareId = 0; middlewareId < 10; middlewareId++)
            {
                app.Use<IntegratedPipelineMiddleware>(RequestNotification.PreExecuteRequestHandler, middlewareId);
            }
        }
    }
}
