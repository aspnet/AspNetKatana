// <copyright file="AppBuilderExtensions.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Owin;

// Not in the OWIN namespace because this is an opt-in feature, not something we want to immediately light up.
namespace Microsoft.Owin.Logging
{
    using TraceFactoryDelegate = Func<string, Func<TraceEventType, int, object, Exception, Func<object, Exception, string>, bool>>;

    /// <summary>
    /// Logging extension methods for IAppBuilder.
    /// </summary>
    public static class AppBuilderLoggerExtensions
    {
        /// <summary>
        /// Sets the server.LoggerFactory in the Properties collection.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="loggerFactory"></param>
        public static void SetLoggerFactory(this IAppBuilder app, ILoggerFactory loggerFactory)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            app.Properties["server.LoggerFactory"] = new TraceFactoryDelegate(name => loggerFactory.Create(name).WriteCore);
        }

        /// <summary>
        /// Retrieves the server.LoggerFactory from the Properties collection.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static ILoggerFactory GetLoggerFactory(this IAppBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            object value;
            if (app.Properties.TryGetValue("server.LoggerFactory", out value))
            {
                TraceFactoryDelegate factory = value as TraceFactoryDelegate;
                if (factory != null)
                {
                    return new WrapLoggerFactory(factory);
                }
            }
            return null;
        }

        /// <summary>
        /// Creates a new ILogger instance from the server.LoggerFactory in the Properties collection.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ILogger CreateLogger(this IAppBuilder app, string name)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            return (GetLoggerFactory(app) ?? LoggerFactory.Default).Create(name);
        }

        /// <summary>
        /// Creates a new ILogger instance from the server.LoggerFactory in the Properties collection.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="component"></param>
        /// <returns></returns>
        public static ILogger CreateLogger(this IAppBuilder app, Type component)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }

            return CreateLogger(app, component.FullName);
        }

        /// <summary>
        /// Creates a new ILogger instance from the server.LoggerFactory in the Properties collection.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="app"></param>
        /// <returns></returns>
        public static ILogger CreateLogger<TType>(this IAppBuilder app)
        {
            return CreateLogger(app, typeof(TType));
        }

        private class WrapLoggerFactory : ILoggerFactory
        {
            private readonly TraceFactoryDelegate _create;

            public WrapLoggerFactory(TraceFactoryDelegate create)
            {
                if (create == null)
                {
                    throw new ArgumentNullException("create");
                }
                _create = create;
            }

            public ILogger Create(string name)
            {
                return new WrappingLogger(_create.Invoke(name));
            }
        }

        private class WrappingLogger : ILogger
        {
            private readonly Func<TraceEventType, int, object, Exception, Func<object, Exception, string>, bool> _write;

            public WrappingLogger(Func<TraceEventType, int, object, Exception, Func<object, Exception, string>, bool> write)
            {
                if (write == null)
                {
                    throw new ArgumentNullException("write");
                }
                _write = write;
            }

            public bool WriteCore(TraceEventType eventType, int eventId, object state, Exception exception, Func<object, Exception, string> message)
            {
                return _write(eventType, eventId, state, exception, message);
            }
        }
    }
}
