using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GrefQL.Tests.Model.Northwind
{
    // Sandbox for testing GraphQL.NET APIs

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
                resolve: context
                    =>
                    {
                        return (context.Source as DbContext)?.Set<Customer>().ToArrayAsync();
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
