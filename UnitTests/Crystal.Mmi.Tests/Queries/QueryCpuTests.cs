using Xunit;
using Crystal.Mmi.Constants;
using Crystal.Mmi.Queries;

namespace Crystal.Mmi.Tests.Queries;

// See the note in QueryOsTests: QueryCpu has the same no-seam constructor, so every test
// here needs a live Windows MI runtime to even construct the object.
public class QueryCpuTests {
  public static bool IsWindows => OperatingSystem.IsWindows();

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Constructor_DoesNotThrow() {
    var exception = Record.Exception(() => new QueryCpu());

    Assert.Null(exception);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void GetInfoDictionary_ContainsRealProcessorProperties() {
    using var queryCpu = new QueryCpu();

    var result = queryCpu.GetInfoDictionary();

    Assert.NotEmpty(result);
    Assert.Contains(CpuConstants.NameKey, result.Keys);
    Assert.Contains(CpuConstants.ManufacturerKey, result.Keys);
    Assert.Contains(CpuConstants.NumberOfCoresKey, result.Keys);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void GetInfoDictionary_ReturnsTheSameInstanceItExposesAsInfoDictionary() {
    using var queryCpu = new QueryCpu();

    var result = queryCpu.GetInfoDictionary();

    Assert.Same(queryCpu.InfoDictionary, result);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Query_ThrowsNotImplementedException() {
    using var queryCpu = new QueryCpu();

    var exception = Record.Exception(() => queryCpu.Query(CpuConstants.QueryString));

    Assert.IsType<NotImplementedException>(exception);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Dispose_CanBeCalledMoreThanOnceWithoutThrowing() {
    var queryCpu = new QueryCpu();

    var exception = Record.Exception(() => {
      queryCpu.Dispose();
      queryCpu.Dispose();
    });

    Assert.Null(exception);
  }
}
