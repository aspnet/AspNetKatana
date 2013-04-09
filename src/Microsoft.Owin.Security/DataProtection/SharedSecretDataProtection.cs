using System;
using System.Security.Cryptography;
using System.Threading;

namespace Microsoft.Owin.Security.DataProtection
{
    public class SharedSecretDataProtection : IDataProtection
    {
        private static readonly RNGCryptoServiceProvider Rng = new RNGCryptoServiceProvider();
        private readonly SymmetricAlgorithm _symmetricAlgorithm;
        private readonly HashAlgorithm _hashAlgorithm;
        private readonly object _generateIvLock = new object();

        public SharedSecretDataProtection(SymmetricAlgorithm symmetricAlgorithm, HashAlgorithm hashAlgorithm)
        {
            _symmetricAlgorithm = symmetricAlgorithm;
            _hashAlgorithm = hashAlgorithm;
        }

        public byte[] Protect(byte[] userData)
        {
            var ivLength = _symmetricAlgorithm.IV.Length;
            var hashLength = _hashAlgorithm.HashSize / 8;

            var userDataLength = userData.Length;

            byte[] key = _symmetricAlgorithm.Key;
            byte[] iv = new byte[ivLength];
            Rng.GetBytes(iv);

            var encryptor = _symmetricAlgorithm.CreateEncryptor(key, iv);

            var inputBlockLength = encryptor.InputBlockSize;
            var outputBlockLength = encryptor.OutputBlockSize;

            var fullBlockCount = userDataLength / inputBlockLength;

            byte[] outputBuffer = new byte[fullBlockCount * outputBlockLength];
            int inputOffset = 0;
            int outputOffset = 0;
            while (inputOffset + inputBlockLength <= userDataLength)
            {
                var outputWritten = encryptor.TransformBlock(userData, inputOffset, inputBlockLength, outputBuffer, outputOffset);
                inputOffset += inputBlockLength;
                outputOffset += outputWritten;
            }

            var finalBuffer = encryptor.TransformFinalBlock(userData, inputOffset, userDataLength - inputOffset);

            var protectedLength = ivLength + outputOffset + finalBuffer.Length + hashLength;
            var protectedData = new byte[protectedLength];

            var mover = new DataMover
            {
                ArraySegment = new ArraySegment<byte>(protectedData)
            };
            mover.Copy(iv);
            mover.Copy(outputBuffer, 0, outputOffset);
            mover.Copy(finalBuffer);

            var hash = _hashAlgorithm.ComputeHash(protectedData, 0, mover.ArraySegment.Offset);
            mover.Copy(hash);

            return protectedData;
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            try
            {
                var ivLength = _symmetricAlgorithm.IV.Length;
                var ivOffset = 0;

                var hashLength = _hashAlgorithm.HashSize / 8;
                var hashOffset = protectedData.Length - hashLength;

                var cipherLength = protectedData.Length - ivLength - hashLength;
                var cipherOffset = ivLength;

                var hash = _hashAlgorithm.ComputeHash(protectedData, ivOffset, ivLength + cipherLength);
                if (hash.Length != hashLength)
                {
                    throw new Exception();
                }
                for (var hashIndex = 0; hashIndex != hashLength; ++hashIndex)
                {
                    if (protectedData[hashOffset + hashIndex] != hash[hashIndex])
                    {
                        throw new Exception();
                    }
                }

                var iv = new byte[ivLength];
                Array.Copy(protectedData, 0, iv, 0, ivLength);

                var decryptor = _symmetricAlgorithm.CreateDecryptor(_symmetricAlgorithm.Key, iv);

                var inputBlockLength = decryptor.InputBlockSize;
                var outputBlockLength = decryptor.OutputBlockSize;

                var fullBlockCount = cipherLength / inputBlockLength;
                byte[] outputBuffer = new byte[fullBlockCount * outputBlockLength];

                int inputOffset = 0;
                int outputOffset = 0;

                while (inputOffset + inputBlockLength <= cipherLength)
                {
                    var outputWritten = decryptor.TransformBlock(protectedData, cipherOffset + inputOffset, inputBlockLength, outputBuffer, outputOffset);
                    inputOffset += inputBlockLength;
                    outputOffset += outputWritten;
                }

                var finalBuffer = decryptor.TransformFinalBlock(protectedData, cipherOffset + inputOffset, cipherLength - inputOffset);

                var userData = new byte[outputOffset + finalBuffer.Length];

                var mover = new DataMover { ArraySegment = new ArraySegment<byte>(userData) };
                mover.Copy(outputBuffer, 0, outputOffset);
                mover.Copy(finalBuffer);
                return userData;
            }
            catch
            {
                // random 5-25ms delay appended to all failures
                // hurts the threadpool, worth doing?
                // probably should make Unprotect async to mitigate a DOS attack?
                var bytes = new byte[2];
                Rng.GetBytes(bytes);
                Thread.Sleep(5 + ((bytes[1] * 256 + bytes[0]) % 20)); 
                return null;
            }
        }

        struct DataMover
        {
            public ArraySegment<byte> ArraySegment;

            public ArraySegment<byte> Copy(byte[] buffer)
            {
                return Copy(buffer, 0, buffer.Length);
            }

            public ArraySegment<byte> Copy(byte[] buffer, int offset, int count)
            {
                Array.Copy(buffer, offset, ArraySegment.Array, ArraySegment.Offset, count);
                ArraySegment = new ArraySegment<byte>(ArraySegment.Array, ArraySegment.Offset + count, ArraySegment.Count - count);
                return new ArraySegment<byte>(ArraySegment.Array, ArraySegment.Offset - count, count);
            }
        }
    }
}