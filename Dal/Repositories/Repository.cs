using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using sjam.Dal.Interfaces;
using sjam.Helpers;
using System.Linq.Expressions;

namespace sjam.Dal.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly EFContext _dbContext;
        private readonly DbSet<T> _dbSet;

        public Repository(EFContext dbContext)
        {
            this._dbContext = dbContext;
            this._dbSet = dbContext.Set<T>();
        }
        public IExecutionStrategy CreateExecutionStrategy()
        {
            return _dbContext.Database.CreateExecutionStrategy();
        }
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _dbContext.Database.BeginTransactionAsync();
        }
        public async Task CommitTransactionAsync(IDbContextTransaction transaction)
        {
            await transaction.CommitAsync();
        }

        public async Task RollbackTransactionAsync(IDbContextTransaction transaction)
        {
            await transaction.RollbackAsync();
        }
        public void SaveChangesManaged()
        {
            _dbContext.SaveChanges();
        }
        public async Task SaveChangesAManaged(T entity)
        {
            this._dbContext.Set<T>().Add(entity);
            await this._dbContext.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            T entity = _dbSet.Find(id);
            _dbSet.Attach(entity);
            _dbContext.Entry<T>(entity).State = EntityState.Deleted;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            IQueryable<T> result;
            result = this._dbContext.Set<T>();


            return await result.ToListAsync();
        }

        public async Task<T> GetSingleAsync(Expression<Func<T, bool>> condition)
        {
            IQueryable<T> result;

            var retValue = await this._dbContext.Set<T>().Where(condition).SingleOrDefaultAsync();

            return retValue;
        }

        public bool Insert(T entity)
        {
            this._dbContext.Set<T>().Add(entity);

            return true;

        }

        public bool Update(T entity)
        {
            this._dbContext.Entry(entity).State = EntityState.Modified;

            return true;
        }

        public bool Delete(T entity)
        {
            this._dbContext.Set<T>().Remove(entity);

            return true;
        }

        public async Task<T> GetSingleAysnc(Expression<Func<T, bool>> condition)
        {
            var retValue = await _dbContext.Set<T>().Where(condition).SingleOrDefaultAsync();

            return retValue;
        }

        //public async Task ExecuteStoredProcedureAsync(string storedProcedureName, Dictionary<string, object> parameters)
        //{
        //    // Build the SQL call string, i.e., "CALL my_procedure(@param1, @param2)"
        //    var sql = $"CALL {storedProcedureName}({string.Join(", ", parameters.Keys.Select(k => "@" + k))})";

        //    // Create the Npgsql parameters
        //    var npgsqlParameters = parameters.Select(p =>
        //        new NpgsqlParameter(p.Key, p.Value) // Assume the correct type is automatically inferred; you can refine this as needed.
        //    ).ToArray();

        //    // Execute the stored procedure with the dynamic parameters
        //    await _dbContext.Database.ExecuteSqlRawAsync(sql, npgsqlParameters);
        //}

        public async Task ExecuteStoredProcedureAsync(string storedProcedureName, Dictionary<string, object> parameters)
        {
            // Build the SQL call string, e.g., "CALL my_procedure(@param1, @param2)"
            var sql = $"CALL {storedProcedureName}({string.Join(", ", parameters.Keys.Select(k => "@" + k))})";

            // Create the Npgsql parameters
            //var npgsqlParameters = parameters.Select(p =>
            //    new NpgsqlParameter(p.Key, p.Value) // Assume the correct type is automatically inferred; you can refine this as needed.
            //).ToArray();

            var npgsqlParameters = parameters.Select(p =>
            {
                // If the value is already an NpgsqlParameter, use it directly
                if (p.Value is NpgsqlParameter existingParam)
                {
                    return existingParam;
                }

                // Otherwise, create a new NpgsqlParameter, inferring the type if possible
                var param = new NpgsqlParameter("@" + p.Key, p.Value ?? DBNull.Value);
                return param;
            }).ToArray();
            // Execute the stored procedure with the dynamic parameters
            await _dbContext.Database.ExecuteSqlRawAsync(sql, npgsqlParameters);
        }



        public IQueryable<T> GetAllByCondition(Expression<Func<T, bool>> condition)
        {
            IQueryable<T> result = this._dbContext.Set<T>();
            if (condition != null)
            {
                result = result.Where(condition);
            }

            return result;
        }

        public bool Add(T entity)
        {
            this._dbContext.Set<T>().Add(entity);
            return true;
        }

        public async Task<ICollection<T>> GetAllByConditionAsync(Expression<Func<T, bool>> condition)
        {
            IQueryable<T> result = this._dbContext.Set<T>();
            if (condition != null)
            {
                result = result.Where(condition);
            }

            return await result.ToListAsync();
        }

        public IEnumerable<T> GetAll()
        {
            return _dbSet.ToList();
        }

        public async Task<Dictionary<TKey, List<TResult>>> GetSelectedColumnGroupByConditionAsync<TKey, TResult>(
            Expression<Func<T, bool>> filterExpression,
            Expression<Func<T, TKey>> groupByKeySelector,
            Expression<Func<T, TResult>> selectExpression)
        {
            var data = await this._dbContext.Set<T>()
            .Where(filterExpression)
            .ToListAsync();
            var groupedResult = data
                .GroupBy(groupByKeySelector.Compile())
                .ToDictionary(group => group.Key, group => group.Select(selectExpression.Compile()).ToList());

            return groupedResult;
        }

        public async Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> condition)
        {
            return await _dbContext.Set<T>().Where(condition).FirstOrDefaultAsync();
        }

    }
}
