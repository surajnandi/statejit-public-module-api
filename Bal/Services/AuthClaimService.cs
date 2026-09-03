using sjam.Bal.Interfaces;
using sjam.Models;
using System.Security.Claims;
using System.Text.Json;

namespace sjam.Bal.Services
{
    public class AuthClaimService : IAuthClaimService
    {
        private readonly AuthClaimModel _user = new();

        public AuthClaimService(IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var claims = httpContextAccessor.HttpContext?.User?.Claims?.ToList();

                if (claims == null || claims.Count == 0)
                    return;

                _user.claims = claims;

                _user.AllClaims = claims.ToDictionary(
                    c => c.Type,
                    c => c.Value);

                string? Get(params string[] keys)
                {
                    foreach (var key in keys)
                    {
                        if (_user.AllClaims.TryGetValue(key, out var value) &&
                            !string.IsNullOrWhiteSpace(value))
                        {
                            return value;
                        }
                    }

                    return null;
                }

                long? GetLong(params string[] keys)
                {
                    return long.TryParse(Get(keys), out var value)
                        ? value
                        : null;
                }

                _user.Id = GetLong(ClaimTypes.NameIdentifier, "nameid");

                _user.Role = Get(ClaimTypes.Role, "role");
                _user.RoleId = GetLong("roleId");

                _user.Level = Get("level");
                _user.LevelId = GetLong("levelId");

                _user.ScopeId = Get("scopeId");
                _user.Scope = Get("scope");
                _user.ParentScope = Get("parent_scope");

                _user.NameId = Get(ClaimTypes.NameIdentifier, "nameid");
                _user.Name = Get(ClaimTypes.Name, "name");
                _user.UserId = Get("userid");

                _user.Email = Get(ClaimTypes.Email, "email");
                _user.PhoneNumber = Get(ClaimTypes.MobilePhone, "phoneNumber");

                _user.Designation = Get("designation");
                _user.CreatedBy = Get("created_by");

                _user.AgencyId = GetLong("aid");

                _user.SessionId = Get("sid");

                _user.TokenId = GetLong("jti");
                _user.PreviousTokenId = GetLong("pti");
                _user.TokenType = Get("typ");

                _user.Issuer = Get("iss");
                _user.Audience = Get("aud");

                _user.FinancialYear = Get("finyear");

                _user.IssuedAt = GetLong("iat");
                _user.NotBefore = GetLong("nbf");
                _user.Expiration = GetLong("exp");

                var permissions = Get("permissions");

                if (!string.IsNullOrWhiteSpace(permissions))
                {
                    try
                    {
                        _user.Permissions = JsonSerializer.Deserialize<List<string>>(permissions) ?? [];
                    }
                    catch
                    {
                        _user.Permissions = [];
                    }
                }
            }
        }

        public AuthClaimModel User => _user;
    }
}
