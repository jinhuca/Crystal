using Xunit;
using Crystal.Mmi.Constants;
using Crystal.Mmi.Queries;

namespace Crystal.Mmi.Tests.Queries;

// QueryOs constructs a real CimSession/CimInstance directly in its constructor -- there's no
// seam (no injected ICimSession/factory) to substitute a fake, so every test here needs a live
// MI runtime to even construct the object. That's the same kind of problem the
// HardwareService/PollAllCore work solved for hardware polling; QueryOs would benefit from the
// same kind of seam if it needs to run in CI without a Windows agent.
public class QueryOsTests {
  public static bool IsWindows => OperatingSystem.IsWindows();

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Constructor_DoesNotThrow() {
    var exception = Record.Exception(() => new QueryOs());

    Assert.Null(exception);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void GetInfoDictionary_ContainsRealOperatingSystemProperties() {
    using var queryOs = new QueryOs();

    var result = queryOs.GetInfoDictionary();

    Assert.NotEmpty(result);
    Assert.Contains(OsConstants.CaptionKey, result.Keys);
    Assert.Contains(OsConstants.VersionKey, result.Keys);
    Assert.Contains(OsConstants.OSArchitectureKey, result.Keys);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void GetInfoDictionary_ReturnsTheSameInstanceItExposesAsInfoDictionary() {
    using var queryOs = new QueryOs();

    var result = queryOs.GetInfoDictionary();

    Assert.Same(queryOs.InfoDictionary, result);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Query_ThrowsNotImplementedException() {
    using var queryOs = new QueryOs();

    var exception = Record.Exception(() => queryOs.Query(OsConstants.QueryString));

    Assert.IsType<NotImplementedException>(exception);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void QueryMultiple_ThrowsNotImplementedException() {
    using var queryOs = new QueryOs();

    var exception = Record.Exception(() => queryOs.QueryMultiple(OsConstants.QueryString));

    Assert.IsType<NotImplementedException>(exception);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Dispose_CanBeCalledMoreThanOnceWithoutThrowing() {
    var queryOs = new QueryOs();

    var exception = Record.Exception(() => {
      queryOs.Dispose();
      queryOs.Dispose();
    });

    Assert.Null(exception);
  }
}
