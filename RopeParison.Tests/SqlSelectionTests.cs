using RopeParison.Protocol;
using Xunit;

namespace RopeParison.Tests;

public sealed partial class SqlApprovalTests
{
    [SqlServerFact]
    public void Omitted_filters_are_unrestricted_but_explicit_empty_selections_match_nothing()
    {
        using var f = new SqlFixture();
        Assert.Single(f.Service.GetRopes_FilteredA(new()));
        Assert.Empty(f.Service.GetRopes_FilteredA(new() { Categorys = new() }));
        Assert.Empty(f.Service.GetRopes_FilteredA(new() { RopeIds = new() }));
        Assert.Single(f.Service.GetRopes_FilteredA(new() { BrandIds = new() }));
        Assert.Single(f.Service.GetRopes_FilteredA(new() { Categorys = new() { Category.Single } }));
        Assert.Single(f.Service.GetRopes_FilteredA(new() { RopeIds = new() { f.RopeId } }));
    }

    [SqlServerFact]
    public void Explicit_rope_selection_cannot_bypass_verification_filter()
    {
        using var f = new SqlFixture();
        using (var db = f.CreateDbContext())
        {
            db.Ropes.Single().Verified = false;
            db.SaveChanges();
        }
        var filter = new RopeFilterADto { RopeIds = new() { f.RopeId } };
        Assert.Empty(f.Service.GetRopes_FilteredA(filter));
        filter.IncludeUnverified = true;
        Assert.Single(f.Service.GetRopes_FilteredA(filter));
        filter.RopeIds.Clear();
        Assert.Empty(f.Service.GetRopes_FilteredA(filter));
    }
}
