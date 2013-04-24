// <copyright file="ProtectionHandler.cs" company="Microsoft Open Technologies, Inc.">
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
using Microsoft.Owin.Security.ModelSerializer;
using Microsoft.Owin.Security.TextEncoding;

namespace Microsoft.Owin.Security
{
    public class ProtectionHandler<TModel> : IProtectionHandler<TModel>
    {
        private readonly IModelSerializer<TModel> _modelSerializer;
        private readonly IDataProtection _dataProtection;
        private readonly ITextEncoding _textEncoding;

        public ProtectionHandler(IModelSerializer<TModel> modelSerializer, IDataProtection dataProtection, ITextEncoding textEncoding)
        {
            _modelSerializer = modelSerializer;
            _dataProtection = dataProtection;
            _textEncoding = textEncoding;
        }

        public string ProtectModel(TModel model)
        {
            byte[] userData = _modelSerializer.Serialize(model);
            byte[] protectedData = _dataProtection.Protect(userData);
            string protectedText = _textEncoding.Encode(protectedData);
            return protectedText;
        }

        public TModel UnprotectModel(string protectedText)
        {
            try
            {
                if (protectedText == null)
                {
                    return default(TModel);
                }

                byte[] protectedData = _textEncoding.Decode(protectedText);
                if (protectedData == null)
                {
                    return default(TModel);
                }

                byte[] userData = _dataProtection.Unprotect(protectedData);
                if (userData == null)
                {
                    return default(TModel);
                }

                TModel model = _modelSerializer.Deserialize(userData);
                return model;
            }
            catch
            {
                // TODO trace exception, but do not leak other information
                return default(TModel);
            }
        }
    }
}
