// -----------------------------------------------------------------------
// <copyright file="TestAppBuilder.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Razor.Owin.IO;
using Owin;
using Xunit;

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
