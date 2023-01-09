#r "nuget: NSwag.Core.Yaml/13.18.0"
#r "nuget: NSwag.CodeGeneration.CSharp/13.18.0"
#r "System.Net.Http"
using System.Net.Http;
using NJsonSchema.Generation;
using NJsonSchema.Yaml;
using NSwag;
using NSwag.CodeGeneration.CSharp;

string yaml;
using (var httpClient = new HttpClient())
    yaml = await httpClient.GetStringAsync("https://raw.githubusercontent.com/Restream/reindexer/v3.11.0/cpp_src/server/contrib/server.yml");

yaml = yaml.Replace("  /allocator/drop_cache:\n    post:", "  /allocator/drop_cache:\n    post:\n      responses:\n        200:\n          $ref: \"#/responses/OK\"");

var openApiDocument = await OpenApiYamlDocument.FromYamlAsync(yaml);

var settings = new CSharpClientGeneratorSettings
{    
    
};

var generator = new CSharpClientGenerator(openApiDocument, settings);    

Output["g.cs"].Append(generator.GenerateFile());