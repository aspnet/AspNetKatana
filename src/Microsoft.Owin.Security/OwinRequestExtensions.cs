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

#if NET45

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Infrastructure;

namespace Microsoft.Owin.Security
{
    using AuthenticateDelegate = Func<string[], Action<IIdentity, IDictionary<string, string>, IDictionary<string, object>, object>, object, Task>;
    
    public static class OwinRequestExtensions
    {
        private static void CallAuthenticate()
        {
            
        }

        //[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
        //    Justification = "Following Owin conventions.")]
        //public static Task GetAuthenticationTypes(this OwinRequest request, Action<IDictionary<string, object>, object> callback, object state)
        //{
        //    return AuthenticateDelegate.Invoke(null, (ignore1, ignore2, extra, innerState) =>
        //        callback.Invoke(extra, innerState), state);
        //}

        //[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
        //    Justification = "Following Owin conventions.")]
        //public static Task GetAuthenticationTypes(this OwinRequest request, Action<IDictionary<string, object>> callback)
        //{
        //    return AuthenticateDelegate.Invoke(null, GetAuthenticationTypesPropertiesDelegate, callback);
        //}

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Following Owin conventions.")]
        public static async Task Authenticate(this OwinRequest request, string[] authenticationTypes, Action<IIdentity, IDictionary<string, string>,
            IDictionary<string, object>, object> callback, object state)
        {
            var authenticate = request.Get<AuthenticateDelegate>(Constants.SecurityAuthenticate);
            if (authenticate != null)
            {
                await authenticate.Invoke(authenticationTypes, callback, state);
            }
        }

        //[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
        //    Justification = "Following Owin conventions.")]
        //public static Task Authenticate(this OwinRequest request, string[] authenticationTypes, Action<IIdentity, IDictionary<string, string>,
        //    IDictionary<string, object>> callback)
        //{
        //    return AuthenticateDelegate.Invoke(authenticationTypes, AuthenticateIdentityExtraPropertiesDelegate, callback);
        //}

        //[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
        //    Justification = "Following Owin conventions.")]
        //public static Task Authenticate(this OwinRequest request, string[] authenticationTypes, Action<IIdentity,
        //    IDictionary<string, string>> callback)
        //{
        //    return AuthenticateDelegate.Invoke(authenticationTypes, AuthenticateIdentityExtraDelegate, callback);
        //}

        //public Task Authenticate(this OwinRequest request, string[] authenticationTypes, Action<IIdentity> callback)
        //{
        //    return AuthenticateDelegate.Invoke(authenticationTypes, AuthenticateIdentityDelegate, callback);
        //}
        
        public static object HookAuthentication(this OwinRequest request, IAuthenticationHandler handler)
        {
            var chained = request.Get<AuthenticateDelegate>(Constants.SecurityAuthenticate);
            var hook = new Hook(handler, chained);
            request.Set<Func<string[], Action<IIdentity, IDictionary<string, string>, IDictionary<string, object>, object>, object, Task>>("security.Authenticate", hook.Authenticate);
            return hook;
        }

        public static void UnhookAuthentication(this OwinRequest request, object state)
        {
            var hook = (Hook)state;
            request.Set(Constants.SecurityAuthenticate, hook.Chained);
        }

        private class Hook
        {
            private readonly IAuthenticationHandler _handler;

            public Hook(IAuthenticationHandler handler, Func<string[], Action<IIdentity, IDictionary<string, string>, IDictionary<string, object>, object>, object, Task> chained)
            {
                _handler = handler;
                Chained = chained;
            }

            public Func<string[], Action<IIdentity, IDictionary<string, string>, IDictionary<string, object>, object>, object, Task> Chained { get; private set; }

            public async Task Authenticate(
                string[] authenticationTypes,
                Action<IIdentity, IDictionary<string, string>, IDictionary<string, object>, object> callback,
                object state)
            {
                if (authenticationTypes == null)
                {
                    callback(null, null, _handler.Description.Properties, state);
                }
                else if (authenticationTypes.Contains(_handler.AuthenticationType, StringComparer.Ordinal))
                {
                    AuthenticationTicket ticket = await _handler.Authenticate();
                    if (ticket != null)
                    {
                        callback(ticket.Identity, ticket.Extra.Properties, _handler.Description.Properties, state);
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

#else

using ResharperCodeFormattingWorkaround = System.Object;

#endif
