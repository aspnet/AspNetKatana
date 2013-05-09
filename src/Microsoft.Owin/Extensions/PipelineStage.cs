// <copyright file="PipelineStage.cs" company="Microsoft Open Technologies, Inc.">
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

namespace Owin
{
    /// <summary>
    /// An ordered list of known Asp.Net integrated pipeline stages.
    /// </summary>
    public enum PipelineStage
    {
        /// <summary>
        /// 
        /// </summary>
        Authenticate,

        /// <summary>
        /// 
        /// </summary>
        PostAuthenticate,

        /// <summary>
        /// 
        /// </summary>
        Authorize,

        /// <summary>
        /// 
        /// </summary>
        PostAuthorize,

        /// <summary>
        /// 
        /// </summary>
        ResolveCache,

        /// <summary>
        /// 
        /// </summary>
        PostResolveCache,

        /// <summary>
        /// 
        /// </summary>
        MapHandler,

        /// <summary>
        /// 
        /// </summary>
        PostMapHandler,

        /// <summary>
        /// 
        /// </summary>
        AcquireState,

        /// <summary>
        /// 
        /// </summary>
        PostAcquireState,

        /// <summary>
        /// 
        /// </summary>
        PreHandlerExecute,
    }
}
