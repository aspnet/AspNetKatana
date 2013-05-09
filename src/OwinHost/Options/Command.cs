// <copyright file="Command.cs" company="Microsoft Open Technologies, Inc.">
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

namespace OwinHost.Options
{
    public class Command
    {
        private readonly IDictionary<Type, object> _properties = new Dictionary<Type, object>();

        public Command()
        {
            Parameters = new List<string>();
        }

        public IList<string> Parameters { get; private set; }

        public CommandModel Model { get; set; }

        public T Get<T>() where T : new()
        {
            object value;
            if (!_properties.TryGetValue(typeof(T), out value))
            {
                value = new T();
                _properties[typeof(T)] = value;
            }
            return (T)value;
        }

        public void Set<T>(T value)
        {
            _properties[typeof(T)] = value;
        }

        public bool Run()
        {
            if (Model != null && Model.Run != null)
            {
                Model.Run(this);
                return true;
            }
            return false;
        }
    }
}
