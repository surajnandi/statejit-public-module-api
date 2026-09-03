using sjam.Models;

namespace sjam.Bal.Interfaces
{
    public interface IOtpService
    {
        Task<ServiceResponse<OtpResponseModel>> GetOtpRequest(OtpRequestModel request, string? ipAddress);
        Task<ServiceResponse<VerifyOtpResponseModel>> VerifyOtpRequest(VerifyOtpRequestModel request, string? ipAddress);
    }
}
