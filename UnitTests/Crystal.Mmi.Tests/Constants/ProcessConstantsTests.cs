using Xunit;
using Crystal.Mmi.Constants;

namespace Crystal.Mmi.Tests.Constants;

public class ProcessConstantsTests {
  [Fact]
  public void QueryString_SelectsAllFromWin32Process() {
    Assert.Equal("SELECT * FROM Win32_Process", ProcessConstants.QueryString);
  }

  [Theory]
  [InlineData(ProcessConstants.CaptionKey, "Caption")]
  [InlineData(ProcessConstants.CommandLineKey, "CommandLine")]
  [InlineData(ProcessConstants.CreationClassNameKey, "CreationClassName")]
  [InlineData(ProcessConstants.CreationDateKey, "CreationDate")]
  [InlineData(ProcessConstants.DescriptionKey, "Description")]
  [InlineData(ProcessConstants.ExecutablePathKey, "ExecutablePath")]
  [InlineData(ProcessConstants.ExecutionStateKey, "ExecutionState")]
  [InlineData(ProcessConstants.HandleKey, "Handle")]
  [InlineData(ProcessConstants.HandleCountKey, "HandleCount")]
  [InlineData(ProcessConstants.InstallDateKey, "InstallDate")]
  [InlineData(ProcessConstants.KernelModeTimeKey, "KernelModeTime")]
  [InlineData(ProcessConstants.MaximumWorkingSetSizeKey, "MaximumWorkingSetSize")]
  [InlineData(ProcessConstants.MinimumWorkingSetSizeKey, "MinimumWorkingSetSize")]
  [InlineData(ProcessConstants.NameKey, "Name")]
  [InlineData(ProcessConstants.PageFaultsKey, "PageFaults")]
  [InlineData(ProcessConstants.PageFileUsageKey, "PageFileUsage")]
  [InlineData(ProcessConstants.ParentProcessIdKey, "ParentProcessId")]
  [InlineData(ProcessConstants.PeakPageFileUsageKey, "PeakPageFileUsage")]
  [InlineData(ProcessConstants.PeakVirtualSizeKey, "PeakVirtualSize")]
  [InlineData(ProcessConstants.PeakWorkingSetSizeKey, "PeakWorkingSetSize")]
  [InlineData(ProcessConstants.PriorityKey, "Priority")]
  [InlineData(ProcessConstants.PrivatePageCountKey, "PrivatePageCount")]
  [InlineData(ProcessConstants.ProcessIdKey, "ProcessId")]
  [InlineData(ProcessConstants.SessionIdKey, "SessionId")]
  [InlineData(ProcessConstants.StatusKey, "Status")]
  [InlineData(ProcessConstants.TerminationDateKey, "TerminationDate")]
  [InlineData(ProcessConstants.ThreadCountKey, "ThreadCount")]
  [InlineData(ProcessConstants.UserModeTimeKey, "UserModeTime")]
  [InlineData(ProcessConstants.VirtualSizeKey, "VirtualSize")]
  [InlineData(ProcessConstants.WorkingSetSizeKey, "WorkingSetSize")]
  public void PropertyKey_MatchesWin32ProcessSchema(string key, string expected) {
    Assert.Equal(expected, key);
  }
}
