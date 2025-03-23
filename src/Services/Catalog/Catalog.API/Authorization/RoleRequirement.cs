using Microsoft.AspNetCore.Authorization;

namespace Catalog.API.Authorization
{
    public class RoleRequirement : IAuthorizationRequirement
    {
        public string Role { get; set; }
    }
}
