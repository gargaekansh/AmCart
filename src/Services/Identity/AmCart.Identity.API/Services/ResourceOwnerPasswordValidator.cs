using AmCart.Identity.API.Models;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;

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
            context.Result = new GrantValidationResult(sub, OidcConstants.AuthenticationMethods.Password);
        }
    }

}
