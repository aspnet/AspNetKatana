// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Owin;

namespace Microsoft.Owin.Hosting.Loader
{
    using AppLoaderFunc = Func<string, IList<string>, Action<IAppBuilder>>;

    /// <summary>
    /// Initializes a new app loader.
    /// </summary>
    public interface IAppLoaderFactory
    {
        /// <summary>
        /// Not currently used.
        /// </summary>
        int Order { get; }

        /// <summary>
        /// Create a new chained app loader.
        /// </summary>
        /// <param name="nextLoader"></param>
        /// <returns></returns>
        AppLoaderFunc Create(AppLoaderFunc nextLoader);
    }
}
