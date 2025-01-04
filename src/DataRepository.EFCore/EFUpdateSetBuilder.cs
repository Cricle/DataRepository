using Microsoft.EntityFrameworkCore.Query;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace DataRepository.EFCore
{
    public sealed class EFUpdateSetBuilder<TEntity> : IUpdateSetBuilder<TEntity>
    {
        private Expression? current;
        private int setCount;

        private readonly ParameterExpression paramter;

        public int SetCount => setCount;

        public Expression Instance => current ?? paramter;

        public EFUpdateSetBuilder()
        {
            paramter = Expression.Parameter(typeof(SetPropertyCalls<TEntity>), "w");

        }

        public Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> Build()
        {
            if (current == null)
                throw new InvalidOperationException("Must at less one set");
            
            return Expression.Lambda<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>>(current, paramter);
        }

        public IUpdateSetBuilder<TEntity> SetProperty<TProperty>(Expression<Func<TEntity, TProperty>> selector, TProperty value)
        {
            current = Expression.Call(Instance, EntityCache.PropertyCache<TProperty>.SetPropertyMethod, selector, Expression.Constant(value));
            setCount++;
            return this;
        }

        public IUpdateSetBuilder<TEntity> SetProperty<TProperty>(Expression<Func<TEntity, TProperty>> selector, Expression<Func<TEntity, TProperty>> valueExp)
        {
            current = Expression.Call(Instance, EntityCache.PropertyCache<TProperty>.SetPropertyExpMethod, selector, valueExp);
            setCount++;
            return this;
        }

        Expression IUpdateSetBuilder<TEntity>.Build() => Build();

        [ExcludeFromCodeCoverage]
        class EntityCache
        {
            private static readonly Type SetPropertyCallType = typeof(SetPropertyCalls<>).MakeGenericType(typeof(TEntity));
            private static readonly MethodInfo[] Methods = SetPropertyCallType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            
            public class PropertyCache<TProperty>
            {

                public static readonly MethodInfo SetPropertyMethod =
                    Methods.FirstOrDefault(x => x.Name == "SetProperty" && x.IsGenericMethod && !x.GetParameters()[1].ParameterType.IsGenericType)?.MakeGenericMethod(typeof(TProperty))
                    ?? throw new InvalidOperationException($"The {SetPropertyCallType} not SetProperty<TProperty>(Func<TSource, TProperty> propertyExpression,TProperty valueExpression) method");

                public static readonly MethodInfo SetPropertyExpMethod =
                    Methods.FirstOrDefault(x => x.Name == "SetProperty" && x.IsGenericMethod && x.GetParameters()[1].ParameterType.IsGenericType)?.MakeGenericMethod(typeof(TProperty))
                    ?? throw new InvalidOperationException($"The {SetPropertyCallType} not SetProperty<TProperty>(Func<TSource, TProperty> propertyExpression,Func<TEntity, TProperty> valueExpression) method");

            }
        }
    }

}
