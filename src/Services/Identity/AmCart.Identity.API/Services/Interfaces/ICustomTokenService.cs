using AmCart.Identity.API.Models;
using System.Security.Claims;

namespace AmCart.Identity.API.Services.Interfaces
{
    public interface ICustomTokenService
    {
        string GenerateJwtToken(ApplicationUser user, bool rememberMe = false, List<Claim>? additionalClaims = null);
    }
}
