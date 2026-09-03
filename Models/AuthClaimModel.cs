using System.Security.Claims;

namespace sjam.Models
{
    public class AuthClaimModel
    {
        public long? Id { get; set; }
        public string? Role { get; set; }
        public long? RoleId { get; set; }
        public string? Level { get; set; }
        public long? LevelId { get; set; }
        public string? ScopeId { get; set; }
        public string? Scope { get; set; }
        public string? ParentScope { get; set; }
        public string? NameId { get; set; }
        public string? Name { get; set; }
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Designation { get; set; }
        public string? CreatedBy { get; set; }
        public long? AgencyId { get; set; }
        public string? SessionId { get; set; }
        public long? TokenId { get; set; }
        public long? PreviousTokenId { get; set; }
        public string? TokenType { get; set; }
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public string? FinancialYear { get; set; }
        public long? IssuedAt { get; set; }
        public long? NotBefore { get; set; }
        public long? Expiration { get; set; }
        public List<string> Permissions { get; set; } = [];
        public List<Claim> claims { get; set; }
        public string RefreshedAccessToken { get; set; }
        public Dictionary<string, string?> AllClaims { get; set; } = [];
    }

}
