using System.Linq;
using System.Linq.Expressions;
using JasperFx.Core.Reflection;
using Marten.Linq.Fields;
using Marten.Linq.Filters;
using Weasel.Postgresql.SqlGeneration;

namespace Marten.Linq.Parsing.Methods;

internal class TenantIsOneOf: IMethodCallParser
{
    public bool Matches(MethodCallExpression expression)
    {
        return expression.Method.Name == nameof(LinqExtensions.TenantIsOneOf)
               && expression.Method.DeclaringType == typeof(LinqExtensions);
    }

    public ISqlFragment Parse(IFieldMapping mapping, IReadOnlyStoreOptions options, MethodCallExpression expression)
    {
        var values = expression.Arguments.Last().Value().As<string[]>();
        return new TenantIsOneOfWhereFragment(values);
    }
}
