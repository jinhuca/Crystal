using Xunit;
using Crystal.Mmi.Constants;

namespace Crystal.Mmi.Tests.Constants;

public class UserConstantsTests {
  [Fact]
  public void QueryString_SelectsAllFromWin32UserAccount() {
    Assert.Equal("SELECT * FROM Win32_UserAccount", UserConstants.QueryString);
  }

  [Theory]
  [InlineData(UserConstants.AccountTypeKey, "AccountType")]
  [InlineData(UserConstants.CaptionKey, "Caption")]
  [InlineData(UserConstants.DescriptionKey, "Description")]
  [InlineData(UserConstants.DisabledKey, "Disabled")]
  [InlineData(UserConstants.DomainKey, "Domain")]
  [InlineData(UserConstants.FullNameKey, "FullName")]
  [InlineData(UserConstants.InstallDateKey, "InstallDate")]
  [InlineData(UserConstants.LocalAccountKey, "LocalAccount")]
  [InlineData(UserConstants.LockoutKey, "Lockout")]
  [InlineData(UserConstants.NameKey, "Name")]
  [InlineData(UserConstants.PasswordChangeableKey, "PasswordChangeable")]
  [InlineData(UserConstants.PasswordExpiresKey, "PasswordExpires")]
  [InlineData(UserConstants.PasswordRequiredKey, "PasswordRequired")]
  [InlineData(UserConstants.SIDKey, "SID")]
  [InlineData(UserConstants.SIDTypeKey, "SIDType")]
  [InlineData(UserConstants.StatusKey, "Status")]
  public void PropertyKey_MatchesWin32UserAccountSchema(string key, string expected) {
    Assert.Equal(expected, key);
  }
}
