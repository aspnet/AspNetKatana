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
    public class CachingMetadataResolver : IMetadataResolver
    {
        private const string SecurityTokenServiceAddressFormat = "https://login.windows.net/{0}/federationmetadata/2007-06/federationmetadata.xml";

        private static readonly XmlReaderSettings _SafeSettings = new XmlReaderSettings { XmlResolver = null, DtdProcessing = DtdProcessing.Prohibit, ValidationType = ValidationType.None };

        private ConcurrentDictionary<string, EndpointMetadata> _metadata = new ConcurrentDictionary<string, EndpointMetadata>();

        public CachingMetadataResolver()
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

        public IList<X509SecurityToken> GetSigningTokens(string tenant)
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

                    var tokens = new List<X509SecurityToken>();
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
