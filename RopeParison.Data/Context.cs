using Microsoft.EntityFrameworkCore;
using RopeParison.Data.Model;

namespace RopeParison.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        public DbSet<Rope> Ropes { get; set; }
        public DbSet<RopeCalculatedParameterSet> RopeCalculatedParameterSets { get; set; }
        public DbSet<Brand> Brands { get; set; }

        public DbSet<RopeEditSuggestion> RopeEditSuggestions { get; set; }
    }
}
