using sjam.Bal.Interfaces;
using sjam.Dal.Entities;
using sjam.Dal.Interfaces;
using sjam.Models;

namespace sjam.Bal.Services
{
    public class MasterService : IMasterService
    {
        private readonly IMasterRepo _masterRepo;
        private readonly IConfiguration _configuration;

        public MasterService(IMasterRepo masterRepo, IConfiguration configuration)
        {
            _masterRepo = masterRepo;
            _configuration = configuration;
        }

        public async Task<ServiceResponse<PagedResult<IEnumerable<dynamic>>>> GetFinancialMasterData(QueryRequest queryRequest)
        {
            return await _masterRepo.GetFinancialMasterData(queryRequest);
        }
    }
}
