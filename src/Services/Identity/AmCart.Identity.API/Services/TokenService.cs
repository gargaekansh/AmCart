

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AmCart.Identity.API.Configuration;
using AmCart.Identity.API.Models;
using AmCart.Identity.API.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
namespace AmCart.Identity.API.Services
{


    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;

        public TokenService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value; // Access the settings from IOptions
        }

        /// <summary>
        /// Generates a JWT token for the authenticated user based on their claims and RememberMe flag.
        /// </summary>
        /// <param name="user">The authenticated user for whom the token will be generated.</param>
        /// <param name="rememberMe">Indicates whether the token should have a longer expiration time (for "Remember Me" functionality).</param>
        /// <param name="additionalClaims"></param>
        /// <returns>A JWT token as a string that the user can use for authenticated requests.</returns>
        public string GenerateJwtToken(ApplicationUser user, bool rememberMe = false, List<Claim>? additionalClaims = null)
        {
            // Define claims to include in the JWT token
            var claims = new List<Claim>
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email)
        };

            if (additionalClaims != null && additionalClaims.Count > 0)
            {
                foreach (var claim in additionalClaims)
                {
                    if (claim.Type == ClaimTypes.Role) // Add only role claims
                    {
                        claims.Add(claim);
                    }
                }


            }

            // Define the secret key used to sign the JWT token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Set expiration time for the token
            var expiration = rememberMe
                ? DateTime.Now.AddDays(_jwtSettings.RememberMeTokenExpirationDays)
                : DateTime.Now.AddHours(_jwtSettings.TokenExpirationHours);

            // Create the JWT token with specified claims, signing credentials, and expiration
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            // Return the token as a string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}

