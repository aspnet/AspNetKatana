namespace Microsoft.Owin.Hosting.Settings
{
    public interface IKatanaSettingsProvider
    {
        IKatanaSettings GetSettings();
    }
}