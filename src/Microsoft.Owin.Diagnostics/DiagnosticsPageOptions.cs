// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if DEBUG
namespace Microsoft.Owin.Diagnostics
{
    /// <summary>
    /// Options for the DiagnosticsPageMiddleware
    /// </summary>
    public class DiagnosticsPageOptions
    {
        /// <summary>
        /// Specifies which requests paths will be responded to. Exact matches only. Leave null to handle all requests.
        /// </summary>
        public PathString Path { get; set; }
    }
}
#endif