// <copyright file="OwinOpaqueParameters.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Owin.Types.Opaque
{
#region OwinOpaqueParameters

    internal partial struct OwinOpaqueParameters
    {
        public static OwinOpaqueParameters Create()
        {
            return new OwinOpaqueParameters(new ConcurrentDictionary<string, object>(StringComparer.Ordinal));
        }
    }
#endregion

#region OwinOpaqueParameters.Generated

    [System.CodeDom.Compiler.GeneratedCode("App_Packages", "")]
    internal partial struct OwinOpaqueParameters
    {
        private readonly IDictionary<string, object> _dictionary;

        public OwinOpaqueParameters(IDictionary<string, object> dictionary)
        {
            _dictionary = dictionary;
        }

        public IDictionary<string, object> Dictionary
        {
            get { return _dictionary; }
        }

#region Value-type equality
        public bool Equals(OwinOpaqueParameters other)
        {
            return Equals(_dictionary, other._dictionary);
        }

        public override bool Equals(object obj)
        {
            return obj is OwinOpaqueParameters && Equals((OwinOpaqueParameters)obj);
        }

        public override int GetHashCode()
        {
            return (_dictionary != null ? _dictionary.GetHashCode() : 0);
        }

        public static bool operator ==(OwinOpaqueParameters left, OwinOpaqueParameters right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(OwinOpaqueParameters left, OwinOpaqueParameters right)
        {
            return !left.Equals(right);
        }
#endregion

        public T Get<T>(string key)
        {
            object value;
            return _dictionary.TryGetValue(key, out value) ? (T)value : default(T);
        }

        public OwinOpaqueParameters Set(string key, object value)
        {
            _dictionary[key] = value;
            return this;
        }

    }
#endregion

}
