using Xunit;
using Crystal.Mmi.Constants;

namespace Crystal.Mmi.Tests.Constants;

public class EventLogConstantsTests {
  [Fact]
  public void QueryString_SelectsAllFromWin32NTEventLogFile() {
    Assert.Equal("SELECT * FROM Win32_NTEventLogFile", EventLogConstants.QueryString);
  }

  [Theory]
  [InlineData(EventLogConstants.CaptionKey, "Caption")]
  [InlineData(EventLogConstants.CreationClassNameKey, "CreationClassName")]
  [InlineData(EventLogConstants.DescriptionKey, "Description")]
  [InlineData(EventLogConstants.FileSizeKey, "FileSize")]
  [InlineData(EventLogConstants.InstallDateKey, "InstallDate")]
  [InlineData(EventLogConstants.LogfileNameKey, "LogfileName")]
  [InlineData(EventLogConstants.MaxFileSizeKey, "MaxFileSize")]
  [InlineData(EventLogConstants.NameKey, "Name")]
  [InlineData(EventLogConstants.NumberOfRecordsKey, "NumberOfRecords")]
  [InlineData(EventLogConstants.OverwriteOutDatedKey, "OverwriteOutDated")]
  [InlineData(EventLogConstants.OverwritePolicyKey, "OverwritePolicy")]
  [InlineData(EventLogConstants.SourcesKey, "Sources")]
  [InlineData(EventLogConstants.StatusKey, "Status")]
  public void PropertyKey_MatchesWin32NTEventLogFileSchema(string key, string expected) {
    Assert.Equal(expected, key);
  }
}
