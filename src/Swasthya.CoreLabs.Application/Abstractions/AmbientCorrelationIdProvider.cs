namespace Swasthya.CoreLabs.Application.Abstractions;

public sealed class AmbientCorrelationIdProvider : ICorrelationIdProvider
{
    private static readonly AsyncLocal<string> _current = new();

    public string Current => _current.Value ?? string.Empty;

    public static IDisposable Scope(string correlationId)
    {
        string previous = _current.Value ?? string.Empty;
        _current.Value = correlationId;
        return new ScopeHandle(previous);
    }

    private sealed class ScopeHandle(string previous) : IDisposable
    {
        public void Dispose() => _current.Value = previous;
    }
}
