// <copyright file="StartContext.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
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

using System.Collections.Generic;
using System.IO;
using Owin;

namespace Microsoft.Owin.Hosting
{
    public class StartContext
    {
        public StartContext()
        {
            Parameters = new StartParameters();
            EnvironmentData = new List<KeyValuePair<string, object>>();
        }

        public StartParameters Parameters { get; set; }

        public object ServerFactory { get; set; }
        public IAppBuilder Builder { get; set; }
        public object App { get; set; }
        public TextWriter Output { get; set; }
        public IList<KeyValuePair<string, object>> EnvironmentData { get; set; }
    }
}
