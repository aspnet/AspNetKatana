// <copyright file="DefaultServerFactoryLoader.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Linq;
using System.Reflection;

namespace Microsoft.Owin.Hosting.ServerFactory
{
    public class DefaultServerFactoryLoader : IServerFactoryLoader
    {
        public IServerFactory Load(string serverName)
        {
            // TODO: error message for server assembly not found
            Assembly serverAssembly = Assembly.Load(serverName);

            // TODO: error message for assembly does not have ServerFactory attribute
            Attribute serverFactory = serverAssembly.GetCustomAttributes(false)
                .Cast<Attribute>()
                .Single(x => x.GetType().Name == "OwinServerFactoryAttribute");

            return new ServerFactoryAdapter(serverFactory);
        }
    }
}
