using System;
using System.Collections.Generic;
using Owin;

namespace Microsoft.Owin.StaticFiles
{
    public static class SendFileExtensions
    {
        private const string ServerCapabilitiesKey = "server.Capabilities";
        private const string SendFileVersionKey = "sendfile.Version";
        private const string SendFileVersion = "1.0";

        public static IAppBuilder UseSendFileFallback(this IAppBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            // Check for advertised support
            if (IsSendFileSupported(builder.Properties))
            {
                return builder;
            }

            // Otherwise, insert a fallback SendFile middleware and advertise support
            SetSendFileCapability(builder.Properties);
            return builder.Use(typeof(SendFileFallback));
        }

        private static bool IsSendFileSupported(IDictionary<string, object> properties)
        {
            object obj;
            if (properties.TryGetValue(ServerCapabilitiesKey, out obj))
            {
                IDictionary<string, object> capabilities = (IDictionary<string, object>)obj;
                if (capabilities.TryGetValue(SendFileVersionKey, out obj)
                    && SendFileVersion.Equals((string)obj, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }

        private static void SetSendFileCapability(IDictionary<string, object> properties)
        {
            object obj;
            if (properties.TryGetValue(ServerCapabilitiesKey, out obj))
            {
                IDictionary<string, object> capabilities = (IDictionary<string, object>)obj;
                capabilities[SendFileVersionKey] = SendFileVersion;
            }
        }
    }
}
