// <copyright file="WindowsAzureCachingMetadataResolver.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Metadata;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;
using System.Xml;

namespace Microsoft.Owin.Security.WindowsAzure
{
    public class WindowsAzureCachingMetadataResolver : IMetadataResolver
    {
        private const string SecurityTokenServiceAddressFormat = "https://login.windows.net/{0}/federationmetadata/2007-06/federationmetadata.xml";

        private static readonly XmlReaderSettings _SafeSettings = new XmlReaderSettings { XmlResolver = null, DtdProcessing = DtdProcessing.Prohibit, ValidationType = ValidationType.None };

        private ConcurrentDictionary<string, EndpointMetadata> _metadata = new ConcurrentDictionary<string, EndpointMetadata>();

        public WindowsAzureCachingMetadataResolver()
        {
            CacheLength = new TimeSpan(1, 0, 0, 0);
        }

        public TimeSpan CacheLength
        {
            get;
            set;
        }

        public string GetIssuer(string tenant)
        {
            return GetMetadata(tenant).Issuer;
        }

        public IList<SecurityToken> GetSigningTokens(string tenant)
        {
            return GetMetadata(tenant).SigningTokens;
        }

        private EndpointMetadata GetMetadata(string tenant)
        {
            if (!_metadata.ContainsKey(tenant) || 
                _metadata[tenant].ExpiresOn < DateTime.Now)
            {
                using (var metaDataReader = XmlReader.Create(string.Format(CultureInfo.InvariantCulture, SecurityTokenServiceAddressFormat, tenant), _SafeSettings))
                {
                    var endpointMetadata = new EndpointMetadata();
                    var serializer = new MetadataSerializer()
                    {
                        CertificateValidationMode = X509CertificateValidationMode.None
                    };

                    MetadataBase metadata = serializer.ReadMetadata(metaDataReader);
                    var entityDescriptor = (EntityDescriptor)metadata;

                    if (!string.IsNullOrWhiteSpace(entityDescriptor.EntityId.Id))
                    {
                        endpointMetadata.Issuer = entityDescriptor.EntityId.Id;
                    }

                    var tokens = new List<SecurityToken>();
                    var stsd = entityDescriptor.RoleDescriptors.OfType<SecurityTokenServiceDescriptor>().First();
                    if (stsd == null)
                    {
                        throw new InvalidOperationException("No SecurityTokenServiceType descriptor in metadata.");
                    }

                    IEnumerable<X509RawDataKeyIdentifierClause> x509DataClauses = stsd.Keys.Where(key => key.KeyInfo != null && (key.Use == KeyType.Signing || key.Use == KeyType.Unspecified)).Select(key => key.KeyInfo.OfType<X509RawDataKeyIdentifierClause>().First());
                    tokens.AddRange(x509DataClauses.Select(token => new X509SecurityToken(new X509Certificate2(token.GetX509RawData()))));

                    endpointMetadata.SigningTokens = tokens.AsReadOnly();
                    endpointMetadata.ExpiresOn = DateTime.Now.Add(CacheLength);

                    lock (_metadata)
                    {
                        _metadata[tenant] = endpointMetadata;
                    }
                }
            }

            return _metadata[tenant];
        }
    }
}
