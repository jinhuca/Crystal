using Xunit;
using Crystal.Mmi.Constants;
using Crystal.Mmi.Queries;

namespace Crystal.Mmi.Tests.Queries;

// See the note in QueryOsTests: QueryBios has the same no-seam constructor, so every test
// here needs a live Windows MI runtime to even construct the object.
public class QueryBiosTests {
  public static bool IsWindows => OperatingSystem.IsWindows();

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Constructor_DoesNotThrow() {
    var exception = Record.Exception(() => new QueryBios());

    Assert.Null(exception);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void GetInfoDictionary_ContainsRealBiosProperties() {
    using var queryBios = new QueryBios();

    var result = queryBios.GetInfoDictionary();

    Assert.NotEmpty(result);
    Assert.Contains(BiosConstants.ManufacturerKey, result.Keys);
    Assert.Contains(BiosConstants.SerialNumberKey, result.Keys);
    Assert.Contains(BiosConstants.SMBIOSBIOSVersionKey, result.Keys);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void GetInfoDictionary_ReturnsTheSameInstanceItExposesAsInfoDictionary() {
    using var queryBios = new QueryBios();

    var result = queryBios.GetInfoDictionary();

    Assert.Same(queryBios.InfoDictionary, result);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Query_ThrowsNotImplementedException() {
    using var queryBios = new QueryBios();

    var exception = Record.Exception(() => queryBios.Query(BiosConstants.QueryString));

    Assert.IsType<NotImplementedException>(exception);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Dispose_CanBeCalledMoreThanOnceWithoutThrowing() {
    var queryBios = new QueryBios();

    var exception = Record.Exception(() => {
      queryBios.Dispose();
      queryBios.Dispose();
    });

    Assert.Null(exception);
  }
}
