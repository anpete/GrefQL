using Microsoft.EntityFrameworkCore.Metadata;

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
    }
}