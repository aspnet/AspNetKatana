// <copyright file="TestAppBuilder.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
// </copyright>
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

using System;
using System.Collections.Generic;
using Owin;

namespace Microsoft.AspNet.Razor.Owin.Tests
{
    public class TestAppBuilder : IAppBuilder
    {
        public TestAppBuilder()
        {
            MiddlewareStack = new Stack<Delegate>();
        }

        public Stack<Delegate> MiddlewareStack { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "Not Implemented")]
        public IDictionary<string, object> Properties
        {
            get { throw new NotImplementedException(); }
        }

        public IAppBuilder Use<TApp>(Func<TApp, TApp> middleware)
        {
            MiddlewareStack.Push(middleware);
            return this;
        }

        public object Build(Type returnType)
        {
            throw new NotImplementedException();
        }

        public IAppBuilder New()
        {
            throw new NotImplementedException();
        }

        public IAppBuilder Use(object middleware, params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
