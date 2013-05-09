// <copyright file="CommandExecutor.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Linq;

namespace OwinHost.Options
{
    public class CommandExecutor
    {
        public bool Execute(Command command, IEnumerable<string> args)
        {
            Parse(command, args);
            if (command.Model.Run != null)
            {
                command.Model.Run(command);
                return true;
            }
            return false;
        }

        public void Parse(Command command, IEnumerable<string> args)
        {
            CommandOption option = null;
            IEnumerator<Action<Command, string>> parameter = command.Model.Parameters.GetEnumerator();

            foreach (var arg in NormalizeArgs(args))
            {
                if (option != null)
                {
                    option.Accept(command, arg);
                    option = null;
                    continue;
                }

                CommandModel model = command.Model.Commands.SingleOrDefault(x => x.Predicate(arg));
                if (model != null)
                {
                    command.Model = model;
                    parameter = command.Model.Parameters.GetEnumerator();
                    if (parameter.MoveNext())
                    {
                        parameter.Current.Invoke(command, arg);
                    }
                    else
                    {
                        throw new Exception(string.Format("Unexpected '{0}'", arg));
                    }
                    continue;
                }

                option = command.Model.Options.SingleOrDefault(x => x.Predicate(arg));
                if (option != null)
                {
                    continue;
                }

                if (parameter.MoveNext())
                {
                    parameter.Current.Invoke(command, arg);
                }
                else
                {
                    throw new Exception(string.Format("Unexpected '{0}'", arg));
                }
            }
        }

        private IEnumerable<string> NormalizeArgs(IEnumerable<string> args)
        {
            foreach (var iter in args)
            {
                string arg = iter;
                if (arg.StartsWith("/"))
                {
                    arg = "--" + arg.Substring(1);
                }

                if (arg.StartsWith("--"))
                {
                    int delimiterIndex = arg.IndexOf(':');
                    if (delimiterIndex != -1)
                    {
                        yield return arg.Substring(0, delimiterIndex);
                        yield return arg.Substring(delimiterIndex + 1);
                    }
                    else
                    {
                        yield return arg;
                    }
                }
                else if (arg.StartsWith("-"))
                {
                    int delimiterIndex = arg.IndexOf(':');
                    if (delimiterIndex != -1)
                    {
                        yield return arg.Substring(0, delimiterIndex);
                        yield return arg.Substring(delimiterIndex + 1);
                    }
                    else
                    {
                        yield return arg;
                    }
                }
                else
                {
                    yield return arg;
                }
            }
        }
    }
}
