using Xunit;
using Crystal.Mmi.Constants;
using Crystal.Mmi.Queries;

namespace Crystal.Mmi.Tests.Queries;

public class QueryUserTests {
  public static bool IsWindows => OperatingSystem.IsWindows();

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Constructor_DoesNotThrow() {
    var exception = Record.Exception(() => new QueryUser());

    Assert.Null(exception);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void GetInfoDictionary_ContainsRealUserAccountProperties() {
    using var queryUser = new QueryUser();

    var result = queryUser.GetInfoDictionary();

    Assert.NotEmpty(result);
    Assert.Contains(UserConstants.NameKey, result.Keys);
    Assert.Contains(UserConstants.SIDKey, result.Keys);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void GetInfoDictionary_ReturnsTheSameInstanceItExposesAsInfoDictionary() {
    using var queryUser = new QueryUser();

    var result = queryUser.GetInfoDictionary();

    Assert.Same(queryUser.InfoDictionary, result);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Query_ThrowsNotImplementedException() {
    using var queryUser = new QueryUser();

    var exception = Record.Exception(() => queryUser.Query(UserConstants.QueryString));

    Assert.IsType<NotImplementedException>(exception);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Dispose_CanBeCalledMoreThanOnceWithoutThrowing() {
    var queryUser = new QueryUser();

    var exception = Record.Exception(() => {
      queryUser.Dispose();
      queryUser.Dispose();
    });

    Assert.Null(exception);
  }
}
