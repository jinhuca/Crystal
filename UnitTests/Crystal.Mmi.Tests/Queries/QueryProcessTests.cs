using Xunit;
using Crystal.Mmi.Constants;
using Crystal.Mmi.Queries;

namespace Crystal.Mmi.Tests.Queries;

public class QueryProcessTests {
  public static bool IsWindows => OperatingSystem.IsWindows();

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Constructor_DoesNotThrow() {
    var exception = Record.Exception(() => new QueryProcess());

    Assert.Null(exception);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void GetInfoDictionary_ContainsRealProcessProperties() {
    using var queryProcess = new QueryProcess();

    var result = queryProcess.GetInfoDictionary();

    Assert.NotEmpty(result);
    Assert.Contains(ProcessConstants.NameKey, result.Keys);
    Assert.Contains(ProcessConstants.ProcessIdKey, result.Keys);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void GetInfoDictionary_ReturnsTheSameInstanceItExposesAsInfoDictionary() {
    using var queryProcess = new QueryProcess();

    var result = queryProcess.GetInfoDictionary();

    Assert.Same(queryProcess.InfoDictionary, result);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Query_ThrowsNotImplementedException() {
    using var queryProcess = new QueryProcess();

    var exception = Record.Exception(() => queryProcess.Query(ProcessConstants.QueryString));

    Assert.IsType<NotImplementedException>(exception);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Dispose_CanBeCalledMoreThanOnceWithoutThrowing() {
    var queryProcess = new QueryProcess();

    var exception = Record.Exception(() => {
      queryProcess.Dispose();
      queryProcess.Dispose();
    });

    Assert.Null(exception);
  }
}
