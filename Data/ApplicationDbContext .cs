using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SitBackTradeAPI.Model;

namespace SitBackTradeAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<User>()
                .Property(u => u.Role)
                .IsRequired();
            builder.Entity<User>()
                .Property(u => u.Wallet)
                .HasColumnType("decimal(18, 2)");
        }

        public DbSet<Store>? Stores { get; set; }
    }
}
