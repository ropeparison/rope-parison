using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RopeParison.Data.Model;
using RopeParison.Protocol;
using Xunit;

namespace RopeParison.Tests;

public sealed partial class SqlApprovalTests
{
    [SqlServerFact]
    public void Missing_ID_reads_and_commands_report_not_found()
    {
        using var f = new SqlFixture();
        int missing = f.RopeId + 100;
        Assert.Throws<RopeNotFoundException>(() => f.Service.GetRopeById(missing));
        Assert.Throws<RopeNotFoundException>(() => f.Service.GetRopeById_IncEditSuggestions(missing));
        Assert.Throws<RopeNotFoundException>(() => f.Service.VerifyRope(missing, f.Version, TestFixture.VerifyPassword));
        Assert.Throws<RopeNotFoundException>(() => f.Service.RemoveRope(missing, f.Version, TestFixture.DeletePassword));
        var update = f.Data.GetRopeById(f.RopeId);
        update.RopeId = missing;
        update.Name = "Missing rope";
        Assert.Throws<RopeNotFoundException>(() => f.Service.UpdateRope(update, TestFixture.EditPassword));
    }

    [SqlServerFact]
    public void Database_rejects_duplicate_insert_even_after_both_callers_check_uniqueness()
    {
        using var f = new SqlFixture();
        var first = f.Data.GetRopeById(f.RopeId);
        var second = f.Data.GetRopeById(f.RopeId);
        first.Name = "New name";
        second.Name = "  NEW NAME  ";
        Assert.True(f.Data.IsRopeNameUnique(0, first.Name));
        Assert.True(f.Data.IsRopeNameUnique(0, second.Name));
        f.Data.AddRope(first); // Deliberately bypass preflight validation to exercise the SQL constraint.
        Assert.Throws<DuplicateRopeNameException>(() => f.Data.AddRope(second));
        using var db = f.CreateDbContext();
        Assert.Equal(2, db.Ropes.Count());
        Assert.Equal(2, db.RopeCalculatedParameterSets.Count());
    }

    [SqlServerFact]
    public void Stale_update_cannot_overwrite_another_edit_or_its_calculations()
    {
        using var f = new SqlFixture();
        var first = f.Data.GetRopeById(f.RopeId);
        var stale = f.Data.GetRopeById(f.RopeId);
        first.MassPerUnitLength = 65;
        f.Service.UpdateRope(first, TestFixture.EditPassword);
        Assert.False(first.RowVersion.SequenceEqual(stale.RowVersion));
        stale.Diameter = 11;
        Assert.Throws<RopeConflictException>(() => f.Service.UpdateRope(stale, TestFixture.EditPassword));
        var saved = f.Data.GetRopeById(f.RopeId);
        Assert.Equal(65, saved.MassPerUnitLength);
        Assert.Equal(10, saved.Diameter);
        Assert.Equal(65 / (25 * Math.PI * 1000), saved.RopeCalculatedParameterSet.Density!.Value, 10);
        saved.Diameter = 11;
        f.Service.UpdateRope(saved, TestFixture.EditPassword); // A reviewed fresh snapshot can be saved.
        Assert.Equal(11, f.Data.GetRopeById(f.RopeId).Diameter);
    }

    [SqlServerFact]
    public void Stale_moderation_commands_and_missing_version_are_rejected()
    {
        using var f = new SqlFixture();
        var stale = f.Data.GetRopeById(f.RopeId);
        var fresh = f.Data.GetRopeById(f.RopeId);
        fresh.MassPerUnitLength = 65;
        f.Service.UpdateRope(fresh, TestFixture.EditPassword);
        Assert.Throws<RopeConflictException>(() => f.Service.VerifyRope(f.RopeId, stale.RowVersion, TestFixture.VerifyPassword));
        Assert.Throws<RopeConflictException>(() => f.Service.RemoveRope(f.RopeId, stale.RowVersion, TestFixture.DeletePassword));
        Assert.Throws<RopeConflictException>(() => f.Service.ApproveRopeEditSuggestion(f.SuggestionId, f.RopeId, stale.RowVersion, TestFixture.EditPassword));
        fresh.RowVersion = Array.Empty<byte>();
        Assert.Throws<RopeConflictException>(() => f.Service.UpdateRope(fresh, TestFixture.EditPassword));
        using var db = f.CreateDbContext();
        Assert.Single(db.RopeEditSuggestions);
        Assert.Equal(65, db.Ropes.Single().MassPerUnitLength);
    }

    [SqlServerFact]
    public void Deleting_a_rope_also_removes_its_related_rows_with_a_fresh_version()
    {
        using var f = new SqlFixture();
        f.Service.RemoveRope(f.RopeId, f.Version, TestFixture.DeletePassword);
        using var db = f.CreateDbContext();
        Assert.Empty(db.Ropes);
        Assert.Empty(db.RopeCalculatedParameterSets);
        Assert.Empty(db.RopeEditSuggestions);
    }

    [SqlServerFact]
    public void EF_checks_version_in_SQL_even_if_a_write_happens_after_loading()
    {
        using var f = new SqlFixture();
        using var first = f.CreateDbContext();
        using var second = f.CreateDbContext();
        var stale = first.Ropes.Single();
        var current = second.Ropes.Single();
        current.MassPerUnitLength = 66;
        second.SaveChanges();
        stale.Diameter = 11;
        Assert.Throws<DbUpdateConcurrencyException>(() => first.SaveChanges());
        using var check = f.CreateDbContext();
        Assert.Equal(10, check.Ropes.Single().Diameter);
        Assert.Equal(66, check.Ropes.Single().MassPerUnitLength);
    }

    [SqlServerFact]
    public void Duplicate_rename_is_rejected_by_SQL_and_rolls_back()
    {
        using var f = new SqlFixture();
        var other = f.Data.GetRopeById(f.RopeId);
        other.Name = "Other rope";
        f.Data.AddRope(other);
        var update = f.Data.GetRopes_All().Single(r => r.Name == "Other rope");
        update.Name = " TEST ROPE ";
        Assert.Throws<DuplicateRopeNameException>(() => f.Data.UpdateRope(update));
        Assert.Equal("Other rope", f.Data.GetRopeById(update.RopeId).Name);
    }

    [SqlServerFact]
    public void Migration_refuses_legacy_duplicates_without_removing_data()
    {
        using var f = new SqlFixture();
        using var db = f.CreateDbContext();
        db.GetService<IMigrator>().Migrate("20240206094359_Mig10");
        db.Database.ExecuteSqlRaw("""
            INSERT INTO [Ropes] ([Name], [BrandId], [Category], [Diameter], [MassPerUnitLength], [Verified])
            SELECT UPPER([Name]), [BrandId], [Category], [Diameter], [MassPerUnitLength], [Verified] FROM [Ropes];
            """);
        var error = Assert.Throws<Microsoft.Data.SqlClient.SqlException>(() => db.Database.Migrate());
        Assert.Equal(51002, error.Number);
        Assert.Equal(2, db.Database.SqlQueryRaw<int>("SELECT COUNT(*) AS [Value] FROM [Ropes]").Single());
        Assert.Contains("20260918090336_RopeNameUniquenessAndConcurrency", db.Database.GetPendingMigrations());
    }
}
