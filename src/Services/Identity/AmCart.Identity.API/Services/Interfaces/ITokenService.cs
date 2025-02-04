using AmCart.Identity.API.Models;

namespace AmCart.Identity.API.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateJwtToken(ApplicationUser user, bool rememberMe);
    }
}
