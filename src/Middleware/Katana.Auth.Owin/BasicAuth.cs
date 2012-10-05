// Copyright 2011-2012 Katana contributors
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
using System.Diagnostics.Contracts;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Katana.Auth.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using AuthCallback = Func<IDictionary<string, object> /*env*/, string/*user*/, string/*psw*/, Task<bool>>;

    public class BasicAuth
    {
        private static readonly Encoding _encoding = Encoding.GetEncoding(28591);

        private readonly AppFunc _nextApp;
        private readonly string _challenge;
        private readonly Options _options;

        public BasicAuth(AppFunc nextApp, Options options)
        {
            _nextApp = nextApp;
            _options = options;

            _challenge = "Basic";
            if (!string.IsNullOrWhiteSpace(options.Realm))
            {
                _challenge += " realm=\"" + options.Realm + "\"";
            }
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            var requestHeaders = env.Get<IDictionary<string, string[]>>(Constants.RequestHeadersKey);
            var authHeader = requestHeaders.GetHeader(Constants.AuthorizationHeader);

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    byte[] data = Convert.FromBase64String(authHeader.Substring(6).Trim());
                    string userAndPass = _encoding.GetString(data);
                    int colonIndex = userAndPass.IndexOf(':');

                    if (colonIndex < 0)
                    {
                        env[Constants.ResponseStatusCodeKey] = 400;
                        return TaskHelpers.Completed();
                    }

                    string user = userAndPass.Substring(0, colonIndex);
                    string pass = userAndPass.Substring(colonIndex + 1);

                    return _options.Authenticate(env, user, pass)
                        .Then(authenticated =>
                        {
                            if (authenticated == false)
                            {
                                // Failure, bad credentials
                                env[Constants.ResponseStatusCodeKey] = 401;
                                AppendChallengeOn401(env);
                                return TaskHelpers.Completed();
                            }

                            var scheme = env.Get<string>(Constants.RequestSchemeKey);
                            if (_options.RequireEncryption && !string.Equals("HTTPS", scheme, StringComparison.OrdinalIgnoreCase))
                            {
                                // Good credentials, but SSL required
                                env[Constants.ResponseStatusCodeKey] = 401;
                                env[Constants.ResponseReasonPhraseKey] = "HTTPS Required";
                                AppendChallengeOn401(env);
                                return TaskHelpers.Completed();
                            }

                            // Success!
                            env[Constants.ServerUserKey] = new GenericPrincipal(
                                new GenericIdentity(user, "Basic"),
                                new string[0]);

                            return _nextApp(env);
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
            var registerOnSendingHeaders = env.Get<Action<Action<object>, object>>(Constants.ServerOnSendingHeadersKey);
            Contract.Assert(registerOnSendingHeaders != null);
            registerOnSendingHeaders(AppendChallengeOn401, env);

            return _nextApp(env);
        }

        private void AppendChallengeOn401(object state)
        {
            IDictionary<string, object> env = (IDictionary<string, object>)state;
            var responseHeaders = env.Get<IDictionary<string, string[]>>(Constants.ResponseHeadersKey);
            if (env.Get<int>(Constants.ResponseStatusCodeKey) == 401)
            {
                responseHeaders.AppendHeader(Constants.WwwAuthenticateHeader, _challenge);
            }
        }

        public class Options
        {
            public string Realm { get; set; }
            public bool RequireEncryption { get; set; }
            public AuthCallback Authenticate { get; set; }
        }
    }
}
