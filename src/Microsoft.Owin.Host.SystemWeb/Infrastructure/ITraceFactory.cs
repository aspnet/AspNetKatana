namespace Microsoft.Owin.Host.SystemWeb.Infrastructure
{
    internal interface ITraceFactory
    {
        ITrace Create(string name);
    }
}