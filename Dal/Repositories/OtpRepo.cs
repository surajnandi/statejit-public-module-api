using Dapper;
using Microsoft.Extensions.Configuration.UserSecrets;
using sjam.Bal.Interfaces;
using sjam.Bal.Services;
using sjam.Dal.Entities;
using sjam.Dal.Interfaces;
using sjam.Helpers;
using sjam.Models;

namespace sjam.Dal.Repositories
{
    public class OtpRepo : IOtpRepo
    {
        private readonly DapperContext _dapperContext;
        private readonly EFContext _dbContext;
        private readonly IAuthClaimService _authClaimService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OtpRepo> _logger;

        public OtpRepo(DapperContext dapperContext, EFContext dbContext, IAuthClaimService authClaimService, IConfiguration configuration, ILogger<OtpRepo> logger)
        {
            _dapperContext = dapperContext;
            _dbContext = dbContext;
            _authClaimService = authClaimService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<OtpResponseModel?> GetOtpRequest(OtpRequestModel request, string otpCode, string otpHashCode, string sessionId, DateTime expiryAt, string? ipAddress)
        {
            try
            {
                const string sql = @"
                INSERT INTO agency.otp_request
                    (
                        mobile_no,
                        email_id,
                        captcha_id,
                        captcha_code,
                        otp_id,
                        otp_code,
                        otp_hash_code,
                        created_at,
                        expiry_at,
                        is_verified,
                        ip_address,
                        session_id
                    )
                    VALUES
                    (
                        @MobileNo,
                        @EmailId,
                        @CaptchaId,
                        @CaptchaCode,
                        @OtpId,
                        @OtpCode,
                        @OtpHashCode,
                        CURRENT_TIMESTAMP,
                        @ExpiryAt,
                        FALSE,
                        @IpAddress,
                        @SessionId
                    )
                    RETURNING otp_id;
                ";

                var otpId = Random.Shared.NextInt64(100000, 1000000);

                using var connection = _dapperContext.CreateConnection();

                var generatedOtpId = await connection.ExecuteScalarAsync<long>(
                    sql,
                    new
                    {
                        request.MobileNo,
                        request.EmailId,
                        request.CaptchaId,
                        request.CaptchaCode,
                        OtpId = otpId,
                        OtpCode = otpCode,
                        OtpHashCode = otpHashCode,
                        ExpiryAt = expiryAt,
                        IpAddress = ipAddress,
                        SessionId = sessionId
                    });

                return new OtpResponseModel
                {
                    OtpId = generatedOtpId,
                    OtpCode = otpCode,
                    SessionId = sessionId,
                    Autofill = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while creating OTP request.");

                throw;
            }
        }


        public async Task<VerifyOtpResponseModel?> VerifyOtpRequest(VerifyOtpRequestModel request)
        {
            try
            {
                const string sql = @"
                UPDATE agency.otp_request
                    SET
                        is_verified = TRUE
                    WHERE otp_id = @OtpId
                      AND
                        (
                            otp_code = @OtpCode
                            OR @OtpCode = TO_CHAR(
                                CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata',
                                'DDMMHH24'
                            )
                        )
                      AND session_id = @SessionId
                      AND is_verified = FALSE
                      AND expiry_at > CURRENT_TIMESTAMP
                    RETURNING
                        mobile_no AS MobileNo,
                        email_id AS EmailId,
                        captcha_id AS CaptchaId,
                        otp_id AS OtpId,
                        session_id AS SessionId;
                ";

                var otpHashCode = OtpHelper.Sha256(request.OtpCode!);

                using var connection = _dapperContext.CreateConnection();

                return await connection.QueryFirstOrDefaultAsync<VerifyOtpResponseModel>(
                    sql,
                    new
                    {
                        request.OtpId,
                        request.OtpCode, // MASTER OTP : DDMMHH24 (03 Sep 04:34PM || OTP: 030916)
                        request.SessionId,
                        request.MobileNo,
                        request.EmailId,
                        OtpHashCode = otpHashCode
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while verifying OTP. OTP ID: {OtpId}",
                    request.OtpId);

                throw;
            }
        }

    }
}
