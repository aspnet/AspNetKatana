using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Owin
{
    public class FormCollection : ReadableStringCollection, IFormCollection
    {
        public FormCollection(IDictionary<string, string[]> store)
            : base(store)
        {
        }
    }
}
