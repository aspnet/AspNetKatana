// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens;
using System.IO;
using System.Net.Http;
using System.Xml;
using Microsoft.IdentityModel.Protocols.WsFederation;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Owin.Security.ActiveDirectory
{
    /// <summary>
    /// Helper for parsing WSFed metadata.
    /// </summary>
    internal static class WsFedMetadataRetriever
    {
        private static readonly XmlReaderSettings SafeSettings = new XmlReaderSettings { XmlResolver = null, DtdProcessing = DtdProcessing.Prohibit, ValidationType = ValidationType.None };

        public static IssuerSigningKeys GetSigningKeys(string metadataEndpoint, TimeSpan backchannelTimeout, HttpMessageHandler backchannelHttpHandler)
        {
            using (var metadataRequest = new HttpClient(backchannelHttpHandler, false))
            {
                metadataRequest.Timeout = backchannelTimeout;
                using (HttpResponseMessage metadataResponse = metadataRequest.GetAsync(metadataEndpoint).Result)
                {
                    metadataResponse.EnsureSuccessStatusCode();
                    Stream metadataStream = metadataResponse.Content.ReadAsStreamAsync().Result;
                    using (XmlReader metaDataReader = XmlReader.Create(metadataStream, SafeSettings))
                    {
                        var serializer = new WsFederationMetadataSerializer();
                        var wsFederationConfiguration = serializer.ReadMetadata(metaDataReader);
                        var x509SecurityTokens = new Collection<System.IdentityModel.Tokens.X509SecurityToken>();

                        var issuerSigningKeys = new IssuerSigningKeys();
                        issuerSigningKeys.Issuer = wsFederationConfiguration.Issuer;

                        foreach (var key in wsFederationConfiguration.SigningKeys)
                        {
                            var x509SecurityKey = key as X509SecurityKey;

                            if (x509SecurityKey != null)
                            {
                                x509SecurityTokens.Add(new X509SecurityToken(x509SecurityKey.Certificate));
                            }
                        }

                        return new IssuerSigningKeys { Issuer = wsFederationConfiguration.Issuer, Tokens = x509SecurityTokens };
                    }
                }
            }
        }
    }
}
