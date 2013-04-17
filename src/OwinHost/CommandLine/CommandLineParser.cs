// <copyright file="CommandLineParser.cs" company="Microsoft Open Technologies, Inc.">
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
    public class CommandLineParser
    {
        public CommandLineParser()
        {
            Options = new List<CommandLineOption>();
        }

        public ICollection<CommandLineOption> Options { get; private set; }

        /// <summary>
        /// Parses while the input strings start with '/' or '-'.
        /// </summary>
        /// <param name="args"></param>
        /// <returns>Any un-parsed parameters.</returns>
        public IList<string> Parse(IEnumerable<string> args)
        {
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }

            bool stillParsing = true;
            List<string> extra = new List<string>();

            foreach (string arg in args)
            {
                if (stillParsing)
                {
                    if (IsArgument(arg))
                    {
                        Parse(arg);
                    }
                    else
                    {
                        stillParsing = false;
                        extra.Add(arg);
                    }
                }
                else
                {
                    extra.Add(arg);
                }
            }

            return extra;
        }

        // Returns true if the input starts with '/' or '-'
        private bool IsArgument(string arg)
        {
            if (arg == null)
            {
                throw new ArgumentNullException("arg");
            }
            if (string.IsNullOrWhiteSpace(arg))
            {
                return false;
            }

            // Marker
            if (arg[0] != '/' && arg[0] != '-')
            {
                return false;
            }
            return true;
        }

        // Format: [- or /]paramname=[value or "value"]
        // or [- or /]paramname (e.g. /?)
        private void Parse(string arg)
        {
            string paramName;
            string value;
            int seperatorIndex = arg.IndexOf('=');
            if (seperatorIndex < 0)
            {
                // /param
                paramName = arg.Substring(1);
                value = string.Empty;
            }
            else
            {
                // /param=value
                paramName = arg.Substring(1, seperatorIndex - 1);
                value = arg.Substring(seperatorIndex + 1);
            }

            if (string.IsNullOrWhiteSpace(paramName))
            {
                throw new FormatException("Missing parameter name: " + arg);
            }

            // Remove quotes
            if (value.Length > 1 && value[0] == '\"' && value[value.Length - 1] == '\"')
            {
                value = value.Substring(1, value.Length - 2);
            }

            // Find a matching option
            foreach (CommandLineOption option in Options)
            {
                foreach (string param in option.Parameters)
                {
                    if (param.Equals(paramName, StringComparison.OrdinalIgnoreCase))
                    {
                        option.Action(value);
                        return;
                    }
                }
            }

            throw new FormatException("Unknown parameter: " + paramName);
        }
    }
}
