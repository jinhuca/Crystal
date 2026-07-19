using Xunit;
using Crystal.Mmi.Constants;
using Crystal.Mmi.Queries;

namespace Crystal.Mmi.Tests.Queries;

public class QueryEventLogTests {
  public static bool IsWindows => OperatingSystem.IsWindows();

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Constructor_DoesNotThrow() {
    var exception = Record.Exception(() => new QueryEventLog());

    Assert.Null(exception);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void GetInfoDictionary_ContainsRealEventLogFileProperties() {
    using var queryEventLog = new QueryEventLog();

    var result = queryEventLog.GetInfoDictionary();

    Assert.NotEmpty(result);
    Assert.Contains(EventLogConstants.LogfileNameKey, result.Keys);
    Assert.Contains(EventLogConstants.NumberOfRecordsKey, result.Keys);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void GetInfoDictionary_ReturnsTheSameInstanceItExposesAsInfoDictionary() {
    using var queryEventLog = new QueryEventLog();

    var result = queryEventLog.GetInfoDictionary();

    Assert.Same(queryEventLog.InfoDictionary, result);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Query_ThrowsNotImplementedException() {
    using var queryEventLog = new QueryEventLog();

    var exception = Record.Exception(() => queryEventLog.Query(EventLogConstants.QueryString));

    Assert.IsType<NotImplementedException>(exception);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Dispose_CanBeCalledMoreThanOnceWithoutThrowing() {
    var queryEventLog = new QueryEventLog();

    var exception = Record.Exception(() => {
      queryEventLog.Dispose();
      queryEventLog.Dispose();
    });

    Assert.Null(exception);
  }
}
