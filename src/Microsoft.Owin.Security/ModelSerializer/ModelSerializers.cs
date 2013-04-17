using System.Collections.Generic;

namespace Microsoft.Owin.Security.ModelSerializer
{
    public static class ModelSerializers
    {
        static ModelSerializers()
        {
            ExtraSerializer = new ExtraDictionarySerializer();
        }

        public static IModelSerializer<IDictionary<string, string>> ExtraSerializer { get; set; }
    }
}
