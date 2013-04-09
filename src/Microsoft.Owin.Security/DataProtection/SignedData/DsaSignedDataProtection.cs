using System;
using System.Security.Cryptography;

namespace Microsoft.Owin.Security.DataProtection
{
    public class DsaSignedDataProtection : IDataProtection
    {
        private readonly DSACryptoServiceProvider _dsa;
        private readonly IDataProtection _dataProtection;

        public DsaSignedDataProtection(DSACryptoServiceProvider dsa, IDataProtection dataProtection)
        {
            _dsa = dsa;
            _dataProtection = dataProtection;
        }

        public byte[] Protect(byte[] userData)
        {
            const int signatureLength = 40;
            var signature = _dsa.SignData(userData);
            var combined = new byte[signatureLength + userData.Length];

            Array.Copy(signature, 0, combined, 0, signatureLength);
            Array.Copy(userData, 0, combined, signatureLength, userData.Length);

            return _dataProtection.Protect(combined);
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            var combined = _dataProtection.Unprotect(protectedData);

            const int signatureLength = 40;
            var signature = new byte[signatureLength];
            var userData = new byte[combined.Length - signature.Length];

            Array.Copy(combined, 0, signature, 0, signature.Length);
            Array.Copy(combined, signature.Length, userData, 0, userData.Length);

            return _dsa.VerifyData(userData, signature) ? userData : null;
        }
    }
}