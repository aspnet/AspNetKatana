// <copyright file="AddressCollection.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin.BuilderProperties
{
    /// <summary>
    /// Wraps the host.Addresses list
    /// </summary>
    public struct AddressCollection : IEnumerable<Address>
    {
        private readonly IList<IDictionary<string, object>> _list;

        /// <summary>
        /// Create a new wrapper
        /// </summary>
        /// <param name="list"></param>
        public AddressCollection(IList<IDictionary<string, object>> list)
        {
            _list = list;
        }

        /// <summary>
        /// The underlying list
        /// </summary>
        public IList<IDictionary<string, object>> List
        {
            get { return _list; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Count
        {
            get { return _list.Count; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Address this[int index]
        {
            get { return new Address(_list[index]); }
            set { _list[index] = value.Dictionary; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        public void Add(Address address)
        {
            _list.Add(address.Dictionary);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Address>)this).GetEnumerator();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Address> GetEnumerator()
        {
            foreach (var entry in List)
            {
                yield return new Address(entry);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static AddressCollection Create()
        {
            return new AddressCollection(new List<IDictionary<string, object>>());
        }

#region Value-type equality

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(AddressCollection other)
        {
            return Equals(_list, other._list);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is AddressCollection && Equals((AddressCollection)obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (_list != null ? _list.GetHashCode() : 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(AddressCollection left, AddressCollection right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(AddressCollection left, AddressCollection right)
        {
            return !left.Equals(right);
        }

#endregion
    }
}
