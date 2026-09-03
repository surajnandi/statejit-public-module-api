using Microsoft.Extensions.Configuration.UserSecrets;
using sjam.Bal.Interfaces;
using sjam.Dal.Enum;
using sjam.Dal.Interfaces;
using sjam.Dal.Repositories;
using sjam.Helpers;
using sjam.Models;

namespace sjam.Bal.Services
{
    public class OtpService : IOtpService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OtpService> _logger;
        private readonly IOtpRepo _otpRepo;

        public OtpService(IConfiguration configuration, ILogger<OtpService> logger, IOtpRepo otpRepo)
        {
            _configuration = configuration;
            _logger = logger;
            _otpRepo = otpRepo;
        }

        public async Task<ServiceResponse<OtpResponseModel>> GetOtpRequest(OtpRequestModel request, string? ipAddress)
        {
            var response = new ServiceResponse<OtpResponseModel>();

            try
            {
                // Generate OTP
                var otpCode = OtpHelper.GenerateOtp();

                // Hash OTP
                var otpHashCode = OtpHelper.Sha256(otpCode);

                // Generate unique session
                var sessionId = OtpHelper.GenerateSessionId();

                // OTP lifetime
                var lifetime = _configuration.GetValue<int>("AppConfig:OtpValidateLifetime");

                if (lifetime <= 0)
                {
                    response.ResponseStatus = APIResponseStatus.Error;
                    response.Message = "OTP validation lifetime is not configured properly.";
                    return response;
                }

                var expiryAt = DateTime.Now.AddMinutes(lifetime);

                // Save OTP
                var result = await _otpRepo.GetOtpRequest(
                    request,
                    otpCode,
                    otpHashCode,
                    sessionId,
                    expiryAt,
                    ipAddress);

                if (result == null)
                {
                    response.ResponseStatus = APIResponseStatus.Error;
                    response.Message = "Unable to generate OTP.";
                    return response;
                }

                // Environment
                var environment = _configuration.GetValue<AppEnvironment>("AppConfig:Environment");

                // DEV / UAT
                if (environment == AppEnvironment.DEV ||
                    environment == AppEnvironment.UAT)
                {
                    result.OtpCode = otpCode;
                    result.OtpHasCode = null;
                    result.Autofill = true;
                }
                else
                {
                    // PROD
                    result.OtpCode = null;
                    result.OtpHasCode = otpHashCode;
                    result.Autofill = false;
                }

                response.Result = result;
                response.ResponseStatus = APIResponseStatus.Success;
                response.Message = "OTP generated successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while generating OTP.");

                response.ResponseStatus = APIResponseStatus.Error;
                response.Message = "Unable to generate OTP.";
            }

            return response;
        }

        public async Task<ServiceResponse<VerifyOtpResponseModel>> VerifyOtpRequest(VerifyOtpRequestModel request, string? ipAddress)
        {
            var response = new ServiceResponse<VerifyOtpResponseModel>();

            try
            {
                var result = await _otpRepo.VerifyOtpRequest(request);

                if (result == null)
                {
                    response.ResponseStatus = APIResponseStatus.Error;
                    response.Message = "OTP expired or Invalid.";

                    return response;
                }

                response.Result = result;
                response.ResponseStatus = APIResponseStatus.Success;
                response.Message = "OTP verified successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while verifying OTP. OTP ID: {OtpId}",
                    request.OtpId);

                response.ResponseStatus = APIResponseStatus.Error;
                response.Message = "Unable to verify OTP.";
            }

            return response;
        }

    }
}
