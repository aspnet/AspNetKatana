// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.DataHandler.Serializer;
using Microsoft.Owin.Security.DataProtection;

namespace Microsoft.Owin.Security.DataHandler
{
    public class PropertiesDataFormat : SecureDataFormat<AuthenticationProperties>
    {
        public PropertiesDataFormat(IDataProtector protector)
            : base(DataSerializers.Properties, protector, TextEncodings.Base64Url)
        {
        }
    }
}
