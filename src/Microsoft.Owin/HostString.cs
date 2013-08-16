// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.Owin
{
    public struct HostString : IEquatable<HostString>
    {
        private readonly string _value;

        public HostString(string value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }

        public override string ToString()
        {
            return ToUriComponent();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "Purpose of the method is to return a string")]
        public string ToUriComponent()
        {
            // REVIEW: what is needed here?
            return _value;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1057:StringUriOverloadsCallSystemUriOverloads", Justification = "Requirements not compatible with Uri processing")]
        public static HostString FromUriComponent(string uriComponent)
        {
            // REVIEW: what is needed here?
            return new HostString(uriComponent);
        }

        public static HostString FromUriComponent(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            // REVIEW: what is needed here?
            return new HostString(uri.GetComponents(UriComponents.HostAndPort, UriFormat.Unescaped));
        }

        public bool Equals(HostString other)
        {
            return string.Equals(_value, other._value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is HostString && Equals((HostString)obj);
        }

        public override int GetHashCode()
        {
            return (_value != null ? _value.GetHashCode() : 0);
        }

        public static bool operator ==(HostString left, HostString right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HostString left, HostString right)
        {
            return !left.Equals(right);
        }
    }
}
