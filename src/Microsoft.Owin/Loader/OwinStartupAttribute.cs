// <copyright file="OwinStartupAttribute.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2013 Microsoft Open Technologies, Inc. All rights reserved.
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

using System;

namespace Microsoft.Owin
{
    /// <summary>
    /// Used to mark which class in an assembly should be used for automatic startup.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class OwinStartupAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="startupType">The startup class</param>
        public OwinStartupAttribute(Type startupType)
            : this(string.Empty, startupType, string.Empty)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="friendlyName">A non-default configuration, e.g. staging.</param>
        /// <param name="startupType">The startup class</param>
        public OwinStartupAttribute(string friendlyName, Type startupType)
            : this(friendlyName, startupType, string.Empty)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startupType">The startup class</param>
        /// <param name="methodName">Specifies which method to call</param>
        public OwinStartupAttribute(Type startupType, string methodName)
            : this(string.Empty, startupType, methodName)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="friendlyName">A non-default configuration, e.g. staging.</param>
        /// <param name="startupType">The startup class</param>
        /// <param name="methodName">Specifies which method to call</param>
        public OwinStartupAttribute(string friendlyName, Type startupType, string methodName)
        {
            if (friendlyName == null)
            {
                throw new ArgumentNullException("friendlyName");
            }
            if (startupType == null)
            {
                throw new ArgumentNullException("startupType");
            }
            if (methodName == null)
            {
                throw new ArgumentNullException("methodName");
            }

            FriendlyName = friendlyName;
            StartupType = startupType;
            MethodName = methodName;
        }

        /// <summary>
        /// A non-default configuration if any. e.g. Staging.
        /// </summary>
        public string FriendlyName
        {
            get;
            private set;
        }

        /// <summary>
        /// The startup class
        /// </summary>
        public Type StartupType
        {
            get; private set;
        }

        /// <summary>
        /// The name of the configuration method
        /// </summary>
        public string MethodName
        {
            get; private set;
        }
    }
}
