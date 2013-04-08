// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;

namespace Owin
{
    internal static class AppBuilderExtensions
    {
        public static void MarkStage(this IAppBuilder appBuilder, string name)
        {
            Contract.Assert(appBuilder != null);
            Action<IAppBuilder, string> stageMarker = GetStageMarker(appBuilder);

            if (stageMarker != null)
            {
                stageMarker.Invoke(appBuilder, name);
            }
        }

        private static Action<IAppBuilder, string> GetStageMarker(this IAppBuilder appBuilder)
        {
            Contract.Assert(appBuilder != null);
            object value;

            if (!appBuilder.Properties.TryGetValue("integratedpipeline.StageMarker", out value))
            {
                return null;
            }

            return value as Action<IAppBuilder, string>;
        }
    }
}
