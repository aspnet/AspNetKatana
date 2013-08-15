// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Owin;

namespace Microsoft.Owin.Hosting.Loader
{
    /// <summary>
    /// Attempts to find the entry point for an app.
    /// </summary>
    public class AppLoader : IAppLoader
    {
        private readonly IEnumerable<IAppLoaderFactory> _providers;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="providers"></param>
        public AppLoader(IEnumerable<IAppLoaderFactory> providers)
        {
            if (providers == null)
            {
                throw new ArgumentNullException("providers");
            }

            _providers = providers;
        }

        /// <summary>
        /// Attempts to find the entry point for a given configuration string.
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public virtual Action<IAppBuilder> Load(string appName, IList<string> errors)
        {
            Func<string, IList<string>, Action<IAppBuilder>> chain = _providers.Aggregate(
                (Func<string, IList<string>, Action<IAppBuilder>>)((arg, arg2) => null),
                (next, provider) => provider.Create(next));

            return chain.Invoke(appName, errors);
        }
    }
}
