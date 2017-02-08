// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OwinHost.Options
{
    public class CommandExecutor
    {
        public void Parse(Command command, IEnumerable<string> args)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

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
                        throw new CommandException(string.Format(CultureInfo.CurrentCulture, Resources.CommandException_UnexpectedCommandLineArgument, arg));
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
                    throw new CommandException(string.Format(CultureInfo.CurrentCulture, Resources.CommandException_UnexpectedCommandLineArgument, arg));
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
                    arg = "-" + arg.Substring(1);
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
