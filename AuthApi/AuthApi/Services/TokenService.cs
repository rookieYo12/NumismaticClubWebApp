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
        private const int EXP = 5; // In minutes

        private static SymmetricSecurityKey GetSymmetricSecurityKey() =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));

        public string GenerateToken(User user)
        {
            // Add other payload information
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub,  user.Id),
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
            };

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
    }
}
