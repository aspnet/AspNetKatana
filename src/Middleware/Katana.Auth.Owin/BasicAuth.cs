using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Diagnostics.Contracts;

namespace Katana.Auth.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    // TODO: Or do we have them return a Task<bool> and let them set whatever they want in the env?
    using AuthCallback = Func<IDictionary<string, object> /*env*/, string/*user*/, string/*psw*/, Task<IPrincipal>>;

    public class BasicAuth
    {
        private static readonly Encoding encoding = Encoding.GetEncoding(28591);

        private AppFunc nextApp;
        private AuthCallback verifyer;
        private string realm;
        private string challenge;

        public BasicAuth(AppFunc nextApp, AuthCallback verifyer)
            : this(nextApp, verifyer, null)
        {
        }

        public BasicAuth(AppFunc nextApp, AuthCallback verifyer, string realm)
        {
            this.nextApp = nextApp;
            this.verifyer = verifyer;
            this.realm = realm;

            this.challenge = "Basic";
            if (!string.IsNullOrWhiteSpace(this.realm))
            {
                this.challenge += " realm=\"" + this.realm + "\"";
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
                    string userAndPass = encoding.GetString(data);
                    int colonIndex = userAndPass.IndexOf(':');

                    if (colonIndex < 0)
                    {
                        env[Constants.ResponseStatusCodeKey] = 400;
                        return TaskHelpers.Completed();
                    }

                    string user = userAndPass.Substring(0, colonIndex);
                    string pass = userAndPass.Substring(colonIndex + 1);

                    return verifyer(env, user, pass)
                        .Then(principal =>
                        {
                            if (principal == null)
                            {
                                // Failure, bad credentials
                                env[Constants.ResponseStatusCodeKey] = 401;
                                AppendChallengeOn401(env);
                                return TaskHelpers.Completed();
                            }

                            // Success!
                            env[Constants.ServerUserKey] = principal;
                            return nextApp(env);
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

            return nextApp(env);
        }

        private void AppendChallengeOn401(object state)
        {
            IDictionary<string, object> env = (IDictionary<string, object>)state;
            var responseHeaders = env.Get<IDictionary<string, string[]>>(Constants.ResponseHeadersKey);
            if (env.Get<int>(Constants.ResponseStatusCodeKey) == 401)
            {
                responseHeaders.AppendHeader(Constants.WwwAuthenticateHeader, challenge);
            }
        }
    }
}
