// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Owin.Hosting.Starter
{
    /// <summary>
    /// This attribute is used to identify custom hosting starters that may be loaded at runtime.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class HostingStarterAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostingStarterType"></param>
        public HostingStarterAttribute(Type hostingStarterType)
        {
            HostingStarterType = hostingStarterType;
        }

        /// <summary>
        /// 
        /// </summary>
        public Type HostingStarterType { get; private set; }
    }
}
