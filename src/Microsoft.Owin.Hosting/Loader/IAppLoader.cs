// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Owin;

namespace Microsoft.Owin.Hosting.Loader
{
    /// <summary>
    /// Attempts to find the entry point for an app.
    /// </summary>
    public interface IAppLoader
    {
        /// <summary>
        /// Attempts to find the entry point for a given configuration string.
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        Action<IAppBuilder> Load(string appName, IList<string> errors);
    }
}
