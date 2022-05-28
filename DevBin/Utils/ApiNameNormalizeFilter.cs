using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DevBin.Utils
{
    public class ApiNameNormalizeFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var dictionaryPath = swaggerDoc.Paths.ToDictionary(x =>
                string.Join('/', x.Key
                .Split('/')
                .Select(part => part.Contains('}') ? part : part.ToLowerInvariant())),
            x => x.Value);
            var newPaths = new OpenApiPaths();
            foreach (var path in dictionaryPath)
            {
                newPaths.Add(path.Key, path.Value);
            }
            swaggerDoc.Paths = newPaths;
        }
    }
}
