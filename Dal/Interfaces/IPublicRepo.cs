using sjam.Models;

namespace sjam.Dal.Interfaces
{
    public interface IPublicRepo
    {
        Task<ServiceResponse<PagedResult<IEnumerable<dynamic>>>> GetAgencyType(QueryRequest queryRequest);
    }
}
