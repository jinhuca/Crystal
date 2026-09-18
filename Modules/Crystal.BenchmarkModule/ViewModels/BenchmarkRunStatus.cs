namespace Crystal.BenchmarkModule.ViewModels;

/// <summary>Lifecycle state of a single benchmark row on the dashboard.</summary>
public enum BenchmarkRunStatus {
  Idle,
  Running,
  Completed,
  Failed,
  Cancelled,
}
