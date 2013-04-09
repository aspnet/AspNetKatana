using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Owin.Security.DataProtection
{
    public class CertSignedDataProtectionProvider : IDataProtectionProvider
    {
        private readonly X509Certificate2 _cert;
        private readonly IDataProtectionProvider _provider;

        public CertSignedDataProtectionProvider(X509Certificate2 cert, IDataProtectionProvider provider)
        {
            _cert = cert;
            _provider = provider;
        }

        public IDataProtection Create(params string[] purposes)
        {
            if (_cert.GetKeyAlgorithm() == "RSA")
            {
                if (_cert.HasPrivateKey)
                {
                    return new RsaSignedDataProtection((RSACryptoServiceProvider)_cert.PrivateKey, _provider.Create(purposes));
                }
                return new RsaSignedDataProtection((RSACryptoServiceProvider)_cert.PublicKey.Key, _provider.Create(purposes));
            }
            if (_cert.GetKeyAlgorithm() == "DSA")
            {
                if (_cert.HasPrivateKey)
                {
                    return new DsaSignedDataProtection((DSACryptoServiceProvider)_cert.PrivateKey, _provider.Create(purposes));
                }
                return new DsaSignedDataProtection((DSACryptoServiceProvider)_cert.PublicKey.Key, _provider.Create(purposes));
            }
            throw new CryptographicUnexpectedOperationException();
        }
    }
}