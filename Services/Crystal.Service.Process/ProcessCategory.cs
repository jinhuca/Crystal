namespace Crystal.Service.Process;

/// <summary>
/// How a process is grouped in the list, Task Manager-style.
/// </summary>
public enum ProcessCategory {
  /// <summary>
  /// Has a visible top-level window in the interactive session — a foreground app.
  /// </summary>
  App,
  /// <summary>
  /// Runs in the interactive session but has no visible window.
  /// </summary>
  BackgroundProcess,
  /// <summary>
  /// Runs in session 0 (services / system) — Windows infrastructure.
  /// </summary>
  WindowsProcess,
}

/// <summary>
/// Helpers for turning a <see cref="ProcessCategory"/> into the text the UI shows. Kept as
/// extensions so the enum stays a plain data type and the presentation strings live in one place.
/// </summary>
public static class ProcessCategoryExtensions {
  /// <summary>
  /// Display label for a category, used as the group header text.
  /// </summary>
  /// <param name="category">The category to render.</param>
  /// <returns>The human-readable group header ("Apps", "Background Processes", …).</returns>
  public static string ToDisplayName(this ProcessCategory category) => category switch {
    ProcessCategory.App => "Apps",
    ProcessCategory.BackgroundProcess => "Background Processes",
    ProcessCategory.WindowsProcess => "Windows Processes",
    _ => "Other",
  };
}
