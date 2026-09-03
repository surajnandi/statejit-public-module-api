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
    public class PublicController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IPublicService _publicService;
        private readonly IAuthClaimService _authClaimService;
        private readonly CaptchaHelper _captchaHelper;
        private readonly IOtpService _otpService;

        public PublicController(IConfiguration config, IPublicService publicService, IAuthClaimService authClaimService, CaptchaHelper captchaHelper, IOtpService otpService)
        {
            _config = config;
            _authClaimService = authClaimService;
            _publicService = publicService;
            _captchaHelper = captchaHelper;
            _otpService = otpService;
        }

        [HttpPost("get-agency-type")]
        public async Task<IActionResult> GetAgencyType([FromBody] QueryRequest queryRequest)
        {
            var response = await _publicService.GetAgencyType(queryRequest);
            return Ok(response);
        }

        [HttpGet("get-captcha")]
        public IActionResult GetCaptcha()
        {
            var response = _captchaHelper.GetCaptcha();

            return Ok(response);
        }

        [HttpPost("send-otp-request")]
        public async Task<IActionResult> SendOtpRequest([FromBody] OtpRequestModel request)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            var response = await _otpService.GetOtpRequest(request, ipAddress);

            return Ok(response);
        }

        [HttpPost("verify-otp-request")]
        public async Task<IActionResult> VerifyOtpRequest([FromBody] VerifyOtpRequestModel request)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            var response = await _otpService.VerifyOtpRequest(request, ipAddress);

            return Ok(response);
        }
    }
}
