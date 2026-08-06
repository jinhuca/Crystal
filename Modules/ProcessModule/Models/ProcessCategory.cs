namespace ProcessModule.Models;

/// <summary>How a process is grouped in the list, Task Manager-style.</summary>
public enum ProcessCategory {
  /// <summary>Has a visible top-level window in the interactive session — a foreground app.</summary>
  App,
  /// <summary>Runs in the interactive session but has no visible window.</summary>
  BackgroundProcess,
  /// <summary>Runs in session 0 (services / system) — Windows infrastructure.</summary>
  WindowsProcess,
}

public static class ProcessCategoryExtensions {
  /// <summary>Display label for a category, used as the group header text.</summary>
  public static string ToDisplayName(this ProcessCategory category) => category switch {
    ProcessCategory.App => "Apps",
    ProcessCategory.BackgroundProcess => "Background Processes",
    ProcessCategory.WindowsProcess => "Windows Processes",
    _ => "Other",
  };
}
