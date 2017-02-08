// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
