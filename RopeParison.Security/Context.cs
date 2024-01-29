using Microsoft.EntityFrameworkCore;
using RopeParison.Security.Model;

namespace RopeParison.Security
{
    public class SecurityContext : DbContext
    {
        public SecurityContext(DbContextOptions<SecurityContext> options) : base(options)
        {

        }

        public DbSet<Password> Passwords { get; set; }
    }
}
