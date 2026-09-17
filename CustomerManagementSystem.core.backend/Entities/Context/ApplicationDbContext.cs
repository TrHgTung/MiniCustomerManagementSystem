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
    }
}
