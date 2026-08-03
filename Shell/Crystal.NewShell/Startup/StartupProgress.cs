namespace Crystal.NewShell.Startup;

/// <summary>Lifecycle state of a single startup component.</summary>
public enum StartupComponentState {
  Pending,
  Loading,
  Completed,
  Failed,
}

/// <summary>
/// A single progress report emitted by <see cref="StartupLoader"/> as it warms up each
/// component. Reports are marshalled to the UI thread by <c>IProgress</c>.
/// </summary>
/// <param name="Name">Human-readable component name (e.g. "CPU").</param>
/// <param name="State">Where this component is in its lifecycle.</param>
/// <param name="Completed">Number of components finished so far.</param>
/// <param name="Total">Total number of components to load.</param>
public sealed record StartupProgress(string Name, StartupComponentState State, int Completed, int Total) {
  /// <summary>Overall completion as a percentage in the range 0-100.</summary>
  public double Percent => Total == 0 ? 100 : Completed * 100.0 / Total;
}
