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

    public class StartContext
    {
        private StartContext()
        {
        }

        public StartOptions Options { get; private set; }

        public IServerFactoryAdapter ServerFactory { get; set; }

        public IAppBuilder Builder { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public AppFunc App { get; set; }

        public Action<IAppBuilder> Startup { get; set; }

        public TextWriter Output { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public IList<KeyValuePair<string, object>> EnvironmentData { get; private set; }

        public static StartContext Create(StartOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            if (options.Settings == null)
            {
                options.Settings = SettingsLoader.LoadFromConfig();
            }
            return new StartContext
            {
                Options = options,
                EnvironmentData = new List<KeyValuePair<string, object>>()
            };
        }
    }
}
