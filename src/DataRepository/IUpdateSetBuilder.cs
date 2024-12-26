﻿using System.Linq.Expressions;

namespace DataRepository
{
    public interface IUpdateSetBuilder<TEntity>
    {
        int SetCount { get; }

        Expression Build();

        IUpdateSetBuilder<TEntity> SetProperty<TProperty>(Expression<Func<TEntity, TProperty>> selector, TProperty value);

        IUpdateSetBuilder<TEntity> SetProperty<TProperty>(Expression<Func<TEntity, TProperty>> selector, Expression<Func<TEntity, TProperty>> valueExp);
    }
}
