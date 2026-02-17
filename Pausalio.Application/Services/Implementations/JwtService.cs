using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Pausalio.Application.DTOs.UserProfile;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Services.Implementations
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;

        public JwtService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public string GenerateToken(UserProfileToReturnDto user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName),
                new Claim("IsActive", user.IsActive.ToString()),
            };

            claims.Add(new Claim(ClaimTypes.Role, user.Role.ToString()));

            var businessIds = user.UserBusinessProfiles
                .Where(ubp => ubp.BusinessProfile != null)
                .Select(ubp => ubp.BusinessProfile.Id.ToString())
                .ToList();

            if (businessIds.Any())
            {
                claims.Add(new Claim("AvailableBusinesses", string.Join(",", businessIds)));

                var businessRoles = user.UserBusinessProfiles
                    .Select(ubp => ubp.Role.ToString())
                    .Distinct();

                foreach (var role in businessRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }
            if (user.ProfilePicture != null)
                claims.Add(new Claim("ProfilePicture", user.ProfilePicture));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}