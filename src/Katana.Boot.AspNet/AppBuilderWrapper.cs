//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Owin;
using Owin.Builder;

namespace Katana.Boot.AspNet
{
    public class AppBuilderWrapper : IAppBuilder
    {
        private readonly IAppBuilder _builder;

        public AppBuilderWrapper()
        {
            _builder = new AppBuilder();
        }

        public IDictionary<string, object> Properties
        {
            get { return _builder.Properties; }
        }

        public IAppBuilder Use(object middleware, params object[] args)
        {
            return _builder.Use(middleware, args);
        }

        public object Build(Type returnType)
        {
            _builder.UseType<AspNetCaller>();
            return _builder.Build(returnType);
        }

        public IAppBuilder New()
        {
            return _builder.New();
        }

        public IAppBuilder AddSignatureConversion(Delegate conversion)
        {
            return _builder.AddSignatureConversion(conversion);
        }
    }
}