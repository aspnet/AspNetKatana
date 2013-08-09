// <copyright file="WsFedMetadataRetriver.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
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
// </copyright>

using System;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;
using System.Xml;

namespace Microsoft.Owin.Security.ActiveDirectory
{
    /// <summary>
    /// Helper for parsing WSFed metadata.
    /// </summary>
    internal static class WsFedMetadataRetriver
    {
        private static readonly XmlReaderSettings SafeSettings = new XmlReaderSettings { XmlResolver = null, DtdProcessing = DtdProcessing.Prohibit, ValidationType = ValidationType.None };

        /// <summary>
        /// Gets the issuer and signing keys from a WSFed metadata endpoint.
        /// </summary>
        /// <param name="metadataEndpoint">The metadata endpoint.</param>
        /// <param name="validateMetadataEndpointCertificate">Value indicating if the endpoint certificate should be validated.</param>
        /// <returns>The issuer and signing keys extracted from the WSFed metadata.</returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public static IssuerSigningKeys GetSigningKeys(string metadataEndpoint, bool validateMetadataEndpointCertificate)
        {
            string issuer = string.Empty;
            var tokens = new List<X509SecurityToken>();

            using (var handler = new WebRequestHandler())
            {
                if (validateMetadataEndpointCertificate)
                {
                    handler.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
                }

                var metadataRequest = new HttpClient(handler);
                HttpResponseMessage metadataResponse = metadataRequest.GetAsync(metadataEndpoint).Result;
                metadataResponse.EnsureSuccessStatusCode();

                using (XmlReader metaDataReader = XmlReader.Create(metadataResponse.Content.ReadAsStreamAsync().Result, SafeSettings))
                {
                    var serializer = new MetadataSerializer { CertificateValidationMode = X509CertificateValidationMode.None };

                    MetadataBase metadata = serializer.ReadMetadata(metaDataReader);
                    var entityDescriptor = (EntityDescriptor)metadata;

                    if (!string.IsNullOrWhiteSpace(entityDescriptor.EntityId.Id))
                    {
                        issuer = entityDescriptor.EntityId.Id;
                    }

                    var stsd = entityDescriptor.RoleDescriptors.OfType<SecurityTokenServiceDescriptor>().First();
                    if (stsd == null)
                    {
                        throw new InvalidOperationException(Properties.Resources.Exception_MissingDescriptor);
                    }

                    IEnumerable<X509RawDataKeyIdentifierClause> x509DataClauses = stsd.Keys.Where(key => key.KeyInfo != null && (key.Use == KeyType.Signing || key.Use == KeyType.Unspecified)).Select(key => key.KeyInfo.OfType<X509RawDataKeyIdentifierClause>().First());
                    tokens.AddRange(x509DataClauses.Select(token => new X509SecurityToken(new X509Certificate2(token.GetX509RawData()))));
                }
            }

            return new IssuerSigningKeys { Issuer = issuer, Tokens = tokens };
        }
    }
}
