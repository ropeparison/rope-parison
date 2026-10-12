using Xunit;

namespace RopeParison.Tests;

public class ApprovalTests
{
    [Fact]
    public void Approval_recalculates_from_the_implemented_edit()
    {
        var rope = TestFixture.Rope();
        var data = new RecordingRopeDataService { RopeAfterApproval = rope };
        data.OnImplement = _ => rope.Diameter = 12;
        TestFixture.Service(data).ApproveRopeEditSuggestion(83, rope.RopeId, TestFixture.Version, TestFixture.EditPassword);
        Assert.Equal(new[] { "Implement:83" }, data.Calls);
        Assert.NotNull(data.Updated);
        Assert.Equal(36 * Math.PI, data.Updated.RopeCalculatedParameterSet.Area!.Value, 10);
    }

    [Fact]
    public void Failed_implementation_propagates_and_does_not_recalculate()
    {
        var failure = new InvalidOperationException("Simulated persistence failure");
        var data = new RecordingRopeDataService { OnImplement = _ => throw failure };
        var actual = Assert.Throws<InvalidOperationException>(() =>
            TestFixture.Service(data).ApproveRopeEditSuggestion(83, 10, TestFixture.Version, TestFixture.EditPassword));
        Assert.Same(failure, actual);
        Assert.Equal(new[] { "Implement:83" }, data.Calls);
        Assert.Null(data.Updated);
    }

    [Fact, Trait("Finding", "F05")]
    public void Approval_does_not_silently_succeed_when_target_rope_is_missing()
    {
        var data = new RecordingRopeDataService { RopeAfterApproval = null };
        var error = Record.Exception(() => TestFixture.Service(data).ApproveRopeEditSuggestion(83, 999, TestFixture.Version, TestFixture.EditPassword));
        Assert.NotNull(error);
        Assert.Null(data.Updated);
    }
}
