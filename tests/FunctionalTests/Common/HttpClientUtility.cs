// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;

namespace FunctionalTests.Common
{
    internal class HttpClientUtility
    {
        public static string GetResponseTextFromUrl(string url, List<KeyValuePair<string, string[]>> additionalHeaders = null)
        {
            HttpResponseMessage httpResponseMessage = null;
            return RetrieveResponseTextFromUrl(HttpMethod.Get, url, out httpResponseMessage, additionalHeaders);
        }

        public static string GetResponseTextFromUrl(string url, out HttpResponseMessage httpResponseMessage, List<KeyValuePair<string, string[]>> additionalHeaders = null)
        {
            return RetrieveResponseTextFromUrl(HttpMethod.Get, url, out httpResponseMessage, additionalHeaders);
        }

        public static string HeadResponseTextFromUrl(string url, List<KeyValuePair<string, string[]>> additionalHeaders = null)
        {
            HttpResponseMessage httpResponseMessage = null;
            return RetrieveResponseTextFromUrl(HttpMethod.Head, url, out httpResponseMessage, additionalHeaders);
        }

        public static string HeadResponseTextFromUrl(string url, out HttpResponseMessage httpResponseMessage, List<KeyValuePair<string, string[]>> additionalHeaders = null)
        {
            return RetrieveResponseTextFromUrl(HttpMethod.Head, url, out httpResponseMessage, additionalHeaders);
        }

        public static string PostResponseTextFromUrl(string url, List<KeyValuePair<string, string[]>> additionalHeaders = null)
        {
            HttpResponseMessage httpResponseMessage = null;
            return RetrieveResponseTextFromUrl(HttpMethod.Post, url, out httpResponseMessage, additionalHeaders);
        }

        public static string PostResponseTextFromUrl(string url, out HttpResponseMessage httpResponseMessage, List<KeyValuePair<string, string[]>> additionalHeaders = null)
        {
            return RetrieveResponseTextFromUrl(HttpMethod.Post, url, out httpResponseMessage, additionalHeaders);
        }

        private static string RetrieveResponseTextFromUrl(HttpMethod httpMethod, string url, out HttpResponseMessage httpResponseMessage, List<KeyValuePair<string, string[]>> additionalHeaders)
        {
            Trace.WriteLine(string.Format("Accessing the URL : {0}", url));
            HttpClient client = new HttpClient();

            if (additionalHeaders != null)
            {
                additionalHeaders.ForEach((kvp) =>
                {
                    string inputHeaderValue = string.Join(", ", kvp.Value);
                    Trace.WriteLine(string.Format("Adding additional input HTTP header : {0} with values : {1}", kvp.Key, inputHeaderValue));
                    client.DefaultRequestHeaders.Add(kvp.Key, string.Join(", ", inputHeaderValue));
                });
            }

            switch (httpMethod.ToString())
            {
                case "GET":
                    httpResponseMessage = client.GetAsync(url).Result;
                    break;
                case "HEAD":
                    var requestMessage = new HttpRequestMessage(HttpMethod.Head, url);
                    httpResponseMessage = client.SendAsync(requestMessage).Result;
                    break;
                case "POST":
                    httpResponseMessage = client.PostAsync(url, new StringContent("Test body")).Result;
                    break;
                default:
                    throw new NotImplementedException(string.Format("Utility code not implemented for this Http method {0}", httpMethod));
            }

            string responseText = httpResponseMessage.Content.ReadAsStringAsync().Result;
            Trace.WriteLine(string.Format("Response Text: {0}", responseText));
            return responseText;
        }
    }
}