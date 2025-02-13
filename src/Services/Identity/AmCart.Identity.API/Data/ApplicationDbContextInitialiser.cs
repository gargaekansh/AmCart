using AmCart.Identity.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace AmCart.Identity.API.Data
{
    public class ApplicationDbContextInitialiser
    {
        private readonly ILogger<ApplicationDbContextInitialiser> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void MigrateDatabaseAndSeed()
        {
            _logger.LogInformation("MigrateDatabaseAndSeedAsync started");

            try
            {
                // Retry logic for SQL Server is generally more stable with a direct approach
                var retryPolicy = Policy.Handle<SqlException>()
                    .WaitAndRetry(
                        retryCount: 5,
                        // 2 secs, 4, 8, 16, 32 
                        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        onRetry: (exception, retryCount, context) =>
                        {
                            _logger.LogError("Retrying MigrateDatabaseAndSeed {RetryCount} of {ContextPolicyKey} at {ContextOperationKey}, due to: {Exception}", retryCount, context.PolicyKey,
                                context.OperationKey, exception);
                        });

                retryPolicy.Execute(MigrateAndSeed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the database");
                throw;
            }

            _logger.LogInformation("MigrateDatabaseAndSeedAsync completed");
        }

        private void MigrateAndSeed()
        {
            //// Apply any pending migrations to the database
            _context.Database.Migrate();

            //// Seed database with initial data
            SeedDatabase().Wait();
        }

        //private async Task SeedDatabase()
        //{
        //    // Default roles
        //    var administratorRole = new IdentityRole("Administrator");

        //    if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
        //    {
        //        await _roleManager.CreateAsync(administratorRole);
        //    }

        //    // Default users
        //    var administrator = new ApplicationUser { UserName = "administrator@localhost", Email = "administrator@localhost" };

        //    if (_userManager.Users.All(u => u.UserName != administrator.UserName))
        //    {
        //        await _userManager.CreateAsync(administrator, "Administrator1!");
        //        await _userManager.AddToRolesAsync(administrator, new[] { administratorRole.Name });
        //    }
        //}

        private async Task SeedDatabase()
        {
            // 1. Create Roles (Idempotent)
            if (!await _roleManager.RoleExistsAsync("Administrator"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Administrator"));
            }

            if (!await _roleManager.RoleExistsAsync("User")) // Create "User" role
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }

            // 2. Create Administrator User (Idempotent)
            var administrator = await _userManager.FindByEmailAsync("administrator@localhost");
            if (administrator == null)
            {
                administrator = new ApplicationUser { UserName = "administrator@localhost", Email = "administrator@localhost" };
                var result = await _userManager.CreateAsync(administrator, "Administrator1!"); // Use env variable!
                if (!result.Succeeded)
                {
                    // Log and handle errors
                    foreach (var error in result.Errors)
                    {
                        _logger.LogError(error.Description);
                    }
                    throw new Exception("Failed to create admin user");
                }
                await _userManager.AddToRoleAsync(administrator,   "Administrator" );
            }

            // 3. Example User Creation (You'll likely do this through registration)
            // This is just an example; you'll typically create users through your registration process.
            var testUser = await _userManager.FindByEmailAsync("testuser@localhost");
            if (testUser == null)
            {
                testUser = new ApplicationUser { UserName = "testuser@localhost", Email = "testuser@localhost" };
                var userResult = await _userManager.CreateAsync(testUser, "UserPassword1!"); // Use a configuration source for passwords!
                if (!userResult.Succeeded)
                {
                    // Log and handle errors
                    foreach (var error in userResult.Errors)
                    {
                        _logger.LogError(error.Description);
                    }
                    throw new Exception("Failed to create test user");
                }
                await _userManager.AddToRoleAsync(testUser,  "User" ); // Add to "User" role
            }
        }
    }
}
