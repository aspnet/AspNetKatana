//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Specialized;
using System.Net;

namespace Microsoft.HttpListener.Owin
{
    /// <summary>
    /// This wraps HttpListenerRequest's WebHeaderCollection (NameValueCollection) and adapts it to 
    /// the OWIN required IDictionary surface area. It remains fully mutable, but you will be subject 
    /// to the header validations performed by the underlying collection.
    /// </summary>
    internal class RequestHeadersDictionary : HeadersDictionaryBase
    {
        internal RequestHeadersDictionary(NameValueCollection headers)
            : base((WebHeaderCollection)headers)
        {
        }
    }
}
