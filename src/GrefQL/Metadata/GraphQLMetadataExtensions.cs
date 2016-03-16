using GrefQL.Metadata;

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public static class GraphQLMetadataExtensions
    {
        public static GraphQLEntityTypeAnnotations GraphQL(this IEntityType entityType)
            => new GraphQLEntityTypeAnnotations(entityType);

        public static GraphQLPropertyAnnotations GraphQL(this IProperty property)
            => new GraphQLPropertyAnnotations(property);
    }
}
