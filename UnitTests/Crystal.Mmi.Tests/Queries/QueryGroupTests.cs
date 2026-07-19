using Xunit;
using Crystal.Mmi.Constants;
using Crystal.Mmi.Queries;

namespace Crystal.Mmi.Tests.Queries;

public class QueryGroupTests {
  public static bool IsWindows => OperatingSystem.IsWindows();

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Constructor_DoesNotThrow() {
    var exception = Record.Exception(() => new QueryGroup());

    Assert.Null(exception);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void GetInfoDictionary_ContainsRealGroupProperties() {
    using var queryGroup = new QueryGroup();

    var result = queryGroup.GetInfoDictionary();

    Assert.NotEmpty(result);
    Assert.Contains(GroupConstants.NameKey, result.Keys);
    Assert.Contains(GroupConstants.SIDKey, result.Keys);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void GetInfoDictionary_ReturnsTheSameInstanceItExposesAsInfoDictionary() {
    using var queryGroup = new QueryGroup();

    var result = queryGroup.GetInfoDictionary();

    Assert.Same(queryGroup.InfoDictionary, result);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Query_ThrowsNotImplementedException() {
    using var queryGroup = new QueryGroup();

    var exception = Record.Exception(() => queryGroup.Query(GroupConstants.QueryString));

    Assert.IsType<NotImplementedException>(exception);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Dispose_CanBeCalledMoreThanOnceWithoutThrowing() {
    var queryGroup = new QueryGroup();

    var exception = Record.Exception(() => {
      queryGroup.Dispose();
      queryGroup.Dispose();
    });

    Assert.Null(exception);
  }
}
