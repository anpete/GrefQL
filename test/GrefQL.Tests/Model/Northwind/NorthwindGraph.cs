using System.Linq;
using GraphQL;
using GraphQL.Http;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace GrefQL.Tests.Model.Northwind
{
    // Sandbox for testing GraphQL.NET APIs
    public class Sandbox : NorthwindTestsBase
    {
        [Fact]
        public void Query_sandbox()
        {
            const string query = @"
                {
                  customers(limit: 10) {
                    customerId
                    companyName
                    contactName
                  }
                }";

            using (var data = CreateContext())
            {
                var schema = new NorthwindGraph();
                var documentExecutor = new DocumentExecuter();

                var result = documentExecutor.ExecuteAsync(schema, data, query, null).Result;

                Assert.Null(result.Errors);

                var jsonResult = new DocumentWriter().Write(result);

                WriteLine();
                WriteLine(jsonResult);
            }
        }

        public Sandbox(NorthwindFixture northwindFixture, ITestOutputHelper testOutputHelper)
            : base(northwindFixture)
        {
            SetTestOutputHelper(testOutputHelper);
        }
    }

    public class NorthwindGraph : GraphQL.Types.Schema
    {
        public NorthwindGraph()
        {
            Query = new NorthwindQuery();
        }
    }

    public class NorthwindQuery : ObjectGraphType
    {
        public NorthwindQuery()
        {
            Name = "Query";

            Field<CustomerType>(
                "customer",
                arguments: new QueryArguments(
                    new[]
                    {
                        new QueryArgument<NonNullGraphType<StringGraphType>>
                        {
                            Name = "customerId",
                            Description = "id of the customer"
                        }
                    }),
                resolve: context
                    =>
                    {
                        // TODO: #4795
                        var customerId = (string)context.Arguments["customerId"];

                        return (context.Source as DbContext)?.Set<Customer>()
                            .SingleAsync(c => c.CustomerId == customerId);
                    });

            Field<ListGraphType<CustomerType>>(
                "customers",
                arguments: new QueryArguments(
                    new[]
                    {
                        new QueryArgument<IntGraphType>
                        {
                            Name = "limit",
                            Description = "maximum number of results to return"
                        }
                    }),
                resolve: context
                    =>
                    {
                        var dbContext = context.Source as DbContext;

                        if (dbContext == null)
                        {
                            return null;
                        }

                        IQueryable<Customer> query = dbContext.Set<Customer>();

                        object limit;
                        if (context.Arguments.TryGetValue("limit", out limit)
                            && limit is int)
                        {
                            query = query.Take((int)limit);
                        }

                        return query.ToArrayAsync();
                    });
        }
    }

    public class CustomerType : ObjectGraphType
    {
        public CustomerType()
        {
            Name = "Customer";
            Description = "A customer.";

            Field<NonNullGraphType<StringGraphType>>("customerId", "The id of the human.");
            Field<StringGraphType>("companyName", "The name of the company.");
            Field<StringGraphType>("contactName", "A contact name for the customer.");

            IsTypeOf = value => value is Customer;
        }
    }
}
