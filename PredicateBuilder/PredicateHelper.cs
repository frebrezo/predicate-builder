using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace PredicateBuilder
{
    public class PredicateHelper<T>
    {
        public static Expression<Func<T, bool>> AddCondition<TProp, TValue>(Expression<Func<T, TProp>> propExprFunc, TValue value, ConditionalOperator op)
        {
            return propExprFunc.ToConditionLambdaExpression(value, op);
        }

        public static Expression<Func<T, bool>> AddCondition<TValue>(LambdaExpression propExprFunc, Type propType, TValue value, ConditionalOperator op)
        {
            return propExprFunc.ToConditionLambdaExpression<T, TValue>(propType, value, op);
        }

        public static Expression<Func<T, bool>> Is<TProp, TValue>(Expression<Func<T, TProp>> propExprFunc, List<TValue> valueList, ConditionalOperator op)
        {
            return propExprFunc.ToConditionLambdaExpression(valueList, op);
        }

        public static Expression<Func<T, bool>> AddCondition<TValue>(LambdaExpression propExprFunc, Type propType, List<TValue> valueList, ConditionalOperator op)
        {
            return propExprFunc.ToConditionLambdaExpression<T, TValue>(propType, valueList, op);
        }
    }

    public static class PredicateHelper
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo> EnumerableAnyMethodMap = new ConcurrentDictionary<Type, MethodInfo>();
        private static readonly ConcurrentDictionary<Type, MethodInfo> EnumerableContainsMethodMap = new ConcurrentDictionary<Type, MethodInfo>();
        private static readonly MethodInfo DbFunctionsLikeMethod = typeof(DbFunctionsExtensions).GetMethod("Like");

        private static MethodInfo GetEnumerableAnyMethod(Type entityType)
        {
            MethodInfo method = EnumerableAnyMethodMap.GetOrAdd(entityType,
                typeof(Enumerable).GetMethods().FirstOrDefault(x => x.Name.Contains("Any") && x.GetParameters().Length == 2).MakeGenericMethod(entityType));
            return method;
        }

        private static MethodInfo GetEnumerableContainsMethod(Type valueType)
        {
            MethodInfo method = EnumerableContainsMethodMap.GetOrAdd(valueType,
                typeof(Enumerable).GetMethods().FirstOrDefault(x => x.Name.Contains("Contains") && x.GetParameters().Length == 2).MakeGenericMethod(valueType));
            return method;
        }

        public static bool IsNullableValueType(Type t)
        {
            return Nullable.GetUnderlyingType(t) != null;
        }

        public static Type GetNullableUnderlyingType(Type t)
        {
            if (t == null) return null;

            return Nullable.GetUnderlyingType(t);
        }

        public static List<Type> GetGenericType(Type t)
        {
            if (t == null) return null;

            if (t.IsGenericType)
            {
                return t.GetGenericArguments().ToList();
            }

            return null;
        }

        public static string PropertyNamePath<T, TProp>(Expression<Func<T, TProp>> propExprFunc)
        {
            return PropertyNamePath(propExprFunc, typeof(T));
        }

        public static string PropertyNamePath(LambdaExpression propExprFunc, Type propType)
        {
            if (propExprFunc == null) return null;

            var propNameStack = new Stack<string>();
            return PropertyNamePath(propExprFunc.Body, propType, propNameStack);
        }

        private static string PropertyNamePath(Expression propExpr, Type propType, Stack<string> propNameStack)
        {
            if (propExpr.Type.FullName == propType.FullName)
            {
                return propNameStack.Count > 0 ? propNameStack.Pop() : null;
            }

            propNameStack.Push(((MemberExpression)propExpr).Member.Name);
            string basePropName = PropertyNamePath(((MemberExpression)propExpr).Expression, propType, propNameStack);
            string propName = propNameStack.Count > 0 ? propNameStack.Pop() : null;
            return string.IsNullOrEmpty(propName) ? basePropName : $"{basePropName}.{propName}";
        }

        public static Expression<Func<T, TProp>> GetPropertyLambdaExpression<T, TProp>(string propName)
        {
            return (Expression<Func<T, TProp>>)GetPropertyLambdaExpression<T>(propName);
        }

        public static LambdaExpression GetPropertyLambdaExpression<T>(string propName)
        {
            Type entityType = typeof(T);
            ParameterExpression argExpr = Expression.Parameter(entityType, "x");
            Expression propExpr = GetPropertyExpression<T>(argExpr, propName);

            LambdaExpression propExprFunc = Expression.Lambda(propExpr, argExpr);

            return propExprFunc;
        }

        private static Expression GetPropertyExpression<T, TValue>(ParameterExpression argExpr, string propName)
        {
            Type propType = typeof(T);
            Type valueType = typeof(TValue);
            Expression propExpr = GetPropertyExpression<T>(argExpr, propName);

            // If either the property or the value is not a string, then if the property type is NOT nullable and the value type is nullable,
            //      then set property to nullable type, value type.
            if (propExpr.Type != typeof(string) && valueType != typeof(string))
            {
                // If propExpr type is NOT a nullable value type and valueExpr type is a nullable value type,
                //      then convert propExpr to valueExpr type.
                if (!IsNullableValueType(propExpr.Type) && IsNullableValueType(valueType))
                {
                    propExpr = Expression.Convert(propExpr, valueType);
                }
            }

            return propExpr;
        }

        private static Expression GetPropertyExpression<T>(ParameterExpression argExpr, string propName)
        {
            Type entityType = typeof(T);
            Expression propExpr = null;

            if (string.IsNullOrEmpty(propName))
            {
                propExpr = argExpr;
            }
            else if (propName.Contains('.'))
            {
                propExpr = argExpr;
                foreach (var member in propName.Split('.'))
                {
                    propExpr = Expression.PropertyOrField(propExpr, member);
                }
            }
            else
            {
                propExpr = Expression.Property(argExpr, entityType, propName);
            }

            return propExpr;
        }

        private static Expression GetPropertyValueExpression<TValue>(TValue value)
        {
            TValue localValue = value;
            Expression<Func<TValue>> valueExprFunc = () => localValue;
            Expression valueExpr = valueExprFunc.Body;

            return valueExpr;
        }

        private static Expression GetPropertyValueExpression<TValue>(Expression propExpr, TValue value)
        {
            Type valueType = value?.GetType() ?? typeof(TValue);
            Expression valueExpr = GetPropertyValueExpression(value);

            // value may be of type object and, if value is an enum or other value type, needs be converted to its actual type.
            if (valueType.IsValueType && value != null)
            {
                valueExpr = Expression.Convert(valueExpr, valueType);
            }

            if (propExpr.Type != typeof(string) && valueExpr.Type != typeof(string))
            {
                // If propExpr type is a nullable value type and valueExpr type is NOT a nullable value type,
                //      then convert valueExpr to propExpr type.
                if (IsNullableValueType(propExpr.Type) && !IsNullableValueType(valueExpr.Type))
                {
                    valueExpr = Expression.Convert(valueExpr, propExpr.Type);
                }
            }

            return valueExpr;
        }

        private static Expression GetPropertyValueExpression<TValue>(Expression propExpr, List<TValue> valueList)
        {
            Type valueType = GetNullableUnderlyingType(GetGenericType(valueList?.GetType())?.FirstOrDefault()) ?? typeof(TValue);
            Expression valueExpr = GetPropertyValueExpression(valueList);

            // valueList may be of type object and, if value is an enum or other value type, needs be converted to its actual type.
            if (valueType.IsValueType && valueList != null)
            {
                // Convert valueExpr to type for List of valueType.
                valueExpr = Expression.Convert(valueExpr, typeof(List<>).MakeGenericType(valueType));
            }

            if (propExpr.Type != typeof(string) && valueExpr.Type != typeof(string))
            {
                // If propExpr type is a nullable value type and valueExpr type is NOT a nullable value type,
                //      then convert valueExpr to propExpr type.
                if (IsNullableValueType(propExpr.Type) && !IsNullableValueType(valueExpr.Type))
                {
                    // Convert valueExpr to type for List of propExpr type.
                    valueExpr = Expression.Convert(valueExpr, typeof(List<>).MakeGenericType(propExpr.Type));
                }
            }

            return valueExpr;
        }

        private static Expression GetConditionalExpression<TValue>(Expression propExpr, Type propType, TValue value, ConditionalOperator op)
        {
            Expression valueExpr = GetPropertyValueExpression(propExpr, value);
            Expression expr = null;

            switch (op)
            {
                case ConditionalOperator.LessThan:
                    expr = Expression.LessThan(propExpr, valueExpr);
                    break;
                case ConditionalOperator.LessThanOrEqual:
                    expr = Expression.LessThan(propExpr, valueExpr);
                    break;
                case ConditionalOperator.GreaterThan:
                    expr = Expression.GreaterThan(propExpr, valueExpr);
                    break;
                case ConditionalOperator.GreaterThanOrEqual:
                    expr = Expression.GreaterThanOrEqual(propExpr, valueExpr);
                    break;
                case ConditionalOperator.NotEqual:
                    expr = Expression.NotEqual(propExpr, valueExpr);
                    break;
                case ConditionalOperator.Like:
                    expr = Expression.Call(null, DbFunctionsLikeMethod, Expression.Constant(EF.Functions), propExpr, valueExpr);
                    break;
                case ConditionalOperator.Equal:
                default:
                    expr = Expression.Equal(propExpr, valueExpr);
                    break;
            }

            return expr;
        }

        public static Expression<Func<T, bool>> ToConditionLambdaExpression<T, TProp, TValue>(this Expression<Func<T, TProp>> propExprFunc, TValue value, ConditionalOperator op)
        {
            Type propType = typeof(T);

            Expression<Func<T, bool>> condExprFunc = propExprFunc.ToConditionLambdaExpression<T, TValue>(propType, value, op);

            return condExprFunc;
        }

        public static Expression<Func<T, bool>> ToConditionLambdaExpression<T, TValue>(this LambdaExpression propExprFunc, Type propType, TValue value, ConditionalOperator op)
        {
            Type entityType = typeof(T);
            ParameterExpression argExpr = Expression.Parameter(entityType, "x");
            string propName = PropertyNamePath(propExprFunc, propType);
            Expression propExpr = GetPropertyExpression<T, TValue>(argExpr, propName);

            Expression condExpr = GetConditionalExpression(propExpr, propType, value, op);

            Expression<Func<T, bool>> condExprFunc = Expression.Lambda<Func<T, bool>>(condExpr, argExpr);

            return condExprFunc;
        }

        public static Expression<Func<T, bool>> ToConditionLambdaExpression<T, TProp, TValue>(this Expression<Func<T, TProp>> propExprFunc, List<TValue> valueList, ConditionalOperator op)
        {
            Type propType = typeof(T);

            Expression<Func<T, bool>> condExprFunc = propExprFunc.ToConditionLambdaExpression<T, TValue>(propType, valueList, op);

            return condExprFunc;
        }

        public static Expression<Func<T, bool>> ToConditionLambdaExpression<T, TValue>(this LambdaExpression propExprFunc, Type propType, List<TValue> valueList, ConditionalOperator op)
        {
            if (valueList != null && valueList.Count == 1)
            {
                return propExprFunc.ToConditionLambdaExpression<T, TValue>(propType, valueList.FirstOrDefault(), op);
            }

            Type entityType = typeof(T);
            Type valueType = typeof(TValue);
            ParameterExpression argExpr = Expression.Parameter(entityType, "x");
            string propName = PropertyNamePath(propExprFunc, propType);
            Expression propExpr = GetPropertyExpression<T, TValue>(argExpr, propName);

            Expression condExpr = null;

            if (propType == valueType && (op == ConditionalOperator.Equal || op == ConditionalOperator.NotEqual))
            {
                MethodInfo method = GetEnumerableContainsMethod(valueType);
                Expression valueExpr = GetPropertyValueExpression(propExpr, valueList);

                switch (op)
                {
                    case ConditionalOperator.NotEqual:
                        condExpr = Expression.Not(Expression.Call(null, method, valueExpr, propExpr));
                        break;
                    case ConditionalOperator.Equal:
                    default:
                        condExpr = Expression.Call(null, method, valueExpr, propExpr);
                        break;
                }
            }
            else
            {
                foreach (TValue value in valueList)
                {
                    TValue localValue = value;
                    Expression nextCondExpr = GetConditionalExpression(propExpr, propType, value, op);

                    condExpr = condExpr == null ? nextCondExpr : Expression.Or(condExpr, nextCondExpr);
                }
            }

            Expression<Func<T, bool>> condExprFunc = Expression.Lambda<Func<T, bool>>(condExpr, argExpr);

            return condExprFunc;
        }

        public static Expression<Func<T, bool>> ToExistsLambdaExpression<T, TChild>(this Expression<Func<T, List<TChild>>> propExprFunc, Expression<Func<TChild, bool>> childPredicate)
        {
            Type entityType = typeof(T);
            Type childEntityType = typeof(TChild);
            ParameterExpression argExpr = Expression.Parameter(entityType, "x");
            string propName = PropertyNamePath(propExprFunc);
            Expression propExpr = GetPropertyExpression<T>(argExpr, propName);

            MethodInfo method = GetEnumerableAnyMethod(childEntityType);

            Expression expr = Expression.Call(null, method, propExpr, childPredicate);

            Expression<Func<T, bool>> exprFunc = Expression.Lambda<Func<T, bool>>(expr, argExpr);

            return exprFunc;
        }
    }
}
