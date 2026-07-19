using Xunit;
using Crystal.Mmi.Constants;
using Crystal.Mmi.Queries;

namespace Crystal.Mmi.Tests.Queries;

public class QueryDiskTests {
  public static bool IsWindows => OperatingSystem.IsWindows();

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Constructor_DoesNotThrow() {
    var exception = Record.Exception(() => new QueryDisk());

    Assert.Null(exception);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void GetInfoDictionary_ContainsRealDiskProperties() {
    using var queryDisk = new QueryDisk();

    var result = queryDisk.GetInfoDictionary();

    Assert.NotEmpty(result);
    Assert.Contains(DiskConstants.ModelKey, result.Keys);
    Assert.Contains(DiskConstants.SizeKey, result.Keys);
    Assert.Contains(DiskConstants.InterfaceTypeKey, result.Keys);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void GetInfoDictionary_ReturnsTheSameInstanceItExposesAsInfoDictionary() {
    using var queryDisk = new QueryDisk();

    var result = queryDisk.GetInfoDictionary();

    Assert.Same(queryDisk.InfoDictionary, result);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Query_ThrowsNotImplementedException() {
    using var queryDisk = new QueryDisk();

    var exception = Record.Exception(() => queryDisk.Query(DiskConstants.QueryString));

    Assert.IsType<NotImplementedException>(exception);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Dispose_CanBeCalledMoreThanOnceWithoutThrowing() {
    var queryDisk = new QueryDisk();

    var exception = Record.Exception(() => {
      queryDisk.Dispose();
      queryDisk.Dispose();
    });

    Assert.Null(exception);
  }
}
