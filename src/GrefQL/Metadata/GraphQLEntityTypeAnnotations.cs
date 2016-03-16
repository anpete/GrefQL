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
        ///     FieldName in GraphQL
        /// </summary>
        public string FieldName
        {
            get { return _entityType[GraphQLAnnotationNames.FieldName] as string ?? _entityType.DisplayName().Camelize(); }
            set { (_entityType as IMutableEntityType)?.AddAnnotation(GraphQLAnnotationNames.FieldName, value); }
        }

        /// <summary>
        ///     Description of field in GraphQL
        /// </summary>
        public string Description
        {
            get { return _entityType[GraphQLAnnotationNames.Description] as string ?? $"{_entityType.DisplayName()} ({_entityType.ClrType.DisplayName()})"; }
            set { (_entityType as IMutableEntityType)?.AddAnnotation(GraphQLAnnotationNames.Description, value); }
        }

        /// <summary>
        ///     FieldName for plural queries in GraphQL
        /// </summary>
        public string PluralFieldName
        {
            get { return _entityType[GraphQLAnnotationNames.PluralFieldName] as string ?? FieldName.Pluralize(); }
            set { (_entityType as IMutableEntityType)?.AddAnnotation(GraphQLAnnotationNames.PluralFieldName, value); }
        }

        /// <summary>
        ///     Description of plural field queries in GraphQL
        /// </summary>
        public string PluralDescription
        {
            get { return _entityType[GraphQLAnnotationNames.PluralDescription] as string ?? Description; }
            set { (_entityType as IMutableEntityType)?.AddAnnotation(GraphQLAnnotationNames.PluralDescription, value); }
        }
    }
}
