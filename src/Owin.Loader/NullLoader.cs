// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Owin.Loader
{
    /// <summary>
    /// A default fallback loader that does nothing.
    /// </summary>
    internal class NullLoader
    {
        private static readonly NullLoader Singleton = new NullLoader();

        /// <summary>
        /// A singleton instance of the NullLoader type.
        /// </summary>
        public static Func<string, IList<string>, Action<IAppBuilder>> Instance
        {
            get { return Singleton.Load; }
        }

        /// <summary>
        /// A placeholder method that always returns null.
        /// </summary>
        /// <param name="startup"></param>
        /// <param name="errors"></param>
        /// <returns>null.</returns>
        public Action<IAppBuilder> Load(string startup, IList<string> errors)
        {
            return null;
        }
    }
}
