using Microsoft.AspNetCore.Authorization;

namespace Catalog.API.Authorization
{
    public class ScopeRequirement : IAuthorizationRequirement
    {
        public string Scope { get; }
        public ScopeRequirement(string scope)
        {
            Scope = scope;
        }
    }
}
