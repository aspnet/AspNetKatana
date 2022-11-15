// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.WsFederation;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Saml;
using Microsoft.IdentityModel.Tokens.Saml2;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Infrastructure;
using Owin;

namespace Microsoft.Owin.Security.WsFederation
{
    /// <summary>
    /// OWIN middleware for obtaining identities using WsFederation protocol.
    /// </summary>
    public class WsFederationAuthenticationMiddleware : AuthenticationMiddleware<WsFederationAuthenticationOptions>
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a <see cref="WsFederationAuthenticationMiddleware"/>
        /// </summary>
        /// <param name="next">The next middleware in the OWIN pipeline to invoke</param>
        /// <param name="app">The OWIN application</param>
        /// <param name="options">Configuration options for the middleware</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "A reference is maintained.")]
        public WsFederationAuthenticationMiddleware(OwinMiddleware next, IAppBuilder app, WsFederationAuthenticationOptions options)
            : base(next, options)
        {
            _logger = app.CreateLogger<WsFederationAuthenticationMiddleware>();

            if (string.IsNullOrWhiteSpace(Options.TokenValidationParameters.AuthenticationType))
            {
                Options.TokenValidationParameters.AuthenticationType = app.GetDefaultSignInAsAuthenticationType();
            }

            if (Options.StateDataFormat == null)
            {
                var dataProtector = app.CreateDataProtector(
                    typeof(WsFederationAuthenticationMiddleware).FullName,
                    Options.AuthenticationType, "v1");
                Options.StateDataFormat = new PropertiesDataFormat(dataProtector);
            }

            if (Options.Notifications == null)
            {
                Options.Notifications = new WsFederationAuthenticationNotifications();
            }

            Uri wreply;
            if (!Options.CallbackPath.HasValue && !string.IsNullOrEmpty(Options.Wreply) && Uri.TryCreate(Options.Wreply, UriKind.Absolute, out wreply))
            {
                // Wreply must be a very specific, case sensitive value, so we can't generate it. Instead we generate CallbackPath from it.
                Options.CallbackPath = PathString.FromUriComponent(wreply);
            }

            if (Options.ConfigurationManager == null)
            {
                if (Options.Configuration != null)
                {
                    Options.ConfigurationManager = new StaticConfigurationManager<WsFederationConfiguration>(Options.Configuration);
                }
                else
                {
                    HttpClient httpClient = new HttpClient(ResolveHttpMessageHandler(Options)); // CodeQL [SM02185] Enabling certificate revocation would be a breaking change. Customers can enable it.
                    httpClient.Timeout = Options.BackchannelTimeout;
                    httpClient.MaxResponseContentBufferSize = 1024 * 1024 * 10; // 10 MB
                    Options.ConfigurationManager = new ConfigurationManager<WsFederationConfiguration>(Options.MetadataAddress,
                        new WsFederationConfigurationRetriever(), httpClient);
                }
            }
        }

        /// <summary>
        /// Provides the <see cref="AuthenticationHandler"/> object for processing authentication-related requests.
        /// </summary>
        /// <returns>An <see cref="AuthenticationHandler"/> configured with the <see cref="WsFederationAuthenticationOptions"/> supplied to the constructor.</returns>
        protected override AuthenticationHandler<WsFederationAuthenticationOptions> CreateHandler()
        {
            return new WsFederationAuthenticationHandler(_logger);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Managed by caller")]
        private static HttpMessageHandler ResolveHttpMessageHandler(WsFederationAuthenticationOptions options)
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
                webRequestHandler.ServerCertificateValidationCallback = options.BackchannelCertificateValidator.Validate; // CodeQL [SM03786] False positive, not disabled by default. Used for testing and extensibility.
            }

            return handler;
        }
    }
}