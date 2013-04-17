namespace Microsoft.Owin.Security.ModelSerializer
{
    public interface IModelSerializer<TModel>
    {
        byte[] Serialize(TModel model);
        TModel Deserialize(byte[] data);
    }
}
