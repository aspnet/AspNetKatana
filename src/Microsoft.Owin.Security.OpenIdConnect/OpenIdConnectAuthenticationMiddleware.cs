// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Infrastructure;
using Owin;

namespace Microsoft.Owin.Security.OpenIdConnect
{
    /// <summary>
    /// OWIN middleware for obtaining identities using OpenIdConnect protocol.
    /// </summary>
    public class OpenIdConnectAuthenticationMiddleware : AuthenticationMiddleware<OpenIdConnectAuthenticationOptions>
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a <see cref="OpenIdConnectAuthenticationMiddleware"/>
        /// </summary>
        /// <param name="next">The next middleware in the OWIN pipeline to invoke</param>
        /// <param name="app">The OWIN application</param>
        /// <param name="options">Configuration options for the middleware</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Managed by caller")]
        public OpenIdConnectAuthenticationMiddleware(OwinMiddleware next, IAppBuilder app, OpenIdConnectAuthenticationOptions options)
            : base(next, options)
        {
            _logger = app.CreateLogger<OpenIdConnectAuthenticationMiddleware>();

            if (string.IsNullOrWhiteSpace(Options.TokenValidationParameters.AuthenticationType))
            {
                Options.TokenValidationParameters.AuthenticationType = app.GetDefaultSignInAsAuthenticationType();
            }

            if (Options.StateDataFormat == null)
            {
                var dataProtector = app.CreateDataProtector(
                    typeof(OpenIdConnectAuthenticationMiddleware).FullName,
                    Options.AuthenticationType, "v1");
                Options.StateDataFormat = new PropertiesDataFormat(dataProtector);
            }
            
            // if the user has not set the AuthorizeCallback, set it from the redirect_uri
            if (!Options.CallbackPath.HasValue)
            {
                Uri redirectUri;
                if (!string.IsNullOrEmpty(Options.RedirectUri) && Uri.TryCreate(Options.RedirectUri, UriKind.Absolute, out redirectUri))
                {
                    // Redirect_Uri must be a very specific, case sensitive value, so we can't generate it. Instead we generate AuthorizeCallback from it.
                    Options.CallbackPath = PathString.FromUriComponent(redirectUri);
                }
            }

            if (Options.Notifications == null)
            {
                Options.Notifications = new OpenIdConnectAuthenticationNotifications();
            }

            if (string.IsNullOrWhiteSpace(Options.TokenValidationParameters.ValidAudience) && !string.IsNullOrWhiteSpace(Options.ClientId))
            {
                Options.TokenValidationParameters.ValidAudience = Options.ClientId;
            }

            if (Options.ConfigurationManager == null)
            {
                if (Options.Configuration != null)
                {
                    Options.ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(Options.Configuration);
                }
                else if (!(string.IsNullOrEmpty(Options.MetadataAddress) && string.IsNullOrEmpty(Options.Authority)))
                {
                    if (string.IsNullOrEmpty(Options.MetadataAddress) && !string.IsNullOrEmpty(Options.Authority))
                    {
                        Options.MetadataAddress = Options.Authority;
                        if (!Options.MetadataAddress.EndsWith("/", StringComparison.Ordinal))
                        {
                            Options.MetadataAddress += "/";
                        }

                        Options.MetadataAddress += ".well-known/openid-configuration";
                    }

                    if (Options.RequireHttpsMetadata && !Options.MetadataAddress.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException("The MetadataAddress or Authority must use HTTPS unless disabled for development by setting RequireHttpsMetadata=false.");
                    }

                    var backchannel = new HttpClient(ResolveHttpMessageHandler(Options));
                    backchannel.DefaultRequestHeaders.UserAgent.ParseAdd("Microsoft ASP.NET Core OpenIdConnect middleware");
                    backchannel.Timeout = Options.BackchannelTimeout;
                    backchannel.MaxResponseContentBufferSize = 1024 * 1024 * 10; // 10 MB

                    Options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(Options.MetadataAddress, new OpenIdConnectConfigurationRetriever(),
                        new HttpDocumentRetriever(backchannel) { RequireHttps = Options.RequireHttpsMetadata });
                }
            }

            if (Options.ConfigurationManager == null)
            {
                throw new InvalidOperationException(string.Format("Provide Authority, MetadataAddress, Configuration, or ConfigurationManager to OpenIdConnectAuthenticationOptions"));
            }
        }

        /// <summary>
        /// Provides the <see cref="AuthenticationHandler"/> object for processing authentication-related requests.
        /// </summary>
        /// <returns>An <see cref="AuthenticationHandler"/> configured with the <see cref="OpenIdConnectAuthenticationOptions"/> supplied to the constructor.</returns>
        protected override AuthenticationHandler<OpenIdConnectAuthenticationOptions> CreateHandler()
        {
            return new OpenIdConnectAuthenticationHandler(_logger);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Managed by caller")]
        private static HttpMessageHandler ResolveHttpMessageHandler(OpenIdConnectAuthenticationOptions options)
        {
            HttpMessageHandler handler = options.BackchannelHttpHandler ?? new WebRequestHandler();

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
