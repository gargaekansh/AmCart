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
            // Apply any pending migrations to the database
            _context.Database.Migrate();

            // Seed database with initial data
            SeedDatabase().Wait();
        }

        private async Task SeedDatabase()
        {
            // Default roles
            var administratorRole = new IdentityRole("Administrator");

            if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
            {
                await _roleManager.CreateAsync(administratorRole);
            }

            // Default users
            var administrator = new ApplicationUser { UserName = "administrator@localhost", Email = "administrator@localhost" };

            if (_userManager.Users.All(u => u.UserName != administrator.UserName))
            {
                await _userManager.CreateAsync(administrator, "Administrator1!");
                await _userManager.AddToRolesAsync(administrator, new[] { administratorRole.Name });
            }
        }
    }
}
