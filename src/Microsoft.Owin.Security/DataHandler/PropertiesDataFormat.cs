// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
