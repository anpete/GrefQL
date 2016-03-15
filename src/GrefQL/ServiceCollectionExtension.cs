using GraphQL.Types;
using GrefQL.Metadata;
using GrefQL.Query;
using GrefQL.Schema;
using Microsoft.EntityFrameworkCore.Metadata;

// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddGraphQL(this IServiceCollection services)
            => services.AddSingleton<IFieldResolverFactory, FieldResolverFactory>()
                .AddSingleton<IGraphTypeMapper, GraphTypeMapper>()
                .AddScoped<IGraphSchemaFactory, GraphSchemaFactory>()
                .AddScoped<IGraphTypeResolverSource, GraphTypeResolverSource>()
                .AddScoped<GraphQLExecutor>()
                .AddScoped<ISchema>(p => p.GetRequiredService<IGraphSchemaFactory>().Create(p.GetRequiredService<IModel>()));
    }
}
