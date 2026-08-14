namespace Crystal.ProcessModule.Models;

/// <summary>Outcome of an End task / Run new task action — <see cref="Succeeded"/> plus a short
/// human-readable message the UI can surface (the reason on failure, empty on success).</summary>
public readonly record struct ProcessActionResult(bool Succeeded, string Message) {
  public static ProcessActionResult Ok => new(true, "");
  public static ProcessActionResult Fail(string message) => new(false, message);
}

/// <summary>
/// Terminates and launches processes on the user's behalf (the Task Manager "End task" and "Run new
/// task" actions). Kept behind an interface so the view model — which decides <em>when</em> to act —
/// stays free of Win32 side effects and can be unit-tested with a fake.
/// </summary>
public interface IProcessController {
  /// <summary>Forcibly terminates the process with <paramref name="processId"/>. Returns a failure
  /// result (never throws) if the process is already gone or access is denied.</summary>
  ProcessActionResult EndTask(uint processId);

  /// <summary>Launches <paramref name="command"/> (an executable path or a name resolvable on PATH),
  /// optionally elevated. Returns a failure result (never throws) if the command can't be started.</summary>
  ProcessActionResult StartTask(string command, bool runAsAdmin = false);

  /// <summary>Opens the folder containing <paramref name="imagePath"/> with the file selected, like
  /// Task Manager's "Open file location". Returns a failure result (never throws) when the path is
  /// unknown or no longer exists.</summary>
  ProcessActionResult OpenFileLocation(string? imagePath);
}
