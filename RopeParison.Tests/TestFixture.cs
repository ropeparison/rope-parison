using Microsoft.Extensions.Configuration;
using RopeParison.Data.Services;
using RopeParison.Logic.Services;
using RopeParison.Protocol;

namespace RopeParison.Tests;

internal static class TestFixture
{
    public static RopeService Service(RecordingRopeDataService data) =>
        new(data, new CategoryService(new CategoryDataService(null!)), Passwords());

    public const string EditPassword = "test-edit-only";
    public static byte[] Version => new byte[8];
    public const string VerifyPassword = "test-verify-only";
    public const string DeletePassword = "test-delete-only";

    public static IPasswordService Passwords()
    {
        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Security:Password:ApproveEdit"] = EditPassword,
                ["Security:Password:Verify"] = VerifyPassword,
                ["Security:Password:Delete"] = DeletePassword
            }).Build();
        return new PasswordService(new RopeParison.Security.Services.PasswordSecurityServiceSettingsBased(configuration));
    }

    public static RopeDto Rope() => new()
    {
        RopeId = 10, RowVersion = Version, Name = "Test rope", Brand = new() { BrandId = 1, Name = "Test brand" },
        Category = Category.Single.ToDto(), Diameter = 10, MassPerUnitLength = 60,
        SheathPercentage = 36, ImpactForce80kgOneStrand = 8,
        StaticElongation80kgOneStrand = 8, DynamicElongation80kgOneStrand = 30,
        DropsBeforeBreak80kgOneStrand = 6
    };
}

// Only the persistence boundary is substituted. Calculations, category rules and
// FluentValidation execute production code. Unexpected reads fail immediately.
internal sealed class RecordingRopeDataService : IRopeDataService
{
    public bool NameIsUnique { get; set; } = true;
    public RopeDto? RopeAfterApproval { get; set; }
    public Action<int>? OnImplement { get; set; }
    public RopeDto? Added { get; private set; }
    public RopeDto? Updated { get; private set; }
    public List<(int RopeId, RopeEditSuggestionDto Suggestion)> Suggestions { get; } = new();
    public List<string> Calls { get; } = new();
    public bool IsRopeNameUnique(int ropeId, string name) => NameIsUnique;
    public void AddRope(RopeDto rope) => Added = rope;
    public void UpdateRope(RopeDto rope) { Calls.Add($"Update:{rope.RopeId}"); Updated = rope; }
    public int SubmissionWrites { get; private set; }
    public void AddRopeEditSuggestions(int ropeId, byte[] expectedVersion, IReadOnlyList<RopeEditSuggestionDto> suggestions)
    {
        SubmissionWrites++;
        Suggestions.AddRange(suggestions.Select(s => (ropeId, s)));
    }
    public void ImplementRopeEditSuggestion(int id, int expectedRopeId, byte[] expectedVersion, Action<RopeDto> prepareForWrite)
    {
        Calls.Add($"Implement:{id}");
        OnImplement?.Invoke(id);
        if (RopeAfterApproval == null || RopeAfterApproval.RopeId != expectedRopeId)
            throw new InvalidOperationException("Missing or mismatched rope.");
        prepareForWrite(RopeAfterApproval);
        Updated = RopeAfterApproval;
    }
    public RopeDto GetRopeById(int id) { Calls.Add($"Get:{id}"); return RopeAfterApproval!; }
    public List<RopeDto> GetRopes_All() => throw new NotSupportedException();
    public List<RopeDto> GetRopes_All_IncEditSuggestions() => throw new NotSupportedException();
    public List<RopeDto> GetRopes_Verified(bool verified) => throw new NotSupportedException();
    public List<RopeDto> GetRopes_WithEditSuggestions() => throw new NotSupportedException();
    public List<RopeDto> GetRopes_FilteredA(RopeFilterADto filter) => throw new NotSupportedException();
    public List<RopeDto> GetRopes_FilteredB(RopeFilterDto filter) => throw new NotSupportedException();
    public RopeDto GetRopeById_IncEditSuggestions(int id) => throw new NotSupportedException();
    public void DeleteRopeEditSuggestion(int id) => Calls.Add($"DeleteSuggestion:{id}");
    public void RemoveRope(int id, byte[] expectedVersion) => Calls.Add($"Remove:{id}");
    public void VerifyRope(int id, byte[] expectedVersion) => Calls.Add($"Verify:{id}");
}
