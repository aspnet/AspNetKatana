// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
