// <copyright file="StartContext.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting.ServerFactory;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Utilities;
using Owin;

namespace Microsoft.Owin.Hosting.Engine
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// This class contains the relevant application and server state during startup.
    /// </summary>
    public class StartContext
    {
        /// <summary>
        /// Create a new StartContext with the given options.
        /// If the given options do not define any settings, then settings will be loaded from the config.
        /// </summary>
        /// <param name="options"></param>
        public StartContext(StartOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            SettingsLoader.LoadFromConfig(options.Settings);
            Options = options;
            EnvironmentData = new List<KeyValuePair<string, object>>();
        }

        /// <summary>
        /// The initial options provided to the constructor.
        /// </summary>
        public StartOptions Options { get; private set; }

        /// <summary>
        /// The factory used to instantiate the server.
        /// </summary>
        public IServerFactoryAdapter ServerFactory { get; set; }

        /// <summary>
        /// The IAppBuilder used to construct the OWIN application pipeline.
        /// </summary>
        public IAppBuilder Builder { get; set; }

        /// <summary>
        /// The constructed OWIN application pipeline.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public AppFunc App { get; set; }

        /// <summary>
        /// The application entry point where the pipeline is defined.
        /// </summary>
        public Action<IAppBuilder> Startup { get; set; }

        /// <summary>
        /// A TextWriter for writing diagnostic data to.
        /// </summary>
        public TextWriter TraceOutput { get; set; }

        /// <summary>
        /// A list of keys and their associated values that will be injected by the host into each OWIN request environment.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public IList<KeyValuePair<string, object>> EnvironmentData { get; private set; }
    }
}
