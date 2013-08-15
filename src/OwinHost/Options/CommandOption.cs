// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace OwinHost.Options
{
    public class CommandOption
    {
        public CommandOption(string name, string shortName, string description, Action<Command, string> accept)
        {
            Name = name;
            ShortName = shortName;
            Description = description;
            Accept = accept;
            Predicate = value =>
            {
                if (value.StartsWith("--", StringComparison.Ordinal))
                {
                    return string.Equals(value.Substring(2), name, StringComparison.OrdinalIgnoreCase);
                }
                if (value.StartsWith("-", StringComparison.Ordinal))
                {
                    return string.Equals(value.Substring(1), shortName, StringComparison.Ordinal);
                }
                return false;
            };
        }

        public string Name { get; private set; }
        public string ShortName { get; private set; }
        public string Description { get; private set; }

        public Func<string, bool> Predicate { get; private set; }
        public Action<Command, string> Accept { get; private set; }
    }
}
