using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GrefQL
{
    public class GraphSchemaFactory : IGraphSchemaFactory
    {
        private readonly IGraphTypeMapper _typeMapper;
        private readonly IFieldResolverFactory _resolveFactory;

        public GraphSchemaFactory(IGraphTypeMapper typeMapper, IFieldResolverFactory resolveFactory)
        {
            _typeMapper = typeMapper;
            _resolveFactory = resolveFactory;
        }

        public Schema Create(IModel model)
        {
            var schema = new Schema();
            var query = schema.Query = new ObjectGraphType();
            foreach (var entityType in model.GetEntityTypes())
            {
                var fb = query.Field<ObjectGraphType>()
                    .Name(CreateFieldName(entityType));

                // TODO pull descriptions from annotations

                var argumentBuilder = ArgumentBuilder(fb.GetType().GetTypeInfo());

                foreach (var arg in CreateArguments(entityType))
                {
                    var boundMethod = argumentBuilder.MakeGenericMethod(arg.Type);
                    boundMethod.Invoke(fb, new object[] { arg.Name, arg.Description });
                }
                fb.FieldType.Resolve = _resolveFactory.CreateResolveEntityByKey(entityType);
            }

            return schema;
        }

        private static MethodInfo ArgumentBuilder(TypeInfo fieldBuilder)
            => fieldBuilder
                .GetDeclaredMethods("Argument")
                .Single(m => m.GetParameters().Length == 2);

        private IEnumerable<QueryArgument> CreateArguments(IEntityType entityType)
        {
            var primaryKey = entityType.FindPrimaryKey();
            return primaryKey?.Properties.Select(prop =>
                new QueryArgument(_typeMapper.FindMapping(prop, notNull: true))
                {
                    Name = prop.Name.ToCamelCase()
                });
        }

        private string CreateFieldName(IEntityType entityType)
            // TODO ensure unique names
            => entityType.Name.Substring(entityType.Name.LastIndexOf('.') + 1).ToCamelCase();
    }
}
