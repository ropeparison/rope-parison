using RopeParison.Protocol;
using Xunit;

namespace RopeParison.Tests;

public sealed partial class SqlApprovalTests
{
    [SqlServerFact]
    public void Both_filters_exclude_unverified_by_default_and_allow_explicit_inclusion()
    {
        using var f = new SqlFixture();
        var pending = f.Data.GetRopeById(f.RopeId);
        pending.RopeId = 0;
        pending.Name = "Pending rope";
        f.Service.AddRope(pending);
        Assert.Equal(f.RopeId, Assert.Single(f.Service.GetRopes_FilteredA(new())).RopeId);
        Assert.Equal(f.RopeId, Assert.Single(f.Service.GetRopes_FilteredB(new())).RopeId);
        Assert.Equal(2, f.Service.GetRopes_FilteredA(new() { IncludeUnverified = true }).Count);
        Assert.Equal(2, f.Service.GetRopes_FilteredB(new() { IncludeUnverified = true }).Count);
    }

    [SqlServerFact]
    public void Legacy_category_filters_execute_on_SQL_for_every_category()
    {
        using var f = new SqlFixture();
        foreach (var category in Enum.GetValues<Category>())
        {
            using (var db = f.CreateDbContext())
            {
                db.Ropes.Single().Category = category;
                db.SaveChanges();
            }
            var result = f.Service.GetRopes_FilteredB(new() { CategoryName = category.ToString(), DiameterMin = 10, DiameterMax = 10 });
            Assert.Equal(f.RopeId, Assert.Single(result).RopeId);
            Assert.Single(f.Service.GetRopes_FilteredA(new() { Categorys = new() { category }, DiameterMin = 10, DiameterMax = 10 }));
            Assert.Empty(f.Service.GetRopes_FilteredB(new() { CategoryName = category.ToString(), DiameterMin = 11 }));
        }
    }

    [SqlServerFact]
    public void Category_text_is_normalized_and_unknown_values_never_broaden_results()
    {
        using var f = new SqlFixture();
        Assert.Single(f.Service.GetRopes_FilteredB(new() { CategoryName = " single " }));
        Assert.Single(f.Service.GetRopes_FilteredB(new() { CategoryName = " " }));
        Assert.Empty(f.Service.GetRopes_FilteredB(new() { CategoryName = "Half" }));
        Assert.Empty(f.Service.GetRopes_FilteredB(new() { CategoryName = "invalid" }));
        Assert.Empty(f.Service.GetRopes_FilteredB(new() { CategoryName = "999" }));
    }
}
