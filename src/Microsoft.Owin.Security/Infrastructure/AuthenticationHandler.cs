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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.DataHandler.Encoder;

namespace Microsoft.Owin.Security.Infrastructure
{
    /// <summary>
    /// Base class for the per-request work performed by most authentication middleware.
    /// </summary>
    public abstract class AuthenticationHandler
    {
        private static readonly RNGCryptoServiceProvider Random = new RNGCryptoServiceProvider();

        private object _registration;

        private Task<AuthenticationTicket> _authenticate;
        private bool _authenticateInitialized;
        private object _authenticateSyncLock;

        private Task _applyResponse;
        private bool _applyResponseInitialized;
        private object _applyResponseSyncLock;

        private AuthenticationOptions _baseOptions;

        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields",
            Justification = "The compiler won't let you call property Setters on structs returned from property Getters. e.g. handler.Response.StatusCode = 200")]
        protected OwinResponse Response;

        protected OwinRequest Request { get; private set; }
        protected string RequestPathBase { get; private set; }
        protected SecurityHelper Helper { get; private set; }
        protected IDictionary<string, string> ErrorDetails { get; private set; }

        internal AuthenticationOptions BaseOptions
        {
            get { return _baseOptions; }
        }

        protected async Task BaseInitialize(AuthenticationOptions options, OwinRequest request, OwinResponse response)
        {
            _baseOptions = options;
            Request = request;
            Response = response;
            Helper = new SecurityHelper(request);
            RequestPathBase = Request.PathBase;

            _registration = Request.RegisterAuthenticationHandler(this);

            Request.OnSendingHeaders(state => ((AuthenticationHandler)state).ApplyResponse().Wait(), this);

            await InitializeCore();

            if (BaseOptions.AuthenticationMode == AuthenticationMode.Active)
            {
                AuthenticationTicket ticket = await Authenticate();
                if (ticket != null && ticket.Identity != null)
                {
                    Helper.AddUserIdentity(ticket.Identity);
                }
            }
        }

        protected virtual Task InitializeCore()
        {
            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Called once per request after Initialize and Invoke. 
        /// </summary>
        /// <returns>async completion</returns>
        internal async Task Teardown()
        {
            await ApplyResponse();
            await TeardownCore();
            Request.UnregisterAuthenticationHandler(_registration);
        }

        protected virtual Task TeardownCore()
        {
            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Called once by common code after initialization. If an authentication middleware responds directly to
        /// specifically known paths it must override this virtual, compare the request path to it's known paths, 
        /// provide any response information as appropriate, and true to stop further processing.
        /// </summary>
        /// <returns>Returning false will cause the common code to call the next middleware in line. Returning true will
        /// cause the common code to begin the async completion journey without calling the rest of the middleware
        /// pipeline.</returns>
        public virtual Task<bool> Invoke()
        {
            return Task.FromResult<bool>(false);
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
        /// once per request. Do not call directly, call the wrapping Authenticate method instead.
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
        protected virtual Task ApplyResponseGrant()
        {
            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Override this method to dela with 401 challenge concerns, if an authentication scheme in question
        /// deals an authentication interaction as part of it's request flow. (like adding a response header, or
        /// changing the 401 result to 302 of a login page or external sign-in location.)
        /// </summary>
        /// <returns></returns>
        protected virtual Task ApplyResponseChallenge()
        {
            return Task.FromResult<object>(null);
        }

        protected void AddErrorDetail(string detailName, string detailValue)
        {
            if (ErrorDetails == null)
            {
                ErrorDetails = new Dictionary<string, string>(StringComparer.Ordinal);
            }
            ErrorDetails[detailName] = detailValue;
        }

        protected void GenerateCorrelationId(AuthenticationExtra extra)
        {
            if (extra == null)
            {
                throw new ArgumentNullException("extra");
            }

            var correlationKey = Constants.CorrelationPrefix + BaseOptions.AuthenticationType;

            var nonceBytes = new byte[32];
            Random.GetBytes(nonceBytes);
            var correlationId = TextEncodings.Base64Url.Encode(nonceBytes);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsSecure
            };

            extra.Properties[correlationKey] = correlationId;

            Response.AddCookie(correlationKey, correlationId, cookieOptions);
        }

        protected bool ValidateCorrelationId(AuthenticationExtra extra, ILogger logger)
        {
            if (extra == null)
            {
                throw new ArgumentNullException("extra");
            }

            var correlationKey = Constants.CorrelationPrefix + BaseOptions.AuthenticationType;

            string correlationCookie;
            if (!Request.GetCookies().TryGetValue(
                correlationKey,
                out correlationCookie))
            {
                logger.WriteWarning(Resources.Warning_CookieNotFound, correlationKey);
                return false;
            }

            Response.DeleteCookie(correlationKey);

            string correlationExtra;
            if (!extra.Properties.TryGetValue(
                correlationKey,
                out correlationExtra))
            {
                logger.WriteWarning(Resources.Warning_StateNotFound, correlationKey);
                return false;
            }

            extra.Properties.Remove(correlationKey);

            if (!string.Equals(correlationCookie, correlationExtra, StringComparison.Ordinal))
            {
                logger.WriteWarning(Resources.Warning_CookieStateMismatch, correlationKey);
                return false;
            }

            return true;
        }
    }
}
