// <copyright file="IntegratedPipelineExtensions.cs" company="Microsoft Open Technologies, Inc.">
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
using Owin;

namespace Microsoft.Owin.Extensions
{
    /// <summary>
    /// Extension methods used to indicate at which stage in the integrated pipeline prior middleware should run.
    /// </summary>
    public static class IntegratedPipelineExtensions
    {
        private const string IntegratedPipelineStageMarker = "integratedpipeline.StageMarker";

        /// <summary>
        /// Call after other middleware to specify when they should run in the integrated pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="stageName">The name of the integrated pipeline in which to run.</param>
        /// <returns>The original IAppBuilder for chaining.</returns>
        public static IAppBuilder UseStageMarker(this IAppBuilder app, string stageName)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }

            object obj;
            if (app.Properties.TryGetValue(IntegratedPipelineStageMarker, out obj))
            {
                Action<IAppBuilder, string> addMarker = (Action<IAppBuilder, string>)obj;
                addMarker(app, stageName);
            }
            return app;
        }

        /// <summary>
        /// Call after other middleware to specify when they should run in the integrated pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="stage">The stage of the integrated pipeline in which to run.</param>
        /// <returns>The original IAppBuilder for chaining.</returns>
        public static IAppBuilder UseStageMarker(this IAppBuilder app, PipelineStage stage)
        {
            return UseStageMarker(app, stage.ToString());
        }
    }
}
