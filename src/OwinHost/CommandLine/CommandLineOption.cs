// <copyright file="CommandLineOption.cs" company="Microsoft Open Technologies, Inc.">
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

namespace OwinHost.CommandLine
{
    public class CommandLineOption
    {
        public CommandLineOption(string[] parameters, string description, Action<string> action)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
            if (description == null)
            {
                throw new ArgumentNullException("description");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            Parameters = parameters;
            Description = description;
            Action = action;
        }

        public IEnumerable<string> Parameters { get; private set; }

        public string Description { get; private set; }

        public Action<string> Action { get; private set; }
    }
}
