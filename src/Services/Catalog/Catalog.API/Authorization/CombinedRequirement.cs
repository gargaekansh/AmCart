using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace Catalog.API.Authorization
{
    public class CombinedRequirement : IAuthorizationRequirement
    {
        public List<IAuthorizationRequirement> ScopeRequirements { get; }
        public List<IAuthorizationRequirement> RoleRequirements { get; }

        public CombinedRequirement(List<IAuthorizationRequirement> scopeRequirements, List<IAuthorizationRequirement> roleRequirements)
        {
            ScopeRequirements = scopeRequirements;
            RoleRequirements = roleRequirements;
        }
    }
}
