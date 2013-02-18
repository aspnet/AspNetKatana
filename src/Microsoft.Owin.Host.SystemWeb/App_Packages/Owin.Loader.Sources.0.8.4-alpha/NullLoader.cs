// <copyright file="NullLoader.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Diagnostics.CodeAnalysis;

namespace Owin.Loader
{
    // <summary>
    // A default fallback loader that does nothing.
    // </summary>
    internal class NullLoader
    {
        private static readonly NullLoader Singleton = new NullLoader();

        // <summary>
        // A singleton instance of the NullLoader type.
        // </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static Func<string, Action<IAppBuilder>> Instance
        {
            get { return Singleton.Load; }
        }

        // <summary>
        // A placeholder method that always returns null.
        // </summary>
        // <param name="startup"></param>
        // <returns>null.</returns>
        public Action<IAppBuilder> Load(string startup)
        {
            return null;
        }
    }
}
