// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace FunctionalTests.Facts.OwinHost
{
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
    public sealed class OwinStartupAttribute : Attribute
    {
        public OwinStartupAttribute(string friendlyName, Type startupType)
        {
            this._friendlyName = friendlyName;
            this._startupType = startupType;
        }

        private Type _startupType;
        public Type StartupType { get { return this._startupType; } }

        private string _friendlyName;
        public string FriendlyName { get { return this._friendlyName; } }
    }
}