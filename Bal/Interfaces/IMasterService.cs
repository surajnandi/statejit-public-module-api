using sjam.Dal.Entities;
using sjam.Models;

namespace sjam.Bal.Interfaces
{
    public interface IMasterService
    {
        Task<ServiceResponse<PagedResult<IEnumerable<dynamic>>>> GetFinancialMasterData(QueryRequest queryRequest);
    }
}
