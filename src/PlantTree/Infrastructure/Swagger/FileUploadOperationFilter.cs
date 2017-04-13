using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PlantTree.Infrastructure.Swagger
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                return;
            if (operation.OperationId.ToLower() != "apiaccountphotopost")
                return;

            // For each IFormFile argument or model property swagger generates params for every property of IFormFile.
            // We should find and replace them by single parameter for each IFormFile. 
            // 1. Find swagger params for IFormFile properties
            var iFormFilePropsParamNames = context.ApiDescription.ParameterDescriptions
                                //.Where(x => x.ModelMetadata.ContainerType.IsAssignableFrom(typeof(IFormFile))) // slow, but more accurate
                                .Where(x => x.ModelMetadata.ContainerType.Name == "IFormFile")
                                .Select(x => x.Name)
                                .ToList();

            if (!iFormFilePropsParamNames.Any())
                return;

            // 2. Remove params
            operation.Parameters = operation.Parameters.Where(p => !iFormFilePropsParamNames.Contains(p.Name)).ToList();

            // 3. Add new parameters: one for each IFormFile
            var actionParams = context.ApiDescription.ActionDescriptor.Parameters;

            //// Find action parameters that have type IFormFile
            //var iFormFileParamNames = actionParams
            //    .Where(x => x.ParameterType.Name == "IFormFile").Select(x => x.Name).ToList();
            //// Find action parameters with type that has property of IFormFile type
            //var iFormFileSubParamNames = actionParams
            //    .SelectMany(x => x.ParameterType.GetProperties())
            //    .Where(x => x.PropertyType.Name == "IFormFile")
            //    .Select(x => x.Name)
            //    .ToList();

            // Find action parameters that have type IFormFile and 
            // action parameters with type that has property of IFormFile type
            var iFormFileParamNames = actionParams.Select(x=> new {Name = x.Name, Type = x.ParameterType.Name })
                .Union(actionParams.SelectMany(x => x.ParameterType.GetProperties()
                                                     .Select(y => new { Name = $"{x.Name}.{y.Name}", Type = y.PropertyType.Name })))
                .Where(x => x.Type == "IFormFile").Select(x => x.Name).ToList();

            foreach (var paramName in iFormFileParamNames)
            {
                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = paramName,
                    In = "formData",
                    Description = "Upload File",
                    Required = true,
                    Type = "file"
                });
            }
            
            operation.Consumes.Add("multipart/form-data");
            
        }


    }
}