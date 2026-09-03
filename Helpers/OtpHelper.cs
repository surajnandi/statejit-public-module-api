using System.Security.Cryptography;
using System.Text;

namespace sjam.Helpers
{
    public static class OtpHelper
    {
        public static string GenerateOtp()
        {
            return RandomNumberGenerator
                .GetInt32(100000, 1000000)
                .ToString();
        }

        public static string GenerateSessionId()
        {
            return Guid.NewGuid().ToString("N");
        }

        public static string Sha256(string value)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));

            return Convert.ToHexString(bytes);
        }

        public static string GenerateMasterOtp()
        {
            return DateTime.Now.ToString("ddMMHH");
        }
    }
}
