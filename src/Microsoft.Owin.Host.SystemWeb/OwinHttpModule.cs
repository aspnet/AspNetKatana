// <copyright file="OwinHttpModule.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin.Host.SystemWeb.IntegratedPipeline;
using Owin;
using Owin.Loader;

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

            string configuration = ConfigurationManager.AppSettings[Constants.OwinConfiguration];
            var loader = new DefaultLoader();
            Action<IAppBuilder> startup = loader.Load(configuration ?? string.Empty);
            if (startup == null)
            {
                return null;
            }

            var appContext = OwinBuilder.Build(builder =>
            {
                EnableIntegratedPipeline(builder, stage => firstStage = stage);
                startup.Invoke(builder);
            });

            if (appContext == null)
            {
                return null;
            }

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
