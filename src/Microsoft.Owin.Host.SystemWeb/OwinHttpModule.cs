// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin.Host.SystemWeb.IntegratedPipeline;
using Owin;

namespace Microsoft.Owin.Host.SystemWeb
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    internal sealed class OwinHttpModule : IHttpModule
    {
        private static IntegratedPipelineBlueprint _blueprint;
        private static bool _blueprintInitialized;
        private static object _blueprintLock = new object();

        public void Init(HttpApplication context)
        {
            IntegratedPipelineBlueprint blueprint = LazyInitializer.EnsureInitialized(
                ref _blueprint,
                ref _blueprintInitialized,
                ref _blueprintLock,
                InitializeBlueprint);

            if (blueprint != null)
            {
                var integratedPipelineContext = new IntegratedPipelineContext(blueprint);
                integratedPipelineContext.Initialize(context);
            }
        }

        public void Dispose()
        {
        }

        private IntegratedPipelineBlueprint InitializeBlueprint()
        {
            IntegratedPipelineBlueprintStage firstStage = null;

            Action<IAppBuilder> startup = OwinBuilder.GetAppStartup();
            OwinAppContext appContext = OwinBuilder.Build(builder =>
            {
                EnableIntegratedPipeline(builder, stage => firstStage = stage);
                startup.Invoke(builder);
            });

            string basePath = Utils.NormalizePath(HttpRuntime.AppDomainAppVirtualPath);

            return new IntegratedPipelineBlueprint(appContext, firstStage, basePath);
        }

        private static void EnableIntegratedPipeline(IAppBuilder app, Action<IntegratedPipelineBlueprintStage> onStageCreated)
        {
            var stage = new IntegratedPipelineBlueprintStage { Name = "PreHandlerExecute" };
            onStageCreated(stage);
            Action<IAppBuilder, string> stageMarker = (builder, name) =>
            {
                Func<AppFunc, AppFunc> decoupler = next =>
                {
                    if (string.Equals(name, stage.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        // no decoupling needed when pipeline is already split at this name
                        return next;
                    }
                    if (!IntegratedPipelineContext.VerifyStageOrder(name, stage.Name))
                    {
                        // Stage markers added out of order will be ignored.
                        // Out of order stages/middleware may be run earlier than expected.
                        // TODO: LOG
                        return next;
                    }
                    stage.EntryPoint = next;
                    stage = new IntegratedPipelineBlueprintStage
                    {
                        Name = name,
                        NextStage = stage,
                    };
                    onStageCreated(stage);
                    return (AppFunc)IntegratedPipelineContext.ExitPointInvoked;
                };
                app.Use(decoupler);
            };
            app.Properties[Constants.IntegratedPipelineStageMarker] = stageMarker;
            app.Properties[Constants.BuilderDefaultApp] = (Func<IDictionary<string, object>, Task>)IntegratedPipelineContext.DefaultAppInvoked;
        }
    }
}
