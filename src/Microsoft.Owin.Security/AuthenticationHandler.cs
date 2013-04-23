using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Infrastructure;

namespace Microsoft.Owin.Security
{
    public abstract class AuthenticationHandler<TOptions> : IAuthenticationHandler where TOptions : AuthenticationOptions
    {
        private object _hookedState;

        private Task<AuthenticationData> _authenticationData;
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
            var authenticationData = await Authenticate();
            if (authenticationData != null)
            {
                Helper.AddUserIdentity(authenticationData.Identity);
            }
        }

        protected Task<AuthenticationData> Authenticate()
        {
            return LazyInitializer.EnsureInitialized(
                ref _authenticationData,
                ref _authenticationDataInitialized,
                ref _authenticationDataSyncLock,
                AuthenticateCore);
        }

        protected abstract Task<AuthenticationData> AuthenticateCore();

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

        string IAuthenticationHandler.AuthenticationType
        {
            get { return Options.AuthenticationType; }
        }

        IDictionary<string, object> IAuthenticationHandler.Description { get { return Options.Description.Properties; } }

        Task<AuthenticationData> IAuthenticationHandler.Authenticate()
        {
            return Authenticate();
        }
    }
}