using Microsoft.Extensions.Configuration;
using RopeParison.Logic.Services;
using RopeParison.Security.Services;
using Xunit;

namespace RopeParison.Tests;

public class AuthorizationTests
{
    private static string RequiredPassword(string operation) => operation switch
    {
        "Verify" => TestFixture.VerifyPassword,
        "Remove" => TestFixture.DeletePassword,
        _ => TestFixture.EditPassword
    };

    public static IEnumerable<object?[]> DeniedCases()
    {
        foreach (var operation in new[] { "Verify", "Remove", "Update", "Approve", "Reject" })
        foreach (var password in new[] { null, "", " ", "wrong", TestFixture.EditPassword, TestFixture.VerifyPassword, TestFixture.DeletePassword })
            if (password != RequiredPassword(operation)) yield return new object?[] { operation, password };
    }

    private static void Invoke(IRopeService service, string operation, string? password)
    {
        switch (operation)
        {
            case "Verify": service.VerifyRope(10, TestFixture.Version, password); break;
            case "Remove": service.RemoveRope(10, TestFixture.Version, password); break;
            case "Update": service.UpdateRope(TestFixture.Rope(), password); break;
            case "Approve": service.ApproveRopeEditSuggestion(83, 10, TestFixture.Version, password); break;
            case "Reject": service.DeleteRopeEditSuggestion(83, password); break;
            default: throw new ArgumentException(nameof(operation));
        }
    }

    [Theory, MemberData(nameof(DeniedCases))]
    public void Protected_commands_reject_missing_wrong_and_other_operation_passwords_before_persistence(string operation, string? password)
    {
        var data = new RecordingRopeDataService { RopeAfterApproval = TestFixture.Rope() };
        Assert.Throws<UnauthorizedAccessException>(() => Invoke(TestFixture.Service(data), operation, password));
        Assert.Empty(data.Calls);
        Assert.Null(data.Added);
        Assert.Null(data.Updated);
        Assert.Empty(data.Suggestions);
    }

    [Theory]
    [InlineData("Verify", "Verify:10")]
    [InlineData("Remove", "Remove:10")]
    [InlineData("Update", "Update:10")]
    [InlineData("Approve", "Implement:83")]
    [InlineData("Reject", "DeleteSuggestion:83")]
    public void Correct_operation_password_allows_the_command(string operation, string expectedCall)
    {
        var data = new RecordingRopeDataService { RopeAfterApproval = TestFixture.Rope() };
        Invoke(TestFixture.Service(data), operation, RequiredPassword(operation));
        Assert.Equal(expectedCall, Assert.Single(data.Calls));
    }

    [Fact]
    public void Unconfigured_passwords_deny_commands_but_do_not_block_public_submissions()
    {
        var data = new RecordingRopeDataService();
        var passwords = new PasswordService(new PasswordSecurityServiceSettingsBased(new ConfigurationBuilder().Build()));
        var service = new RopeService(data, new CategoryService(new RopeParison.Data.Services.CategoryDataService(null!)), passwords);
        foreach (var operation in new[] { "Verify", "Remove", "Update", "Approve", "Reject" })
            Assert.Throws<UnauthorizedAccessException>(() => Invoke(service, operation, "anything"));
        Assert.Empty(data.Calls);
        service.AddRope(TestFixture.Rope());
        Assert.NotNull(data.Added);
        Assert.False(data.Added.Verified);
        var original = TestFixture.Rope();
        var edit = TestFixture.Rope();
        edit.Diameter = 11;
        service.SubmitRopeEditSuggestion(original, edit);
        Assert.Single(data.Suggestions);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData(" ", " ")]
    [InlineData("anything", null)]
    [InlineData(null, "configured")]
    [InlineData("WRONG", "configured")]
    public void Password_comparison_fails_closed(string? supplied, string? configured)
        => Assert.False(PasswordSecurityService.CheckMatch(supplied, configured));
}
