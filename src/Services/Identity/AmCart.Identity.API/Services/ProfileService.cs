using AmCart.Identity.API.Models;
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AmCart.Identity.API.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ProfileService> _logger;

        public ProfileService(UserManager<ApplicationUser> userManager, ILogger<ProfileService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        // Get user claims to include in the token
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            Debug.WriteLine("🔹 ProfileService HIT!");
            Console.WriteLine("🔹 ProfileService HIT!");

            // Check if claims are requested
            if (context.RequestedClaimTypes != null)
            {
                Console.WriteLine($"🔹 Requested Claims: {string.Join(", ", context.RequestedClaimTypes)}");
            }

            var user = await _userManager.FindByIdAsync(context.Subject.GetSubjectId()); // Get user by ID

            if (user != null)
            {
                // Create a list of claims to include in the access token
                var claims = new List<Claim>
                {
                    new Claim(JwtClaimTypes.PreferredUserName, user.UserName), // ✅ Best option for IdentityServer4
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Ensure ID is string
                    new Claim(JwtClaimTypes.Subject, user.Id), // IdentityServer4 expects 'sub' claim
                    new Claim(JwtClaimTypes.PreferredUserName, user.UserName), // Username
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName), // Unique Name claim
                };

                // Include additional user properties like Gender, MobileNumber if necessary
                if (!string.IsNullOrEmpty(user.Gender))
                {
                    claims.Add(new Claim("gender", user.Gender)); // Add gender as claim
                }

                if (!string.IsNullOrEmpty(user.MobileNumber))
                {
                    claims.Add(new Claim("mobile_number", user.MobileNumber)); // Add mobile number as claim
                }

                // Fetch user roles and add them as claims
                var roles = await _userManager.GetRolesAsync(user);

                context.IssuedClaims.AddRange(roles.Select(role => new Claim(JwtClaimTypes.Role, role)));

                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role)); // Standard ASP.NET Role claim
                    claims.Add(new Claim("role", role));          // Explicitly add "role" for compatibility
                    claims.Add(new Claim("roles", role));         // Sometimes expected as an array
                    claims.Add(new Claim(JwtClaimTypes.Role, role)); // 🔹 IdentityServer4 role claim
                }

                // Debugging: Log the subject claim to ensure it's included
                var subClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (subClaim != null)
                {
                    _logger.LogInformation($"Sub claim value: {subClaim.Value}");
                }
                else
                {
                    _logger.LogWarning("Sub claim was not added.");
                }

                // Add all claims to the context, which will be included in the access token
                context.IssuedClaims.AddRange(claims);
            }
            else
            {
                _logger.LogWarning($"User not found: {context.Subject.GetSubjectId()}");
            }
        }

        // Check if the user is active (for example, if they are not locked out)
        public async Task IsActiveAsync(IsActiveContext context)
        {
            var user = await _userManager.FindByIdAsync(context.Subject.GetSubjectId());
            context.IsActive = user != null; // User is active if found
        }
    }
}
