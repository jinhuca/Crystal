using ProcessModule.Models;

namespace ProcessModule.Tests;

// Records End task / Run new task calls instead of touching real processes, and returns a canned
// result so the failure path can be exercised. Set NextResult before an action to simulate success
// or a specific failure message.
internal sealed class FakeProcessController : IProcessController {
  public ProcessActionResult NextResult { get; set; } = ProcessActionResult.Ok;

  public uint? EndedPid { get; private set; }
  public int EndCallCount { get; private set; }

  public string? StartedCommand { get; private set; }
  public bool StartedAsAdmin { get; private set; }
  public int StartCallCount { get; private set; }

  public string? OpenedLocationPath { get; private set; }
  public int OpenLocationCallCount { get; private set; }

  public ProcessActionResult EndTask(uint processId) {
    EndCallCount++;
    EndedPid = processId;
    return NextResult;
  }

  public ProcessActionResult StartTask(string command, bool runAsAdmin = false) {
    StartCallCount++;
    StartedCommand = command;
    StartedAsAdmin = runAsAdmin;
    return NextResult;
  }

  public ProcessActionResult OpenFileLocation(string? imagePath) {
    OpenLocationCallCount++;
    OpenedLocationPath = imagePath;
    return NextResult;
  }
}
