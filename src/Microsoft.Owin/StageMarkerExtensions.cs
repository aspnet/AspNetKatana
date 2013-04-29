using System;

namespace Owin
{
    public static class StageMarkerExtensions
    {
        public static void StageMarker(this IAppBuilder app, string stageMarker)
        {
            object value;
            if (app.Properties.TryGetValue("integratedpipeline.StageMarker", out value))
            {
                var stageMarkerApi = value as Action<IAppBuilder, string>;
                if (stageMarkerApi != null)
                {
                    stageMarkerApi.Invoke(app, stageMarker);
                }
            }
        }
    }
}
