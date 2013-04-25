// <copyright file="SecureDataHandler.cs" company="Microsoft Open Technologies, Inc.">
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

using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.DataSerializer;
using Microsoft.Owin.Security.TextEncoding;

namespace Microsoft.Owin.Security
{
    public class SecureDataHandler<TData> : ISecureDataHandler<TData>
    {
        private readonly IDataSerializer<TData> _serializer;
        private readonly IDataProtecter _protecter;
        private readonly ITextEncoder _encoder;

        public SecureDataHandler(IDataSerializer<TData> serializer, IDataProtecter protecter, ITextEncoder encoder)
        {
            _serializer = serializer;
            _protecter = protecter;
            _encoder = encoder;
        }

        public string Protect(TData data)
        {
            byte[] userData = _serializer.Serialize(data);
            byte[] protectedData = _protecter.Protect(userData);
            string protectedText = _encoder.Encode(protectedData);
            return protectedText;
        }

        public TData Unprotect(string protectedText)
        {
            try
            {
                if (protectedText == null)
                {
                    return default(TData);
                }

                byte[] protectedData = _encoder.Decode(protectedText);
                if (protectedData == null)
                {
                    return default(TData);
                }

                byte[] userData = _protecter.Unprotect(protectedData);
                if (userData == null)
                {
                    return default(TData);
                }

                TData model = _serializer.Deserialize(userData);
                return model;
            }
            catch
            {
                // TODO trace exception, but do not leak other information
                return default(TData);
            }
        }
    }
}
