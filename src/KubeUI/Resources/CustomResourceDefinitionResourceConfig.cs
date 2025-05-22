using k8s.Models;
using k8s;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;

namespace KubeUI.Resources;

public partial class CustomResourceDefinitionResourceConfig<T> : ResourceConfigBase<T> where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    private bool _showNamespaces = true;

    public override bool IsNamespaced => _showNamespaces;

    private V1CustomResourceDefinition _customResourceDefinition;

    private readonly List<IResourceListColumn> _columns = [];

    public void Generate(V1CustomResourceDefinition crd)
    {
        _customResourceDefinition = crd;

        // Add Name Column
        _columns.Add(NameColumn(SortDirection.Ascending));

        var version = crd.Spec.Versions.First(x => x.Storage);

        //Check if its a namespaced crd
        if (crd.Spec.Scope == "Namespaced")
        {
            // Add Namespace Column
            _columns.Add(NamespaceColumn());
        }
        else
        {
            _showNamespaces = false;
        }

        if (version.AdditionalPrinterColumns != null)
        {
            foreach (var item in version.AdditionalPrinterColumns)
            {
            start:

                try
                {
                    if (item.JsonPath == ".metadata.creationTimestamp")
                    {
                        continue;
                    }

                    if (item.Type == "string")
                    {
                        var exp = JsonPathLINQ.JsonPathLINQ.GetExpression<T, string>(item.JsonPath, true);

                        var colDef = new ResourceListColumn<T, string>()
                        {
                            Name = item.Name,
                            FieldExpression = exp,
                        };

                        _columns.Add(colDef);
                    }
                    else if (item.Type == "number")
                    {
                        var exp = JsonPathLINQ.JsonPathLINQ.GetExpression<T, double>(item.JsonPath, true);

                        var colDef = new ResourceListColumn<T, double>()
                        {
                            Name = item.Name,
                            FieldExpression = exp,
                        };

                        _columns.Add(colDef);
                    }
                    else if (item.Type == "integer" && item.Format == "int64")
                    {
                        var exp = JsonPathLINQ.JsonPathLINQ.GetExpression<T, long>(item.JsonPath, true);

                        var colDef = new ResourceListColumn<T, long>()
                        {
                            Name = item.Name,
                            FieldExpression = exp,
                        };

                        _columns.Add(colDef);
                    }
                    else if (item.Type == "integer")
                    {
                        var exp = JsonPathLINQ.JsonPathLINQ.GetExpression<T, int>(item.JsonPath, true);

                        var colDef = new ResourceListColumn<T, int>()
                        {
                            Name = item.Name,
                            FieldExpression = exp,
                        };

                        _columns.Add(colDef);
                    }
                    else if (item.Type == "date")
                    {
                        var exp = JsonPathLINQ.JsonPathLINQ.GetExpression<T, DateTime>(item.JsonPath, true);

                        var colDef = new ResourceListColumn<T, DateTime>()
                        {
                            Name = item.Name,
                            FieldExpression = exp,
                        };

                        _columns.Add(colDef);
                    }
                    else if (item.Type == "boolean")
                    {
                        var exp = JsonPathLINQ.JsonPathLINQ.GetExpression<T, bool>(item.JsonPath, true);

                        var colDef = new ResourceListColumn<T, bool>()
                        {
                            Name = item.Name,
                            FieldExpression = exp,
                        };

                        _columns.Add(colDef);
                    }
                    else if (item.Type == "enum")
                    {
                        var exp = JsonPathLINQ.JsonPathLINQ.GetExpression<T, Enum>(item.JsonPath, true);

                        var colDef = new ResourceListColumn<T, string>()
                        {
                            Name = item.Name,
                            FieldExpression = TransformToFuncOfString(exp.Body, exp.Parameters),
                        };

                        _columns.Add(colDef);
                    }
                    else
                    {
                        _logger.LogWarning("CRD Column Type not supported: {type}", item.Type);
                    }
                }
                catch (InvalidOperationException ex) when (ex.Message.StartsWith("No coercion operator is defined between types", StringComparison.Ordinal))
                {
                    // The type defined in the AdditionalPrinterColumn is not correct
                    var match = TypeErrorRegex().Match(ex.Message);
                    if (match.Success)
                    {
                        var typeString = match.Groups[1].Value;

                        if (typeString.StartsWith("System.Nullable`1[", StringComparison.Ordinal))
                        {
                            typeString = typeString["System.Nullable`1[".Length..].TrimEnd(']');
                        }

                        var type = Type.GetType(typeString);

                        if (type == null)
                        {
                            type = Type.Assembly.GetType(typeString);

                            if (type == null)
                            {
                                _logger.LogError(ex, "Unable to load type for column: {Name} in type {type}", typeString, crd.Name());
                                continue;
                            }
                        }

                        if (type.IsGenericType)
                        {
                            type = type.GenericTypeArguments[0];
                        }

                        if (type == typeof(string))
                        {
                            item.Type = "string";
                        }
                        else if (type == typeof(double))
                        {
                            item.Type = "number";
                        }
                        else if (type == typeof(int))
                        {
                            item.Type = "integer";
                        }
                        else if (type == typeof(long))
                        {
                            item.Type = "integer";
                            item.Format = "int64";
                        }
                        else if (type == typeof(DateTime))
                        {
                            item.Type = "date";
                        }
                        else if (type == typeof(bool))
                        {
                            item.Type = "boolean";
                        }
                        else if (type.IsEnum)
                        {
                            item.Type = "enum";
                        }
                        else
                        {
                            _logger.LogError(ex, "Unable to generate CRD Column: {Name} with type {Type}", item.Name, type);
                            continue;
                        }

                        goto start;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Unable to generate CRD Column: {Name} in {crd}", item.Name, crd.Name());
                }
            }
        }

        _columns.Add(AgeColumn());
    }

    public override IList<IResourceListColumn> Columns()
    {
        return _columns;
    }

    [GeneratedRegex("types '(.+)' and '(.+)'", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex TypeErrorRegex();

    private static Expression<Func<T, string>> TransformToFuncOfString(Expression expression, ReadOnlyCollection<ParameterExpression> parameters)
    {
        // Check if the expression type is an enum
        if (expression.Type == typeof(Enum))
        {
            // Create a method to get the enum member name from the JsonStringEnumMemberNameAttribute
            var getEnumMemberNameMethod = typeof(ResourceListViewModel<V1Pod>).GetMethod(nameof(GetEnumMemberName), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(expression.Type);

            // Call the method to get the enum member name
            var bodyAsString = Expression.Call(getEnumMemberNameMethod, expression);

            // Create a new lambda expression
            return Expression.Lambda<Func<T, string>>(bodyAsString, parameters);
        }
        else
        {
            Expression bodyAsString;

            // Convert the body of the original expression to return a string
            if (Nullable.GetUnderlyingType(expression.Type) != null)
            {
                bodyAsString = Expression.Condition(
                    Expression.Equal(expression, Expression.Constant(null, expression.Type)),
                    Expression.Constant(string.Empty),
                    Expression.Call(expression, nameof(object.ToString), Type.EmptyTypes)
                );
            }
            else
            {
                bodyAsString = Expression.Call(expression, nameof(object.ToString), Type.EmptyTypes);
            }

            // Create a new lambda expression
            return Expression.Lambda<Func<T, string>>(bodyAsString, parameters);
        }
    }

    private static string GetEnumMemberName<TEnum>(TEnum enumValue) where TEnum : Enum
    {
        var memberInfo = typeof(TEnum).GetMember(enumValue.ToString()).FirstOrDefault();
        if (memberInfo != null)
        {
            var attribute = memberInfo.GetCustomAttribute<JsonStringEnumMemberNameAttribute>();
            if (attribute != null)
            {
                return attribute.Name;
            }
        }
        return enumValue.ToString();
    }
}
