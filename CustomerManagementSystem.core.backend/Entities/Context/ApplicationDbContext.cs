using CustomerManagementSystem.core.backend.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagementSystem.core.backend.Entities.AppDataContext
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<OrgMember> OrgMembers { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Idempotency> Idempotencies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Idempotency>()
                .HasIndex(x => x.Key)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasIndex(x => x.OrgId);
                entity.HasOne(x => x.OrgMember)
                    .WithMany()
                    .HasForeignKey(x => x.OrgId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
