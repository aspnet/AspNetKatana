// Licensed under one or more contributor license agreements.  
// See the NOTICE.txt file distributed with this work for 
// additional information regarding copyright ownership.  The 
// copyright owners license this file to you under the Apache 
// License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain 
// a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    internal static partial class StartupExtensions
    {
        public static IAppBuilder UseType<TMiddleware>(this IAppBuilder builder, params object[] args)
        {
            return builder.Use(typeof(TMiddleware), args);
        }

        public static IAppBuilder UseType(this IAppBuilder builder, Type type, params object[] args)
        {
            return builder.Use(type, args);
        }
    }
}
