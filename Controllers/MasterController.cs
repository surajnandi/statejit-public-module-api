using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sjam.Bal.Interfaces;
using sjam.Helpers;
using sjam.Models;

namespace sjam.Controllers
{
    [NoAuthorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MasterController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IMasterService _masterService;
        private readonly IAuthClaimService _authClaimService;

        public MasterController (IConfiguration config, IMasterService masterService, IAuthClaimService authClaimService)
        {
            _config = config;
            _masterService = masterService;
            _authClaimService = authClaimService;
        }

        [HttpPost("get-financial-master-data")]
        public async Task<IActionResult> GetFinancialMasterData([FromBody] QueryRequest queryRequest)
        {
            var response = await _masterService.GetFinancialMasterData(queryRequest);
            return Ok(response);
        }
    }
}
