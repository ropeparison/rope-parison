namespace RopeParison.Protocol;

public sealed class RopeNotFoundException : InvalidOperationException
{
    public RopeNotFoundException(int id) : base($"Rope {id} was not found. It may have been deleted.") { }
}

public sealed class RopeConflictException : InvalidOperationException
{
    public RopeConflictException() : base("This rope has changed since you opened it. Refresh and review the latest values before saving again.") { }
}

public sealed class DuplicateRopeNameException : InvalidOperationException
{
    public DuplicateRopeNameException() : base("A rope with this name already exists. Choose a different name.") { }
}
