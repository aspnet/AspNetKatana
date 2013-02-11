// <copyright file="OwinResponse.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2013 Microsoft Open Technologies, Inc. All rights reserved.
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SendFileAsyncDelegate = System.Func<string, long, long?, System.Threading.CancellationToken, System.Threading.Tasks.Task>;

namespace Owin.Types
{
#region OwinResponse

    internal partial struct OwinResponse
    {
        public OwinResponse(OwinRequest request)
        {
            _dictionary = request.Dictionary;
        }
    }
#endregion

#region OwinResponse.Generated

    [System.CodeDom.Compiler.GeneratedCode("App_Packages", "")]
    internal partial struct OwinResponse
    {
        private readonly IDictionary<string, object> _dictionary;

        public OwinResponse(IDictionary<string, object> dictionary)
        {
            _dictionary = dictionary;
        }

        public IDictionary<string, object> Dictionary
        {
            get { return _dictionary; }
        }

#region Value-type equality
        public bool Equals(OwinResponse other)
        {
            return Equals(_dictionary, other._dictionary);
        }

        public override bool Equals(object obj)
        {
            return obj is OwinResponse && Equals((OwinResponse)obj);
        }

        public override int GetHashCode()
        {
            return (_dictionary != null ? _dictionary.GetHashCode() : 0);
        }

        public static bool operator ==(OwinResponse left, OwinResponse right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(OwinResponse left, OwinResponse right)
        {
            return !left.Equals(right);
        }
#endregion

        public T Get<T>(string key)
        {
            object value;
            return _dictionary.TryGetValue(key, out value) ? (T)value : default(T);
        }

        public OwinResponse Set(string key, object value)
        {
            _dictionary[key] = value;
            return this;
        }


        public string GetHeader(string key)
        {
            return Helpers.OwinHelpers.GetHeader(Headers, key);
        }

        public IEnumerable<string> GetHeaderSplit(string key)
        {
            return Helpers.OwinHelpers.GetHeaderSplit(Headers, key);
        }

        public string[] GetHeaderUnmodified(string key)
        {
            return Helpers.OwinHelpers.GetHeaderUnmodified(Headers, key);
        }

        public OwinResponse SetHeader(string key, string value)
        {
            Helpers.OwinHelpers.SetHeader(Headers, key, value);
            return this;
        }

        public OwinResponse SetHeaderJoined(string key, params string[] values)
        {
            Helpers.OwinHelpers.SetHeaderJoined(Headers, key, values);
            return this;
        }

        public OwinResponse SetHeaderJoined(string key, IEnumerable<string> values)
        {
            Helpers.OwinHelpers.SetHeaderJoined(Headers, key, values);
            return this;
        }

        public OwinResponse SetHeaderUnmodified(string key, params string[] values)
        {
            Helpers.OwinHelpers.SetHeaderUnmodified(Headers, key, values);
            return this;
        }

        public OwinResponse SetHeaderUnmodified(string key, IEnumerable<string> values)
        {
            Helpers.OwinHelpers.SetHeaderUnmodified(Headers, key, values);
            return this;
        }

        public OwinResponse AddHeader(string key, string value)
        {
            Helpers.OwinHelpers.AddHeader(Headers, key, value);
            return this;
        }

        public OwinResponse AddHeaderJoined(string key, params string[] values)
        {
            Helpers.OwinHelpers.AddHeaderJoined(Headers, key, values);
            return this;
        }

        public OwinResponse AddHeaderJoined(string key, IEnumerable<string> values)
        {
            Helpers.OwinHelpers.AddHeaderJoined(Headers, key, values);
            return this;
        }

        public OwinResponse AddHeaderUnmodified(string key, params string[] values)
        {
            Helpers.OwinHelpers.AddHeaderUnmodified(Headers, key, values);
            return this;
        }

        public OwinResponse AddHeaderUnmodified(string key, IEnumerable<string> values)
        {
            Helpers.OwinHelpers.AddHeaderUnmodified(Headers, key, values);
            return this;
        }
    }
#endregion

#region OwinResponse.Spec-Owin

    internal partial struct OwinResponse
    {
        public string OwinVersion
        {
            get { return Get<string>(OwinConstants.OwinVersion); }
            set { Set(OwinConstants.OwinVersion, value); }
        }

        public CancellationToken CallCancelled
        {
            get { return Get<CancellationToken>(OwinConstants.CallCancelled); }
            set { Set(OwinConstants.CallCancelled, value); }
        }

        public int StatusCode
        {
            get { return Get<int>(OwinConstants.ResponseStatusCode); }
            set { Set(OwinConstants.ResponseStatusCode, value); }
        }

        public string ReasonPhrase
        {
            get { return Get<string>(OwinConstants.ResponseReasonPhrase); }
            set { Set(OwinConstants.ResponseReasonPhrase, value); }
        }

        public string Protocol
        {
            get { return Get<string>(OwinConstants.ResponseProtocol); }
            set { Set(OwinConstants.ResponseProtocol, value); }
        }

        public IDictionary<string, string[]> Headers
        {
            get { return Get<IDictionary<string, string[]>>(OwinConstants.ResponseHeaders); }
            set { Set(OwinConstants.ResponseHeaders, value); }
        }

        public Stream Body
        {
            get { return Get<Stream>(OwinConstants.ResponseBody); }
            set { Set(OwinConstants.ResponseBody, value); }
        }
    }
#endregion

#region OwinResponse.Spec-SendFile

    internal partial struct OwinResponse
    {
        public bool CanSendFile
        {
            get { return SendFileAsyncDelegate != null; }
        }

        public SendFileAsyncDelegate SendFileAsyncDelegate
        {
            get { return Get<SendFileAsyncDelegate>(OwinConstants.SendFiles.SendAsync); }
            set { Set(OwinConstants.SendFiles.SendAsync, value); }
        }

        public Task SendFileAsync(string filePath, long offset, long? count, CancellationToken cancel)
        {
            var sendFile = SendFileAsyncDelegate;
            if (sendFile == null)
            {
                throw new NotSupportedException(OwinConstants.SendFiles.SendAsync);
            }
            return sendFile.Invoke(filePath, offset, count, cancel);
        }

        public Task SendFileAsync(string filePath, long offset, long? count)
        {
            return SendFileAsync(filePath, offset, count, CancellationToken.None);
        }

        public Task SendFileAsync(string filePath, CancellationToken cancel)
        {
            return SendFileAsync(filePath, 0, null, cancel);
        }

        public Task SendFileAsync(string filePath)
        {
            return SendFileAsync(filePath, 0, null, CancellationToken.None);
        }
    }
#endregion

}
