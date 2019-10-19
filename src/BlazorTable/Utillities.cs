using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BlazorTable
{
    public static class Utillities
    {
        public static string GetDescription<T>(this T e) where T : IConvertible
        {
            if (e is Enum)
            {
                Type type = e.GetType();
                Array values = System.Enum.GetValues(type);

                foreach (int val in values)
                {
                    if (val == e.ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val));
                        var descriptionAttribute = memInfo[0]
                            .GetCustomAttributes(typeof(DescriptionAttribute), false)
                            .FirstOrDefault() as DescriptionAttribute;

                        if (descriptionAttribute != null)
                        {
                            return descriptionAttribute.Description;
                        }
                    }
                }
            }

            return null; // could also return string.Empty
        }

        public static Expression<Func<T, bool>> CallMethod<T>(Expression<Func<T, object>> expression, string value, string method)
        {
            MethodInfo equalsMethod = typeof(string).GetMethod(method, new[] { typeof(string) });

            return Expression.Lambda<Func<T, bool>>(
                Expression.Call(
                    expression.Body,
                    equalsMethod,
                    Expression.Constant(value)),
                expression.Parameters);
        }

        public static Expression<Func<T, bool>> CallMethod2<T>(Expression<Func<T, object>> expression, string value, string method)
        {
            return Expression.Lambda<Func<T, bool>>(
                Expression.Call(
                    expression.Body,
                    method,
                    null,
                    Expression.Constant(value)),
                expression.Parameters);
        }

        public static Expression<Func<T, bool>> CallMethod3<T>(Expression<Func<T, object>> expression, string value, string method, Type[] methodParams = null)
        {
            //MethodInfo methodInfo;

            //if (methodParams != null)
            //{
            //    methodInfo = expression.GetPropertyMemberInfo().GetMemberUnderlyingType().GetMethod(method, methodParams);
            //}
            //else
            //{
            //    methodInfo = expression.GetPropertyMemberInfo().GetMemberUnderlyingType().GetMethod(method);
            //}

            return Expression.Lambda<Func<T, bool>>(
                Expression.Call(
                    expression.Body,
                    method,
                    null,
                    Expression.Constant(value)),
                expression.Parameters);
        }

        public static Type GetMemberUnderlyingType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                default:
                    throw new ArgumentException("MemberInfo must be if type FieldInfo, PropertyInfo or EventInfo", nameof(member));
            }
        }

        public static MemberInfo GetPropertyMemberInfo<T>(this Expression<Func<T, object>> expression)
        {
            if (expression == null)
            {
                return null;
            }

            if (!(expression.Body is MemberExpression body))
            {
                UnaryExpression ubody = (UnaryExpression)expression.Body;
                body = ubody.Operand as MemberExpression;
            }

            return body?.Member;
        }

        public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expression)
        {
            return Expression.Lambda<Func<T, bool>>(Expression.Not(expression.Body), expression.Parameters[0]);
        }
    }
}
