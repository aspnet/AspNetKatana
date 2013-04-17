using Microsoft.Owin.Security.TextEncoding;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.ModelSerializer;

namespace Microsoft.Owin.Security
{
    public class ProtectionHandler<TModel> : IProtectionHandler<TModel>
    {
        private readonly IModelSerializer<TModel> _modelSerializer;
        private readonly IDataProtection _dataProtection;
        private readonly ITextEncoding _textEncoding;

        public ProtectionHandler(IModelSerializer<TModel> modelSerializer, IDataProtection dataProtection, ITextEncoding textEncoding)
        {
            _modelSerializer = modelSerializer;
            _dataProtection = dataProtection;
            _textEncoding = textEncoding;
        }

        public string ProtectModel(TModel model)
        {
            var userData = _modelSerializer.Serialize(model);
            var protectedData = _dataProtection.Protect(userData);
            var protectedText = _textEncoding.Encode(protectedData);
            return protectedText;
        }

        public TModel UnprotectModel(string protectedText) 
        {
            if (protectedText == null)
            {
                return default(TModel);
            }

            var protectedData = _textEncoding.Decode(protectedText);
            if (protectedData == null)
            {
                return default(TModel);
            }

            var userData = _dataProtection.Unprotect(protectedData);
            if (userData == null)
            {
                return default(TModel);
            }

            var model = _modelSerializer.Deserialize(userData);
            return model;
        }
    }
}
