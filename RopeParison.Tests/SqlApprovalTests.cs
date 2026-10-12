using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RopeParison.Data;
using RopeParison.Data.Model;
using RopeParison.Data.Services;
using RopeParison.Logic.Services;
using RopeParison.Protocol;
using Xunit;

namespace RopeParison.Tests;

// Opt-in because these tests require a local SQL Express instance and CREATE DATABASE permission.
public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("ROPEPARISON_SQL_TESTS") != "1")
            Skip = "Set ROPEPARISON_SQL_TESTS=1 to run isolated local SQL Server tests.";
    }
}

public sealed partial class SqlApprovalTests
{
    [SqlServerFact]
    public void Approval_commits_measurements_calculations_and_suggestion_removal_together()
    {
        using var fixture = new SqlFixture();
        fixture.Service.ApproveRopeEditSuggestion(fixture.SuggestionId, fixture.RopeId, fixture.Version, TestFixture.EditPassword);
        using var db = fixture.CreateDbContext();
        var rope = db.Ropes.Include(r => r.RopeCalculatedParameterSet).Single();
        Assert.Equal(12, rope.Diameter);
        Assert.Equal(36 * Math.PI, rope.RopeCalculatedParameterSet.Area!.Value, 10);
        Assert.Equal(1.2, rope.RopeCalculatedParameterSet.SheathThickness!.Value, 10);
        Assert.True(rope.Verified);
        Assert.Empty(db.RopeEditSuggestions);
        Assert.True(fixture.Data.GetRopeById(fixture.RopeId).Verified);
        Assert.Equal(1.2, fixture.Data.GetRopeById(fixture.RopeId).RopeCalculatedParameterSet.SheathThickness!.Value, 10);
        Assert.Throws<InvalidOperationException>(() => fixture.Service.ApproveRopeEditSuggestion(fixture.SuggestionId, fixture.RopeId, fixture.Version, TestFixture.EditPassword));
    }

    [SqlServerFact]
    public void Failure_after_SQL_writes_rolls_back_rope_calculations_and_suggestion_deletion()
    {
        using var fixture = new SqlFixture();
        fixture.Failure.Enabled = true;
        Assert.Throws<InjectedFailure>(() => fixture.Service.ApproveRopeEditSuggestion(fixture.SuggestionId, fixture.RopeId, fixture.Version, TestFixture.EditPassword));
        fixture.AssertUnchanged();
    }

    [SqlServerFact]
    public void Mismatched_target_is_rejected_without_changing_either_record()
    {
        using var fixture = new SqlFixture();
        Assert.Throws<InvalidOperationException>(() => fixture.Service.ApproveRopeEditSuggestion(fixture.SuggestionId, fixture.RopeId + 1, fixture.Version, TestFixture.EditPassword));
        fixture.AssertUnchanged();
    }

    [SqlServerFact]
    public void Missing_suggestion_is_reported_without_updating_rope()
    {
        using var fixture = new SqlFixture();
        Assert.Throws<InvalidOperationException>(() => fixture.Service.ApproveRopeEditSuggestion(fixture.SuggestionId + 1, fixture.RopeId, fixture.Version, TestFixture.EditPassword));
        fixture.AssertUnchanged();
    }

    [SqlServerFact]
    public void Invalid_result_is_rejected_and_suggestion_remains_pending()
    {
        using var fixture = new SqlFixture();
        using (var db = fixture.CreateDbContext())
        {
            db.RopeEditSuggestions.Single().Diameter = 0;
            db.SaveChanges();
        }
        Assert.Throws<FluentValidation.ValidationException>(() => fixture.Service.ApproveRopeEditSuggestion(fixture.SuggestionId, fixture.RopeId, fixture.Version, TestFixture.EditPassword));
        fixture.AssertUnchanged();
    }

    [SqlServerFact]
    public void Brand_approval_preserves_the_new_foreign_key()
    {
        using var fixture = new SqlFixture();
        int brandId;
        using (var db = fixture.CreateDbContext())
        {
            var brand = new Brand { Name = "Other brand" };
            db.Brands.Add(brand);
            db.SaveChanges();
            brandId = brand.BrandId;
            var suggestion = db.RopeEditSuggestions.Single();
            suggestion.RopeProperty = RopeProperty.Brand;
            suggestion.BrandId = brandId;
            db.SaveChanges();
        }
        fixture.Service.ApproveRopeEditSuggestion(fixture.SuggestionId, fixture.RopeId, fixture.Version, TestFixture.EditPassword);
        using var check = fixture.CreateDbContext();
        Assert.Equal(brandId, check.Ropes.Single().BrandId);
        Assert.Empty(check.RopeEditSuggestions);
    }

