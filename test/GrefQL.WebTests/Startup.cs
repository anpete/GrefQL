using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using GraphQL;
using GrefQL.Tests.Model;

namespace GrefQL.WebTests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddEntityFramework()
                .AddDbContext<StarWarsContext>();
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Information);

            app.UseFileServer();

            app.Run(async context =>
                {
                    if (context.Request.Method.Equals("POST", System.StringComparison.OrdinalIgnoreCase)
                        && context.Request.Path.Equals("/graphql", System.StringComparison.OrdinalIgnoreCase))
                    {
                        GraphQLQuery query;
                        using (var streamReader = new StreamReader(context.Request.Body))
                        {
                            query = JsonConvert.DeserializeObject<GraphQLQuery>(await streamReader.ReadToEndAsync());
                        }

                        ExecutionResult result;
                        using (var db = app.ApplicationServices.GetRequiredService<StarWarsContext>())
                        {
                            result = await db.FromGraphQLQueryAsync(query.Query, query.Variables);
                        }

                        context.Response.Headers.Add(HeaderNames.ContentType, "application/json");
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(result));
                    }
                });
        }
    }

    public class GraphQLQuery
    {
        public string Query { get; set; }
        public string Variables { get; set; }
    }
}
