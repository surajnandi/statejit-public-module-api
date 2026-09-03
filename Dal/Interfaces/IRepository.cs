using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace sjam.Dal.Interfaces
{
    public interface IRepository<T>
    {
        Task<IEnumerable<T>> GetAllAsync();
        // Task<T> GetByIdAsync(int id);
        IExecutionStrategy CreateExecutionStrategy();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync(IDbContextTransaction transaction);
        Task RollbackTransactionAsync(IDbContextTransaction transaction);

        public bool Insert(T entity);
        // Task<int> UpdateAsync(T entity);
        public Task DeleteAsync(int id);
        Task<T> GetSingleAysnc(Expression<Func<T, bool>> condition);
        public bool Update(T entity);
        Task SaveChangesAManaged(T entity);
        void SaveChangesManaged();
        Task ExecuteStoredProcedureAsync(string storedProcedureName, Dictionary<string, object> parameters);
        IQueryable<T> GetAllByCondition(Expression<Func<T, bool>> condition);
        bool Add(T entity);
        Task<ICollection<T>> GetAllByConditionAsync(Expression<Func<T, bool>> condition);
        IEnumerable<T> GetAll();
        Task<Dictionary<TKey, List<TResult>>> GetSelectedColumnGroupByConditionAsync<TKey, TResult>(
            Expression<Func<T, bool>> filterExpression,
            Expression<Func<T, TKey>> groupByKeySelector,
            Expression<Func<T, TResult>> selectExpression);
        Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> condition);

    }
}
