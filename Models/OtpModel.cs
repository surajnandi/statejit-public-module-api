using System.ComponentModel.DataAnnotations;

namespace sjam.Models
{
    public class OtpRequestModel
    {
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Mobile number must be a valid 10-digit.")]
        public string? MobileNo { get; set; }

        [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
        public string? EmailId { get; set; }

        public long? CaptchaId { get; set; }

        public string? CaptchaCode { get; set; }
    }

    public class VerifyOtpRequestModel
    {
        [RegularExpression(@"^[6-9]\d{9}$",
            ErrorMessage = "Mobile number must be a valid 10-digit.")]
        public string? MobileNo { get; set; }

        [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
        public string? EmailId { get; set; }

        public long? CaptchaId { get; set; }

        public string? CaptchaCode { get; set; }

        [Required(ErrorMessage = "OTP ID is required.")]
        public long? OtpId { get; set; }

        [Required(ErrorMessage = "OTP is required.")]
        [RegularExpression(@"^\d{6}$",
        ErrorMessage = "OTP must be a 6-digit number.")]
        public string? OtpCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Session ID is required.")]
        public string? SessionId { get; set; }
    }

    public class OtpResponseModel
    {
        public long? OtpId { get; set; }
        public string? OtpCode { get; set; }
        public string? OtpHasCode { get; set; }
        public string? SessionId { get; set; }
        public bool? Autofill { get; set; }
    }

    public class VerifyOtpResponseModel
    {
        public string? MobileNo { get; set; }
        public string? EmailId { get; set; }
        public long? CaptchaId { get; set; }
        public long? OtpId { get; set; }
        public string? SessionId { get; set; }
    }

    public class OtpCaptchaRequestModel
    {
        [Required(ErrorMessage = "Mobile Number is required.")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Mobile number must be a valid 10-digit.")]
        public string? MobileNo { get; set; }

        [Required(ErrorMessage = "Captcha Code is required.")]
        public string? CaptchaCode { get; set; }
    }

    public class OtpCaptchaResponseModel
    {
        public string? MobileNo { get; set; }
        public string? CaptchaCode { get; set; }
        public string? OtpCode { get; set; }
        public bool? Autofill { get; set; }
    }

}
