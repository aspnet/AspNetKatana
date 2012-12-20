namespace Microsoft.Owin.Hosting.Starter
{
    public interface IHostingStarterFactory
    {
        IHostingStarter Create(string name);
    }

}