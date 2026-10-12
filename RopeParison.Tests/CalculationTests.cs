using Xunit;

namespace RopeParison.Tests;

public class CalculationTests
{
    [Fact]
    public void Calculates_geometry_and_density_for_known_dimensions()
    {
        var rope = TestFixture.Rope();
        TestFixture.Service(new()).UpdateRopeCalculatedParameterSet(rope);
        var actual = rope.RopeCalculatedParameterSet;
        Assert.Equal(25 * Math.PI, actual.Area!.Value, 10);
        Assert.Equal(9 * Math.PI, actual.SheathArea!.Value, 10);
        Assert.Equal(16 * Math.PI, actual.CoreArea!.Value, 10);
        Assert.Equal(8, actual.CoreDiameter!.Value, 10);
        Assert.Equal(60 / (25 * Math.PI * 1000), actual.Density!.Value, 10);
    }

    [Fact]
    public void Removing_optional_sheath_percentage_clears_stale_derived_values()
    {
        var rope = TestFixture.Rope();
        var service = TestFixture.Service(new());
        service.UpdateRopeCalculatedParameterSet(rope);
        Assert.NotNull(rope.RopeCalculatedParameterSet.CoreDiameter);
        rope.SheathPercentage = null;
        service.UpdateRopeCalculatedParameterSet(rope);
        var actual = rope.RopeCalculatedParameterSet;
        Assert.Null(actual.SheathArea);
        Assert.Null(actual.CoreArea);
        Assert.Null(actual.CoreDiameter);
        Assert.Null(actual.SheathThickness);
        Assert.Equal(25 * Math.PI, actual.Area!.Value, 10);
    }

    [Fact, Trait("Finding", "F08")]
    public void Sheath_thickness_is_radial_under_the_existing_area_fraction_assumption()
    {
        var rope = TestFixture.Rope();
        TestFixture.Service(new()).UpdateRopeCalculatedParameterSet(rope);
        // A 10 mm outer diameter and 8 mm core leave 1 mm on each side.
        Assert.Equal(1, rope.RopeCalculatedParameterSet.SheathThickness!.Value, 10);
    }
}
