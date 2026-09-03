using sjam.Bal.Interfaces;
using sjam.Dal.Interfaces;
using sjam.Models;

namespace sjam.Bal.Services
{
    public class PublicService : IPublicService
    {
        private readonly IPublicRepo _publicRepo;
        private readonly IConfiguration _configuration;
        public PublicService(IPublicRepo publicRepo, IConfiguration configuration)
        {
            _publicRepo = publicRepo;
            _configuration = configuration;
        }

        public async Task<ServiceResponse<PagedResult<IEnumerable<dynamic>>>> GetAgencyType(QueryRequest queryRequest)
        {
            var res = await _publicRepo.GetAgencyType(queryRequest);
            return res;
        }
    }
}
