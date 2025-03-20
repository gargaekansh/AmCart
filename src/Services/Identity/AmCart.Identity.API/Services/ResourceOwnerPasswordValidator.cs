using AmCart.Identity.API.Models;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AmCart.Identity.API.Services
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ResourceOwnerPasswordValidator(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var user = await _userManager.FindByNameAsync(context.UserName);
            if (user == null)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Invalid username or password");
                return;
            }

            var valid = await _userManager.CheckPasswordAsync(user, context.Password);
            if (!valid)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Invalid username or password");
                return;
            }

            var sub = await _userManager.GetUserIdAsync(user);

            // ✅ Fetch user roles
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
    {
        new Claim(JwtClaimTypes.Subject, sub),
        new Claim(JwtClaimTypes.Id, sub),
        new Claim(JwtClaimTypes.Name, user.UserName)
    };

            // ✅ Add roles as claims
            foreach (var role in roles)
            {
                //claims.Add(new Claim(JwtClaimTypes.Role, role)); // "role" claim

                claims.Add(new Claim(ClaimTypes.Role, role)); // Standard ASP.NET Role claim
                claims.Add(new Claim("role", role));          // Explicitly add "role" for compatibility
                claims.Add(new Claim("roles", role));         // Sometimes expected as an array
                claims.Add(new Claim(JwtClaimTypes.Role, role)); // 🔹 IdentityServer4 role claim

            }

            Console.WriteLine($"✅ User Roles Added to Token: {string.Join(", ", roles)}");

            context.Result = new GrantValidationResult(sub, OidcConstants.AuthenticationMethods.Password, claims);
        }

    }

}
