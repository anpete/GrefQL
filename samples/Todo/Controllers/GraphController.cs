using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Todo.Models;

namespace Todo.Controllers
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
        public async Task<IActionResult> Execute([FromBody] GraphQLQuery query, [FromServices] TodoContext context)
        {
            var result = await context.ExecuteGraphQLQueryAsync(query.Query, query.Variables, query.OperationName);
            return Json(result, DefaultSettings);
        }
    }

    public class GraphQLQuery
    {
        public string OperationName { get; set; }
        public string Query { get; set; }
        public Dictionary<string,object> Variables { get; set; }
    }
}
