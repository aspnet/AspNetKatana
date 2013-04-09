using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.Serialization;
using Owin.Types;
using Owin.Types.Extensions;
using Owin.Types.Helpers;

namespace Microsoft.Owin.Security.Forms
{
    class FormsAuthenticationContext
    {
        private static readonly Action<object> ApplyResponseDelegate = obj => ((FormsAuthenticationContext)obj).ApplyResponse();

        private readonly FormsAuthenticationOptions _options;
        private OwinRequest _request;
        private OwinResponse _response;
        private SecurityHelper _helper;
        private Func<string[], Action<IIdentity, object>, object, Task> _chainGetIdentities;
        private string _requestPathBase;
        private string _requestPath;

        private Task<IIdentity> _getIdentity;
        private bool _getIdentityInitialized;
        private object _getIdentitySyncLock;

        private bool _applyResponse;
        private bool _applyResponseInitialized;
        private object _applyResponseSyncLock;

        public FormsAuthenticationContext(FormsAuthenticationOptions options, IDictionary<string, object> env)
        {
            _options = options;
            _request = new OwinRequest(env);
            _response = new OwinResponse(env);
            _helper = new SecurityHelper(env);
        }

        public async Task Initialize()
        {
            _chainGetIdentities = _request.GetIdentitiesDelegate;
            _request.GetIdentitiesDelegate = GetIdentities;

            _requestPath = _request.Path;
            _requestPathBase = _request.PathBase;
            _request.OnSendingHeaders(ApplyResponseDelegate, this);

            if (_options.AuthenticationMode == AuthenticationMode.Active)
            {
                await ApplyIdentity();
            }
        }

        public void Teardown()
        {
            ApplyResponse();
        }

        private async Task ApplyIdentity()
        {
            var identity = await GetIdentity();
            if (identity != null)
            {
                _helper.AddUserIdentity(identity);
            }
        }

        private async Task GetIdentities(string[] authenticationTypes, Action<IIdentity, object> callback, object state)
        {
            if (authenticationTypes == null)
            {
                callback(new ClaimsIdentity(_options.AuthenticationType), state);
            }
            else if (authenticationTypes.Contains(_options.AuthenticationType, StringComparer.Ordinal))
            {
                var identity = await GetIdentity();
                if (identity != null)
                {
                    callback(identity, state);
                }
            }
            if (_chainGetIdentities != null)
            {
                await _chainGetIdentities(authenticationTypes, callback, state);
            }
        }

        private Task<IIdentity> GetIdentity()
        {
            return LazyInitializer.EnsureInitialized(
                ref _getIdentity,
                ref _getIdentityInitialized,
                ref _getIdentitySyncLock,
                GetIdentityOnce);
        }

        private async Task<IIdentity> GetIdentityOnce()
        {
            try
            {
                var cookies = _request.GetCookies();
                string cookie;
                if (!cookies.TryGetValue(_options.CookieName, out cookie))
                {
                    return null;
                }

                var protectedData = Convert.FromBase64String(cookie);
                var userData = _options.DataProtection.Unprotect(protectedData);
#if DEBUG
                var peek = Encoding.UTF8.GetString(userData);
#endif
                var formsData = DataModelSerialization.Deserialize(userData);
                var identity = formsData.Principal.Identity;

                if (_options.Provider != null)
                {
                    var command = new FormsValidateIdentityContext(identity);
                    await _options.Provider.ValidateIdentity(command);
                    identity = command.Identity;
                }

                return identity;
            }
            catch (Exception ex)
            {
                // TODO: trace
                return null;
            }
        }


        private bool ApplyResponse()
        {
            return LazyInitializer.EnsureInitialized(
                ref _applyResponse,
                ref _applyResponseInitialized,
                ref _applyResponseSyncLock,
                ApplyResponseOnce);
        }

        private bool ApplyResponseOnce()
        {
            ApplyResponseGrant();
            ApplyResponseChallenge();
            return default(bool);
        }

        private void ApplyResponseGrant()
        {
            var signin = _helper.LookupSignin(_options.AuthenticationType);
            var signout = _helper.LookupSignout(_options.AuthenticationType, _options.AuthenticationMode);

            if (signin.ShouldHappen || signout.ShouldHappen)
            {
                // TODO: verify we are on login/logout/etc path?
                // TODO: need a "set expiration" flag?
                var cookieOptions = new CookieOptions
                {
                    Domain = _options.CookieDomain,
                    HttpOnly = _options.CookieHttpOnly,
                    Path = _options.CookiePath ?? "/",
                    Secure = _options.CookieSecure,
                };

                if (signin.ShouldHappen)
                {
                    var formsData = new DataModel(
                        new ClaimsPrincipal(signin.Identity),
                        new Dictionary<string, string>
                        {
                            { "IsPersistent", "true" },
                            { "ExpireUtc", DateTimeOffset.UtcNow.Add(_options.ExpireTimeSpan).ToString(CultureInfo.InvariantCulture) }
                        });
                    var userData = DataModelSerialization.Serialize(formsData);

                    var protectedData = _options.DataProtection.Protect(userData);
                    _response.AddCookie(
                        _options.CookieName,
                        Convert.ToBase64String(protectedData),
                        cookieOptions);
                }
                else if (signout.ShouldHappen)
                {
                    _response.DeleteCookie(
                        _options.CookieName,
                        cookieOptions);
                }

                var shouldLoginRedirect = signin.ShouldHappen && !string.IsNullOrEmpty(_options.LoginPath) && string.Equals(_requestPath, _options.LoginPath, StringComparison.OrdinalIgnoreCase);
                var shouldLogoutRedirect = signout.ShouldHappen && !string.IsNullOrEmpty(_options.LogoutPath) && string.Equals(_requestPath, _options.LogoutPath, StringComparison.OrdinalIgnoreCase);

                if (shouldLoginRedirect || shouldLogoutRedirect)
                {
                    var query = _request.GetQuery();
                    string[] redirectUri;
                    if (query.TryGetValue("redirect_uri", out redirectUri) && redirectUri != null && redirectUri.Length == 1)
                    {
                        // TODO: safe redirect rules
                        _response.StatusCode = 302;
                        _response.AddHeader("Location", redirectUri[0]);
                    }
                }
            }
        }

        private void ApplyResponseChallenge()
        {
            if (_response.StatusCode != 401 || string.IsNullOrEmpty(_options.LoginPath))
            {
                return;
            }

            var challenge = _helper.LookupChallenge(_options.AuthenticationType, _options.AuthenticationMode);

            if (challenge.ShouldHappen)
            {
                var prefix = _request.Scheme + "://" + _request.Host + _request.PathBase;

                var queryString = _request.QueryString;

                var redirectUri = string.IsNullOrEmpty(queryString) ?
                    prefix + _request.Path :
                    prefix + _request.Path + "?" + queryString;

                var location = prefix + _options.LoginPath + "?redirect_uri=" + Uri.EscapeDataString(redirectUri);

                _response.StatusCode = 302;
                _response.SetHeader("Location", location);
            }
        }
    }
}
