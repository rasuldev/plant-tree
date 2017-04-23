using Microsoft.AspNetCore.Builder;

namespace PlantTree.Infrastructure.Common
{
    public static class GlobalConf
    {
        public static string Host { get; private set; } = "";

        public static void DetectAndSaveHostUrl(IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                if (Host == "")
                {
                    // TODO check version with // in the beginning
                    Host = "http://" + context.Request.Host.Value.ToString();
                }
                await next();
            });
        }
    }
}