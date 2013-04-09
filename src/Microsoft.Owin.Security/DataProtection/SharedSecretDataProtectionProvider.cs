using System.IO;
using System.Security.Cryptography;

namespace Microsoft.Owin.Security.DataProtection
{
    public class SharedSecretDataProtectionProvider : IDataProtectionProvider
    {
        private readonly string _sharedSecret;
        private readonly string _symmetricAlgorithmName;
        private readonly string _hashAlgorithmName;

        public SharedSecretDataProtectionProvider(string sharedSecret, string symmetricAlgorithmName, string hashAlgorithmName)
        {
            _sharedSecret = sharedSecret;
            _symmetricAlgorithmName = symmetricAlgorithmName;
            _hashAlgorithmName = hashAlgorithmName;
        }

        public IDataProtection Create(params string[] purposes)
        {
            var salt = new MemoryStream();
            using (var writer = new BinaryWriter(salt))
            {
                foreach (var purpose in purposes)
                {
                    writer.Write(purpose);
                }
                writer.Write("Microsoft.Owin.Security.DataProtection.SharedSecretDataProtectionProvider");
            }
            
            var deriveBytes = new Rfc2898DeriveBytes(_sharedSecret, salt.ToArray());

            var symmetricAlgorithm = SymmetricAlgorithm.Create(_symmetricAlgorithmName);
            symmetricAlgorithm.Key = deriveBytes.GetBytes(symmetricAlgorithm.Key.Length);

            var hashAlgorithm = KeyedHashAlgorithm.Create(_hashAlgorithmName);
            hashAlgorithm.Key = deriveBytes.GetBytes(hashAlgorithm.Key.Length);

            return new SharedSecretDataProtection(symmetricAlgorithm, hashAlgorithm);
        }
    }
}
