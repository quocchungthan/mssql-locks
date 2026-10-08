namespace MiniProject.ComplexLogicInMiddle;

/// <summary>
/// Lets long-running jobs (bulk seeding) mute EF command logging for their own async flow.
/// </summary>
public static class EfCommandLogging
{
    private static readonly AsyncLocal<bool> Suppressed = new();

    public static bool IsSuppressed => Suppressed.Value;

    public static IDisposable Suppress()
    {
        var previous = Suppressed.Value;
        Suppressed.Value = true;
        return new Restore(previous);
    }

    private sealed class Restore(bool previous) : IDisposable
    {
        public void Dispose() => Suppressed.Value = previous;
    }
}
