// <copyright file="DirectHostingStarter.cs" company="Microsoft Open Technologies, Inc.">
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
using Microsoft.Owin.Hosting.Engine;

namespace Microsoft.Owin.Hosting.Starter
{
    /// <summary>
    /// Executes the IHostingEngine without making any changes to the current execution environment.
    /// </summary>
    public class DirectHostingStarter : IHostingStarter
    {
        private readonly IHostingEngine _engine;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="engine"></param>
        public DirectHostingStarter(IHostingEngine engine)
        {
            _engine = engine;
        }

        /// <summary>
        /// Executes the IHostingEngine without making any changes to the current execution environment.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public virtual IDisposable Start(StartOptions options)
        {
            return _engine.Start(new StartContext(options));
        }
    }
}
