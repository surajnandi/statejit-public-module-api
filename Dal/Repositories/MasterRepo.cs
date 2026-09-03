using Dapper;
using sjam.Bal.Interfaces;
using sjam.Dal.Entities;
using sjam.Dal.Enum;
using sjam.Dal.Interfaces;
using sjam.Helpers;
using sjam.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace sjam.Dal.Repositories
{
    public class MasterRepo : IMasterRepo
    {
        private readonly DapperContext _dapperContext;
        private readonly EFContext _efContext;
        private readonly IAuthClaimService _authClaimService;

        public MasterRepo(DapperContext dapperContext, EFContext efContext, IAuthClaimService authClaimService)
        {
            _dapperContext = dapperContext;
            _efContext = efContext;
            _authClaimService = authClaimService;
        }

        public async Task<ServiceResponse<PagedResult<IEnumerable<dynamic>>>> GetFinancialMasterData(QueryRequest queryRequest)
        {
            try
            {
                using var conn = _dapperContext.CreateConnection();
                var sql = $@"
                    SELECT
                    fin_year_short, fin_year_long, current_fin_year, is_active, created_at, created_by 
                    FROM master.financial_year
                    ORDER BY id DESC";

                // Apply in-memory filter, search, sort, pagination
                var res = await DapperQueryHelper.ExecuteAsync<dynamic>(
                    conn,
                    sql,
                    queryRequest
                );

                return new ServiceResponse<PagedResult<IEnumerable<dynamic>>>
                {
                    Result = res,
                    ResponseStatus = res.Data.Any() ? APIResponseStatus.Success : APIResponseStatus.Warning,
                    Message = res.Data.Any() ? "Data found" : "No data found!"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<PagedResult<IEnumerable<dynamic>>>
                {
                    ResponseStatus = APIResponseStatus.Error,
                    Message = ex.Message
                };
            }
        }
    }
}
