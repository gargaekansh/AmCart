//using Microsoft.EntityFrameworkCore;
//using IdentityServer4.EntityFramework.DbContexts;
//using IdentityServer4.EntityFramework.Options;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;

//namespace AmCart.Identity.API.Data
//{


//    public class PersistedGrantDbContext : IdentityServer4.EntityFramework.DbContexts.PersistedGrantDbContext
//    {
//        public PersistedGrantDbContext(DbContextOptions<PersistedGrantDbContext> options)
//            : base(options)
//        {
//        }

//        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//        {
//            // Configure connection string here (ensure it's correct)
//            base.OnConfiguring(optionsBuilder);
//            optionsBuilder.UseSqlServer("YourConnectionStringHere"); // Replace with your connection string
//        }
//    }




//}
