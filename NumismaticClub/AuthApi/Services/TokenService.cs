using System.CodeDom.Compiler;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using AuthApi.Models;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AuthApi.Services
{
    public class TokenService
    {
        // Key and some payload information
        private const string KEY = "mysupersecret_secretsecretsecretkey!123";
        private const string ISS = "AuthApi";
        private const string AUD = "ApiGateway";
        private const int EXP = 15; // In minutes

        private static SymmetricSecurityKey GetSymmetricSecurityKey() =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));

        public string GenerateToken(List<Claim> claims)
        {
            // Build token
            var jwtToken = new JwtSecurityToken(
                issuer: ISS,
                audience: AUD,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(EXP),
                signingCredentials: new SigningCredentials(
                    GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToHexString(randomNumber);
            }
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = ISS,
                ValidateAudience = true,
                ValidAudience = AUD,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = GetSymmetricSecurityKey()
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            // If validation param are met return security token and claims
            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            // Try convert to jwt
            var jwtToken = validatedToken as JwtSecurityToken;

            // Token is wrong if is not jwt or not same alg
            if (jwtToken == null || !jwtToken.Header.Alg.Equals(
                SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token.");
            }

            return principal;
        }
    }
}
