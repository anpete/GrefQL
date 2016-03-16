using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace GrefQL.Metadata
{
    public class GraphQLPropertyAnnotations
    {
        private readonly IProperty _property;

        public GraphQLPropertyAnnotations(IProperty property)
        {
            _property = property;
        }

        /// <summary>
        ///     FieldName of the property in GraphQL
        /// </summary>
        public string FieldName
        {
            get { return _property[GraphQLAnnotationNames.FieldName] as string ?? _property.Name.Camelize(); }
            set { (_property as IMutableProperty)?.AddAnnotation(GraphQLAnnotationNames.FieldName, value); }
        }

        /// <summary>
        ///     Description of field that appears in GraphQL
        /// </summary>
        public string Description
        {
            get { return _property[GraphQLAnnotationNames.Description] as string ?? $"{_property.DeclaringEntityType.DisplayName()}.{_property.Name} ({_property.DeclaringEntityType.ClrType.DisplayName()}.{_property.Name})"; }
            set { (_property as IMutableProperty)?.AddAnnotation(GraphQLAnnotationNames.Description, value); }
        }
    }
}