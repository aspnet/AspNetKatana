using System;
using System.Diagnostics;
using Microsoft.Owin.Logging;

namespace Owin
{
    using TraceFactoryDelegate = Func<string, Func<TraceEventType, int, object, Exception, Func<object, Exception, string>, bool>>;

    public static class AppBuilderExtensions
    {
        public static void SetLoggerFactory(this IAppBuilder app, ILoggerFactory loggerFactory)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            app.Properties["server.LoggerFactory"] = new TraceFactoryDelegate(name => loggerFactory.Create(name).WriteCore);
        }

        public static ILoggerFactory GetLoggerFactory(this IAppBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            object value;
            if (app.Properties.TryGetValue("server.LoggerFactory", out value) && value is TraceFactoryDelegate)
            {
                return new WrapLoggerFactory(value as TraceFactoryDelegate);
            }
            return null;
        }

        public static ILogger CreateLogger(this IAppBuilder app, string name)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            return (GetLoggerFactory(app) ?? LoggerFactory.Default).Create(name);
        }

        public static ILogger CreateLogger(this IAppBuilder app, Type component)
        {
            return CreateLogger(app, component.FullName);
        }

        public static ILogger CreateLogger<Type>(this IAppBuilder app)
        {
            return CreateLogger(app, typeof(Type));
        }

        class WrapLoggerFactory : ILoggerFactory
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

        class WrappingLogger : ILogger
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
