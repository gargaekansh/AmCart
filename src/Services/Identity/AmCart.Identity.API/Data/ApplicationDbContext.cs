//using AmCart.Identity.API.Models;
//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore;

//namespace AmCart.Identity.API.Data
//{
//    /// <summary>
//    /// Database context for Identity Service.
//    /// </summary>
//    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
//    {
//        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

//        protected override void OnModelCreating(ModelBuilder builder)
//        {
//            base.OnModelCreating(builder);
//        }
//    }
//}


using AmCart.Identity.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AmCart.Identity.API.Data
{
    /// <summary>
    /// Database context for Identity-related tables, including users and roles.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ApplicationDbContext"/>.
        /// </summary>
        /// <param name="options">The database context options.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        /// <summary>
        /// Configures entity relationships and constraints using Fluent API.
        /// </summary>
        /// <param name="builder">The model builder.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}


//using AmCart.Identity.API.Models;
//using IdentityServer4.EntityFramework.Entities;
//using IdentityServer4.EntityFramework.Interfaces;
//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore;
//using System.Reflection.Emit;
//using System.Threading.Tasks;

//namespace AmCart.Identity.API.Data
//{
//    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IPersistedGrantDbContext
//    {
//        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

//        // IdentityServer4 Persisted Grant Stores
//        public DbSet<PersistedGrant> PersistedGrants { get; set; }
//        public DbSet<DeviceFlowCodes> DeviceFlowCodes { get; set; } = null!; // 👈 Add null-forgiving operator

//        /// <summary>
//        /// Implements SaveChangesAsync() as required by IPersistedGrantDbContext.
//        /// </summary>
//        public async Task<int> SaveChangesAsync() // No CancellationToken!
//        {
//            return await base.SaveChangesAsync(default);
//        }

//        protected override void OnModelCreating(ModelBuilder builder)
//        {
//            base.OnModelCreating(builder);

//            // Ensure DeviceFlowCodes is ignored if it exists
//            builder.Entity<DeviceFlowCodes>().HasNoKey();

//            builder.Entity<PersistedGrant>()
//               .HasKey(pg => pg.Key);

//            // Apply IdentityServer4 configurations for Persisted Grants
//            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
//        }
//    }
//}




