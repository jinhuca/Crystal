using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace Crystal.ProcessModule.Models;

/// <summary>
/// Win32-backed <see cref="IProcessController"/>: End task via <see cref="Process.Kill()"/>, Run new
/// task via <see cref="Process.Start(ProcessStartInfo)"/> with the shell so PATH lookups and elevation
/// work. Every failure mode (process already exited, access denied, bad command, UAC declined) is
/// caught and returned as a message — terminating or launching a process must never crash the
/// dashboard.
/// </summary>
public sealed class ProcessController : IProcessController {
  public ProcessActionResult EndTask(uint processId) {
    try {
      using var process = Process.GetProcessById((int)processId);
      process.Kill();
      return ProcessActionResult.Ok;
    }
    // The PID vanished between the click and the kill — treat as already-ended, not an error the
    // user needs to act on.
    catch (ArgumentException) {
      return ProcessActionResult.Fail($"Process {processId} is no longer running.");
    }
    catch (InvalidOperationException) {
      return ProcessActionResult.Fail($"Process {processId} is no longer running.");
    }
    // Protected / higher-integrity process the current token can't touch even when elevated.
    catch (Win32Exception ex) {
      return ProcessActionResult.Fail($"Can't end process {processId}: {ex.Message}");
    }
  }

  public ProcessActionResult StartTask(string command, bool runAsAdmin = false) {
    if (string.IsNullOrWhiteSpace(command))
      return ProcessActionResult.Fail("Enter a command to run.");

    // UseShellExecute lets the OS resolve bare names on PATH (e.g. "notepad") and is required for
    // the "runas" verb that triggers the UAC elevation prompt.
    var info = new ProcessStartInfo {
      FileName = command.Trim(),
      UseShellExecute = true,
      Verb = runAsAdmin ? "runas" : string.Empty,
    };

    try {
      using var started = Process.Start(info);
      return ProcessActionResult.Ok;
    }
    // The user dismissed the UAC prompt (error 1223, ERROR_CANCELLED) or the command couldn't be
    // found / launched. Win32Exception covers both.
    catch (Win32Exception ex) {
      return ProcessActionResult.Fail($"Couldn't run \"{command}\": {ex.Message}");
    }
  }

  public ProcessActionResult OpenFileLocation(string? imagePath) {
    if (string.IsNullOrWhiteSpace(imagePath))
      return ProcessActionResult.Fail("File location is unavailable for this process.");
    if (!File.Exists(imagePath))
      return ProcessActionResult.Fail($"File no longer exists: {imagePath}");

    // explorer /select, opens the containing folder with the file highlighted. The path is quoted
    // to survive spaces; arguments go through UseShellExecute=false so /select is parsed by Explorer.
    try {
      using var started = Process.Start(new ProcessStartInfo {
        FileName = "explorer.exe",
        Arguments = $"/select,\"{imagePath}\"",
        UseShellExecute = true,
      });
      return ProcessActionResult.Ok;
    }
    catch (Win32Exception ex) {
      return ProcessActionResult.Fail($"Couldn't open file location: {ex.Message}");
    }
  }
}
