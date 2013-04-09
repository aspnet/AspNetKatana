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

namespace Owin.Types.AppBuilder
{
#region AddressCollection

    internal partial struct AddressCollection : IEnumerable<Address>
    {
        public int Count
        {
            get { return _list.Count; }
        }

        public Address this[int index]
        {
            get { return new Address(_list[index]); }
            set { _list[index] = value.Dictionary; }
        }

        public void Add(Address address)
        {
            _list.Add(address.Dictionary);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Address>)this).GetEnumerator();
        }

        public IEnumerator<Address> GetEnumerator()
        {
            foreach (var entry in List)
            {
                yield return new Address(entry);
            }
        }

        public static AddressCollection Create()
        {
            return new AddressCollection(new List<IDictionary<string, object>>());
        }
    }
#endregion

#region AddressCollection.Generated

    [System.CodeDom.Compiler.GeneratedCode("App_Packages", "")]
    internal partial struct AddressCollection
    {
        private readonly IList<IDictionary<string, object>> _list;

        public AddressCollection(IList<IDictionary<string, object>> list)
        {
            _list = list;
        }

        public IList<IDictionary<string, object>> List
        {
            get { return _list; }
        }

#region Value-type equality
        public bool Equals(AddressCollection other)
        {
            return Equals(_list, other._list);
        }

        public override bool Equals(object obj)
        {
            return obj is AddressCollection && Equals((AddressCollection)obj);
        }

        public override int GetHashCode()
        {
            return (_list != null ? _list.GetHashCode() : 0);
        }

        public static bool operator ==(AddressCollection left, AddressCollection right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AddressCollection left, AddressCollection right)
        {
            return !left.Equals(right);
        }
#endregion
    }
#endregion

}
