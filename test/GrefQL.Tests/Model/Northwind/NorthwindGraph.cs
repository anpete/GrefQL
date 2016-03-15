using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GrefQL.Tests.Model.Northwind
{
    public class NorthwindGraph : Schema
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
                    => (context.Source as DbContext)?.Set<Customer>()
                        .SingleAsync(c => c.CustomerId == (string)context.Arguments["customerId"]));
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
