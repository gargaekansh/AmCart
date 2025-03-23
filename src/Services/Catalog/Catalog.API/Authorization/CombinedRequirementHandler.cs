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

            var scopeSucceeded = false; // Initialize to false

            if (requirement.ScopeRequirements.Any())
            {
                scopeSucceeded = requirement.ScopeRequirements.Any(r => // Use Any() instead of All()
                {
                    if (r is ScopeRequirement scopeRequirement)
                    {
                        if (scopeRequirement.Scope == null)
                        {
                            Console.WriteLine("⚠️ ScopeRequirement.Scope is null.");
                            return false;
                        }
                        return context.User.HasClaim(c => c.Type == "scope" && c.Value == scopeRequirement.Scope);
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ Unexpected IAuthorizationRequirement type in ScopeRequirements.");
                        return false;
                    }
                });
            }

            var roleSucceeded = true;

            //        roleSucceeded = requirement.RoleRequirements
            //.Where(r => r != null)
            //.All(r =>
            //{
            //    var roleProperty = r.GetType().GetProperty("Role");
            //    var roleValue = roleProperty?.GetValue(r) as string;

            //    return roleValue != null &&
            //           (context.User.IsInRole(roleValue) || // Check using IsInRole
            //            context.User.Claims.Any(c => c.Type == "role" && c.Value == roleValue)); // Check raw claim
            //});

            //roleSucceeded = requirement.RoleRequirements
            //    .Where(r => r != null)
            //    .All(r =>
            //    {
            //        var roleProperty = r.GetType().GetProperty("Role");
            //        var roleValue = roleProperty?.GetValue(r) as string;

            //        if (string.IsNullOrWhiteSpace(roleValue))
            //        {
            //            Console.WriteLine("❌ RoleRequirement has null/empty role!");
            //            return false;
            //        }

            //        Console.WriteLine($"🔹 Required Role: {roleValue}");

            //        // 🔹 Retrieve roles from claims (support both "role" and schema-based "http://schemas...")
            //        var userRoles = context.User.Claims
            //            .Where(c => c.Type == "role" || c.Type == ClaimTypes.Role)
            //            .Select(c => c.Value)
            //            .ToHashSet(StringComparer.OrdinalIgnoreCase);

            //        Console.WriteLine($"✅ User Roles in JWT: [{string.Join(", ", userRoles)}]");

            //        return userRoles.Contains(roleValue);
            //    });

            if (requirement.RoleRequirements?.Any() == true)
            {
                // Retrieve roles from claims (support both "role" and schema-based "http://schemas...")
                var userRoles = context.User.Claims
                    .Where(c => c.Type == "role" || c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

               //var  roleSucceededTest = userRoles.Contains("Administrator");
               // Console.WriteLine($"🔹 roleSucceededTest: {roleSucceededTest}");

                Console.WriteLine($"✅ User Roles in JWT: [{string.Join(", ", userRoles)}]");

                roleSucceeded = requirement.RoleRequirements
                    .Where(r => r != null)
                    .Any(r =>
                    {
                        if (r is RoleRequirement roleRequirement)
                        {
                            string roleValue = roleRequirement.Role;

                            if (string.IsNullOrWhiteSpace(roleValue))
                            {
                                Console.WriteLine("❌ RoleRequirement has null/empty role!");
                                return false;
                            }

                            Console.WriteLine($"🔹 Required Role: {roleValue}");
                            Console.WriteLine($"🔹 RoleRequirement.Role value: {roleRequirement.Role}"); // Added logging

                            return userRoles.Contains(roleValue);
                        }
                        else
                        {
                            Console.WriteLine($"⚠️ Unexpected IAuthorizationRequirement type in RoleRequirements.");
                            return false;
                        }
                    });
            }


            ////inside the handler:
            //roleSucceeded = requirement.RoleRequirements
            //    .Where(r => r != null)
            //    .All(r =>
            //    {
            //        if (string.IsNullOrWhiteSpace(r.Role))
            //        {
            //            Console.WriteLine("❌ RoleRequirement has null/empty role!");
            //            return false;
            //        }

            //        Console.WriteLine($"🔹 Required Role: {r.Role}");

            //        var userRoles = context.User.Claims
            //            .Where(c => c.Type == "role" || c.Type == ClaimTypes.Role)
            //            .Select(c => c.Value)
            //            .ToHashSet(StringComparer.OrdinalIgnoreCase);

            //        Console.WriteLine($"✅ User Roles in JWT: [{string.Join(", ", userRoles)}]");

            //        return userRoles.Contains(r.Role);
            //    });



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
            //var userRoles = context.User.Claims
            //    .Where(c => c.Type == ClaimTypes.Role)
            //    .Select(c => c.Value)
            //    .ToList();

            //Console.WriteLine($"📌 User Roles in JWT: {string.Join(", ", userRoles)}");

            // Final validation
            if (!roleSucceeded)
            {
                Console.WriteLine("❌ Role validation failed. User does not meet the role requirements.");
            }

            Console.WriteLine($"Scope Validation: {scopeSucceeded}");
            Console.WriteLine($"Role Validation: {roleSucceeded}");

            if (scopeSucceeded || roleSucceeded)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
