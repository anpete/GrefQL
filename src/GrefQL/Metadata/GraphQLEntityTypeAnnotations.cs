using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace GrefQL.Metadata
{
    public class GraphQLEntityTypeAnnotations
    {
        private readonly IEntityType _entityType;

        public GraphQLEntityTypeAnnotations(IEntityType entityType)
        {
            _entityType = entityType;
        }

        /// <summary>
        ///     Description of field that appears in GraphQL
        /// </summary>
        public string Description
        {
            get { return _entityType[GraphQLAnnotationNames.Description] as string; }
            set { (_entityType as IMutableEntityType)?.AddAnnotation(GraphQLAnnotationNames.Description, value); }
        }

        public string DescriptionOrDefault()
            => Description ?? $"{_entityType.DisplayName()} ({_entityType.ClrType.DisplayName()})";
    }
}