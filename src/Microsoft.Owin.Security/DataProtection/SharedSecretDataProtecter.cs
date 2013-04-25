// <copyright file="SharedSecretDataProtection.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Security.Cryptography;
using System.Threading;

namespace Microsoft.Owin.Security.DataProtection
{
    internal class SharedSecretDataProtecter : IDataProtecter
    {
        private static readonly RNGCryptoServiceProvider Rng = new RNGCryptoServiceProvider();
        private readonly SymmetricAlgorithm _symmetricAlgorithm;
        private readonly HashAlgorithm _hashAlgorithm;
        private readonly object _generateIvLock = new object();

        public SharedSecretDataProtecter(SymmetricAlgorithm symmetricAlgorithm, HashAlgorithm hashAlgorithm)
        {
            _symmetricAlgorithm = symmetricAlgorithm;
            _hashAlgorithm = hashAlgorithm;
        }

        public byte[] Protect(byte[] userData)
        {
            int initializationVectorLength = _symmetricAlgorithm.IV.Length;
            int hashLength = _hashAlgorithm.HashSize / 8;

            int userDataLength = userData.Length;

            byte[] key = _symmetricAlgorithm.Key;
            var initializationVector = new byte[initializationVectorLength];
            Rng.GetBytes(initializationVector);

            ICryptoTransform encryptor = _symmetricAlgorithm.CreateEncryptor(key, initializationVector);

            int inputBlockLength = encryptor.InputBlockSize;
            int outputBlockLength = encryptor.OutputBlockSize;

            int fullBlockCount = userDataLength / inputBlockLength;

            var outputBuffer = new byte[fullBlockCount * outputBlockLength];
            int inputOffset = 0;
            int outputOffset = 0;
            while (inputOffset + inputBlockLength <= userDataLength)
            {
                int outputWritten = encryptor.TransformBlock(userData, inputOffset, inputBlockLength, outputBuffer, outputOffset);
                inputOffset += inputBlockLength;
                outputOffset += outputWritten;
            }

            byte[] finalBuffer = encryptor.TransformFinalBlock(userData, inputOffset, userDataLength - inputOffset);

            int protectedLength = initializationVectorLength + outputOffset + finalBuffer.Length + hashLength;
            var protectedData = new byte[protectedLength];

            var mover = new DataMover
            {
                ArraySegment = new ArraySegment<byte>(protectedData)
            };
            mover.Copy(initializationVector);
            mover.Copy(outputBuffer, 0, outputOffset);
            mover.Copy(finalBuffer);

            byte[] hash = _hashAlgorithm.ComputeHash(protectedData, 0, mover.ArraySegment.Offset);
            mover.Copy(hash);

            return protectedData;
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            try
            {
                int initializationVectorLength = _symmetricAlgorithm.IV.Length;
                int initializationVectorOffset = 0;

                int hashLength = _hashAlgorithm.HashSize / 8;
                int hashOffset = protectedData.Length - hashLength;

                int cipherLength = protectedData.Length - initializationVectorLength - hashLength;
                int cipherOffset = initializationVectorLength;

                byte[] hash = _hashAlgorithm.ComputeHash(protectedData, initializationVectorOffset, initializationVectorLength + cipherLength);
                if (hash.Length != hashLength)
                {
                    throw new Exception();
                }
                for (int hashIndex = 0; hashIndex != hashLength; ++hashIndex)
                {
                    if (protectedData[hashOffset + hashIndex] != hash[hashIndex])
                    {
                        throw new Exception();
                    }
                }

                var iv = new byte[initializationVectorLength];
                Array.Copy(protectedData, 0, iv, 0, initializationVectorLength);

                ICryptoTransform decryptor = _symmetricAlgorithm.CreateDecryptor(_symmetricAlgorithm.Key, iv);

                int inputBlockLength = decryptor.InputBlockSize;
                int outputBlockLength = decryptor.OutputBlockSize;

                int fullBlockCount = cipherLength / inputBlockLength;
                var outputBuffer = new byte[fullBlockCount * outputBlockLength];

                int inputOffset = 0;
                int outputOffset = 0;

                while (inputOffset + inputBlockLength <= cipherLength)
                {
                    int outputWritten = decryptor.TransformBlock(protectedData, cipherOffset + inputOffset, inputBlockLength, outputBuffer, outputOffset);
                    inputOffset += inputBlockLength;
                    outputOffset += outputWritten;
                }

                byte[] finalBuffer = decryptor.TransformFinalBlock(protectedData, cipherOffset + inputOffset, cipherLength - inputOffset);

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
                Thread.Sleep(5 + (((bytes[1] * 256) + bytes[0]) % 20));
                return null;
            }
        }

        private struct DataMover
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
