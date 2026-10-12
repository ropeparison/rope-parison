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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var rope = modelBuilder.Entity<Rope>();
            rope.Property(r => r.Name).IsRequired().HasMaxLength(150);
            rope.Property(r => r.RowVersion).IsRowVersion();
            rope.Property<string>("NormalizedName").HasMaxLength(150)
                .HasComputedColumnSql("LTRIM(RTRIM([Name])) COLLATE Latin1_General_100_CI_AS", stored: true);
            rope.HasIndex("NormalizedName").IsUnique().HasDatabaseName("UX_Ropes_NormalizedName").HasFilter(null);
            rope.ToTable("Ropes", t => t.HasCheckConstraint("CK_Ropes_Name_NotBlank", "LEN(LTRIM(RTRIM([Name]))) > 0"));
            modelBuilder.Entity<RopeEditSuggestion>().HasIndex(r => new { r.RopeId, r.SubmissionId });
        }
    }
}
