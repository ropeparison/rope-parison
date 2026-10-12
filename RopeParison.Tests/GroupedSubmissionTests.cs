using Microsoft.EntityFrameworkCore;
using RopeParison.Protocol;
using Xunit;

namespace RopeParison.Tests;

public sealed class GroupedSubmissionTests
{
    [Fact]
    public void Multiple_changes_use_one_persistence_call()
    {
        var data = new RecordingRopeDataService();
        var original = TestFixture.Rope();
        var edited = TestFixture.Rope();
        edited.Diameter = 11;
        edited.MassPerUnitLength = 65;
        TestFixture.Service(data).SubmitRopeEditSuggestion(original, edited);
        Assert.Equal(1, data.SubmissionWrites);
        Assert.Equal(2, data.Suggestions.Count);
    }

    [Fact]
    public void Unchanged_edit_does_not_create_a_submission()
    {
        var data = new RecordingRopeDataService();
        TestFixture.Service(data).SubmitRopeEditSuggestion(TestFixture.Rope(), TestFixture.Rope());
        Assert.Equal(0, data.SubmissionWrites);
    }
}

public sealed partial class SqlApprovalTests
{
    private static int SubmitHalfCategory(SqlFixture fixture)
    {
        var original = fixture.Data.GetRopeById(fixture.RopeId);
        var edited = fixture.Data.GetRopeById(fixture.RopeId);
        edited.Category = Category.Half.ToDto();
        edited.ImpactForce55kgOneStrand = 6;
        edited.DynamicElongation55kgOneStrand = 30;
        edited.DropsBeforeBreak55kgOneStrand = 6;
        fixture.Service.SubmitRopeEditSuggestion(original, edited);
        using var db = fixture.CreateDbContext();
        var group = db.RopeEditSuggestions.Where(s => s.SubmissionId != null).ToList();
        Assert.True(group.Count > 1);
        Assert.Single(group.Select(s => s.SubmissionId).Distinct());
        return group.First(s => s.RopeProperty == RopeProperty.Category).RopeEditSuggestionId;
    }

    [SqlServerFact]
    public void Category_and_required_measurements_are_approved_together()
    {
        using var f = new SqlFixture();
        var id = SubmitHalfCategory(f);
        var dto = f.Data.GetRopeById_IncEditSuggestions(f.RopeId);
        Assert.Contains(dto.RopeEditSuggestions, s => s.SubmissionId.HasValue);
        f.Service.ApproveRopeEditSuggestion(id, f.RopeId, f.Version, TestFixture.EditPassword);
        using var db = f.CreateDbContext();
        var rope = db.Ropes.Single();
        Assert.Equal(Category.Half, rope.Category);
        Assert.Equal(6, rope.ImpactForce55kgOneStrand);
        Assert.Equal(30, rope.DynamicElongation55kgOneStrand);
        Assert.Equal(6, rope.DropsBeforeBreak55kgOneStrand);
        Assert.Null(rope.ImpactForce80kgOneStrand);
        Assert.Null(rope.DropsBeforeBreak80kgOneStrand);
        Assert.Equal(f.SuggestionId, Assert.Single(db.RopeEditSuggestions).RopeEditSuggestionId);
    }

    [SqlServerFact]
    public void Failure_during_submission_rolls_back_every_field()
    {
        using var f = new SqlFixture();
        f.Failure.Enabled = true;
        Assert.Throws<InjectedFailure>(() => SubmitHalfCategory(f));
        f.AssertUnchanged();
    }

    [SqlServerFact]
    public void Failure_during_group_approval_preserves_entire_submission()
    {
        using var f = new SqlFixture();
        var id = SubmitHalfCategory(f);
        using var before = f.CreateDbContext();
        var count = before.RopeEditSuggestions.Count();
        f.Failure.Enabled = true;
        Assert.Throws<InjectedFailure>(() => f.Service.ApproveRopeEditSuggestion(id, f.RopeId, f.Version, TestFixture.EditPassword));
        using var db = f.CreateDbContext();
        Assert.Equal(count, db.RopeEditSuggestions.Count());
        Assert.Equal(Category.Single, db.Ropes.Single().Category);
    }

    [SqlServerFact]
    public void Rejecting_group_preserves_other_submissions_and_legacy_rows()
    {
        using var f = new SqlFixture();
        var id = SubmitHalfCategory(f);
        var original = f.Data.GetRopeById(f.RopeId);
        var edit = f.Data.GetRopeById(f.RopeId);
        edit.Diameter = 11;
        f.Service.SubmitRopeEditSuggestion(original, edit);
        f.Service.DeleteRopeEditSuggestion(id, TestFixture.EditPassword);
        using var db = f.CreateDbContext();
        Assert.Equal(2, db.RopeEditSuggestions.Count());
        Assert.Single(db.RopeEditSuggestions.Where(s => s.SubmissionId != null));
        Assert.Equal(Category.Single, db.Ropes.Single().Category);
        Assert.Equal(10, db.Ropes.Single().Diameter);
    }

    [SqlServerFact]
    public void Invalid_group_cannot_be_partially_approved()
    {
        using var f = new SqlFixture();
        var id = SubmitHalfCategory(f);
        using var db = f.CreateDbContext();
        var invalid = db.RopeEditSuggestions.Single(s => s.RopeProperty == RopeProperty.DropsBeforeBreak55kgOneStrand);
        invalid.DropsBeforeBreak55kgOneStrand = 0;
        db.SaveChanges();
        var count = db.RopeEditSuggestions.Count();
        Assert.Throws<FluentValidation.ValidationException>(() => f.Service.ApproveRopeEditSuggestion(id, f.RopeId, f.Version, TestFixture.EditPassword));
        Assert.Equal(count, db.RopeEditSuggestions.Count());
        Assert.Equal(Category.Single, db.Ropes.Single().Category);
    }

    [SqlServerFact]
    public void Stale_submission_is_rejected_without_saving_any_fields()
    {
        using var f = new SqlFixture();
        var original = f.Data.GetRopeById(f.RopeId);
        var edit = f.Data.GetRopeById(f.RopeId);
        edit.Diameter = 11;
        using (var db = f.CreateDbContext())
        {
            db.Ropes.Single().MassPerUnitLength = 61;
            db.SaveChanges();
        }
        Assert.Throws<RopeConflictException>(() => f.Service.SubmitRopeEditSuggestion(original, edit));
        using var check = f.CreateDbContext();
        Assert.Single(check.RopeEditSuggestions);
    }
}
