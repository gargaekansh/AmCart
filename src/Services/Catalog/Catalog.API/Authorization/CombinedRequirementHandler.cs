using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Security.Claims;
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

            roleSucceeded = requirement.RoleRequirements
    .Where(r => r != null)
    .All(r =>
    {
        var roleProperty = r.GetType().GetProperty("Role");
        var roleValue = roleProperty?.GetValue(r) as string;

        return roleValue != null &&
               (context.User.IsInRole(roleValue) || // Check using IsInRole
                context.User.Claims.Any(c => c.Type == "role" && c.Value == roleValue)); // Check raw claim
    });

            //if (requirement.RoleRequirements?.Any() == true)
            //{
            //    Console.WriteLine("Role requirements found. Checking roles...");

            //    roleSucceeded = requirement.RoleRequirements
            //        .Where(r => r != null) // Ensure no null values
            //        .All(r =>
            //        {
            //            var roleProperty = r.GetType().GetProperty("Role");
            //            var roleValue = roleProperty?.GetValue(r) as string;

            //            if (roleValue == null)
            //            {
            //                Console.WriteLine("⚠️ Role property is missing or null in requirement.");
            //                return false;
            //            }

            //            Console.WriteLine($"🔍 Checking if user is in role: {roleValue}");

            //            bool isInRole = context.User.IsInRole(roleValue);
            //            Console.WriteLine($"➡️ User {(isInRole ? "IS" : "IS NOT")} in role: {roleValue}");

            //            return isInRole;
            //        });

            //    Console.WriteLine($"✅ Role Succeeded: {roleSucceeded}");
            //}

            //// Debugging: Print all roles assigned to the user
            var userRoles = context.User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            Console.WriteLine($"📌 User Roles in JWT: {string.Join(", ", userRoles)}");

            // Final validation
            if (!roleSucceeded)
            {
                Console.WriteLine("❌ Role validation failed. User does not meet the role requirements.");
            }


            if (scopeSucceeded || roleSucceeded)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
