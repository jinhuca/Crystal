namespace Crystal.Mmi.Tests.Helpers;

internal sealed class TestConsoleWriter : IDisposable
{
    private readonly StringWriter _writer = new();

    public TextWriter Writer => _writer;

    public string Output => _writer.ToString();

    public void Dispose() => _writer.Dispose();
}
