using Microsoft.EntityFrameworkCore.Metadata;

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
            set { (_property as IMutableEntityType)?.AddAnnotation(GraphQLAnnotationNames.Description, value); }
        }
    }
}