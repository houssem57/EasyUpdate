using EasyUpdate.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace EasyUpdate.Data
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<SoftwareApp> SoftwareApps { get; set; }
        public DbSet<ScheduledUpdate> ScheduledUpdates { get; set; }
    }
}
