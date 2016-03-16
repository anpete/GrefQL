using System;
using System.Linq;
using System.Reflection;
using GraphQL.Types;
using GrefQL.Query;

namespace GrefQL.Types
{
    public static class GraphTypeExtensions
    {
        public static void AddField<TGraphType>(this GraphType root, string name, string description, FieldResolver resolver = null)
            where TGraphType : GraphType
            => root.Field<TGraphType>(
                name: name,
                description: description,
                arguments: resolver?.Arguments,
                resolve: resolver?.Resolve);

        public static void AddField(this GraphType root, Type graphType, string name, string description, FieldResolver resolver = null)
        {
            var boundMethod = _fieldMethod.MakeGenericMethod(graphType);
            boundMethod.Invoke(null, new object[] { root, name, description, resolver });
        }

        private static readonly MethodInfo _fieldMethod = typeof(GraphTypeExtensions).GetTypeInfo()
            .GetDeclaredMethods(nameof(GraphTypeExtensions.AddField))
            .Single(m => m.ContainsGenericParameters);
    }
}
