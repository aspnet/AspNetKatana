// <copyright file="IntegratedPipelineBlueprint.cs" company="Microsoft Open Technologies, Inc.">
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
