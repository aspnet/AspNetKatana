// <copyright file="AppBuilderWrapper.cs" company="Microsoft Open Technologies, Inc.">
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
using Owin;

namespace Katana.Boot.AspNet
{
    public class AppBuilderWrapper : IAppBuilder
    {
        private readonly IAppBuilder _builder;

        public AppBuilderWrapper(IAppBuilder builder)
        {
            _builder = builder;
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
            _builder.Use(new Func<object, object>(_ => new AspNetCaller()));
            return _builder.Build(returnType);
        }

        public IAppBuilder New()
        {
            return _builder.New();
        }
    }
}
