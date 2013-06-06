// <copyright file="CommandModel.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace OwinHost.Options
{
    public class CommandModel
    {
        public CommandModel()
        {
            Commands = new List<CommandModel>();
            Options = new List<CommandOption>();
            Parameters = new List<Action<Command, string>>();
        }

        public CommandModel Parent { get; set; }

        public CommandModel Root
        {
            get { return Parent ?? this; }
        }

        public string Name { get; set; }
        public Func<string, bool> Predicate { get; set; }
        public IList<CommandModel> Commands { get; private set; }
        public IList<CommandOption> Options { get; private set; }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public IList<Action<Command, string>> Parameters { get; private set; }

        public Action<Command> Run { get; private set; }

        public CommandModel Parameter<T>(Action<Command, T> action)
        {
            Parameters.Add(ValueParser(action));
            return this;
        }

        public CommandModel Option<T>(string name, string shortName, string description, Action<Command, T> action)
        {
            Options.Add(new CommandOption(name, shortName, description, ValueParser(action)));
            return this;
        }

        public CommandModel Option<T>(string name, string description, Action<Command, T> action)
        {
            return Option(name, null, description, action);
        }

        public CommandModel Option<TData, TValue>(string name, string shortName, string description, Action<TData, TValue> action) where TData : new()
        {
            return Option<TValue>(name, shortName, description, (command, value) => action(command.Get<TData>(), value));
        }

        public CommandModel Option<TData, TValue>(string name, string description, Action<TData, TValue> action) where TData : new()
        {
            return Option<TValue>(name, null, description, (command, value) => action(command.Get<TData>(), value));
        }

        public CommandModel Execute(Action<Command> action)
        {
            Run = action;
            return this;
        }

        public CommandModel Command(string name, Func<string, bool> predicate, Action<Command, string> accept)
        {
            var command = new CommandModel { Name = name, Predicate = predicate, Parent = this };
            command.Parameters.Add(accept);
            Commands.Add(command);
            return command;
        }

        public CommandModel Command(string name)
        {
            return Command(name, value => string.Equals(value, name, StringComparison.Ordinal), (cmd, value) => { });
        }

        public CommandModel Execute<TData>(Action<TData> action) where TData : new()
        {
            return Execute(cmd => action(cmd.Get<TData>()));
        }

        public CommandModel Execute<TData, TResult>(Func<TData, TResult> action) where TData : new()
        {
            return Execute(cmd => cmd.Set(action(cmd.Get<TData>())));
        }

        private static Action<Command, string> ValueParser<T>(Action<Command, T> action)
        {
            if (typeof(T) == typeof(int))
            {
                return (cmd, value) => action(cmd, (T)(object)int.Parse(value, CultureInfo.InvariantCulture));
            }
            if (typeof(T) == typeof(string))
            {
                return (cmd, value) => action(cmd, (T)(object)value);
            }
            throw new FormatException("Unknown switch type");
        }

        public Command Parse(params string[] args)
        {
            var command = new Command { Model = this };
            new CommandExecutor().Parse(command, args);
            return command;
        }
    }
}
