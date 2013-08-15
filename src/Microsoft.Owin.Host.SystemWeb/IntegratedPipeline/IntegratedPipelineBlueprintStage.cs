// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Owin.Host.SystemWeb.IntegratedPipeline
{
    internal class IntegratedPipelineBlueprintStage
    {
        public string Name { get; set; }
        public IntegratedPipelineBlueprintStage NextStage { get; set; }

        public Func<IDictionary<string, object>, Task> EntryPoint { get; set; }
    }
}
