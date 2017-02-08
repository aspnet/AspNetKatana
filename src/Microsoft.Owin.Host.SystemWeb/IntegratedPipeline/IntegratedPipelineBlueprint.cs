// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Owin.Host.SystemWeb.IntegratedPipeline
{
    internal class IntegratedPipelineBlueprint
    {
        private readonly OwinAppContext _appContext;
        private readonly IntegratedPipelineBlueprintStage _firstStage;
        private readonly string _pathBase;

        public IntegratedPipelineBlueprint(
            OwinAppContext appContext,
            IntegratedPipelineBlueprintStage firstStage,
            string pathBase)
        {
            _appContext = appContext;
            _firstStage = firstStage;
            _pathBase = pathBase;
        }

        public OwinAppContext AppContext
        {
            get { return _appContext; }
        }

        public IntegratedPipelineBlueprintStage FirstStage
        {
            get { return _firstStage; }
        }

        public string PathBase
        {
            get { return _pathBase; }
        }
    }
}
