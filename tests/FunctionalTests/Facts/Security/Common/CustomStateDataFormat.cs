// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Owin.Security;

namespace FunctionalTests.Facts.Security.Common
{
    public class CustomStateDataFormat : ISecureDataFormat<AuthenticationProperties>
    {
        private static string lastSavedAuthenticationProperties;
        private DataContractSerializer serializer = new DataContractSerializer(typeof(AuthenticationProperties));

        public string Protect(AuthenticationProperties data)
        {
            lastSavedAuthenticationProperties = Serialize(data);
            return "ValidStateData";
        }

        public AuthenticationProperties Unprotect(string state)
        {
            return state == "ValidStateData" ? DeSerialize(lastSavedAuthenticationProperties) : null;
        }

        private string Serialize(AuthenticationProperties data)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, data);
                memoryStream.Position = 0;
                return new StreamReader(memoryStream).ReadToEnd();
            }
        }

        private AuthenticationProperties DeSerialize(string state)
        {
            var stateDataAsBytes = Encoding.UTF8.GetBytes(state);

            using (var ms = new MemoryStream(stateDataAsBytes, false))
            {
                return (AuthenticationProperties)serializer.ReadObject(ms);
            }
        }
    }
}