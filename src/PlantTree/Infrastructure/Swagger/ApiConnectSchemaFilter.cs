using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantTree.Infrastructure.Common;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PlantTree.Infrastructure.Swagger
{
    public class ApiConnectSchemaFilter : IDocumentFilter
    {
        private readonly Func<string> _getHost;
        public ApiConnectSchemaFilter(Func<string> getHost)
        {
            _getHost = getHost;
        }

        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Host = _getHost().Replace("http://","").Replace("https://","");

            // Swagger generates several methods for one action /api/projects 
            // such as /api/projects/status/{status}, /api/projects/status/{status}/page/{page} etc.
            // Here we remove this reduntant methods
            var apiMethods = swaggerDoc.Paths
                .Where(p => p.Key.StartsWith("/api/projects/status") || p.Key.StartsWith("/api/projects/user/page"))
                .Select(p => p.Key)
                .ToList();
            foreach (var key in apiMethods)
            {
                swaggerDoc.Paths.Remove(key);
            }

            // 
            //var apiTokenConnect = new PathItem();
            //apiTokenConnect.Post = new Operation()
            //{

            //};

            //foreach (var path in swaggerDoc.Paths)
            //{
            //    Console.WriteLine($"{path.Key}\r\n{path.Value}");

            //}
        }
    }
}
