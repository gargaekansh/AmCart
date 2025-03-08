using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AmCart.Identity.API.Data
{
    /// <summary>
    /// Database context for storing IdentityServer4 persisted grants and device flow codes.
    /// </summary>
    public class PersistedGrantDbContext : DbContext, IPersistedGrantDbContext
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PersistedGrantDbContext"/>.
        /// </summary>
        /// <param name="options">The database context options.</param>
        public PersistedGrantDbContext(DbContextOptions<PersistedGrantDbContext> options) : base(options) { }

        /// <summary>
        /// Gets or sets the database set for persisted grants.
        /// </summary>
        public DbSet<PersistedGrant> PersistedGrants { get; set; }

        /// <summary>
        /// Gets or sets the database set for device flow codes.
        /// </summary>
        public DbSet<DeviceFlowCodes> DeviceFlowCodes { get; set; } = null!;

        /// <summary>
        /// Asynchronously saves changes to the database.
        /// </summary>
        /// <returns>The number of state entries written to the database.</returns>
        public async Task<int> SaveChangesAsync()
        {
            return await base.SaveChangesAsync();
        }

        /// <summary>
        /// Configures entity relationships and constraints using Fluent API.
        /// </summary>
        /// <param name="builder">The model builder.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure the DeviceFlowCodes table without a primary key
            builder.Entity<DeviceFlowCodes>().HasNoKey();

            // Set the primary key for PersistedGrant table
            builder.Entity<PersistedGrant>().HasKey(pg => pg.Key);
        }
    }
}
