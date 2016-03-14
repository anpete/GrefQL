using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GrefQL.Tests.Model.Northwind
{
    public class NorthwindGraph : Schema
    {
        public NorthwindGraph(DbContext data)
        {
            Query = new NorthwindQuery(data);
        }
    }

    public class NorthwindQuery : ObjectGraphType
    {
        public NorthwindQuery(DbContext data)
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
                    => data.Set<Customer>()
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
