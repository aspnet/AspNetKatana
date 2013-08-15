// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
