// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.Twitter.Messages;
using Owin;

namespace Microsoft.Owin.Security.Twitter
{
    /// <summary>
    /// OWIN middleware for authenticating users using Twitter
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Middleware are not disposable.")]
    public class TwitterAuthenticationMiddleware : AuthenticationMiddleware<TwitterAuthenticationOptions>
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a <see cref="TwitterAuthenticationMiddleware"/>
        /// </summary>
        /// <param name="next">The next middleware in the OWIN pipeline to invoke</param>
        /// <param name="app">The OWIN application</param>
        /// <param name="options">Configuration options for the middleware</param>
        public TwitterAuthenticationMiddleware(
            OwinMiddleware next,
            IAppBuilder app,
            TwitterAuthenticationOptions options)
            : base(next, options)
        {
            _logger = app.CreateLogger<TwitterAuthenticationMiddleware>();

            if (string.IsNullOrWhiteSpace(Options.ConsumerSecret))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, "ConsumerSecret"));
            }

            if (string.IsNullOrWhiteSpace(Options.ConsumerKey))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, "ConsumerKey"));
            }

            Options.CookieManager ??= app.GetDefaultCookieManager();

            if (Options.Provider == null)
            {
                Options.Provider = new TwitterAuthenticationProvider();
            }
            if (Options.StateDataFormat == null)
            {
                IDataProtector dataProtector = app.CreateDataProtector(
                    typeof(TwitterAuthenticationMiddleware).FullName,
                    Options.AuthenticationType, "v1");
                Options.StateDataFormat = new SecureDataFormat<RequestToken>(
                    Serializers.RequestToken,
                    dataProtector,
                    TextEncodings.Base64Url);
            }
            if (String.IsNullOrEmpty(Options.SignInAsAuthenticationType))
            {
                Options.SignInAsAuthenticationType = app.GetDefaultSignInAsAuthenticationType();
            }

            _httpClient = new HttpClient(ResolveHttpMessageHandler(Options)); // CodeQL [SM02185] Enabling certificate revocation would be a breaking change. Customers can enable it.
            _httpClient.Timeout = Options.BackchannelTimeout;
            _httpClient.MaxResponseContentBufferSize = 1024 * 1024 * 10; // 10 MB
            _httpClient.DefaultRequestHeaders.Accept.ParseAdd("*/*");
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Microsoft Owin Twitter middleware");
            _httpClient.DefaultRequestHeaders.ExpectContinue = false;
        }

        /// <summary>
        /// Provides the <see cref="AuthenticationHandler"/> object for processing authentication-related requests.
        /// </summary>
        /// <returns>An <see cref="AuthenticationHandler"/> configured with the <see cref="TwitterAuthenticationOptions"/> supplied to the constructor.</returns>
        protected override AuthenticationHandler<TwitterAuthenticationOptions> CreateHandler()
        {
            return new TwitterAuthenticationHandler(_httpClient, _logger);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Managed by caller")]
        private static HttpMessageHandler ResolveHttpMessageHandler(TwitterAuthenticationOptions options)
        {
            HttpMessageHandler handler = options.BackchannelHttpHandler ?? new WebRequestHandler();

            // Set the cert validate callback
            var webRequestHandler = handler as WebRequestHandler;
            if (webRequestHandler == null)
            {
                if (options.BackchannelCertificateValidator != null)
                {
                    throw new InvalidOperationException(Resources.Exception_ValidatorHandlerMismatch);
                }
            }
            else if (options.BackchannelCertificateValidator != null)
            {
                webRequestHandler.ServerCertificateValidationCallback = options.BackchannelCertificateValidator.Validate; // CodeQL [SM03786] False positive, not disabled by default. Used for testing and extensibility.
            }

            return handler;
        }
    }
}
