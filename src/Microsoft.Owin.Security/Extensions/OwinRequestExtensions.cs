// <copyright file="OwinRequestExtensions.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Owin.Security;

// ReSharper disable CheckNamespace

namespace Microsoft.Owin
{
    using AuthenticateCallback = Action<IIdentity, IDictionary<string, string>, IDictionary<string, object>, object>;
    using AuthenticateDelegate = Func<string[], Action<IIdentity, IDictionary<string, string>, IDictionary<string, object>, object>, object, Task>;

    public static class OwinRequestExtensions
    {
        public static object HookAuthentication(this OwinRequest request, IAuthenticationHandler handler)
        {
            var chained = request.Get<AuthenticateDelegate>("security.Authenticate");
            var hook = new Hook(handler, chained);
            request.Set<AuthenticateDelegate>("security.Authenticate", hook.Authenticate);
            return hook;
        }

        public static void UnhookAuthentication(this OwinRequest request, object state)
        {
            var hook = (Hook)state;
            request.Set("security.Authenticate", hook.Chained);
        }

        public class Hook
        {
            private readonly IAuthenticationHandler _handler;

            public Hook(IAuthenticationHandler handler, AuthenticateDelegate chained)
            {
                _handler = handler;
                Chained = chained;
            }

            public AuthenticateDelegate Chained { get; private set; }

            public async Task Authenticate(
                string[] authenticationTypes,
                AuthenticateCallback callback,
                object state)
            {
                if (authenticationTypes == null)
                {
                    callback(null, null, _handler.Description.Properties, state);
                }
                else if (authenticationTypes.Contains(_handler.AuthenticationType, StringComparer.Ordinal))
                {
                    AuthenticationData model = await _handler.Authenticate();
                    if (model != null)
                    {
                        callback(model.Identity, model.Extra, _handler.Description.Properties, state);
                    }
                }
                if (Chained != null)
                {
                    await Chained(authenticationTypes, callback, state);
                }
            }
        }
    }
}
