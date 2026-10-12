using RopeParison.Protocol;
using Xunit;

namespace RopeParison.Tests;

public class CommandTests
{
    [Fact]
    public void Add_marks_unverified_normalizes_category_and_calculates_before_persisting()
    {
        var data = new RecordingRopeDataService();
        var rope = TestFixture.Rope();
        rope.Verified = true;
        rope.ImpactForce55kgOneStrand = 6;
        TestFixture.Service(data).AddRope(rope);
        Assert.NotNull(data.Added);
        Assert.False(data.Added.Verified);
        Assert.Null(data.Added.ImpactForce55kgOneStrand);
        Assert.Equal(25 * Math.PI, data.Added.RopeCalculatedParameterSet.Area!.Value, 10);
    }

    [Fact]
    public void Diameter_edit_preserves_target_and_proposed_value()
    {
        var data = new RecordingRopeDataService();
        var original = TestFixture.Rope();
        var edited = TestFixture.Rope();
        edited.Category = original.Category;
        edited.Diameter = 11;
        TestFixture.Service(data).SubmitRopeEditSuggestion(original, edited);
        var entry = Assert.Single(data.Suggestions);
        Assert.Equal(original.RopeId, entry.RopeId);
        Assert.Equal(RopeProperty.Diameter, entry.Suggestion.RopeProperty);
        Assert.Equal(11d, entry.Suggestion.Diameter);
    }

    [Fact, Trait("Finding", "F07")]
    public void Equivalent_separately_loaded_categories_do_not_create_suggestions()
    {
        var data = new RecordingRopeDataService();
        TestFixture.Service(data).SubmitRopeEditSuggestion(TestFixture.Rope(), TestFixture.Rope());
        Assert.Empty(data.Suggestions);
    }

    [Theory, Trait("Finding", "F04")]
    [InlineData(false)]
    [InlineData(true)]
    public void Invalid_diameter_never_reaches_persistence(bool update)
    {
        var data = new RecordingRopeDataService();
        var rope = TestFixture.Rope();
        rope.Diameter = 0;
        var service = TestFixture.Service(data);
        Assert.Throws<FluentValidation.ValidationException>(() => { if (update) service.UpdateRope(rope, TestFixture.EditPassword); else service.AddRope(rope); });
        Assert.Null(data.Added);
        Assert.Null(data.Updated);
    }

    [Fact, Trait("Finding", "F06")]
    public void Updating_to_single_clears_half_only_measurements()
    {
        var data = new RecordingRopeDataService();
        var rope = TestFixture.Rope();
        rope.ImpactForce55kgOneStrand = 6;
        TestFixture.Service(data).UpdateRope(rope, TestFixture.EditPassword);
        Assert.NotNull(data.Updated);
        Assert.Null(data.Updated.ImpactForce55kgOneStrand);
    }

    [Fact]
    public void Invalid_edit_is_rejected_before_any_suggestion_is_persisted()
    {
        var data = new RecordingRopeDataService();
        var edit = TestFixture.Rope();
        edit.Name = "Changed name";
        edit.SheathPercentage = 150;
        Assert.Throws<FluentValidation.ValidationException>(() =>
            TestFixture.Service(data).SubmitRopeEditSuggestion(TestFixture.Rope(), edit));
        Assert.Empty(data.Suggestions);
    }

    [Fact]
    public void Missing_calculation_object_is_created_before_update()
    {
        var data = new RecordingRopeDataService();
        var rope = TestFixture.Rope();
        rope.RopeCalculatedParameterSet = null!;
        TestFixture.Service(data).UpdateRope(rope, TestFixture.EditPassword);
        Assert.Equal(1d, data.Updated!.RopeCalculatedParameterSet.SheathThickness);
    }
}
