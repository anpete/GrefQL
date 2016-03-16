using System.Threading.Tasks;
using GrefQL.Tests.Model.Northwind;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GrefQL.WebTests.Controllers
{
    public class GraphController : Controller
    {
        private static readonly JsonSerializerSettings DefaultSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFF'Z'"
        };

        [Route("/graphql")]
        public async Task<IActionResult> Execute([FromBody] GraphQLQuery query, [FromServices] NorthwindContext context)
        {
            var result = await context.ExecuteGraphQLQueryAsync(query.Query, query.Variables);
            return Json(result, DefaultSettings);
        }
    }

    public class GraphQLQuery
    {
        public string Query { get; set; }
        public string Variables { get; set; }
    }
}
