using HealthCheck.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthCheck.API.Persistence
{
    public class HealthCheckDbContext : DbContext
    {
        public HealthCheckDbContext(DbContextOptions<HealthCheckDbContext> options) : base(options)
        {
            
        }

        public DbSet<HeathCheck> HeathCheck;
    }
}
