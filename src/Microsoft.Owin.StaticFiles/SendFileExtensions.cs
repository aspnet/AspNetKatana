// <copyright file="SendFileExtensions.cs" company="Microsoft Open Technologies, Inc.">
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
using Microsoft.Owin.StaticFiles;

namespace Owin
{
    /// <summary>
    /// Extension methods for the SendFileMiddleware
    /// </summary>
    public static class SendFileExtensions
    {
        /// <summary>
        /// Provide a SendFileFunc if another component does not.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
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
            return builder.Use(typeof(SendFileMiddleware));
        }

        private static bool IsSendFileSupported(IDictionary<string, object> properties)
        {
            object obj;
            if (properties.TryGetValue(Constants.ServerCapabilitiesKey, out obj))
            {
                var capabilities = (IDictionary<string, object>)obj;
                if (capabilities.TryGetValue(Constants.SendFileVersionKey, out obj)
                    && Constants.SendFileVersion.Equals((string)obj, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }

        private static void SetSendFileCapability(IDictionary<string, object> properties)
        {
            object obj;
            if (properties.TryGetValue(Constants.ServerCapabilitiesKey, out obj))
            {
                var capabilities = (IDictionary<string, object>)obj;
                capabilities[Constants.SendFileVersionKey] = Constants.SendFileVersion;
            }
        }
    }
}
