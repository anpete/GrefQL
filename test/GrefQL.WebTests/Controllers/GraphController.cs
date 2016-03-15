using System.Threading.Tasks;
using GraphQL;
using GraphQL.Http;
using GrefQL.Tests.Model.Northwind;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GrefQL.WebTests.Controllers
{
    public class GraphController : Controller
    {
        [Route("/graphql")]
        public async Task<IActionResult> Execute([FromBody] GraphQLQuery query, [FromServices] NorthwindContext context)
        {
            var schema = new NorthwindGraph(context);
            var documentExecutor = new DocumentExecuter();

            var result = await documentExecutor.ExecuteAsync(schema, null, query.Query, null);

            return Json(result, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFF'Z'",
            });
        }
    }

    public class GraphQLQuery
    {
        public string Query { get; set; }
        public string Variables { get; set; }
    }
}