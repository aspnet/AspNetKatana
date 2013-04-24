// <copyright file="AuthenticationHandler.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Infrastructure;

namespace Microsoft.Owin.Security
{
    public abstract class AuthenticationHandler<TOptions> : IAuthenticationHandler where TOptions : AuthenticationOptions
    {
        private object _hookedState;

        private Task<AuthenticationTicket> _authenticationData;
        private bool _authenticationDataInitialized;
        private object _authenticationDataSyncLock;

        private Task _applyResponse;
        private bool _applyResponseInitialized;
        private object _applyResponseSyncLock;

        protected TOptions Options;
        protected OwinRequest Request;
        protected OwinResponse Response;
        protected string RequestPathBase;

        protected SecurityHelper Helper;

        public string AuthenticationType
        {
            get { return Options.AuthenticationType; }
        }

        public AuthenticationDescription Description
        {
            get { return Options.Description; }
        }

        public virtual async Task Initialize(TOptions options, OwinRequest request, OwinResponse response)
        {
            Options = options;
            Request = request;
            Response = response;
            Helper = new SecurityHelper(response.Environment);
            RequestPathBase = Request.PathBase;

            _hookedState = Request.HookAuthentication(this);

            Request.OnSendingHeaders(state => ((AuthenticationHandler<TOptions>)state).ApplyResponse().Wait(), this);

            if (Options.AuthenticationMode == AuthenticationMode.Active)
            {
                await ApplyIdentity();
            }
        }

        public virtual async Task<bool> Invoke()
        {
            return false;
        }

        public virtual async Task Teardown()
        {
            await ApplyResponse();
            Request.UnhookAuthentication(_hookedState);
        }

        protected async Task ApplyIdentity()
        {
            AuthenticationTicket authenticationTicket = await Authenticate();
            if (authenticationTicket != null)
            {
                Helper.AddUserIdentity(authenticationTicket.Identity);
            }
        }

        public Task<AuthenticationTicket> Authenticate()
        {
            return LazyInitializer.EnsureInitialized(
                ref _authenticationData,
                ref _authenticationDataInitialized,
                ref _authenticationDataSyncLock,
                AuthenticateCore);
        }

        protected abstract Task<AuthenticationTicket> AuthenticateCore();

        private Task ApplyResponse()
        {
            return LazyInitializer.EnsureInitialized(
                ref _applyResponse,
                ref _applyResponseInitialized,
                ref _applyResponseSyncLock,
                ApplyResponseCore);
        }

        protected virtual async Task ApplyResponseCore()
        {
            await ApplyResponseGrant();
            await ApplyResponseChallenge();
        }

        protected virtual async Task ApplyResponseGrant()
        {
        }

        protected virtual async Task ApplyResponseChallenge()
        {
        }
    }
}

#endif
