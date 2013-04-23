// <copyright file="AppBuilderExtensions.cs" company="Microsoft Open Technologies, Inc.">
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
using Owin;

namespace Microsoft.Owin.Extensions
{
    public static class AppBuilderExtensions
    {
        public static void AddSignatureConversion(this IAppBuilder builder, Delegate conversion)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            object obj;
            if (!builder.Properties.TryGetValue("builder.AddSignatureConversion", out obj) || !(obj is Action<Delegate>))
            {
                throw new MissingMethodException(builder.GetType().FullName, "AddSignatureConversion");
            }
            ((Action<Delegate>)obj)(conversion);
        }

        public static void AddSignatureConversion<T1, T2>(this IAppBuilder builder, Func<T1, T2> conversion)
        {
            AddSignatureConversion(builder, (Delegate)conversion);
        }
    }
}
