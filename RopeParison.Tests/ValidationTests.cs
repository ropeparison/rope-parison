using RopeParison.Logic.Protocol.Validators;
using Xunit;

namespace RopeParison.Tests;

public class ValidationTests
{
    [Fact]
    public void Valid_single_rope_with_optional_values_absent_is_accepted()
    {
        var rope = TestFixture.Rope();
        rope.SheathPercentage = null;
        rope.SheathSlippage = null;
        Assert.True(new RopeDtoValidator(TestFixture.Service(new())).Validate(rope).IsValid);
    }

    [Theory]
    [InlineData("Name", "")]
    [InlineData("Diameter", 0d)]
    [InlineData("Diameter", -1d)]
    [InlineData("Diameter", double.NaN)]
    [InlineData("Diameter", double.PositiveInfinity)]
    [InlineData("MassPerUnitLength", double.PositiveInfinity)]
    [InlineData("ImpactForce80kgOneStrand", double.NaN)]
    [InlineData("MassPerUnitLength", 0d)]
    [InlineData("ImpactForce80kgOneStrand", null)]
    [InlineData("StaticElongation80kgOneStrand", -1d)]
    [InlineData("DynamicElongation80kgOneStrand", 0d)]
    public void Rejects_invalid_required_field(string field, object? value)
    {
        var rope = TestFixture.Rope();
        typeof(RopeParison.Protocol.RopeDto).GetProperty(field)!.SetValue(rope, value);
        var result = new RopeDtoValidator(TestFixture.Service(new())).Validate(rope);
        Assert.Contains(result.Errors, error => error.PropertyName == field);
    }

    [Fact]
    public void Rejects_duplicate_name()
    {
        var validator = new RopeDtoValidator(TestFixture.Service(new() { NameIsUnique = false }));
        Assert.Contains(validator.Validate(TestFixture.Rope()).Errors, error => error.PropertyName == "Name");
    }

    [Theory, Trait("Finding", "F04")]
    [InlineData("SheathPercentage", -1d)]
    [InlineData("SheathPercentage", 150d)]
    [InlineData("SheathPercentage", double.NaN)]
    [InlineData("SheathPercentage", 0d)]
    [InlineData("SheathPercentage", 100d)]
    [InlineData("SheathSlippage", -1d)]
    [InlineData("DropsBeforeBreak80kgOneStrand", -5)]
    [InlineData("DropsBeforeBreak80kgOneStrand", null)]
    public void Rejects_impossible_measurements_or_missing_required_drop_count(string field, object? value)
    {
        var rope = TestFixture.Rope();
        typeof(RopeParison.Protocol.RopeDto).GetProperty(field)!.SetValue(rope, value);
        var result = new RopeDtoValidator(TestFixture.Service(new())).Validate(rope);
        Assert.Contains(result.Errors, error => error.PropertyName == field);
    }

    [Fact]
    public void Missing_brand_and_category_produce_validation_errors_without_throwing()
    {
        var rope = TestFixture.Rope();
        rope.Brand = null!;
        rope.Category = null!;
        var errors = new RopeDtoValidator(TestFixture.Service(new())).Validate(rope).Errors;
        Assert.Contains(errors, e => e.PropertyName == "Brand");
        Assert.Contains(errors, e => e.PropertyName == "Category");
    }

    [Theory]
    [InlineData(RopeParison.Protocol.Category.Half, "DropsBeforeBreak55kgOneStrand")]
    [InlineData(RopeParison.Protocol.Category.Twin, "DropsBeforeBreak80kgTwoStrand")]
    [InlineData(RopeParison.Protocol.Category.HalfTwin, "DropsBeforeBreak55kgOneStrand")]
    [InlineData(RopeParison.Protocol.Category.Triple, "DropsBeforeBreak80kgTwoStrand")]
    public void Drop_count_rules_follow_category(RopeParison.Protocol.Category category, string requiredField)
    {
        var rope = TestFixture.Rope();
        rope.Category.Category = category;
        var errors = new RopeDtoValidator(TestFixture.Service(new())).Validate(rope).Errors;
        Assert.Contains(errors, e => e.PropertyName == requiredField);
    }
}
