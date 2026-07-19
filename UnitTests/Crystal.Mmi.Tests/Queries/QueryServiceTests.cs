using Xunit;
using Crystal.Mmi.Constants;
using Crystal.Mmi.Queries;

namespace Crystal.Mmi.Tests.Queries;

public class QueryServiceTests {
  public static bool IsWindows => OperatingSystem.IsWindows();

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Constructor_DoesNotThrow() {
    var exception = Record.Exception(() => new QueryService());

    Assert.Null(exception);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void GetInfoDictionary_ContainsRealServiceProperties() {
    using var queryService = new QueryService();

    var result = queryService.GetInfoDictionary();

    Assert.NotEmpty(result);
    Assert.Contains(ServiceConstants.NameKey, result.Keys);
    Assert.Contains(ServiceConstants.StateKey, result.Keys);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void GetInfoDictionary_ReturnsTheSameInstanceItExposesAsInfoDictionary() {
    using var queryService = new QueryService();

    var result = queryService.GetInfoDictionary();

    Assert.Same(queryService.InfoDictionary, result);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Query_ThrowsNotImplementedException() {
    using var queryService = new QueryService();

    var exception = Record.Exception(() => queryService.Query(ServiceConstants.QueryString));

    Assert.IsType<NotImplementedException>(exception);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Dispose_CanBeCalledMoreThanOnceWithoutThrowing() {
    var queryService = new QueryService();

    var exception = Record.Exception(() => {
      queryService.Dispose();
      queryService.Dispose();
    });

    Assert.Null(exception);
  }
}
