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
        private const string StageAuthenticate = "Authenticate";
        private const string StagePostAuthenticate = "PostAuthenticate";
        private const string StageAuthorize = "Authorize";
        private const string StagePostAuthorize = "PostAuthorize";
        private const string StageResolveCache = "ResolveCache";
        private const string StagePostResolveCache = "PostResolveCache";
        private const string StageMapHandler = "MapHandler";
        private const string StagePostMapHandler = "PostMapHandler";
        private const string StageAcquireState = "AcquireState";
        private const string StagePostAcquireState = "PostAcquireState";
        private const string StagePreHandlerExecute = "PreHandlerExecute";

        /// <summary>
        /// Call after other middleware to specify when they should run in the integrated pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="stageName">The name of the integrated pipeline in which to run.</param>
        /// <returns>The original IAppBuilder for chaining.</returns>
        public static IAppBuilder UseStageMarker(this IAppBuilder app, string stageName)
        {
            object obj;
            if (app.Properties.TryGetValue(IntegratedPipelineStageMarker, out obj))
            {
                Action<IAppBuilder, string> addMarker = (Action<IAppBuilder, string>)obj;
                addMarker(app, stageName);
            }
            return app;
        }

        /// <summary>
        /// Call after other middleware to specify that they should run in the Authenticate stage or earlier.
        /// </summary>
        /// <param name="app"></param>
        /// <returns>The original IAppBuilder for chaining.</returns>
        public static IAppBuilder UseStageMarkerAuthenticate(this IAppBuilder app)
        {
            return app.UseStageMarker(StageAuthenticate);
        }

        /// <summary>
        /// Call after other middleware to specify that they should run in the PostAuthenticate stage or earlier.
        /// </summary>
        /// <param name="app"></param>
        /// <returns>The original IAppBuilder for chaining.</returns>
        public static IAppBuilder UseStageMarkerPostAuthenticate(this IAppBuilder app)
        {
            return app.UseStageMarker(StagePostAuthenticate);
        }

        /// <summary>
        /// Call after other middleware to specify that they should run in the Authorize stage or earlier.
        /// </summary>
        /// <param name="app"></param>
        /// <returns>The original IAppBuilder for chaining.</returns>
        public static IAppBuilder UseStageMarkerAuthorize(this IAppBuilder app)
        {
            return app.UseStageMarker(StageAuthorize);
        }

        /// <summary>
        /// Call after other middleware to specify that they should run in the PostAuthorize stage or earlier.
        /// </summary>
        /// <param name="app"></param>
        /// <returns>The original IAppBuilder for chaining.</returns>
        public static IAppBuilder UseStageMarkerPostAuthorize(this IAppBuilder app)
        {
            return app.UseStageMarker(StagePostAuthorize);
        }

        /// <summary>
        /// Call after other middleware to specify that they should run in the ResolveCache stage or earlier.
        /// </summary>
        /// <param name="app"></param>
        /// <returns>The original IAppBuilder for chaining.</returns>
        public static IAppBuilder UseStageMarkerResolveCache(this IAppBuilder app)
        {
            return app.UseStageMarker(StageResolveCache);
        }

        /// <summary>
        /// Call after other middleware to specify that they should run in the PostResolveCache stage or earlier.
        /// </summary>
        /// <param name="app"></param>
        /// <returns>The original IAppBuilder for chaining.</returns>
        public static IAppBuilder UseStageMarkerPostResolveCache(this IAppBuilder app)
        {
            return app.UseStageMarker(StagePostResolveCache);
        }

        /// <summary>
        /// Call after other middleware to specify that they should run in the MapHandler stage or earlier.
        /// </summary>
        /// <param name="app"></param>
        /// <returns>The original IAppBuilder for chaining.</returns>
        public static IAppBuilder UseStageMarkerMapHandler(this IAppBuilder app)
        {
            return app.UseStageMarker(StageMapHandler);
        }

        /// <summary>
        /// Call after other middleware to specify that they should run in the PostMapHandler stage or earlier.
        /// </summary>
        /// <param name="app"></param>
        /// <returns>The original IAppBuilder for chaining.</returns>
        public static IAppBuilder UseStageMarkerPostMapHandler(this IAppBuilder app)
        {
            return app.UseStageMarker(StagePostMapHandler);
        }

        /// <summary>
        /// Call after other middleware to specify that they should run in the AcquireState stage or earlier.
        /// </summary>
        /// <param name="app"></param>
        /// <returns>The original IAppBuilder for chaining.</returns>
        public static IAppBuilder UseStageMarkerAcquireState(this IAppBuilder app)
        {
            return app.UseStageMarker(StageAcquireState);
        }

        /// <summary>
        /// Call after other middleware to specify that they should run in the PostAcquireState stage or earlier.
        /// </summary>
        /// <param name="app"></param>
        /// <returns>The original IAppBuilder for chaining.</returns>
        public static IAppBuilder UseStageMarkerPostAcquireState(this IAppBuilder app)
        {
            return app.UseStageMarker(StagePostAcquireState);
        }

        /// <summary>
        /// Call after other middleware to specify that they should run in the PreHandlerExecute stage or earlier.
        /// </summary>
        /// <param name="app"></param>
        /// <returns>The original IAppBuilder for chaining.</returns>
        public static IAppBuilder UseStageMarkerPreHandlerExecute(this IAppBuilder app)
        {
            return app.UseStageMarker(StagePreHandlerExecute);
        }
    }
}
