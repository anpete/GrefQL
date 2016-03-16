using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphQL.Types;
using GrefQL.Query;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace GrefQL.Schema
{
    public class GraphSchemaFactory : IGraphSchemaFactory
    {
        private readonly IGraphTypeMapper _typeMapper;
        private readonly IFieldResolverFactory _resolveFactory;
        private readonly IGraphTypeResolverSource _graphTypeResolverSource;

        public GraphSchemaFactory(IGraphTypeMapper typeMapper,
            IFieldResolverFactory resolveFactory,
            IGraphTypeResolverSource graphTypeResolverSource)
        {
            _typeMapper = typeMapper;
            _resolveFactory = resolveFactory;
            _graphTypeResolverSource = graphTypeResolverSource;
        }

        public ISchema Create(IModel model)
        {
            var schema = new GraphQL.Types.Schema(_graphTypeResolverSource.Resolve);
            var query = schema.Query = new ObjectGraphType();
            foreach (var entityType in model.GetEntityTypes())
            {
                var boundMethod = _CreateGraphType.MakeGenericMethod(entityType.ClrType);
                boundMethod.Invoke(this, new object[] { query, entityType });
            }

            return schema;
        }

        // ReSharper disable once InconsistentNaming
        private static readonly MethodInfo _CreateGraphType
            = typeof(GraphSchemaFactory)
                .GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateGraphType));

        private void CreateGraphType<TEntity>(ObjectGraphType root, IEntityType entityType)
        {
            var fb = root.Field<ObjectGraphType<TEntity>>()
                // TODO ensure this is in a safe format for the schema
                .Name(entityType.DisplayName().ToCamelCase())
                .Description(entityType.GraphQL().Description ?? $"{entityType.DisplayName()} ({entityType.ClrType.DisplayName(fullName: true)})");

            var argumentBuilder = ArgumentBuilder(fb.GetType().GetTypeInfo());

            foreach (var arg in CreateArguments(entityType))
            {
                var boundMethod = argumentBuilder.MakeGenericMethod(arg.Type);
                boundMethod.Invoke(fb, new object[] { arg.Name, arg.Description });
            }

            fb.FieldType.Resolve = _resolveFactory.CreateResolveEntityByKey(entityType);

            var graphType = new ObjectGraphType<TEntity>();

            foreach (var prop in entityType.GetProperties())
            {
                var fieldGraphType = _typeMapper.FindMapping(prop);
                if (fieldGraphType == null)
                {
                    // TODO handle unmapped clr types
                    continue;
                }
                var boundMethod = _AddField.MakeGenericMethod(fieldGraphType);
                boundMethod.Invoke(null, new object[] { graphType, prop });
            }
            _graphTypeResolverSource.AddResolver<ObjectGraphType<TEntity>>(() => graphType);
        }

        // ReSharper disable once InconsistentNaming
        private static readonly MethodInfo _AddField
            = typeof(GraphSchemaFactory)
                .GetTypeInfo()
                .GetDeclaredMethod(nameof(AddField));

        private static void AddField<TGraphType>(GraphType graphType, IProperty property)
            where TGraphType : GraphType
            => graphType.Field<TGraphType>()
                .Name(property.Name.ToCamelCase())
                .Description(property.GraphQL().Description
                             ?? $"{property.DeclaringEntityType.DisplayName()}.{property.Name} ({property.DeclaringEntityType.ClrType.DisplayName(fullName: true)}.{property.Name})");

        private static MethodInfo ArgumentBuilder(TypeInfo fieldBuilder)
            => fieldBuilder
                .GetDeclaredMethods("Argument")
                .Single(m => m.GetParameters().Length == 2);

        private IEnumerable<QueryArgument> CreateArguments(IEntityType entityType)
        {
            var primaryKey = entityType.FindPrimaryKey();
            return primaryKey?.Properties
                .Select(p => new
                {
                    Name = p.Name.ToCamelCase(),
                    Mapping = _typeMapper.FindMapping(p)
                })
                .Where(m => m.Mapping != null)
                .Select(m =>
                new QueryArgument(m.Mapping)
                {
                    Name = m.Name
                });
        }
    }
}
