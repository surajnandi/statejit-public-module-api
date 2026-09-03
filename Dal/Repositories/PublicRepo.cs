using sjam.Bal.Interfaces;
using sjam.Dal.Enum;
using sjam.Dal.Interfaces;
using sjam.Helpers;
using sjam.Models;

namespace sjam.Dal.Repositories
{
    public class PublicRepo : IPublicRepo
    {
        private readonly DapperContext _dapperContext;
        private readonly EFContext _dbContext;
        private readonly IAuthClaimService _authClaimService;
        private readonly IConfiguration _configuration;

        public PublicRepo(DapperContext dapperContext, EFContext dbContext, IAuthClaimService authClaimService, IConfiguration configuration)
        {
            _dapperContext = dapperContext;
            _dbContext = dbContext;
            _authClaimService = authClaimService;
            _configuration = configuration;
        }


        public async Task<ServiceResponse<PagedResult<IEnumerable<dynamic>>>> GetAgencyType(QueryRequest queryRequest)
        {
            try
            {
                using var conn = _dapperContext.CreateJitReplicationDBConnection();
                var sql = $@"SELECT * FROM ifmsadmin.agency_type ORDER BY id ASC";

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
