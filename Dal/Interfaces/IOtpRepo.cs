using sjam.Dal.Entities;
using sjam.Models;

namespace sjam.Dal.Interfaces
{
    public interface IOtpRepo
    {
        Task<OtpResponseModel?> GetOtpRequest(OtpRequestModel request, string otpCode, string otpHashCode, string sessionId, DateTime expiryAt, string? ipAddress);
        Task<VerifyOtpResponseModel?> VerifyOtpRequest(VerifyOtpRequestModel request);
    }
}
