using sjam.Dal.Entities;
using sjam.Models;

namespace sjam.Dal.Interfaces
{
    public interface IMasterRepo
    {
        Task<ServiceResponse<PagedResult<IEnumerable<dynamic>>>> GetFinancialMasterData(QueryRequest queryRequest);
    }
}