    private sealed class InjectedFailure : Exception { }
    [SqlServerFact]
    public void Category_approval_clears_measurements_that_no_longer_apply()
    {
        using var fixture = new SqlFixture();
        using (var db = fixture.CreateDbContext())
        {
            var rope = db.Ropes.Single();
            rope.Category = Category.Triple;
            rope.ImpactForce55kgOneStrand = 6;
            rope.ImpactForce80kgTwoStrand = 10;
            rope.StaticElongation80kgTwoStrand = 7;
            rope.DynamicElongation55kgOneStrand = 30;
            rope.DynamicElongation80kgTwoStrand = 30;
            rope.DropsBeforeBreak55kgOneStrand = 6;
            rope.DropsBeforeBreak80kgTwoStrand = 12;
            var suggestion = db.RopeEditSuggestions.Single();
            suggestion.RopeProperty = RopeProperty.Category;
            suggestion.Category = Category.Single;
            db.SaveChanges();
        }
        fixture.Service.ApproveRopeEditSuggestion(fixture.SuggestionId, fixture.RopeId, fixture.Version, TestFixture.EditPassword);
        using var check = fixture.CreateDbContext();
        var updated = check.Ropes.Single();
        Assert.Equal(Category.Single, updated.Category);
        Assert.Null(updated.ImpactForce55kgOneStrand);
        Assert.Null(updated.ImpactForce80kgTwoStrand);
        Assert.Null(updated.DropsBeforeBreak55kgOneStrand);
        Assert.Null(updated.DropsBeforeBreak80kgTwoStrand);
        Assert.Equal(6, updated.DropsBeforeBreak80kgOneStrand);
    }

    private sealed class FailAfterSave : SaveChangesInterceptor
    {
        public bool Enabled { get; set; }
        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            if (Enabled) throw new InjectedFailure();
            return result;
        }
    }

    private sealed class SqlFixture : IDbContextFactory<DataContext>, IDisposable
    {
        private readonly string database = "RopeParison_Tests_" + Guid.NewGuid().ToString("N");
        public FailAfterSave Failure { get; } = new();
        public RopeDataService Data { get; }
        public RopeService Service { get; }
        public int RopeId { get; }
        public int SuggestionId { get; }
        public byte[] Version => Data.GetRopeById(RopeId).RowVersion;

        public SqlFixture()
        {
            var brands = new BrandDataService(this);
            Data = new RopeDataService(this, new RopeCalculatedParameterSetDataService(this), brands,
                new RopeEditSuggestionDataService(this, brands));
            Service = new RopeService(Data, new CategoryService(new CategoryDataService(this)), TestFixture.Passwords());
            try
            {
                using var db = CreateDbContext();
                db.Database.Migrate();
                var brand = new Brand { Name = "Test brand" };
                db.Brands.Add(brand);
                db.SaveChanges();
                var dto = TestFixture.Rope();
                dto.RopeId = 0;
                dto.Brand.BrandId = brand.BrandId;
                Service.AddRope(dto);
                var rope = db.Ropes.Single();
                rope.Verified = true;
                RopeId = rope.RopeId;
                var suggestion = new RopeEditSuggestion { RopeId = RopeId, RopeProperty = RopeProperty.Diameter, Diameter = 12 };
                db.RopeEditSuggestions.Add(suggestion);
                db.SaveChanges();
                SuggestionId = suggestion.RopeEditSuggestionId;
            }
            catch { Dispose(); throw; }
        }

        public DataContext CreateDbContext() => new(new DbContextOptionsBuilder<DataContext>()
            .UseSqlServer($@"Server=localhost\SQLEXPRESS;Database={database};Integrated Security=True;Encrypt=True;TrustServerCertificate=True;")
            .AddInterceptors(Failure).Options);

        public void AssertUnchanged()
        {
            using var db = CreateDbContext();
            var rope = db.Ropes.Include(r => r.RopeCalculatedParameterSet).Single();
            Assert.Equal(10, rope.Diameter);
            Assert.Equal(25 * Math.PI, rope.RopeCalculatedParameterSet.Area!.Value, 10);
            Assert.Equal(1, rope.RopeCalculatedParameterSet.SheathThickness!.Value, 10);
            Assert.Single(db.RopeEditSuggestions);
        }

        public void Dispose()
        {
            // Only the generated test database can ever be dropped. No caller-supplied connection string.
            if (!System.Text.RegularExpressions.Regex.IsMatch(database, "^RopeParison_Tests_[0-9a-f]{32}$"))
                throw new InvalidOperationException("Refusing to remove a non-test database.");
            using var db = CreateDbContext();
            db.Database.EnsureDeleted();
        }
    }
}
