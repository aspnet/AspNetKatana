// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;

namespace FunctionalTests.Facts.Security.AuthServer
{
    public class AuthZ
    {
        public static string CreateAuthZUri(string baseUri, string response_type, string client_id, string redirect_uri, string scope, string state)
        {
            return CreateUri(baseUri + "AuthorizeEndpoint", response_type, client_id, redirect_uri, scope, state);
        }

        private static string CreateUri(string baseUri, string response_type, string client_id, string redirect_uri, string scope, string state)
        {
            List<string> queryParameters = new List<string>();

            Action<string, string> AddQueryValue = (parameterName, parameterValue) =>
            {
                if (!string.IsNullOrWhiteSpace(parameterValue))
                {
                    queryParameters.Add(string.Format("{0}={1}", parameterName, parameterValue));
                }
            };

            AddQueryValue("response_type", response_type);
            AddQueryValue("client_id", client_id);
            AddQueryValue("redirect_uri", redirect_uri);
            AddQueryValue("scope", scope);
            AddQueryValue("state", state);

            return string.Format("{0}?{1}", baseUri, string.Join("&", queryParameters.ToArray()));
        }

        public static FormUrlEncodedContent CreateTokenEndpointContent(IList<KeyValuePair<string, string>> kvps)
        {
            return new FormUrlEncodedContent(kvps);
        }
    }
}
