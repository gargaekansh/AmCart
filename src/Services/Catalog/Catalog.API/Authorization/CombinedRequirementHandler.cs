using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.API.Authorization
{
    public class CombinedRequirementHandler : AuthorizationHandler<CombinedRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CombinedRequirement requirement)
        {
            if (context.User == null)
            {
                return Task.CompletedTask; // Not authenticated
            }

            // Check if either scope or role requirements are met
            var scopeSucceeded = true;
            if (requirement.ScopeRequirements.Any())
            {
                scopeSucceeded = requirement.ScopeRequirements.All(r => context.User.HasClaim(c => c.Type == "scope" && requirement.ScopeRequirements.Any(sr => sr.GetType().GetProperty("AllowedValues").GetValue(sr) as string[] == null ? true : (sr.GetType().GetProperty("AllowedValues").GetValue(sr) as string[]).Contains(c.Value))));
            }

            var roleSucceeded = true;
            if (requirement.RoleRequirements.Any())
            {
                roleSucceeded = requirement.RoleRequirements.All(r => context.User.IsInRole(r.GetType().GetProperty("Role").GetValue(r) as string)); // Simplified role check
            }

            if (scopeSucceeded || roleSucceeded)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
