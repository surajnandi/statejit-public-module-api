using sjam.Models;

namespace sjam.Bal.Interfaces
{
    public interface IPublicService
    {
        Task<ServiceResponse<PagedResult<IEnumerable<dynamic>>>> GetAgencyType(QueryRequest queryRequest);
    }
}
