// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Infrastructure;
using Owin;

namespace Microsoft.Owin.Security.Google
{
    /// <summary>
    /// OWIN middleware for authenticating users using Google OAuth 2.0
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Auth",
        Justification = "OAuth2 is a valid word.")]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Middleware are not disposable.")]
    public class GoogleOAuth2AuthenticationMiddleware : AuthenticationMiddleware<GoogleOAuth2AuthenticationOptions>
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a <see cref="GoogleOAuth2AuthenticationMiddleware"/>
        /// </summary>
        /// <param name="next">The next middleware in the OWIN pipeline to invoke</param>
        /// <param name="app">The OWIN application</param>
        /// <param name="options">Configuration options for the middleware</param>
        public GoogleOAuth2AuthenticationMiddleware(
            OwinMiddleware next,
            IAppBuilder app,
            GoogleOAuth2AuthenticationOptions options)
            : base(next, options)
        {
            if (string.IsNullOrWhiteSpace(Options.ClientId))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, "ClientId"));
            }
            if (string.IsNullOrWhiteSpace(Options.ClientSecret))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, "ClientSecret"));
            }

            _logger = app.CreateLogger<GoogleOAuth2AuthenticationMiddleware>();

            if (Options.Provider == null)
            {
                Options.Provider = new GoogleOAuth2AuthenticationProvider();
            }
            if (Options.StateDataFormat == null)
            {
                IDataProtector dataProtecter = app.CreateDataProtector(
                    typeof(GoogleOAuth2AuthenticationMiddleware).FullName,
                    Options.AuthenticationType, "v1");
                Options.StateDataFormat = new PropertiesDataFormat(dataProtecter);
            }
            if (String.IsNullOrEmpty(Options.SignInAsAuthenticationType))
            {
                Options.SignInAsAuthenticationType = app.GetDefaultSignInAsAuthenticationType();
            }

            _httpClient = new HttpClient(ResolveHttpMessageHandler(Options));
            _httpClient.Timeout = Options.BackchannelTimeout;
            _httpClient.MaxResponseContentBufferSize = 1024 * 1024 * 10; // 10 MB
        }

        /// <summary>
        /// Provides the <see cref="AuthenticationHandler"/> object for processing authentication-related requests.
        /// </summary>
        /// <returns>An <see cref="AuthenticationHandler"/> configured with the <see cref="GoogleOAuth2AuthenticationOptions"/> supplied to the constructor.</returns>
        protected override AuthenticationHandler<GoogleOAuth2AuthenticationOptions> CreateHandler()
        {
            return new GoogleOAuth2AuthenticationHandler(_httpClient, _logger);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Managed by caller")]
        private static HttpMessageHandler ResolveHttpMessageHandler(GoogleOAuth2AuthenticationOptions options)
        {
            HttpMessageHandler handler = options.BackchannelHttpHandler ?? new WebRequestHandler();

            // If they provided a validator, apply it or fail.
            if (options.BackchannelCertificateValidator != null)
            {
                // Set the cert validate callback
                var webRequestHandler = handler as WebRequestHandler;
                if (webRequestHandler == null)
                {
                    throw new InvalidOperationException(Resources.Exception_ValidatorHandlerMismatch);
                }
                webRequestHandler.ServerCertificateValidationCallback = options.BackchannelCertificateValidator.Validate;
            }

            return handler;
        }
    }
}
