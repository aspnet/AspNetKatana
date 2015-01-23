// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Owin.Security;

namespace FunctionalTests.Facts.Security.Common
{
    public class CustomCertificateValidator : ICertificateValidator
    {
        public bool Validate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            var requestHeaders = ((HttpWebRequest)sender).Headers;
            if (requestHeaders["InvalidCert"] != null)
            {
                return !bool.Parse(requestHeaders["InvalidCert"]);
            }
            else
            {
                return true;
            }
        }
    }
}
