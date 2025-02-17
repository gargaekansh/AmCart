using AmCart.Identity.API.Controllers;
using AmCart.Identity.API.Models;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
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

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var user = await _userManager.FindByIdAsync(context.Subject.GetSubjectId()); // Get user by ID

            if (user != null)
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) // Convert to string if necessary
                // Add other user claims as needed (e.g., user.Gender, user.MobileNumber)


            };

                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role)); // Add roles as claims
                }

                // Check if the claim was added (for debugging)
                var subClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (subClaim != null)
                {
                    _logger.LogInformation($"Sub claim value: {subClaim.Value}"); // Log the value
                }
                else
                {
                    _logger.LogWarning("Sub claim was not added."); // Log if it's missing
                }

                context.IssuedClaims.AddRange(claims); // Add claims to the token
            }
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var user = await _userManager.FindByIdAsync(context.Subject.GetSubjectId());
            context.IsActive = user != null; // User is active if found
        }
    }
}
