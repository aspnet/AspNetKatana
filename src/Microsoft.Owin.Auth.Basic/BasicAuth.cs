// <copyright file="BasicAuth.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Owin.Auth.Basic
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using AuthCallback = Func<IDictionary<string, object> /*env*/, string /*user*/, string /*psw*/, Task<bool>>;

    public class BasicAuth
    {
        private static readonly Encoding Encoding = Encoding.GetEncoding(28591);

        private readonly AppFunc _nextApp;
        private readonly string _challenge;
        private readonly Options _options;

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public BasicAuth(AppFunc nextApp, Options options)
        {
            if (nextApp == null)
            {
                throw new ArgumentNullException("nextApp");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            _nextApp = nextApp;
            _options = options;

            _challenge = "Basic";
            if (!string.IsNullOrWhiteSpace(options.Realm))
            {
                _challenge += " realm=\"" + options.Realm + "\"";
            }
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            var requestHeaders = environment.Get<IDictionary<string, string[]>>(Constants.RequestHeadersKey);
            string authHeader = requestHeaders.GetHeader(Constants.AuthorizationHeader);

            string basicPrefix = "Basic ";

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith(basicPrefix, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    byte[] data = Convert.FromBase64String(authHeader.Substring(basicPrefix.Length).Trim());
                    string userAndPass = Encoding.GetString(data);
                    int colonIndex = userAndPass.IndexOf(':');

                    if (colonIndex < 0)
                    {
                        environment[Constants.ResponseStatusCodeKey] = (int)HttpStatusCode.BadRequest;
                        return TaskHelpers.Completed();
                    }

                    string user = userAndPass.Substring(0, colonIndex);
                    string pass = userAndPass.Substring(colonIndex + 1);

                    return _options.Authenticate(environment, user, pass)
                        .Then(authenticated =>
                        {
                            if (authenticated == false)
                            {
                                // Failure, bad credentials
                                environment[Constants.ResponseStatusCodeKey] = (int)HttpStatusCode.Unauthorized;
                                AppendChallengeOn401(environment);
                                return TaskHelpers.Completed();
                            }

                            var scheme = environment.Get<string>(Constants.RequestSchemeKey);
                            if (_options.RequireEncryption && !string.Equals(Uri.UriSchemeHttps, scheme, StringComparison.OrdinalIgnoreCase))
                            {
                                // Good credentials, but SSL required
                                environment[Constants.ResponseStatusCodeKey] = (int)HttpStatusCode.Unauthorized;
                                environment[Constants.ResponseReasonPhraseKey] = "HTTPS Required";
                                AppendChallengeOn401(environment);
                                return TaskHelpers.Completed();
                            }

                            // Success!
                            environment[Constants.ServerUserKey] = new GenericPrincipal(
                                new GenericIdentity(user, "Basic"),
                                new string[0]);

                            return _nextApp(environment);
                        })
                        .Catch(catchInfo =>
                        {
                            // TODO: 500 error
                            return catchInfo.Throw();
                        });
                }
                catch (Exception)
                {
                    // TODO: 500 error
                    throw;
                }
            }

            // Hook the OnSendHeaders event and append our challenge if there's a 401.
            var registerOnSendingHeaders = environment.Get<Action<Action<object>, object>>(Constants.ServerOnSendingHeadersKey);
            Contract.Assert(registerOnSendingHeaders != null);
            registerOnSendingHeaders(AppendChallengeOn401, environment);

            return _nextApp(environment);
        }

        private void AppendChallengeOn401(object state)
        {
            var env = (IDictionary<string, object>)state;
            var responseHeaders = env.Get<IDictionary<string, string[]>>(Constants.ResponseHeadersKey);
            if (env.Get<int>(Constants.ResponseStatusCodeKey) == 401)
            {
                responseHeaders.AppendHeader(Constants.WwwAuthenticateHeader, _challenge);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "By design")]
        public class Options
        {
            public string Realm { get; set; }
            public bool RequireEncryption { get; set; }
            [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
            public AuthCallback Authenticate { get; set; }
        }
    }
}
