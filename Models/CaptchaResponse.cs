namespace sjam.Models
{
    public class CaptchaResponse
    {
        public string? CaptchaImg { get; set; }

        public long? CaptchaId { get; set; }

        public string? CaptchaCode { get; set; }

        public bool? Autofill { get; set; }
    }
}
