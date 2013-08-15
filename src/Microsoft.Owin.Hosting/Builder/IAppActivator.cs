// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.Owin.Hosting.Builder
{
    /// <summary>
    /// Used to instantiate the application entry point. e.g. the Startup class.
    /// </summary>
    public interface IAppActivator
    {
        /// <summary>
        /// Instantiate an instance of the given type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        object Activate(Type type);
    }
}
