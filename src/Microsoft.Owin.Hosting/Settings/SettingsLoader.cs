// <copyright file="SettingsLoader.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Microsoft.Owin.Hosting.Settings
{
    public static class SettingsLoader
    {
        private static IDictionary<string, string> _fromConfigImplementation;

        public static IDictionary<string, string> FromConfig()
        {
            return LazyInitializer.EnsureInitialized(
                ref _fromConfigImplementation,
                () => new FromConfigImplementation());
        }

        public static IDictionary<string, string> FromSettingsFile(string settingsFile)
        {
            var settings = new Dictionary<string, string>(StringComparer.Ordinal);
            FromSettingsFile(settingsFile, settings);
            return settings;
        }

        public static void FromSettingsFile(string settingsFile, IDictionary<string, string> settings)
        {
            if (settingsFile == null)
            {
                throw new ArgumentNullException("settingsFile");
            }
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }
            using (var streamReader = new StreamReader(settingsFile))
            {
                while (true)
                {
                    string line = streamReader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    if (line.StartsWith("#", StringComparison.Ordinal) ||
                        string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    int delimiterIndex = line.IndexOf('=');
                    string name = line.Substring(0, delimiterIndex).Trim();
                    string value = line.Substring(delimiterIndex + 1).Trim();
                    settings[name] = value;
                }
            }
        }

        private class FromConfigImplementation : IDictionary<string, string>
        {
            private readonly NameValueCollection _appSettings;

            public FromConfigImplementation()
            {
                Type configurationManagerType = Type.GetType("System.Configuration.ConfigurationManager, System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                PropertyInfo appSettingsProperty = configurationManagerType.GetProperty("AppSettings");
                _appSettings = (NameValueCollection)appSettingsProperty.GetValue(null, new object[0]);
            }

            public int Count
            {
                get { throw new System.NotImplementedException(); }
            }

            public bool IsReadOnly
            {
                get { throw new System.NotImplementedException(); }
            }

            public ICollection<string> Keys
            {
                get { throw new System.NotImplementedException(); }
            }

            public ICollection<string> Values
            {
                get { throw new System.NotImplementedException(); }
            }

            public string this[string key]
            {
                get { return _appSettings[key]; }
                set { throw new System.NotImplementedException(); }
            }

            public bool TryGetValue(string key, out string value)
            {
                value = _appSettings[key];
                return value != null;
            }

            #region Implementation of IEnumerable

            public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            {
                throw new System.NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion

            #region Implementation of ICollection<KeyValuePair<string,string>>

            public void Add(KeyValuePair<string, string> item)
            {
                throw new System.NotImplementedException();
            }

            public void Clear()
            {
                throw new System.NotImplementedException();
            }

            public bool Contains(KeyValuePair<string, string> item)
            {
                throw new System.NotImplementedException();
            }

            public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
            {
                throw new System.NotImplementedException();
            }

            public bool Remove(KeyValuePair<string, string> item)
            {
                throw new System.NotImplementedException();
            }

            #endregion

            #region Implementation of IDictionary<string,string>

            public bool ContainsKey(string key)
            {
                throw new System.NotImplementedException();
            }

            public void Add(string key, string value)
            {
                throw new System.NotImplementedException();
            }

            public bool Remove(string key)
            {
                throw new System.NotImplementedException();
            }

            #endregion
        }
    }
}
