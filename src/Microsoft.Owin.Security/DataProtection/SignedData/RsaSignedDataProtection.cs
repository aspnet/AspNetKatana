using System;
using System.Security.Cryptography;

namespace Microsoft.Owin.Security.DataProtection
{
    public class RsaSignedDataProtection : IDataProtection
    {
        private readonly RSACryptoServiceProvider _rsa;
        private readonly IDataProtection _dataProtection;

        public RsaSignedDataProtection(RSACryptoServiceProvider rsa, IDataProtection dataProtection)
        {
            _rsa = rsa;
            _dataProtection = dataProtection;
        }

        public byte[] Protect(byte[] userData)
        {
            var signature = _rsa.SignData(userData, "SHA1");
            var combined = new byte[signature.Length + userData.Length];

            Array.Copy(signature, 0, combined, 0, signature.Length);
            Array.Copy(userData, 0, combined, signature.Length, userData.Length);

            return _dataProtection.Protect(combined);
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            var combined = _dataProtection.Unprotect(protectedData);
            if (combined == null)
            {
                return null;
            }

            var signatureLength = _rsa.KeySize / 8;
            var signature = new byte[signatureLength];
            var userData = new byte[combined.Length - signature.Length];

            Array.Copy(combined, 0, signature, 0, signature.Length);
            Array.Copy(combined, signature.Length, userData, 0, userData.Length);

            return _rsa.VerifyData(userData, "SHA1", signature) ? userData : null;
        }
    }
}