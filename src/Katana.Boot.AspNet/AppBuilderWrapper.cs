// Copyright 2011-2012 Katana contributors
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