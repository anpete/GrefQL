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
        ///     Description of field that appears in GraphQL
        /// </summary>
        public string Description
        {
            get { return _property[GraphQLAnnotationNames.Description] as string; }
            set { (_property as IMutableProperty)?.AddAnnotation(GraphQLAnnotationNames.Description, value); }
        }

        public string DescriptionOrDefault() 
            => Description ?? $"{_property.DeclaringEntityType.DisplayName()}.{_property.Name} ({_property.DeclaringEntityType.ClrType.DisplayName()}.{_property.Name})";

        /// <summary>
        ///     Name of the property in GraphQL
        /// </summary>
        public string Name
        {
            get { return _property[GraphQLAnnotationNames.Name] as string; }
            set { (_property as IMutableProperty)?.AddAnnotation(GraphQLAnnotationNames.Name, value); }
        }

        public string NameOrDefault() 
            => Name ?? _property.Name.Camelize();
    }
}