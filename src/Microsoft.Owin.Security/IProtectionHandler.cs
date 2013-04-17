namespace Microsoft.Owin.Security
{
    public interface IProtectionHandler<TModel>
    {
        string ProtectModel(TModel model);
        TModel UnprotectModel(string protectedText);
    }
}