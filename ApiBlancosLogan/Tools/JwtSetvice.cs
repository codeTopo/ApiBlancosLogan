using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlancosLoganApi.Tools
{
    public class JwtService
    {
        private readonly string _secretKey;
        public JwtService(string secretKey)
        {
            _secretKey = secretKey;
        }
        public string GenerateToken(string email, List<string> roles)
        {
            var keyBytes = Encoding.ASCII.GetBytes(_secretKey);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, email)
            };
            // Agrega los roles como Claims
            if (roles != null)
            {
                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            }
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

    };
}
