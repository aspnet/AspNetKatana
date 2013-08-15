// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
                var addMarker = (Action<IAppBuilder, string>)obj;
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
