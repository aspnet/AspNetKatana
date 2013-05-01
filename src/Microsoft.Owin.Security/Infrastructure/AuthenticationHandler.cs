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

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Infrastructure
{
    /// <summary>
    /// Base class for the per-request work performed by most authentication middleware.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public abstract class AuthenticationHandler<TOptions> : IAuthenticationHandler where TOptions : AuthenticationOptions
    {
        private object _registration;

        private Task<AuthenticationTicket> _authenticate;
        private bool _authenticateInitialized;
        private object _authenticateSyncLock;

        private Task _applyResponse;
        private bool _applyResponseInitialized;
        private object _applyResponseSyncLock;

        protected TOptions Options;
        protected OwinRequest Request;
        protected OwinResponse Response;
        protected string RequestPathBase;

        protected SecurityHelper Helper;

        /// <summary>
        /// Implementing IAuthenticationHandler.AuthenticationType enables the common code to know
        /// when this authentication type is being addressed by name.
        /// </summary>
        public string AuthenticationType
        {
            get { return Options.AuthenticationType; }
        }

        /// <summary>
        /// Implementing IAuthenticationHandler.Description enables the common code to flow this 
        /// authentication types properties, like Caption, back to the application code.
        /// </summary>
        public AuthenticationDescription Description
        {
            get { return Options.Description; }
        }

        /// <summary>
        /// Initialize is called once per request to contextualize this instance with appropriate state.
        /// </summary>
        /// <param name="options">The original options passed by the application control behavior</param>
        /// <param name="request">The utility object to observe the current request</param>
        /// <param name="response">The utility object to effect the current response</param>
        /// <returns>async completion</returns>
        public virtual async Task Initialize(TOptions options, OwinRequest request, OwinResponse response)
        {
            Options = options;
            Request = request;
            Response = response;
            Helper = new SecurityHelper(request);
            RequestPathBase = Request.PathBase;

            _registration = Request.RegisterAuthenticationHandler(this);

            Request.OnSendingHeaders(state => ((AuthenticationHandler<TOptions>)state).ApplyResponse().Wait(), this);

            if (Options.AuthenticationMode == AuthenticationMode.Active)
            {
                AuthenticationTicket ticket = await Authenticate();
                if (ticket != null && ticket.Identity != null)
                {
                    Helper.AddUserIdentity(ticket.Identity);
                }
            }
        }

        /// <summary>
        /// Called once by common code after initialization. If an authentication middleware responds directly to
        /// specifically known paths it must override this virtual, compare the request path to it's known paths, 
        /// provide any response information as appropriate, and true to stop further processing.
        /// </summary>
        /// <returns>Returning false will cause the common code to call the next middleware in line. Returning true will
        /// cause the common code to begin the async completion journey without calling the rest of the middleware
        /// pipeline.</returns>
        public virtual async Task<bool> Invoke()
        {
            return false;
        }

        /// <summary>
        /// Called once per request after Initialize and Invoke. 
        /// </summary>
        /// <returns>async completion</returns>
        public virtual async Task Teardown()
        {
            await ApplyResponse();
            Request.UnregisterAuthenticationHandler(_registration);
        }

        /// <summary>
        /// Causes the authentication logic in AuthenticateCore to be performed for the current request 
        /// at most once and returns the results. Calling Authenticate more than once will always return 
        /// the original value. 
        /// 
        /// This method should always be called instead of calling AuthenticateCore directly.
        /// </summary>
        /// <returns>The ticket data provided by the authentication logic</returns>
        public Task<AuthenticationTicket> Authenticate()
        {
            return LazyInitializer.EnsureInitialized(
                ref _authenticate,
                ref _authenticateInitialized,
                ref _authenticateSyncLock,
                AuthenticateCore);
        }

        /// <summary>
        /// The core authentication logic which must be provided by the handler. Will be invoked at most
        /// once per requst. Do not call directly, call the wrapping Authenticate method instead.
        /// </summary>
        /// <returns>The ticket data provided by the authentication logic</returns>
        protected abstract Task<AuthenticationTicket> AuthenticateCore();

        /// <summary>
        /// Causes the ApplyResponseCore to be invoked at most once per request. This method will be
        /// invoked either earlier, when the response headers are sent as a result of a response write or flush,
        /// or later, as the last step when the original async call to the middleware is returning.
        /// </summary>
        /// <returns></returns>
        private Task ApplyResponse()
        {
            return LazyInitializer.EnsureInitialized(
                ref _applyResponse,
                ref _applyResponseInitialized,
                ref _applyResponseSyncLock,
                ApplyResponseCore);
        }

        /// <summary>
        /// Core method that may be overridden by handler. The default behavior is to call two common response 
        /// activities, one that deals with sign-in/sign-out concerns, and a second to deal with 401 challenges.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task ApplyResponseCore()
        {
            await ApplyResponseGrant();
            await ApplyResponseChallenge();
        }

        /// <summary>
        /// Override this method to dela with sign-in/sign-out concerns, if an authentication scheme in question
        /// deals with grant/revoke as part of it's request flow. (like setting/deleting cookies)
        /// </summary>
        /// <returns></returns>
        protected virtual async Task ApplyResponseGrant()
        {
        }

        /// <summary>
        /// Override this method to dela with 401 challenge concerns, if an authentication scheme in question
        /// deals an authentication interaction as part of it's request flow. (like adding a response header, or
        /// changing the 401 result to 302 of a login page or external sign-in location.)
        /// </summary>
        /// <returns></returns>
        protected virtual async Task ApplyResponseChallenge()
        {
        }
    }
}
