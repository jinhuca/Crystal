using Xunit;
using Crystal.Mmi.Constants;

namespace Crystal.Mmi.Tests.Constants;

public class ServiceConstantsTests {
  [Fact]
  public void QueryString_SelectsAllFromWin32Service() {
    Assert.Equal("SELECT * FROM Win32_Service", ServiceConstants.QueryString);
  }

  [Theory]
  [InlineData(ServiceConstants.AcceptPauseKey, "AcceptPause")]
  [InlineData(ServiceConstants.AcceptStopKey, "AcceptStop")]
  [InlineData(ServiceConstants.CaptionKey, "Caption")]
  [InlineData(ServiceConstants.CreationClassNameKey, "CreationClassName")]
  [InlineData(ServiceConstants.DelayedAutoStartKey, "DelayedAutoStart")]
  [InlineData(ServiceConstants.DescriptionKey, "Description")]
  [InlineData(ServiceConstants.DesktopInteractKey, "DesktopInteract")]
  [InlineData(ServiceConstants.DisplayNameKey, "DisplayName")]
  [InlineData(ServiceConstants.ErrorControlKey, "ErrorControl")]
  [InlineData(ServiceConstants.ExitCodeKey, "ExitCode")]
  [InlineData(ServiceConstants.InstallDateKey, "InstallDate")]
  [InlineData(ServiceConstants.NameKey, "Name")]
  [InlineData(ServiceConstants.PathNameKey, "PathName")]
  [InlineData(ServiceConstants.ProcessIdKey, "ProcessId")]
  [InlineData(ServiceConstants.ServiceSpecificExitCodeKey, "ServiceSpecificExitCode")]
  [InlineData(ServiceConstants.ServiceTypeKey, "ServiceType")]
  [InlineData(ServiceConstants.StartedKey, "Started")]
  [InlineData(ServiceConstants.StartModeKey, "StartMode")]
  [InlineData(ServiceConstants.StartNameKey, "StartName")]
  [InlineData(ServiceConstants.StateKey, "State")]
  [InlineData(ServiceConstants.StatusKey, "Status")]
  [InlineData(ServiceConstants.SystemCreationClassNameKey, "SystemCreationClassName")]
  [InlineData(ServiceConstants.SystemNameKey, "SystemName")]
  [InlineData(ServiceConstants.TagIdKey, "TagId")]
  [InlineData(ServiceConstants.WaitHintKey, "WaitHint")]
  public void PropertyKey_MatchesWin32ServiceSchema(string key, string expected) {
    Assert.Equal(expected, key);
  }
}
