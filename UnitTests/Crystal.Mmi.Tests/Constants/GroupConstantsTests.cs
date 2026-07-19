using Xunit;
using Crystal.Mmi.Constants;

namespace Crystal.Mmi.Tests.Constants;

public class GroupConstantsTests {
  [Fact]
  public void QueryString_SelectsAllFromWin32Group() {
    Assert.Equal("SELECT * FROM Win32_Group", GroupConstants.QueryString);
  }

  [Theory]
  [InlineData(GroupConstants.CaptionKey, "Caption")]
  [InlineData(GroupConstants.DescriptionKey, "Description")]
  [InlineData(GroupConstants.DomainKey, "Domain")]
  [InlineData(GroupConstants.InstallDateKey, "InstallDate")]
  [InlineData(GroupConstants.LocalAccountKey, "LocalAccount")]
  [InlineData(GroupConstants.NameKey, "Name")]
  [InlineData(GroupConstants.SIDKey, "SID")]
  [InlineData(GroupConstants.SIDTypeKey, "SIDType")]
  [InlineData(GroupConstants.StatusKey, "Status")]
  public void PropertyKey_MatchesWin32GroupSchema(string key, string expected) {
    Assert.Equal(expected, key);
  }
}
