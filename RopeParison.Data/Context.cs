using Microsoft.EntityFrameworkCore;
using RopeParison.Data.Model;

namespace RopeParison.Data
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {

        }

        public DbSet<Rope> Ropes { get; set; }
        public DbSet<RopeCalculatedParameterSet> RopeCalculatedParameterSets { get; set; }
        public DbSet<Brand> Brands { get; set; }
    }
}
