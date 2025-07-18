﻿using Stockat.Core.Consts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.IRepositories;

public interface IBaseRepository<T> where T : class
{
    T GetById(int id);
    Task<T> GetByIdAsync(int id);
    Task<T> GetByIdAsync(string id);
    IEnumerable<T> GetAll();
    Task<IEnumerable<T>> GetAllAsync();
    T Find(Expression<Func<T, bool>> criteria, string[] includes = null);
    Task<T> FindAsync(Expression<Func<T, bool>> criteria, string[] includes = null);
    IEnumerable<T> FindAll(Expression<Func<T, bool>> criteria, string[] includes = null);
    IEnumerable<T> FindAll(Expression<Func<T, bool>> criteria, int take, int skip);
    IEnumerable<T> FindAll(Expression<Func<T, bool>> criteria, int? take, int? skip,
        Expression<Func<T, object>> orderBy = null, string orderByDirection = OrderBy.Ascending);

    Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, string[] includes = null, int skip = 0);
    Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, int skip, int take, object includes);
    Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, int? skip, int? take, string[] includes = null,
        Expression<Func<T, object>> orderBy = null, string orderByDirection = OrderBy.Ascending);
    T Add(T entity);
    Task<T> AddAsync(T entity);
    IEnumerable<T> AddRange(IEnumerable<T> entities);
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
    T Update(T entity);
    void Delete(T entity);
    void DeleteRange(IEnumerable<T> entities);
    int Count(Expression<Func<T, bool>> criteria);
    Task<int> CountAsync(Expression<Func<T, bool>> criteria);
    Task DeleteAsync(System.Linq.Expressions.Expression<System.Func<T, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
}
