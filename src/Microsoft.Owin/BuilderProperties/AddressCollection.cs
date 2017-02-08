// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Owin.BuilderProperties
{
    /// <summary>
    /// Wraps the host.Addresses list.
    /// </summary>
    public struct AddressCollection : IEnumerable<Address>
    {
        private readonly IList<IDictionary<string, object>> _list;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Owin.BuilderProperties.AddressCollection" /> class.
        /// </summary>
        /// <param name="list">The address list to set to the collection.</param>
        public AddressCollection(IList<IDictionary<string, object>> list)
        {
            _list = list;
        }

        /// <summary>
        /// Gets the underlying address list.
        /// </summary>
        /// <returns>The underlying address list.</returns>
        public IList<IDictionary<string, object>> List
        {
            get { return _list; }
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        /// <returns>The number of elements in the collection.</returns>
        public int Count
        {
            get { return _list.Count; }
        }

        /// <summary>
        /// Gets the item with the specified index from the collection.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The item with the specified index.</returns>
        public Address this[int index]
        {
            get { return new Address(_list[index]); }
            set { _list[index] = value.Dictionary; }
        }

        /// <summary>
        /// Adds the specified address to the collection.
        /// </summary>
        /// <param name="address">The address to add to the collection.</param>
        public void Add(Address address)
        {
            _list.Add(address.Dictionary);
        }

        /// <summary>
        /// Gets the enumerator that iterates through the collection.
        /// </summary>
        /// <returns>The enumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Address>)this).GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator that iterates through the collection.
        /// </summary>
        /// <returns>The enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<Address> GetEnumerator()
        {
            foreach (var entry in List)
            {
                yield return new Address(entry);
            }
        }

        /// <summary>
        /// Creates a new empty instance of <see cref="T:Microsoft.Owin.BuilderProperties.AddressCollection" />.
        /// </summary>
        /// <returns>A new empty instance of <see cref="T:Microsoft.Owin.BuilderProperties.AddressCollection" />.</returns>
        public static AddressCollection Create()
        {
            return new AddressCollection(new List<IDictionary<string, object>>());
        }

        #region Value-type equality

        /// <summary>
        /// Determines whether the current collection is equal to the specified collection.
        /// </summary>
        /// <param name="other">The other collection to compare to the current collection.</param>
        /// <returns>true if current collection is equal to the specified collection; otherwise, false.</returns>
        public bool Equals(AddressCollection other)
        {
            return Equals(_list, other._list);
        }

        /// <summary>
        /// Determines whether the current collection is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object to compare to the current collection.</param>
        /// <returns>true if current collection is equal to the specified object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj is AddressCollection && Equals((AddressCollection)obj);
        }

        /// <summary>
        /// Gets the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return (_list != null ? _list.GetHashCode() : 0);
        }

        /// <summary>
        /// Determines whether the first collection is equal to the second collection.
        /// </summary>
        /// <param name="left">The first collection to compare.</param>
        /// <param name="right">The second collection to compare.</param>
        /// <returns>true if both collections are equal; otherwise, false.</returns>
        public static bool operator ==(AddressCollection left, AddressCollection right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether the first collection is not equal to the second collection.
        /// </summary>
        /// <param name="left">The first collection to compare.</param>
        /// <param name="right">The second collection to compare.</param>
        /// <returns>true if both collections are not equal; otherwise, false.</returns>
        public static bool operator !=(AddressCollection left, AddressCollection right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}
